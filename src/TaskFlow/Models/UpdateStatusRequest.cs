namespace TaskFlow.Models;

/// <summary>The payload for moving a task to a new status.</summary>
public class UpdateStatusRequest
{
    public TaskItemStatus Status { get; set; }
}
