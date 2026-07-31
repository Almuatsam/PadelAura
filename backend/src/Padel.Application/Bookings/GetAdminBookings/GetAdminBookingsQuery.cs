using MediatR;
using Padel.Domain.Enums;

namespace Padel.Application.Bookings.GetAdminBookings;

public sealed record GetAdminBookingsQuery(
    long? CourtId,
    DateOnly? Date,
    BookingStatus? Status,
    PaymentMethod? PaymentMethod,
    string? Phone,
    int Page = 1) : IRequest<List<AdminBookingListItemDto>>;
