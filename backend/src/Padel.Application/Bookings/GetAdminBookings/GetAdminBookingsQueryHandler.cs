using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Common.Interfaces;

namespace Padel.Application.Bookings.GetAdminBookings;

public sealed class GetAdminBookingsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAdminBookingsQuery, List<AdminBookingListItemDto>>
{
    public async Task<List<AdminBookingListItemDto>> Handle(GetAdminBookingsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Items)
            .ThenInclude(i => i.Court)
            .AsQueryable();

        if (request.Status is not null)
        {
            query = query.Where(b => b.Status == request.Status);
        }

        if (request.PaymentMethod is not null)
        {
            query = query.Where(b => b.PaymentMethod == request.PaymentMethod);
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            query = query.Where(b => b.Customer!.Phone.Contains(request.Phone));
        }

        if (request.CourtId is not null)
        {
            query = query.Where(b => b.Items.Any(i => i.CourtId == request.CourtId));
        }

        if (request.Date is not null)
        {
            query = query.Where(b => b.Items.Any(i => i.BookingDate == request.Date));
        }

        var page = request.Page < 1 ? 1 : request.Page;

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * BookingPolicy.AdminBookingsPageSize)
            .Take(BookingPolicy.AdminBookingsPageSize)
            .ToListAsync(cancellationToken);

        return bookings
            .Select(b => new AdminBookingListItemDto(
                b.Id,
                b.BookingReference,
                b.Customer!.Phone,
                b.Customer.FullName,
                b.Status.ToString(),
                b.PaymentMethod.ToString(),
                b.PaymentStatus.ToString(),
                b.Total,
                b.CreatedAt,
                b.Items
                    .Select(i => new AdminBookingItemDto(i.Court!.Name, i.BookingDate, i.StartTime, i.EndTime, i.Price))
                    .ToList()))
            .ToList();
    }
}
