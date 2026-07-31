namespace Padel.Application.Auth.Login;

public sealed record LoginResult(string Token, DateTime ExpiresAt);
