namespace Padel.Application.Common;

/// <summary>
/// Oman (Asia/Muscat) has no DST, so a fixed UTC+4 offset is always correct and avoids
/// depending on tzdata being available wherever this runs.
/// </summary>
public static class OmanClock
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(4);

    public static DateTime Now() => DateTime.UtcNow + Offset;

    /// <summary>Converts a UTC instant to the Oman-local calendar date it falls on.</summary>
    public static DateOnly ToOmanDate(DateTime utc) => DateOnly.FromDateTime(utc + Offset);

    /// <summary>
    /// The UTC instant at which the given Oman-local calendar date begins — the inverse of
    /// <see cref="ToOmanDate"/>. Useful for building a UTC range to filter UTC-stamped columns
    /// (e.g. Booking.CreatedAt) by an Oman-local date window.
    /// </summary>
    public static DateTime StartOfOmanDateUtc(DateOnly date) => date.ToDateTime(TimeOnly.MinValue) - Offset;
}
