using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Padel.Application.Common.Interfaces;
using Padel.Infrastructure.Identity;
using Padel.Infrastructure.Payments;
using Padel.Infrastructure.Persistence;

namespace Padel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<PadelDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<PadelDbContext>());

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.Configure<ThawaniOptions>(configuration.GetSection(ThawaniOptions.SectionName));
        services.AddHttpClient<IThawaniClient, ThawaniClient>((sp, client) =>
        {
            var baseUrl = configuration[$"{ThawaniOptions.SectionName}:BaseUrl"]
                ?? throw new InvalidOperationException("Thawani:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }
}
