using Padel.Domain.Common;
using Padel.Domain.Enums;

namespace Padel.Domain.Entities;

public class Payment : Entity
{
    public long BookingId { get; private set; }
    public Booking? Booking { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string? SessionId { get; private set; }
    public PaymentTransactionStatus Status { get; private set; }
    public string? TransactionReference { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Payment() { }

    public Payment(long bookingId, string provider, decimal amount, string? sessionId = null)
    {
        BookingId = bookingId;
        Provider = provider;
        Amount = amount;
        SessionId = sessionId;
        Status = PaymentTransactionStatus.Initiated;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkSuccess(string transactionReference)
    {
        Status = PaymentTransactionStatus.Success;
        TransactionReference = transactionReference;
    }

    public void MarkFailed(string? transactionReference = null)
    {
        Status = PaymentTransactionStatus.Failed;
        TransactionReference = transactionReference ?? TransactionReference;
    }
}
