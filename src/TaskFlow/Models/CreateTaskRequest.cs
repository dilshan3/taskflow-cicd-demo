namespace TaskFlow.Models;

/// <summary>The payload a client sends to create a task.</summary>
/// <remarks>
/// Deliberately smaller than <see cref="TaskItem"/>: the server owns
/// <c>Id</c>, <c>CreatedAt</c>, and the starting <c>Status</c> (always Todo).
/// </remarks>
public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime DueDate { get; set; }
}
