using TaskFlow.Models;
using TaskFlow.Services;
using Xunit;

namespace TaskFlow.Tests;

public class TaskServiceTests
{
    private static TaskService NewService() => new();

    private static CreateTaskRequest ValidRequest(string title = "Write the docs") => new()
    {
        Title = title,
        Description = "A perfectly valid task",
        Priority = TaskPriority.Medium,
        DueDate = DateTime.UtcNow.AddDays(3),
    };

    // ---------- Create: title validation ----------

    [Fact]
    public void Create_WithValidRequest_StoresTaskAsTodo()
    {
        var service = NewService();

        var task = service.Create(ValidRequest());

        Assert.True(task.Id > 0);
        Assert.Equal(TaskItemStatus.Todo, task.Status);
        Assert.Single(service.GetAll());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_Throws(string? title)
    {
        var service = NewService();
        var request = ValidRequest();
        request.Title = title!;

        Assert.Throws<TaskValidationException>(() => service.Create(request));
    }

    [Fact]
    public void Create_WithTitleOver100Chars_Throws()
    {
        var service = NewService();

        Assert.Throws<TaskValidationException>(
            () => service.Create(ValidRequest(new string('x', 101))));
    }

    [Fact]
    public void Create_WithTitleExactly100Chars_Succeeds()
    {
        var service = NewService();

        var task = service.Create(ValidRequest(new string('x', 100)));

        Assert.Equal(100, task.Title.Length);
    }

    // ---------- Create: due date validation ----------

    [Fact]
    public void Create_WithPastDueDate_Throws()
    {
        var service = NewService();
        var request = ValidRequest();
        request.DueDate = DateTime.UtcNow.AddDays(-1);

        Assert.Throws<TaskValidationException>(() => service.Create(request));
    }

    // ---------- UpdateStatus: valid transitions ----------

    [Fact]
    public void UpdateStatus_TodoToInProgress_Succeeds()
    {
        var service = NewService();
        var task = service.Create(ValidRequest());

        var updated = service.UpdateStatus(task.Id, TaskItemStatus.InProgress);

        Assert.Equal(TaskItemStatus.InProgress, updated.Status);
    }

    [Fact]
    public void UpdateStatus_InProgressToDone_Succeeds()
    {
        var service = NewService();
        var task = service.Create(ValidRequest());
        service.UpdateStatus(task.Id, TaskItemStatus.InProgress);

        var updated = service.UpdateStatus(task.Id, TaskItemStatus.Done);

        Assert.Equal(TaskItemStatus.Done, updated.Status);
    }

    // ---------- UpdateStatus: invalid transitions ----------

    [Fact]
    public void UpdateStatus_TodoDirectlyToDone_Throws()
    {
        var service = NewService();
        var task = service.Create(ValidRequest());

        Assert.Throws<TaskValidationException>(
            () => service.UpdateStatus(task.Id, TaskItemStatus.Done));
    }

    [Fact]
    public void UpdateStatus_BackwardsFromDone_Throws()
    {
        var service = NewService();
        var task = service.Create(ValidRequest());
        service.UpdateStatus(task.Id, TaskItemStatus.InProgress);
        service.UpdateStatus(task.Id, TaskItemStatus.Done);

        Assert.Throws<TaskValidationException>(
            () => service.UpdateStatus(task.Id, TaskItemStatus.InProgress));
    }

    [Fact]
    public void UpdateStatus_ForUnknownId_ThrowsKeyNotFound()
    {
        var service = NewService();

        Assert.Throws<KeyNotFoundException>(
            () => service.UpdateStatus(999, TaskItemStatus.InProgress));
    }

    // ---------- Overdue detection ----------

    [Fact]
    public void GetOverdue_ReturnsOnlyPastDueTasksThatAreNotDone()
    {
        var service = NewService();
        service.Seed(new[]
        {
            new TaskItem { Title = "Open and overdue", Status = TaskItemStatus.InProgress, DueDate = DateTime.UtcNow.AddDays(-1) },
            new TaskItem { Title = "Done and past due", Status = TaskItemStatus.Done, DueDate = DateTime.UtcNow.AddDays(-2) },
            new TaskItem { Title = "Still due in the future", Status = TaskItemStatus.Todo, DueDate = DateTime.UtcNow.AddDays(2) },
        });

        var overdue = service.GetOverdue().ToList();

        Assert.Single(overdue);
        Assert.Equal("Open and overdue", overdue[0].Title);
    }

    [Fact]
    public void GetOverdue_WhenNothingIsPastDue_ReturnsEmpty()
    {
        var service = NewService();
        service.Seed(new[]
        {
            new TaskItem { Title = "Future work", Status = TaskItemStatus.Todo, DueDate = DateTime.UtcNow.AddDays(5) },
        });

        Assert.Empty(service.GetOverdue());
    }
}
