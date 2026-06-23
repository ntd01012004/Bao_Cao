namespace MiniDddCqrsJwt.Domain.Todos;

public sealed record TodoTitle
{
    private TodoTitle(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static TodoTitle Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Todo title is required.", nameof(value));
        }

        if (value.Length > 120)
        {
            throw new ArgumentException("Todo title must be 120 characters or less.", nameof(value));
        }

        return new TodoTitle(value.Trim());
    }
}
