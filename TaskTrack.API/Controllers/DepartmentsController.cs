using Microsoft.AspNetCore.Mvc;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service)
    {
        _service = service;
    }

    /// <summary>List all active departments.</summary>
    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetAll() => Ok(await _service.GetAllAsync());

    /// <summary>Search departments by name (partial match).</summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<DepartmentDto>>> Search([FromQuery] string? name) =>
        Ok(await _service.SearchAsync(name));

    /// <summary>Get one department and its projects.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDetailDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create(DepartmentRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.DepartmentId }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> Update(int id, DepartmentRequest request) =>
        Ok(await _service.UpdateAsync(id, request));

    /// <summary>Delete a department. Returns 400 if it still has projects.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
