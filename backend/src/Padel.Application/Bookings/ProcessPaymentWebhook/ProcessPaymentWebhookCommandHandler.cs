using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Bookings.Services;
using Padel.Application.Common.Interfaces;

namespace Padel.Application.Bookings.ProcessPaymentWebhook;

/// <summary>
/// Thawani's webhook payload shape/signing isn't publicly documented, so this never trusts the
/// body for the actual status — it only uses session_id as a lookup key, then re-verifies via
/// PaymentStatusSynchronizer (which calls Thawani directly with our own secret key).
/// </summary>
public sealed class ProcessPaymentWebhookCommandHandler(
    IApplicationDbContext context,
    IPaymentStatusSynchronizer paymentStatusSynchronizer)
    : IRequestHandler<ProcessPaymentWebhookCommand>
{
    public async Task Handle(ProcessPaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            return;
        }

        var payment = await context.Payments
            .FirstOrDefaultAsync(p => p.SessionId == request.SessionId, cancellationToken);
        if (payment is null)
        {
            return;
        }

        var booking = await context.Bookings
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.Id == payment.BookingId, cancellationToken);
        if (booking is null)
        {
            return;
        }

        await paymentStatusSynchronizer.SyncAsync(booking, cancellationToken);
    }
}
