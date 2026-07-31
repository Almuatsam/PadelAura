namespace Padel.Application.Common;

/// <summary>
/// Oman (Asia/Muscat) has no DST, so a fixed UTC+4 offset is always correct and avoids
/// depending on tzdata being available wherever this runs.
/// </summary>
public static class OmanClock
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(4);

    public static DateTime Now() => DateTime.UtcNow + Offset;
}
