using MediatR;
using CleanArchitectureDemo.Application.Interfaces;
using CleanArchitectureDemo.Domain.Entities;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly IUserRepository _repo;

    public CreateUserHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email
        };

        await _repo.Add(user);
        return user.Id;
    }
}
