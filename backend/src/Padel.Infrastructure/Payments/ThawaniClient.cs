using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Padel.Application.Common.Interfaces;

namespace Padel.Infrastructure.Payments;

/// <summary>
/// Talks to Thawani's checkout API per docs/08-Payment-Integration.md. Amounts are in baisa
/// (1 OMR = 1000 baisa) — callers are responsible for that conversion.
/// </summary>
public sealed class ThawaniClient : IThawaniClient
{
    private readonly HttpClient _httpClient;
    private readonly ThawaniOptions _options;

    public ThawaniClient(HttpClient httpClient, IOptions<ThawaniOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException("Thawani:SecretKey is not configured.");
        }

        _httpClient.DefaultRequestHeaders.Add("thawani-api-key", _options.SecretKey);
    }

    public string BuildCheckoutUrl(string sessionId) =>
        $"{_options.CheckoutUrl}{sessionId}?key={_options.PublishableKey}";

    public async Task<string> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request, CancellationToken cancellationToken)
    {
        var body = new CreateSessionRequestBody
        {
            ClientReferenceId = request.ClientReferenceId,
            Mode = "payment",
            Products =
            [
                new ProductBody
                {
                    Name = request.ProductName,
                    Quantity = 1,
                    UnitAmount = request.UnitAmountBaisa,
                }
            ],
            SuccessUrl = $"{_options.FrontendBaseUrl}/booking/{request.ClientReferenceId}?status=success",
            CancelUrl = $"{_options.FrontendBaseUrl}/booking/{request.ClientReferenceId}?status=cancelled",
            Metadata = new MetadataBody
            {
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                OrderId = request.ClientReferenceId,
            },
        };

        using var response = await _httpClient.PostAsJsonAsync("checkout/session", body, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateSessionResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Thawani returned an empty create-session response.");

        return result.Data.SessionId;
    }

    public async Task<ThawaniSessionStatus> GetSessionStatusAsync(
        string sessionId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"checkout/session/{sessionId}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SessionStatusResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Thawani returned an empty session-status response.");

        var status = result.Data.PaymentStatus switch
        {
            "paid" => ThawaniPaymentStatus.Paid,
            "cancelled" => ThawaniPaymentStatus.Cancelled,
            _ => ThawaniPaymentStatus.Unpaid,
        };

        return new ThawaniSessionStatus(
            result.Data.SessionId, status, result.Data.ClientReferenceId, result.Data.TotalAmount);
    }

    private sealed class CreateSessionRequestBody
    {
        [JsonPropertyName("client_reference_id")]
        public required string ClientReferenceId { get; init; }

        [JsonPropertyName("mode")]
        public required string Mode { get; init; }

        [JsonPropertyName("products")]
        public required List<ProductBody> Products { get; init; }

        [JsonPropertyName("success_url")]
        public required string SuccessUrl { get; init; }

        [JsonPropertyName("cancel_url")]
        public required string CancelUrl { get; init; }

        [JsonPropertyName("metadata")]
        public MetadataBody? Metadata { get; init; }
    }

    private sealed class ProductBody
    {
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("quantity")]
        public required int Quantity { get; init; }

        [JsonPropertyName("unit_amount")]
        public required int UnitAmount { get; init; }
    }

    private sealed class MetadataBody
    {
        [JsonPropertyName("customer_name")]
        public string? CustomerName { get; init; }

        [JsonPropertyName("customer_phone")]
        public string? CustomerPhone { get; init; }

        [JsonPropertyName("order_id")]
        public string? OrderId { get; init; }
    }

    private sealed class CreateSessionResponse
    {
        [JsonPropertyName("data")]
        public required CreateSessionResponseData Data { get; init; }
    }

    private sealed class CreateSessionResponseData
    {
        [JsonPropertyName("session_id")]
        public required string SessionId { get; init; }
    }

    private sealed class SessionStatusResponse
    {
        [JsonPropertyName("data")]
        public required SessionStatusResponseData Data { get; init; }
    }

    private sealed class SessionStatusResponseData
    {
        [JsonPropertyName("session_id")]
        public required string SessionId { get; init; }

        [JsonPropertyName("payment_status")]
        public required string PaymentStatus { get; init; }

        [JsonPropertyName("client_reference_id")]
        public string? ClientReferenceId { get; init; }

        [JsonPropertyName("total_amount")]
        public int TotalAmount { get; init; }
    }
}
