namespace Padel.Application.Bookings.GetBookingQuote;

/// <summary>
/// Subtotal is the undiscounted sum of each slot's unit price (what the customer sees per-slot in
/// the cart); Total is what will actually be charged after any applicable promotion.
/// </summary>
public sealed record BookingQuoteDto(decimal Subtotal, decimal Discount, decimal Total);
