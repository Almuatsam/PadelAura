using Padel.Domain.Entities;

namespace Padel.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) Generate(Admin admin);
}
