using Microsoft.Extensions.DependencyInjection;

namespace MiniDddCqrsJwt.Application.Abstractions.CQRS;

internal sealed class CqrsDispatcher : ICqrsDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CqrsDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        return handler.Handle((dynamic)command, cancellationToken);
    }

    public Task<TResponse> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        return handler.Handle((dynamic)query, cancellationToken);
    }
}
