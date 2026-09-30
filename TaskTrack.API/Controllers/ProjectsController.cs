using Microsoft.AspNetCore.Mvc;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _service;

    public ProjectsController(IProjectService service)
    {
        _service = service;
    }

    /// <summary>List all active projects (with department name).</summary>
    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetAll() => Ok(await _service.GetAllAsync());

    /// <summary>Filter projects. All parameters are optional.</summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<ProjectDto>>> Search(
        [FromQuery] string? name, [FromQuery] int? status, [FromQuery] int? departmentId) =>
        Ok(await _service.SearchAsync(name, status, departmentId));

    /// <summary>Get projects of one department.</summary>
    [HttpGet("department/{departmentId:int}")]
    public async Task<ActionResult<List<ProjectDto>>> GetByDepartment(int departmentId) =>
        Ok(await _service.GetByDepartmentAsync(departmentId));

    /// <summary>Get one project and its tasks.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDetailDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(ProjectRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.ProjectId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProjectDto>> Update(int id, ProjectRequest request) =>
        Ok(await _service.UpdateAsync(id, request));

    /// <summary>Delete a project. Returns 400 if it still has tasks.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
