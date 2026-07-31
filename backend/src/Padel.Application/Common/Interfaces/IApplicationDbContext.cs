using Microsoft.EntityFrameworkCore;
using Padel.Domain.Entities;

namespace Padel.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Admin> Admins { get; }
    DbSet<Court> Courts { get; }
    DbSet<CourtSchedule> CourtSchedules { get; }
    DbSet<CourtClosure> CourtClosures { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
