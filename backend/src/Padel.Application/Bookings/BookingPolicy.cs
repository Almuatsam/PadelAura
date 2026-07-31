namespace Padel.Application.Bookings;

public static class BookingPolicy
{
    /// <summary>
    /// How long a Pending Online booking (abandoned/incomplete Thawani checkout) still counts as
    /// occupying its slot. After this, it no longer blocks availability or new bookings for that
    /// slot — without this, an abandoned checkout would squat on a court forever, since nothing
    /// else ever flips a Pending booking away from occupying its slot.
    /// </summary>
    public const int PendingPaymentGraceMinutes = 15;
}
