using MediatR;

namespace Padel.Application.Bookings.GetAvailability;

public sealed record GetAvailabilityQuery(DateOnly Date) : IRequest<List<AvailabilitySlotDto>>;
