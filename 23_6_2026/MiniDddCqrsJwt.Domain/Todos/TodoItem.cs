using MiniDddCqrsJwt.Domain.Common;

namespace MiniDddCqrsJwt.Domain.Todos;

public sealed class TodoItem : Entity<Guid>
{
    private TodoItem(Guid id, TodoTitle title, DateTime createdAtUtc)
        : base(id)
    {
        Title = title;
        CreatedAtUtc = createdAtUtc;
    }

    public TodoTitle Title { get; private set; }

    public bool IsCompleted { get; private set; }

    public DateTime CreatedAtUtc { get; }

    public DateTime? CompletedAtUtc { get; private set; }

    public static TodoItem Create(TodoTitle title, DateTime createdAtUtc)
    {
        return new TodoItem(Guid.NewGuid(), title, createdAtUtc);
    }

    public void Complete(DateTime completedAtUtc)
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;
        CompletedAtUtc = completedAtUtc;
    }
}
