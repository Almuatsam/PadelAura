namespace Padel.Application.Common.Interfaces;

public sealed record ThawaniSessionStatus(
    string SessionId,
    ThawaniPaymentStatus PaymentStatus,
    string? ClientReferenceId,
    int TotalAmountBaisa);
