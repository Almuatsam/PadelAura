using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Bookings.Services;
using Padel.Application.Common;
using Padel.Application.Common.Exceptions;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Bookings.CreateBooking;

public sealed class CreateBookingCommandHandler(IApplicationDbContext context, IThawaniClient thawaniClient)
    : IRequestHandler<CreateBookingCommand, CreateBookingResult>
{
    private const string ReferenceAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public async Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var activeCourts = await context.Courts
            .Include(c => c.Schedules)
            .Where(c => c.Status == CourtStatus.Active)
            .ToListAsync(cancellationToken);

        var dates = request.Slots.Select(s => s.Date).Distinct().ToList();
        var closures = await context.CourtClosures
            .Where(c => dates.Contains(c.ClosureDate))
            .ToListAsync(cancellationToken);

        var orderedSlots = request.Slots.OrderBy(s => s.Date).ThenBy(s => s.StartTime).ToList();

        await using var transaction = await context.BeginTransactionAsync(cancellationToken);

        var assignments = new List<(BookingSlotInput Slot, long CourtId, decimal Price)>();

        foreach (var slot in orderedSlots)
        {
            var dayOfWeek = (int)slot.Date.DayOfWeek;
            var closuresForDate = closures.Where(c => c.ClosureDate == slot.Date).ToList();

            var eligibleCourts = SlotAvailabilityCalculator.GetEligibleCourts(
                activeCourts, closuresForDate, dayOfWeek, slot.StartTime, slot.EndTime);

            if (eligibleCourts.Count == 0)
            {
                throw new SlotUnavailableException(slot.Date, slot.StartTime, slot.EndTime);
            }

            var eligibleIds = eligibleCourts.Select(c => c.Id).ToList();
            var takenIds = await context.GetCourtIdsWithActiveBookingForUpdateAsync(
                eligibleIds, slot.Date, slot.StartTime, cancellationToken);

            var availableCourts = eligibleCourts.Where(c => !takenIds.Contains(c.Id)).ToList();
            if (availableCourts.Count == 0)
            {
                throw new SlotUnavailableException(slot.Date, slot.StartTime, slot.EndTime);
            }

            var assignedCourt = availableCourts[Random.Shared.Next(availableCourts.Count)];

            // Slot price is the min eligible-court rate, not the assigned court's own rate — the
            // customer is quoted and charged this price regardless of which court the random draw
            // picks (see the pricing decision in the Phase 3 plan).
            var slotPrice = eligibleCourts.Min(c => c.HourPrice);

            assignments.Add((slot, assignedCourt.Id, slotPrice));
        }

        var totalHours = assignments.Count;
        var rawSubtotal = assignments.Sum(a => a.Price);

        var activePromotions = await context.Promotions
            .Include(p => p.Rules)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(OmanClock.Now());
        var (subtotal, discount, _) = PricingCalculator.Calculate(totalHours, rawSubtotal, activePromotions, today);

        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.Phone == request.Phone, cancellationToken);

        if (customer is null)
        {
            customer = new Customer(request.Phone, request.FullName, request.Email);
            context.Customers.Add(customer);
            await context.SaveChangesAsync(cancellationToken);
        }
        else if (request.FullName is not null || request.Email is not null)
        {
            customer.UpdateContactInfo(request.FullName, request.Email);
        }

        var bookingReference = await GenerateUniqueReferenceAsync(cancellationToken);
        var booking = new Booking(bookingReference, customer.Id, request.PaymentMethod);
        context.Bookings.Add(booking);

        // Save first so `booking.Id` is populated before it's used as the FK for the booking
        // items below (mirrors the court/schedule insert order used elsewhere in this codebase).
        await context.SaveChangesAsync(cancellationToken);

        foreach (var (slot, courtId, price) in assignments)
        {
            booking.AddItem(courtId, slot.Date, slot.StartTime, slot.EndTime, price);
        }

        booking.ApplyPricing(subtotal, discount);

        if (request.PaymentMethod == PaymentMethod.PayOnArrival)
        {
            booking.Confirm();
        }

        // Online payments stay Pending until Thawani confirms payment (lazily re-verified on read,
        // or via the webhook — see PaymentStatusSynchronizer). The Thawani call itself happens
        // after the transaction below commits: it's a network call, and must never happen while
        // still holding the FOR UPDATE row/gap lock from the slot-resolution loop above.
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        string? paymentUrl = null;
        if (request.PaymentMethod == PaymentMethod.Online)
        {
            paymentUrl = await CreateThawaniSessionAsync(booking, request, totalHours, cancellationToken);
        }

        return new CreateBookingResult(booking.BookingReference, booking.Total, paymentUrl);
    }

    private async Task<string?> CreateThawaniSessionAsync(
        Booking booking, CreateBookingCommand request, int totalHours, CancellationToken cancellationToken)
    {
        var checkoutRequest = new CreateCheckoutSessionRequest(
            booking.BookingReference,
            $"Padel court booking - {totalHours} hour(s)",
            (int)Math.Round(booking.Total * 1000),
            request.FullName,
            request.Phone);

        try
        {
            var sessionId = await thawaniClient.CreateCheckoutSessionAsync(checkoutRequest, cancellationToken);

            context.Payments.Add(new Payment(booking.Id, "Thawani", booking.Total, sessionId));
            await context.SaveChangesAsync(cancellationToken);

            return thawaniClient.BuildCheckoutUrl(sessionId);
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException)
        {
            // The booking/slot is already reserved regardless — record the failed attempt rather
            // than losing the reservation because the payment gateway hiccuped.
            var payment = new Payment(booking.Id, "Thawani", booking.Total);
            payment.MarkFailed();
            context.Payments.Add(payment);
            await context.SaveChangesAsync(cancellationToken);

            return null;
        }
    }

    private async Task<string> GenerateUniqueReferenceAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var suffix = new string(Enumerable.Range(0, 6)
                .Select(_ => ReferenceAlphabet[Random.Shared.Next(ReferenceAlphabet.Length)])
                .ToArray());
            var reference = $"PDL-{suffix}";

            var exists = await context.Bookings.AnyAsync(b => b.BookingReference == reference, cancellationToken);
            if (!exists)
            {
                return reference;
            }
        }

        throw new InvalidOperationException("Failed to generate a unique booking reference.");
    }
}
