using MediatR;

namespace Padel.Application.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;
