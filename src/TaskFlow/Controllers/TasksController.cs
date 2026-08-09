using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models;
using TaskFlow.Services;

namespace TaskFlow.Controllers;

[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _service;

    public TasksController(ITaskService service) => _service = service;

    /// <summary>Lists every task.</summary>
    [HttpGet]
    public ActionResult<IEnumerable<TaskItem>> GetAll() => Ok(_service.GetAll());

    /// <summary>Lists tasks that are past their due date and not yet Done.</summary>
    [HttpGet("overdue")]
    public ActionResult<IEnumerable<TaskItem>> GetOverdue() => Ok(_service.GetOverdue());

    /// <summary>Gets a single task by id.</summary>
    [HttpGet("{id:int}")]
    public ActionResult<TaskItem> GetById(int id)
    {
        var task = _service.GetById(id);
        return task is null ? NotFound() : Ok(task);
    }

    /// <summary>Creates a task. New tasks always start in the Todo status.</summary>
    [HttpPost]
    public ActionResult<TaskItem> Create(CreateTaskRequest request)
    {
        try
        {
            var task = _service.Create(request);
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }
        catch (TaskValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Moves a task to a new status, following the Todo -> InProgress -> Done rules.</summary>
    [HttpPut("{id:int}/status")]
    public ActionResult<TaskItem> UpdateStatus(int id, UpdateStatusRequest request)
    {
        try
        {
            return Ok(_service.UpdateStatus(id, request.Status));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (TaskValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Deletes a task.</summary>
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id) =>
        _service.Delete(id) ? NoContent() : NotFound();
}
