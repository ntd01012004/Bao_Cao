using MiniDddCqrsJwt.Domain.Users;

namespace MiniDddCqrsJwt.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    string Generate(AppUser user);
}
