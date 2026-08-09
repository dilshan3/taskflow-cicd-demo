namespace TaskFlow.Services;

/// <summary>
/// Thrown when a task breaks a business rule (bad title, past due date,
/// illegal status transition). The controller maps this to HTTP 400.
/// </summary>
public class TaskValidationException : Exception
{
    public TaskValidationException(string message) : base(message)
    {
    }
}
