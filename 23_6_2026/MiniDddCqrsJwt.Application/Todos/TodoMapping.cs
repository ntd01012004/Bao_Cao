using MiniDddCqrsJwt.Domain.Todos;

namespace MiniDddCqrsJwt.Application.Todos;

internal static class TodoMapping
{
    public static TodoResponse ToResponse(this TodoItem todo)
    {
        return new TodoResponse(
            todo.Id,
            todo.Title.Value,
            todo.IsCompleted,
            todo.CreatedAtUtc,
            todo.CompletedAtUtc);
    }
}
