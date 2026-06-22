using MediatR;
using CleanArchitectureDemo.Application.Interfaces;
using CleanArchitectureDemo.Application.DTOs;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly IUserRepository _repo;

    public GetAllUsersHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var users = await _repo.GetAll();

        return users.Select(x => new UserDto
        {
            Id = x.Id,
            Name = x.Name
        }).ToList();
    }
}