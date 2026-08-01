using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Padel.Application.Common.Interfaces;

namespace Padel.Api.Common;

public sealed class CurrentAdminService(IHttpContextAccessor httpContextAccessor) : ICurrentAdminService
{
    public long AdminId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;

            // JwtSecurityTokenHandler maps the "sub" claim to ClaimTypes.NameIdentifier inbound by
            // default; check both so this doesn't silently break if that mapping is ever disabled.
            var value = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (!long.TryParse(value, out var adminId))
            {
                throw new InvalidOperationException(
                    "No authenticated admin identity is available on the current request.");
            }

            return adminId;
        }
    }
}
