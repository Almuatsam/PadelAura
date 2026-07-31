namespace Padel.Application.Common.Interfaces;

public interface IThawaniClient
{
    Task<string> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request, CancellationToken cancellationToken);

    Task<ThawaniSessionStatus> GetSessionStatusAsync(string sessionId, CancellationToken cancellationToken);

    /// <summary>Builds the customer-facing payment link for a created session.</summary>
    string BuildCheckoutUrl(string sessionId);
}
