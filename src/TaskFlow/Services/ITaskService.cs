using TaskFlow.Models;

namespace TaskFlow.Services;

/// <summary>The business logic surface the controller depends on.</summary>
public interface ITaskService
{
    IEnumerable<TaskItem> GetAll();

    TaskItem? GetById(int id);

    /// <summary>Creates a task after validating the request.</summary>
    /// <exception cref="TaskValidationException">The request breaks a rule.</exception>
    TaskItem Create(CreateTaskRequest request);

    /// <summary>Moves a task to <paramref name="newStatus"/> if the transition is legal.</summary>
    /// <exception cref="KeyNotFoundException">No task has that id.</exception>
    /// <exception cref="TaskValidationException">The transition is not allowed.</exception>
    TaskItem UpdateStatus(int id, TaskItemStatus newStatus);

    /// <summary>Removes a task. Returns false when the id does not exist.</summary>
    bool Delete(int id);

    /// <summary>Tasks whose due date has passed and that are not yet Done.</summary>
    IEnumerable<TaskItem> GetOverdue();
}
