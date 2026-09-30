using Microsoft.AspNetCore.Mvc;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController : ControllerBase
{
    private readonly ITagService _service;

    public TagsController(ITagService service)
    {
        _service = service;
    }

    /// <summary>List all tags.</summary>
    [HttpGet]
    public async Task<ActionResult<List<TagUsageDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult<TagUsageDto>> Create(TagRequest request)
    {
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetAll), new { }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TagUsageDto>> Update(int id, TagRequest request) =>
        Ok(await _service.UpdateAsync(id, request));

    /// <summary>Delete a tag. Returns 400 if any task uses it.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
