using MiniDddCqrsJwt.Application.Abstractions.CQRS;

namespace MiniDddCqrsJwt.Application.Todos.CompleteTodo;

public sealed record CompleteTodoCommand(Guid Id) : ICommand<TodoResponse>;
