namespace MiniDddCqrsJwt.Infrastructure.Authentication;

internal static class Base64Url
{
    public static string Encode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static byte[] Decode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        var padding = (4 - base64.Length % 4) % 4;
        base64 = base64.PadRight(base64.Length + padding, '=');
        return Convert.FromBase64String(base64);
    }
}
