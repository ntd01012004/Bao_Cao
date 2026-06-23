namespace MiniDddCqrsJwt.Application.Auth.Login;

public sealed record LoginResponse(string AccessToken, string TokenType, int ExpiresInSeconds);
