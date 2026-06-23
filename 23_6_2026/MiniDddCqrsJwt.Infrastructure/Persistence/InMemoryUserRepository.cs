using MiniDddCqrsJwt.Application.Abstractions.Data;
using MiniDddCqrsJwt.Domain.Users;

namespace MiniDddCqrsJwt.Infrastructure.Persistence;

internal sealed class InMemoryUserRepository : IUserRepository
{
    private static readonly IReadOnlyList<AppUser> Users =
    [
        new AppUser(Guid.Parse("b7b7df28-0cf5-4b2c-b22c-8c2d515af9a4"), "admin", "Admin@123")
    ];

    public Task<AppUser?> GetByUserName(string userName, CancellationToken cancellationToken)
    {
        var user = Users.FirstOrDefault(candidate =>
            string.Equals(candidate.UserName, userName, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(user);
    }
}
