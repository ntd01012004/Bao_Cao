using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MiniDddCqrsJwt.Application.Abstractions.Security;
using MiniDddCqrsJwt.Domain.Users;

namespace MiniDddCqrsJwt.Infrastructure.Authentication;

internal sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string Generate(AppUser user)
    {
        var now = DateTimeOffset.UtcNow;
        var expires = now.AddMinutes(_options.ExpiresInMinutes);

        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };

        var payload = new Dictionary<string, object>
        {
            ["sub"] = user.Id.ToString(),
            ["name"] = user.UserName,
            ["iss"] = _options.Issuer,
            ["aud"] = _options.Audience,
            ["iat"] = now.ToUnixTimeSeconds(),
            ["exp"] = expires.ToUnixTimeSeconds()
        };

        var headerPart = Base64Url.Encode(JsonSerializer.SerializeToUtf8Bytes(header));
        var payloadPart = Base64Url.Encode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signaturePart = CreateSignature($"{headerPart}.{payloadPart}");

        return $"{headerPart}.{payloadPart}.{signaturePart}";
    }

    private string CreateSignature(string unsignedToken)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.Secret));
        return Base64Url.Encode(hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken)));
    }
}
