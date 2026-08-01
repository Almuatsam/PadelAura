using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Bookings.Services;
using Padel.Application.Common;
using Padel.Application.Common.Exceptions;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Enums;

namespace Padel.Application.Bookings.GetBookingQuote;

/// <summary>
/// Read-only preview of what CreateBookingCommandHandler will charge for the same slot selection —
/// same eligibility rule (min eligible-court rate per slot) and the same PricingCalculator, so the
/// customer never sees a total on the website that differs from what's actually charged. Skips
/// occupancy/locking (a quote isn't a reservation) since price only depends on eligibility, not on
/// whether a slot is currently taken.
/// </summary>
public sealed class GetBookingQuoteQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetBookingQuoteQuery, BookingQuoteDto>
{
    public async Task<BookingQuoteDto> Handle(GetBookingQuoteQuery request, CancellationToken cancellationToken)
    {
        var activeCourts = await context.Courts
            .Include(c => c.Schedules)
            .Where(c => c.Status == CourtStatus.Active)
            .ToListAsync(cancellationToken);

        var dates = request.Slots.Select(s => s.Date).Distinct().ToList();
        var closures = await context.CourtClosures
            .Where(c => dates.Contains(c.ClosureDate))
            .ToListAsync(cancellationToken);

        var slotPrices = new List<decimal>();

        foreach (var slot in request.Slots)
        {
            var dayOfWeek = (int)slot.Date.DayOfWeek;
            var closuresForDate = closures.Where(c => c.ClosureDate == slot.Date).ToList();

            var eligibleCourts = SlotAvailabilityCalculator.GetEligibleCourts(
                activeCourts, closuresForDate, dayOfWeek, slot.StartTime, slot.EndTime);

            if (eligibleCourts.Count == 0)
            {
                throw new SlotUnavailableException(slot.Date, slot.StartTime, slot.EndTime);
            }

            slotPrices.Add(eligibleCourts.Min(c => c.HourPrice));
        }

        var totalHours = slotPrices.Count;
        var rawSubtotal = slotPrices.Sum();

        var activePromotions = await context.Promotions
            .Include(p => p.Rules)
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(OmanClock.Now());
        var (_, discount, total) = PricingCalculator.Calculate(totalHours, rawSubtotal, activePromotions, today);

        return new BookingQuoteDto(rawSubtotal, discount, total);
    }
}
