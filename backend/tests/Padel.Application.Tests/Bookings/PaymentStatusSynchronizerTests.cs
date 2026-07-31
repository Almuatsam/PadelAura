using FluentAssertions;
using NSubstitute;
using Padel.Application.Bookings.Services;
using Padel.Application.Common.Interfaces;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Tests.Bookings;

public sealed class PaymentStatusSynchronizerTests
{
    private static async Task<(Booking Booking, Payment Payment)> SeedPendingOnlineBookingAsync(
        Padel.Infrastructure.Persistence.PadelDbContext context, string sessionId = "checkout_123")
    {
        var customer = new Customer("+96891234567", null, null);
        context.Customers.Add(customer);
        await context.SaveChangesAsync(CancellationToken.None);

        var booking = new Booking("PDL-ABCDEF", customer.Id, PaymentMethod.Online);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync(CancellationToken.None);

        booking.AddItem(1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(9, 0), new TimeOnly(10, 0), 15m);
        booking.ApplyPricing(15m, 0m);

        var payment = new Payment(booking.Id, "Thawani", 15m, sessionId);
        context.Payments.Add(payment);
        await context.SaveChangesAsync(CancellationToken.None);

        return (booking, payment);
    }

    [Fact]
    public async Task SyncAsync_ConfirmsBookingAndMarksPaymentSuccess_WhenThawaniReportsPaid()
    {
        await using var context = TestDbContextFactory.Create();
        var (booking, _) = await SeedPendingOnlineBookingAsync(context);

        var thawaniClient = Substitute.For<IThawaniClient>();
        thawaniClient.GetSessionStatusAsync("checkout_123", Arg.Any<CancellationToken>())
            .Returns(new ThawaniSessionStatus("checkout_123", ThawaniPaymentStatus.Paid, booking.BookingReference, 15000));

        var synchronizer = new PaymentStatusSynchronizer(new TestApplicationDbContext(context), thawaniClient);

        await synchronizer.SyncAsync(booking, CancellationToken.None);

        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.PaymentStatus.Should().Be(PaymentStatus.Paid);
        context.Payments.Single().Status.Should().Be(PaymentTransactionStatus.Success);
    }

    [Fact]
    public async Task SyncAsync_CancelsBookingAndMarksPaymentFailed_WhenThawaniReportsCancelled()
    {
        await using var context = TestDbContextFactory.Create();
        var (booking, _) = await SeedPendingOnlineBookingAsync(context);

        var thawaniClient = Substitute.For<IThawaniClient>();
        thawaniClient.GetSessionStatusAsync("checkout_123", Arg.Any<CancellationToken>())
            .Returns(new ThawaniSessionStatus("checkout_123", ThawaniPaymentStatus.Cancelled, booking.BookingReference, 15000));

        var synchronizer = new PaymentStatusSynchronizer(new TestApplicationDbContext(context), thawaniClient);

        await synchronizer.SyncAsync(booking, CancellationToken.None);

        booking.Status.Should().Be(BookingStatus.Cancelled);
        context.Payments.Single().Status.Should().Be(PaymentTransactionStatus.Failed);
    }

    [Fact]
    public async Task SyncAsync_LeavesBookingPending_WhenThawaniReportsUnpaid()
    {
        await using var context = TestDbContextFactory.Create();
        var (booking, _) = await SeedPendingOnlineBookingAsync(context);

        var thawaniClient = Substitute.For<IThawaniClient>();
        thawaniClient.GetSessionStatusAsync("checkout_123", Arg.Any<CancellationToken>())
            .Returns(new ThawaniSessionStatus("checkout_123", ThawaniPaymentStatus.Unpaid, booking.BookingReference, 15000));

        var synchronizer = new PaymentStatusSynchronizer(new TestApplicationDbContext(context), thawaniClient);

        await synchronizer.SyncAsync(booking, CancellationToken.None);

        booking.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task SyncAsync_DoesNotCallThawani_ForPayOnArrivalBooking()
    {
        await using var context = TestDbContextFactory.Create();
        var customer = new Customer("+96891234567", null, null);
        context.Customers.Add(customer);
        await context.SaveChangesAsync(CancellationToken.None);
        var booking = new Booking("PDL-ABCDEF", customer.Id, PaymentMethod.PayOnArrival);
        booking.Confirm();

        var thawaniClient = Substitute.For<IThawaniClient>();
        var synchronizer = new PaymentStatusSynchronizer(new TestApplicationDbContext(context), thawaniClient);

        await synchronizer.SyncAsync(booking, CancellationToken.None);

        await thawaniClient.DidNotReceive().GetSessionStatusAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_DoesNotCallThawani_WhenBookingIsNoLongerPending()
    {
        await using var context = TestDbContextFactory.Create();
        var (booking, _) = await SeedPendingOnlineBookingAsync(context);
        booking.MarkPaid();
        booking.Confirm();

        var thawaniClient = Substitute.For<IThawaniClient>();
        var synchronizer = new PaymentStatusSynchronizer(new TestApplicationDbContext(context), thawaniClient);

        await synchronizer.SyncAsync(booking, CancellationToken.None);

        await thawaniClient.DidNotReceive().GetSessionStatusAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
