using Padel.Domain.Common;

namespace Padel.Domain.Entities;

public class BookingItem : Entity
{
    public long BookingId { get; private set; }
    public Booking? Booking { get; private set; }
    public long CourtId { get; private set; }
    public Court? Court { get; private set; }
    public DateOnly BookingDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public decimal Price { get; private set; }

    /// <summary>
    /// Set when the parent booking is cancelled. Lets the DB-level uniqueness guard
    /// (court+date+start-time) exclude cancelled slots so they become bookable again.
    /// </summary>
    public DateTime? CancelledAt { get; private set; }

    private BookingItem() { }

    internal BookingItem(long bookingId, long courtId, DateOnly bookingDate, TimeOnly startTime, TimeOnly endTime, decimal price)
    {
        BookingId = bookingId;
        CourtId = courtId;
        BookingDate = bookingDate;
        StartTime = startTime;
        EndTime = endTime;
        Price = price;
    }

    internal void MarkCancelled()
    {
        CancelledAt = DateTime.UtcNow;
    }
}
