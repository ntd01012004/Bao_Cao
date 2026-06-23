using MiniDddCqrsJwt.Application.Abstractions.CQRS;

namespace MiniDddCqrsJwt.Application.Todos.CreateTodo;

public sealed record CreateTodoCommand(string Title) : ICommand<TodoResponse>;
