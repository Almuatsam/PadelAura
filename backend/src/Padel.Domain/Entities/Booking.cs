using Padel.Domain.Common;
using Padel.Domain.Enums;

namespace Padel.Domain.Entities;

public class Booking : Entity
{
    public string BookingReference { get; private set; } = string.Empty;
    public long CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public BookingStatus Status { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Total { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<BookingItem> _items = [];
    public IReadOnlyCollection<BookingItem> Items => _items.AsReadOnly();

    private Booking() { }

    public Booking(string bookingReference, long customerId, PaymentMethod paymentMethod)
    {
        BookingReference = bookingReference;
        CustomerId = customerId;
        PaymentMethod = paymentMethod;
        Status = BookingStatus.Pending;
        PaymentStatus = PaymentStatus.Unpaid;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(long courtId, DateOnly bookingDate, TimeOnly startTime, TimeOnly endTime, decimal price)
    {
        _items.Add(new BookingItem(Id, courtId, bookingDate, startTime, endTime, price));
    }

    public void ApplyPricing(decimal subtotal, decimal discount)
    {
        Subtotal = subtotal;
        Discount = discount;
        Total = subtotal;
    }

    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
    }

    public void Cancel()
    {
        Status = BookingStatus.Cancelled;
        foreach (var item in _items)
        {
            item.MarkCancelled();
        }
    }

    public void Complete()
    {
        Status = BookingStatus.Completed;
    }

    public void MarkPaid()
    {
        PaymentStatus = PaymentStatus.Paid;
    }

    public void MarkPaymentFailed()
    {
        PaymentStatus = PaymentStatus.Failed;
    }

    public void MarkRefunded()
    {
        PaymentStatus = PaymentStatus.Refunded;
    }
}
