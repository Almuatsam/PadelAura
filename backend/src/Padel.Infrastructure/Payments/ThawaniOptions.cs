namespace Padel.Infrastructure.Payments;

public sealed class ThawaniOptions
{
    public const string SectionName = "Thawani";

    public string SecretKey { get; init; } = string.Empty;
    public string PublishableKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public string CheckoutUrl { get; init; } = string.Empty;

    /// <summary>
    /// Where the customer lands after Thawani checkout. Phase 6 hasn't built this page yet, but
    /// Thawani doesn't need success_url/cancel_url to resolve for session creation to succeed.
    /// </summary>
    public string FrontendBaseUrl { get; init; } = string.Empty;
}
