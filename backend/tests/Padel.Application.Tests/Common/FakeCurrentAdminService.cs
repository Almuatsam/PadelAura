using Padel.Application.Common.Interfaces;

namespace Padel.Application.Tests.Common;

public sealed class FakeCurrentAdminService(long adminId = 1) : ICurrentAdminService
{
    public long AdminId { get; } = adminId;
}
