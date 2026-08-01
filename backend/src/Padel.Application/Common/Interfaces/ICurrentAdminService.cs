namespace Padel.Application.Common.Interfaces;

/// <summary>
/// The authenticated admin making the current request, resolved from the JWT — kept out of
/// Application (which has no HttpContext concept) and implemented in Padel.Api. Only used by
/// [Authorize]-protected admin handlers, where a valid admin identity is guaranteed to exist by
/// the time the handler runs, so this throws rather than returning null on that invariant being
/// violated (mirrors JwtTokenGenerator/ThawaniClient's fail-fast style elsewhere in this codebase).
/// </summary>
public interface ICurrentAdminService
{
    long AdminId { get; }
}
