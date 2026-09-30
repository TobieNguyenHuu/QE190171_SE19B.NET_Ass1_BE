using Microsoft.AspNetCore.Mvc;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _service;

    public TasksController(ITaskService service)
    {
        _service = service;
    }

    /// <summary>List all active tasks.</summary>
    [HttpGet]
    public async Task<ActionResult<List<TaskDto>>> GetAll() => Ok(await _service.GetAllAsync());

    /// <summary>Filter tasks. All parameters are optional.</summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<TaskDto>>> Search(
        [FromQuery] string? title,
        [FromQuery] int? status,
        [FromQuery] int? priority,
        [FromQuery] int? projectId,
        [FromQuery] int? tagId) =>
        Ok(await _service.SearchAsync(title, status, priority, projectId, tagId));

    /// <summary>Get tasks of one project.</summary>
    [HttpGet("project/{projectId:int}")]
    public async Task<ActionResult<List<TaskDto>>> GetByProject(int projectId) =>
        Ok(await _service.GetByProjectAsync(projectId));

    /// <summary>Get one task including its tags.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Create a task. TagIds is optional.</summary>
    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(TaskRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.TaskId }, created);
    }

    /// <summary>Update a task, replace its tags and set ModifiedDate.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskDto>> Update(int id, TaskRequest request) =>
        Ok(await _service.UpdateAsync(id, request));

    /// <summary>Soft delete (IsActive = false).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
