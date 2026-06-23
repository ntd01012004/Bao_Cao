using MiniDddCqrsJwt.Application.Abstractions.CQRS;
using MiniDddCqrsJwt.Application.Abstractions.Data;
using MiniDddCqrsJwt.Application.Abstractions.Security;

namespace MiniDddCqrsJwt.Application.Auth.Login;

internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _users;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(IUserRepository users, IJwtTokenGenerator jwtTokenGenerator)
    {
        _users = users;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _users.GetByUserName(command.UserName, cancellationToken);

        if (user is null || user.Password != command.Password)
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var token = _jwtTokenGenerator.Generate(user);
        return new LoginResponse(token, "Bearer", 3600);
    }
}
