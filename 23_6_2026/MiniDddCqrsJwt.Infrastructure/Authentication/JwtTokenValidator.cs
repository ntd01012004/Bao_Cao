using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace MiniDddCqrsJwt.Infrastructure.Authentication;

public sealed class JwtTokenValidator
{
    private readonly JwtOptions _options;

    public JwtTokenValidator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public ClaimsPrincipal? Validate(string token)
    {
        var parts = token.Split('.');

        if (parts.Length != 3)
        {
            return null;
        }

        if (!HasValidSignature(parts[0], parts[1], parts[2]))
        {
            return null;
        }

        using var payload = JsonDocument.Parse(Base64Url.Decode(parts[1]));
        var root = payload.RootElement;

        if (!root.TryGetProperty("exp", out var exp) ||
            DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()) <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        if (!HasValue(root, "iss", _options.Issuer) || !HasValue(root, "aud", _options.Audience))
        {
            return null;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, root.GetProperty("sub").GetString() ?? string.Empty),
            new(ClaimTypes.Name, root.GetProperty("name").GetString() ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claims, "Bearer");
        return new ClaimsPrincipal(identity);
    }

    private bool HasValidSignature(string header, string payload, string signature)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.Secret));
        var expected = Base64Url.Encode(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{header}.{payload}")));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature));
    }

    private static bool HasValue(JsonElement root, string propertyName, string expectedValue)
    {
        return root.TryGetProperty(propertyName, out var property) &&
            string.Equals(property.GetString(), expectedValue, StringComparison.Ordinal);
    }
}
