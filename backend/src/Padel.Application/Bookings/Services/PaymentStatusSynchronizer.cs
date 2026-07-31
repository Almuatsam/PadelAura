using Microsoft.EntityFrameworkCore;
using Padel.Application.Common.Interfaces;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Bookings.Services;

public sealed class PaymentStatusSynchronizer(IApplicationDbContext context, IThawaniClient thawaniClient)
    : IPaymentStatusSynchronizer
{
    public async Task SyncAsync(Booking booking, CancellationToken cancellationToken)
    {
        if (booking.PaymentMethod != PaymentMethod.Online || booking.Status != BookingStatus.Pending)
        {
            return;
        }

        var payment = await context.Payments
            .Where(p => p.BookingId == booking.Id)
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment?.SessionId is null)
        {
            return;
        }

        var session = await thawaniClient.GetSessionStatusAsync(payment.SessionId, cancellationToken);

        switch (session.PaymentStatus)
        {
            case ThawaniPaymentStatus.Paid:
                booking.MarkPaid();
                booking.Confirm();
                payment.MarkSuccess(payment.SessionId);
                await context.SaveChangesAsync(cancellationToken);
                break;

            case ThawaniPaymentStatus.Cancelled:
                booking.Cancel();
                payment.MarkFailed(payment.SessionId);
                await context.SaveChangesAsync(cancellationToken);
                break;

            case ThawaniPaymentStatus.Unpaid:
            default:
                break; // still awaiting payment — nothing to update yet.
        }
    }
}
