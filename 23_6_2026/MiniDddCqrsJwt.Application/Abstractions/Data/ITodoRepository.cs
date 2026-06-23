using MiniDddCqrsJwt.Domain.Todos;

namespace MiniDddCqrsJwt.Application.Abstractions.Data;

public interface ITodoRepository
{
    Task Add(TodoItem todo, CancellationToken cancellationToken);

    Task<TodoItem?> GetById(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TodoItem>> GetAll(CancellationToken cancellationToken);

    Task SaveChanges(CancellationToken cancellationToken);
}
