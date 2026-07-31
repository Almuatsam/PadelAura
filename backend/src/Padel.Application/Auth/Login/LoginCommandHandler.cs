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
    // A fixed, never-matching BCrypt hash verified against when no admin exists for the given
    // email, so an unknown-email login takes roughly the same time as a wrong-password one instead
    // of short-circuiting — closes a timing side-channel that could otherwise be used to enumerate
    // valid admin emails even though the error message itself is identical either way.
    private const string DummyPasswordHash = "$2a$11$CwTycUXWue0Thq9StjUM0uJ8kNbNMoLoJTNqhw.Z6DiHbeoBd5J.i";

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var admin = await context.Admins
            .FirstOrDefaultAsync(a => a.Email == request.Email, cancellationToken);

        var passwordIsValid = passwordHasher.Verify(request.Password, admin?.PasswordHash ?? DummyPasswordHash);

        if (admin is null || !passwordIsValid)
        {
            throw new AuthenticationFailedException();
        }

        var (token, expiresAt) = jwtTokenGenerator.Generate(admin);

        return new LoginResult(token, expiresAt);
    }
}
