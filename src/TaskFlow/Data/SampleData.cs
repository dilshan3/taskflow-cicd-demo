using TaskFlow.Models;

namespace TaskFlow.Data;

/// <summary>
/// The tasks loaded into memory on startup so the demo has data to show
/// immediately. Dates are relative to "now" so the overdue example always works.
/// </summary>
public static class SampleData
{
    public static IEnumerable<TaskItem> Tasks()
    {
        var now = DateTime.UtcNow;

        return new[]
        {
            new TaskItem
            {
                Title = "Set up the project repository",
                Description = "Create the Git repo and push the initial scaffold.",
                Priority = TaskPriority.High,
                Status = TaskItemStatus.Done,
                DueDate = now.AddDays(-5),
            },
            new TaskItem
            {
                Title = "Design the TaskItem model",
                Description = "Agree on the fields and validation rules for a task.",
                Priority = TaskPriority.Medium,
                Status = TaskItemStatus.Done,
                DueDate = now.AddDays(-1),
            },
            new TaskItem
            {
                Title = "Write the CI pipeline",
                Description = "Build and test on every push with GitHub Actions.",
                Priority = TaskPriority.High,
                Status = TaskItemStatus.InProgress,
                DueDate = now.AddDays(2),
            },
            new TaskItem
            {
                Title = "Review pull request #12",
                Description = "Started but never finished, and now past its due date.",
                Priority = TaskPriority.Low,
                Status = TaskItemStatus.InProgress,
                DueDate = now.AddDays(-2),
            },
            new TaskItem
            {
                Title = "Add the overdue endpoint",
                Description = "Expose GET /api/tasks/overdue.",
                Priority = TaskPriority.Medium,
                Status = TaskItemStatus.Todo,
                DueDate = now.AddDays(3),
            },
            new TaskItem
            {
                Title = "Prepare the intern demo",
                Description = "Walk through the pipeline from commit to deploy.",
                Priority = TaskPriority.High,
                Status = TaskItemStatus.Todo,
                DueDate = now.AddDays(7),
            },
        };
    }
}
