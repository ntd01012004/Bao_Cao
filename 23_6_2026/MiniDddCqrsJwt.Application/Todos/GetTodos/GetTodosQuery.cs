using MiniDddCqrsJwt.Application.Abstractions.CQRS;

namespace MiniDddCqrsJwt.Application.Todos.GetTodos;

public sealed record GetTodosQuery : IQuery<IReadOnlyList<TodoResponse>>;
