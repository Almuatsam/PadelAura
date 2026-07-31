using FluentAssertions;
using NSubstitute;
using Padel.Application.Auth.Login;
using Padel.Application.Common.Exceptions;
using Padel.Application.Common.Interfaces;
using Padel.Application.Tests.Common;
using Padel.Domain.Entities;
using Padel.Domain.Enums;

namespace Padel.Application.Tests.Auth;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsToken_WhenCredentialsAreValid()
    {
        await using var context = TestDbContextFactory.Create();
        var admin = new Admin("Padel Admin", "admin@padel.local", "hashed-password", AdminRole.SuperAdmin);
        context.Admins.Add(admin);
        await context.SaveChangesAsync(CancellationToken.None);

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify("Padel@12345", "hashed-password").Returns(true);

        var jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        var expiresAt = DateTime.UtcNow.AddHours(1);
        jwtTokenGenerator.Generate(admin).Returns(("a-token", expiresAt));

        var handler = new LoginCommandHandler(context, passwordHasher, jwtTokenGenerator);

        var result = await handler.Handle(
            new LoginCommand("admin@padel.local", "Padel@12345"), CancellationToken.None);

        result.Token.Should().Be("a-token");
        result.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public async Task Handle_ThrowsAuthenticationFailedException_WhenEmailIsUnknown()
    {
        await using var context = TestDbContextFactory.Create();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        var handler = new LoginCommandHandler(context, passwordHasher, jwtTokenGenerator);

        var act = () => handler.Handle(
            new LoginCommand("unknown@padel.local", "whatever"), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }

    [Fact]
    public async Task Handle_ThrowsAuthenticationFailedException_WhenPasswordIsWrong()
    {
        await using var context = TestDbContextFactory.Create();
        var admin = new Admin("Padel Admin", "admin@padel.local", "hashed-password", AdminRole.SuperAdmin);
        context.Admins.Add(admin);
        await context.SaveChangesAsync(CancellationToken.None);

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify("wrong-password", "hashed-password").Returns(false);

        var jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        var handler = new LoginCommandHandler(context, passwordHasher, jwtTokenGenerator);

        var act = () => handler.Handle(
            new LoginCommand("admin@padel.local", "wrong-password"), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
    }
}
