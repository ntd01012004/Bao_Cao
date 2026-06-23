using MiniDddCqrsJwt.Application.Abstractions.CQRS;
using MiniDddCqrsJwt.Application.Abstractions.Data;

namespace MiniDddCqrsJwt.Application.Todos.CompleteTodo;

internal sealed class CompleteTodoCommandHandler : ICommandHandler<CompleteTodoCommand, TodoResponse>
{
    private readonly ITodoRepository _todos;

    public CompleteTodoCommandHandler(ITodoRepository todos)
    {
        _todos = todos;
    }

    public async Task<TodoResponse> Handle(CompleteTodoCommand command, CancellationToken cancellationToken)
    {
        var todo = await _todos.GetById(command.Id, cancellationToken);

        if (todo is null)
        {
            throw new KeyNotFoundException("Todo was not found.");
        }

        todo.Complete(DateTime.UtcNow);
        await _todos.SaveChanges(cancellationToken);

        return todo.ToResponse();
    }
}
