using MiniDddCqrsJwt.Application.Abstractions.CQRS;
using MiniDddCqrsJwt.Application.Abstractions.Data;

namespace MiniDddCqrsJwt.Application.Todos.GetTodos;

internal sealed class GetTodosQueryHandler : IQueryHandler<GetTodosQuery, IReadOnlyList<TodoResponse>>
{
    private readonly ITodoRepository _todos;

    public GetTodosQueryHandler(ITodoRepository todos)
    {
        _todos = todos;
    }

    public async Task<IReadOnlyList<TodoResponse>> Handle(GetTodosQuery query, CancellationToken cancellationToken)
    {
        var todos = await _todos.GetAll(cancellationToken);
        return todos.Select(todo => todo.ToResponse()).ToList();
    }
}
