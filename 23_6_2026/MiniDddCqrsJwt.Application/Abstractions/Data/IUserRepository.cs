using MiniDddCqrsJwt.Domain.Users;

namespace MiniDddCqrsJwt.Application.Abstractions.Data;

public interface IUserRepository
{
    Task<AppUser?> GetByUserName(string userName, CancellationToken cancellationToken);
}
