using Padel.Domain.Entities;

namespace Padel.Application.Bookings.Services;

/// <summary>
/// Confirms an Online booking's real payment status against Thawani. Thawani's UAT sandbox
/// doesn't reliably deliver webhooks (docs/08-Payment-Integration.md §5), so this is called both
/// from a webhook handler and lazily whenever a booking's status is read.
/// </summary>
public interface IPaymentStatusSynchronizer
{
    Task SyncAsync(Booking booking, CancellationToken cancellationToken);
}
