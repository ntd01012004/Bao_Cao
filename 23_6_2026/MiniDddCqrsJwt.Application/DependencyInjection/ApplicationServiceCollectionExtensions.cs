using Microsoft.Extensions.DependencyInjection;
using MiniDddCqrsJwt.Application.Abstractions.CQRS;
using MiniDddCqrsJwt.Application.Auth.Login;
using MiniDddCqrsJwt.Application.Todos;
using MiniDddCqrsJwt.Application.Todos.CompleteTodo;
using MiniDddCqrsJwt.Application.Todos.CreateTodo;
using MiniDddCqrsJwt.Application.Todos.GetTodos;

namespace MiniDddCqrsJwt.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICqrsDispatcher, CqrsDispatcher>();

        services.AddScoped<ICommandHandler<LoginCommand, LoginResponse>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<CreateTodoCommand, TodoResponse>, CreateTodoCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteTodoCommand, TodoResponse>, CompleteTodoCommandHandler>();
        services.AddScoped<IQueryHandler<GetTodosQuery, IReadOnlyList<TodoResponse>>, GetTodosQueryHandler>();

        return services;
    }
}
