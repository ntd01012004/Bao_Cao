namespace MiniDddCqrsJwt.Application.Abstractions.CQRS;

public interface ICqrsDispatcher
{
    Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);

    Task<TResponse> Query<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
