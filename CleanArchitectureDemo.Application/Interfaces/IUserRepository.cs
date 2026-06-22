using CleanArchitectureDemo.Domain.Entities;

namespace CleanArchitectureDemo.Application.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAll();
    Task<User?> GetById(int id);
    Task Add(User user);
}
