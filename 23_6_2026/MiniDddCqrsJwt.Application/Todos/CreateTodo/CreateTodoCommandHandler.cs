using MiniDddCqrsJwt.Application.Abstractions.CQRS;
using MiniDddCqrsJwt.Application.Abstractions.Data;
using MiniDddCqrsJwt.Domain.Todos;

namespace MiniDddCqrsJwt.Application.Todos.CreateTodo;

internal sealed class CreateTodoCommandHandler : ICommandHandler<CreateTodoCommand, TodoResponse>
{
    private readonly ITodoRepository _todos;

    public CreateTodoCommandHandler(ITodoRepository todos)
    {
        _todos = todos;
    }

    public async Task<TodoResponse> Handle(CreateTodoCommand command, CancellationToken cancellationToken)
    {
        var todo = TodoItem.Create(TodoTitle.Create(command.Title), DateTime.UtcNow);

        await _todos.Add(todo, cancellationToken);
        await _todos.SaveChanges(cancellationToken);

        return todo.ToResponse();
    }
}
