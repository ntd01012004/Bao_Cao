namespace MiniDddCqrsJwt.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "mini-ddd-cqrs-jwt";

    public string Audience { get; init; } = "mini-ddd-cqrs-jwt-client";

    public string Secret { get; init; } = "change-this-secret-key-minimum-32-chars";

    public int ExpiresInMinutes { get; init; } = 60;
}
