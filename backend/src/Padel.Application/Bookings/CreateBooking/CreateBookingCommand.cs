using MediatR;
using Padel.Domain.Enums;

namespace Padel.Application.Bookings.CreateBooking;

public sealed record CreateBookingCommand(
    string Phone,
    string? FullName,
    string? Email,
    PaymentMethod PaymentMethod,
    IReadOnlyList<BookingSlotInput> Slots) : IRequest<CreateBookingResult>;
