using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Padel.Api.Middleware;
using Padel.Application;
using Padel.Infrastructure;
using Padel.Infrastructure.Persistence;
using Padel.Infrastructure.Persistence.Seed;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console());

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Secret"] ?? string.Empty)),
        };
    });
builder.Services.AddAuthorization();

// Rate limiting on sensitive/public endpoints — required by docs/02-TDD.md's "Rate Limiting on
// login and booking to prevent brute-force/spam booking" and extended here to the other
// unauthenticated customer-facing endpoints for the same reason. Partitioned by remote IP so one
// caller can't exhaust another's quota.
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }

        var problemDetails = new ProblemDetails
        {
            Title = "Too many requests.",
            Status = StatusCodes.Status429TooManyRequests,
        };
        await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(problemDetails), cancellationToken);
    };

    static string PartitionKey(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    options.AddPolicy("login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        PartitionKey(httpContext),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(5) }));

    options.AddPolicy("booking", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        PartitionKey(httpContext),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 8, Window = TimeSpan.FromMinutes(1) }));

    options.AddPolicy("lookup", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        PartitionKey(httpContext),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1) }));

    options.AddPolicy("availability", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        PartitionKey(httpContext),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1) }));

    options.AddPolicy("webhook", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        PartitionKey(httpContext),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1) }));
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PadelDbContext>();
    await db.Database.MigrateAsync();

    // Only creates the well-known admin@padel.local seed login when explicitly enabled — see
    // DbSeeder.SeedAsync's doc comment. Set in appsettings.Development.json; absent (false) in
    // appsettings.json so production doesn't get it without an explicit opt-in.
    var createDefaultAdmin = builder.Configuration.GetValue("Seed:CreateDefaultAdmin", false);
    await DbSeeder.SeedAsync(db, createDefaultAdmin);
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();
