namespace TaskFlow.Models;

/// <summary>A single unit of work tracked by the API.</summary>
public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }
}
