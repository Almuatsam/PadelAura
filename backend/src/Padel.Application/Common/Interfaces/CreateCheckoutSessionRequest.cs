namespace Padel.Application.Common.Interfaces;

public sealed record CreateCheckoutSessionRequest(
    string ClientReferenceId,
    string ProductName,
    int UnitAmountBaisa,
    string? CustomerName,
    string? CustomerPhone);
