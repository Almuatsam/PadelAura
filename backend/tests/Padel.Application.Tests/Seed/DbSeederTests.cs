using FluentAssertions;
using Padel.Application.Tests.Common;
using Padel.Infrastructure.Persistence.Seed;

namespace Padel.Application.Tests.Seed;

public sealed class DbSeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesDefaultAdmin_WhenCreateDefaultAdminIsTrue()
    {
        await using var context = TestDbContextFactory.Create();

        await DbSeeder.SeedAsync(context, createDefaultAdmin: true);

        context.Admins.Should().ContainSingle(a => a.Email == DbSeeder.DefaultAdminEmail);
        context.Courts.Should().NotBeEmpty();
        context.Promotions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SeedAsync_SkipsDefaultAdmin_ButStillSeedsCourtsAndPromotions_WhenCreateDefaultAdminIsFalse()
    {
        await using var context = TestDbContextFactory.Create();

        await DbSeeder.SeedAsync(context, createDefaultAdmin: false);

        context.Admins.Should().BeEmpty();
        context.Courts.Should().NotBeEmpty();
        context.Promotions.Should().NotBeEmpty();
    }
}
