using MiniDddCqrsJwt.Domain.Common;

namespace MiniDddCqrsJwt.Domain.Users;

public sealed class AppUser : Entity<Guid>
{
    public AppUser(Guid id, string userName, string password)
        : base(id)
    {
        UserName = userName;
        Password = password;
    }

    public string UserName { get; }

    public string Password { get; }
}
