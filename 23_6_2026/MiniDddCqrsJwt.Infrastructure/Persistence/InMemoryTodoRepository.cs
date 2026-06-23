using System.Collections.Concurrent;
using MiniDddCqrsJwt.Application.Abstractions.Data;
using MiniDddCqrsJwt.Domain.Todos;

namespace MiniDddCqrsJwt.Infrastructure.Persistence;

internal sealed class InMemoryTodoRepository : ITodoRepository
{
    private readonly ConcurrentDictionary<Guid, TodoItem> _todos = new();

    public Task Add(TodoItem todo, CancellationToken cancellationToken)
    {
        _todos[todo.Id] = todo;
        return Task.CompletedTask;
    }

    public Task<TodoItem?> GetById(Guid id, CancellationToken cancellationToken)
    {
        _todos.TryGetValue(id, out var todo);
        return Task.FromResult(todo);
    }

    public Task<IReadOnlyList<TodoItem>> GetAll(CancellationToken cancellationToken)
    {
        IReadOnlyList<TodoItem> todos = _todos.Values
            .OrderByDescending(todo => todo.CreatedAtUtc)
            .ToList();

        return Task.FromResult(todos);
    }

    public Task SaveChanges(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
