using FluentAssertions;
using Padel.Application.Bookings.CreateBooking;
using Padel.Application.Bookings.GetBookingQuote;
using Padel.Application.Common.Exceptions;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Tests.Bookings;

public sealed class GetBookingQuoteQueryHandlerTests
{
    private static readonly DateOnly FutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));

    private static async Task<Court> SeedCourtAsync(
        Padel.Infrastructure.Persistence.PadelDbContext context, decimal hourPrice = 15m)
    {
        var court = new Court("Court A", hourPrice);
        context.Courts.Add(court);
        await context.SaveChangesAsync(CancellationToken.None);

        court.ReplaceSchedules([new CourtSchedule(court.Id, (int)FutureDate.DayOfWeek, new TimeOnly(8, 0), new TimeOnly(23, 0))]);
        await context.SaveChangesAsync(CancellationToken.None);

        return court;
    }

    private static async Task SeedActivePromotionAsync(
        Padel.Infrastructure.Persistence.PadelDbContext context, int minimumHours, DiscountType type, decimal value)
    {
        var promotion = new Promotion("Test promo", isActive: true, startDate: null, endDate: null);
        context.Promotions.Add(promotion);
        await context.SaveChangesAsync(CancellationToken.None);

        promotion.ReplaceRules([new PricingRule(promotion.Id, minimumHours, type, value)]);
        await context.SaveChangesAsync(CancellationToken.None);
    }

    private static GetBookingQuoteQueryHandler CreateHandler(Padel.Infrastructure.Persistence.PadelDbContext context) =>
        new(new TestApplicationDbContext(context));

    [Fact]
    public async Task Handle_ReturnsRawPrice_WhenNoPromotionIsActive()
    {
        await using var context = TestDbContextFactory.Create();
        await SeedCourtAsync(context, hourPrice: 12m);

        var handler = CreateHandler(context);
        var query = new GetBookingQuoteQuery(
            [new BookingSlotInput(FutureDate, new TimeOnly(9, 0), new TimeOnly(10, 0))]);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Subtotal.Should().Be(12m);
        result.Discount.Should().Be(0m);
        result.Total.Should().Be(12m);
    }

    [Fact]
    public async Task Handle_AppliesActivePromotion_SoTotalMatchesWhatBookingCreationWillCharge()
    {
        // Reproduces the reported bug: a court priced at 12.000 OMR/hour with an active
        // "1 hour+ -> 10.000 OMR/hour" fixed-rate promotion must quote a 10.000 total, matching
        // exactly what CreateBookingCommandHandler charges (and what gets sent to Thawani) for the
        // same cart — never the undiscounted 12.000 the raw per-slot price would suggest.
        await using var context = TestDbContextFactory.Create();
        await SeedCourtAsync(context, hourPrice: 12m);
        await SeedActivePromotionAsync(context, minimumHours: 1, DiscountType.FixedRate, value: 10m);

        var handler = CreateHandler(context);
        var query = new GetBookingQuoteQuery(
            [new BookingSlotInput(FutureDate, new TimeOnly(9, 0), new TimeOnly(10, 0))]);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Subtotal.Should().Be(12m);
        result.Discount.Should().Be(2m);
        result.Total.Should().Be(10m);
    }

    [Fact]
    public async Task Handle_MatchesCreateBookingTotal_ForTheSameCartAndActivePromotion()
    {
        await using var context = TestDbContextFactory.Create();
        await SeedCourtAsync(context, hourPrice: 12m);
        await SeedActivePromotionAsync(context, minimumHours: 1, DiscountType.FixedRate, value: 10m);

        var slots = new[] { new BookingSlotInput(FutureDate, new TimeOnly(9, 0), new TimeOnly(10, 0)) };

        var quoteHandler = CreateHandler(context);
        var quote = await quoteHandler.Handle(new GetBookingQuoteQuery(slots), CancellationToken.None);

        var bookingHandler = new CreateBookingCommandHandler(
            new TestApplicationDbContext(context), NSubstitute.Substitute.For<Padel.Application.Common.Interfaces.IThawaniClient>());
        var booking = await bookingHandler.Handle(
            new CreateBookingCommand("+96891234567", null, null, PaymentMethod.PayOnArrival, slots),
            CancellationToken.None);

        quote.Total.Should().Be(booking.Total);
    }

    [Fact]
    public async Task Handle_ThrowsSlotUnavailableException_WhenNoCourtIsScheduledForThatWindow()
    {
        await using var context = TestDbContextFactory.Create();
        await SeedCourtAsync(context);

        var handler = CreateHandler(context);
        var query = new GetBookingQuoteQuery(
            [new BookingSlotInput(FutureDate, new TimeOnly(6, 0), new TimeOnly(7, 0))]);

        var act = () => handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<SlotUnavailableException>();
    }
}
