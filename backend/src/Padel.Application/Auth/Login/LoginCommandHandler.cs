using MediatR;
using Microsoft.EntityFrameworkCore;
using Padel.Application.Common.Exceptions;
using Padel.Application.Common.Interfaces;

namespace Padel.Application.Auth.Login;

public sealed class LoginCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var admin = await context.Admins
            .FirstOrDefaultAsync(a => a.Email == request.Email, cancellationToken);

        if (admin is null || !passwordHasher.Verify(request.Password, admin.PasswordHash))
        {
            throw new AuthenticationFailedException();
        }

        var (token, expiresAt) = jwtTokenGenerator.Generate(admin);

        return new LoginResult(token, expiresAt);
    }
}
