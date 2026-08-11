using TaskFlow.Models;

namespace TaskFlow.Services;

/// <summary>
/// In-memory implementation of <see cref="ITaskService"/>. Registered as a
/// singleton so the list survives between requests. A real app would swap this
/// for a database-backed implementation without touching the controller.
/// </summary>
public class TaskService : ITaskService
{
    private readonly List<TaskItem> _tasks = new();
    private readonly object _lock = new();
    private int _nextId = 1;

    // The only legal moves. Anything not listed here (skipping Todo -> Done,
    // moving backwards, or staying put) is rejected.
    private static readonly IReadOnlyDictionary<TaskItemStatus, TaskItemStatus[]> ValidTransitions =
        new Dictionary<TaskItemStatus, TaskItemStatus[]>
        {
            [TaskItemStatus.Todo] = new[] { TaskItemStatus.InProgress },
            [TaskItemStatus.InProgress] = new[] { TaskItemStatus.Done },
            [TaskItemStatus.Done] = Array.Empty<TaskItemStatus>(),
        };

    public IEnumerable<TaskItem> GetAll()
    {
        lock (_lock)
        {
            return _tasks.ToList();
        }
    }

    public TaskItem? GetById(int id)
    {
        lock (_lock)
        {
            return _tasks.FirstOrDefault(t => t.Id == id);
        }
    }

    public TaskItem Create(CreateTaskRequest request)
    {
        var title = request.Title?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new TaskValidationException("Title is required.");
        }

        if (title.Length > 100)
        {
            throw new TaskValidationException("Title cannot be longer than 100 characters.");
        }

        if (request.DueDate < DateTime.UtcNow)
        {
            throw new TaskValidationException("Due date cannot be in the past.");
        }

        lock (_lock)
        {
            var task = new TaskItem
            {
                Id = _nextId++,
                Title = title,
                Description = request.Description,
                Priority = request.Priority,
                Status = TaskItemStatus.Todo,
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow,
            };

            _tasks.Add(task);
            return task;
        }
    }

    public TaskItem UpdateStatus(int id, TaskItemStatus newStatus)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id)
                ?? throw new KeyNotFoundException($"Task {id} was not found.");

            if (!ValidTransitions[task.Status].Contains(newStatus))
            {
                throw new TaskValidationException(
                    $"Cannot move a task from {task.Status} to {newStatus}.");
            }

            task.Status = newStatus;
            return task;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task is null)
            {
                return false;
            }

            _tasks.Remove(task);
            return true;
        }
    }

    public IEnumerable<TaskItem> GetOverdue()
    {
        var now = DateTime.UtcNow;

        lock (_lock)
        {
            return _tasks
                .Where(t => t.Status != TaskItemStatus.Done
                            && t.DueDate.HasValue
                            && t.DueDate.Value < now)
                .ToList();
        }
    }

    /// <summary>
    /// Bulk-loads ready-made tasks, assigning ids and skipping the
    /// <see cref="Create"/> validation. Used to seed demo data on startup and
    /// to set up known states (for example, past-due tasks) in tests.
    /// </summary>
    public void Seed(IEnumerable<TaskItem> tasks)
    {
        lock (_lock)
        {
            foreach (var task in tasks)
            {
                task.Id = _nextId++;
                if (task.CreatedAt == default)
                {
                    task.CreatedAt = DateTime.UtcNow;
                }

                _tasks.Add(task);
            }
        }
    }
}
