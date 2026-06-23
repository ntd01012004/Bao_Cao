using MiniDddCqrsJwt.Application.Abstractions.CQRS;

namespace MiniDddCqrsJwt.Application.Auth.Login;

public sealed record LoginCommand(string UserName, string Password) : ICommand<LoginResponse>;
