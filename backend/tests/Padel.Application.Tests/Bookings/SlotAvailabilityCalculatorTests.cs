using FluentAssertions;
using Padel.Application.Bookings.Services;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Tests.Bookings;

public sealed class SlotAvailabilityCalculatorTests
{
    private static readonly DateOnly Date = new(2026, 8, 2); // a Sunday (dayOfWeek 0)

    private static Court CreateCourt(string name, CourtStatus status = CourtStatus.Active)
    {
        var court = new Court(name, 15m, status);
        court.ReplaceSchedules([new CourtSchedule(0, 0, new TimeOnly(8, 0), new TimeOnly(23, 0))]);
        return court;
    }

    [Fact]
    public void GetEligibleCourts_ReturnsCourt_WhenSlotIsWithinSchedule()
    {
        var court = CreateCourt("Court A");

        var result = SlotAvailabilityCalculator.GetEligibleCourts(
            [court], [], 0, new TimeOnly(9, 0), new TimeOnly(10, 0));

        result.Should().ContainSingle();
    }

    [Fact]
    public void GetEligibleCourts_ExcludesCourt_WhenSlotIsBeforeOpening()
    {
        var court = CreateCourt("Court A");

        var result = SlotAvailabilityCalculator.GetEligibleCourts(
            [court], [], 0, new TimeOnly(7, 0), new TimeOnly(8, 0));

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetEligibleCourts_ExcludesInactiveCourt()
    {
        var court = CreateCourt("Court A", CourtStatus.Inactive);

        var result = SlotAvailabilityCalculator.GetEligibleCourts(
            [court], [], 0, new TimeOnly(9, 0), new TimeOnly(10, 0));

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetEligibleCourts_ExcludesCourt_WhenFullDayClosureApplies()
    {
        var court = CreateCourt("Court A");
        var closure = new CourtClosure(null, Date, null, null, "Maintenance");

        var result = SlotAvailabilityCalculator.GetEligibleCourts(
            [court], [closure], 0, new TimeOnly(9, 0), new TimeOnly(10, 0));

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetEligibleCourts_ExcludesOnlyOverlappingWindow_ForPartialClosure()
    {
        var court = CreateCourt("Court A");
        var closure = new CourtClosure(court.Id, Date, new TimeOnly(10, 0), new TimeOnly(12, 0), "Event");

        var before = SlotAvailabilityCalculator.GetEligibleCourts(
            [court], [closure], 0, new TimeOnly(9, 0), new TimeOnly(10, 0));
        var during = SlotAvailabilityCalculator.GetEligibleCourts(
            [court], [closure], 0, new TimeOnly(10, 0), new TimeOnly(11, 0));

        before.Should().ContainSingle();
        during.Should().BeEmpty();
    }
}
