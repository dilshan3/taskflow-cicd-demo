namespace TaskFlow.Models;

/// <summary>
/// Where a task sits in its lifecycle. The only valid moves are
/// Todo -> InProgress -> Done (see <see cref="Services.TaskService"/>).
/// Named <c>TaskItemStatus</c> rather than <c>TaskStatus</c> to avoid clashing
/// with <see cref="System.Threading.Tasks.TaskStatus"/> under implicit usings.
/// </summary>
public enum TaskItemStatus
{
    Todo,
    InProgress,
    Done
}
