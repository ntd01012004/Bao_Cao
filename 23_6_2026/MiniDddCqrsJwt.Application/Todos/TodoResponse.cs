namespace MiniDddCqrsJwt.Application.Todos;

public sealed record TodoResponse(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc);
