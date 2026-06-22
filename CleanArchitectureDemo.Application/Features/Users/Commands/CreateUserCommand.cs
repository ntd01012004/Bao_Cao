using MediatR;

public record CreateUserCommand(string Name, string Email) : IRequest<int>;
