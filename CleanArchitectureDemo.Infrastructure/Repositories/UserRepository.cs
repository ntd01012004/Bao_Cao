using CleanArchitectureDemo.Application.Interfaces;
using CleanArchitectureDemo.Domain.Entities;

namespace CleanArchitectureDemo.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private static List<User> _users = new();
    private static int _id = 1;

    public Task<List<User>> GetAll()
    {
        return Task.FromResult(_users);
    }

    public Task<User?> GetById(int id)
    {
        return Task.FromResult(_users.FirstOrDefault(x => x.Id == id));
    }

    public Task Add(User user)
    {
        user.Id = _id++;
        _users.Add(user);
        return Task.CompletedTask;
    }
}