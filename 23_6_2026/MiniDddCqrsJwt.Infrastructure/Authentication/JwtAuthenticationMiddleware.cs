using Microsoft.AspNetCore.Http;

namespace MiniDddCqrsJwt.Infrastructure.Authentication;

public sealed class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public JwtAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, JwtTokenValidator validator)
    {
        var authorization = context.Request.Headers.Authorization.ToString();

        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authorization["Bearer ".Length..].Trim();
            var user = validator.Validate(token);

            if (user is not null)
            {
                context.User = user;
            }
        }

        await _next(context);
    }
}
