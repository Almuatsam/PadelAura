using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public const string DefaultAdminEmail = "admin@padel.local";
    public const string DefaultAdminPassword = "Padel@12345";

    /// <summary>
    /// <paramref name="createDefaultAdmin"/> gates only the seeded admin account — courts and
    /// promotions always seed regardless, since they're sample data, not a credential. Defaults to
    /// `true` in Development (see appsettings.Development.json) and is absent (so effectively
    /// `false`) in production config, so a real deployment doesn't silently get a well-known
    /// admin@padel.local / Padel@12345 login unless a deployer explicitly opts in.
    /// </summary>
    public static async Task SeedAsync(PadelDbContext db, bool createDefaultAdmin = true, CancellationToken cancellationToken = default)
    {
        if (createDefaultAdmin && !db.Admins.Any())
        {
            var admin = new Admin(
                fullName: "Padel Admin",
                email: DefaultAdminEmail,
                passwordHash: BCrypt.Net.BCrypt.HashPassword(DefaultAdminPassword),
                role: AdminRole.SuperAdmin);

            db.Admins.Add(admin);
        }

        if (!db.Courts.Any())
        {
            var weekdaySchedule = Enumerable.Range(0, 7)
                .Select(day => new { Day = day, Open = new TimeOnly(8, 0), Close = new TimeOnly(23, 0) })
                .ToList();

            var courts = new[]
            {
                new Court("Court A", hourPrice: 15.000m),
                new Court("Court B", hourPrice: 15.000m),
                new Court("Court C", hourPrice: 12.000m),
            };

            db.Courts.AddRange(courts);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var court in courts)
            {
                court.ReplaceSchedules(weekdaySchedule.Select(s =>
                    new CourtSchedule(court.Id, s.Day, s.Open, s.Close)));
            }
        }

        if (!db.Promotions.Any())
        {
            var promotion = new Promotion(
                name: "عرض الساعات المتعددة",
                isActive: true,
                startDate: null,
                endDate: null);

            db.Promotions.Add(promotion);
            await db.SaveChangesAsync(cancellationToken);

            promotion.ReplaceRules(
            [
                new PricingRule(promotion.Id, minimumHours: 1, DiscountType.FixedRate, discountValue: 10.000m),
                new PricingRule(promotion.Id, minimumHours: 2, DiscountType.FixedRate, discountValue: 8.000m),
            ]);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
