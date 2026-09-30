using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Exceptions;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.Service.Implementations;

public class TagService : ITagService
{
    private readonly ITagRepository _tags;

    public TagService(ITagRepository tags)
    {
        _tags = tags;
    }

    public async Task<List<TagUsageDto>> GetAllAsync() =>
        (await _tags.GetAllWithUsageAsync()).Select(t => t.ToUsageDto()).ToList();

    public async Task<TagUsageDto> CreateAsync(TagRequest request)
    {
        var name = request.TagName.Trim();
        await EnsureUniqueNameAsync(name, null);

        var tag = new Tag { TagName = name, Color = request.Color.NullIfBlank() };
        await _tags.AddAsync(tag);
        return tag.ToUsageDto();
    }

    public async Task<TagUsageDto> UpdateAsync(int id, TagRequest request)
    {
        var tag = await _tags.GetByIdAsync(id)
            ?? throw new NotFoundException($"Tag {id} was not found.");

        var name = request.TagName.Trim();
        await EnsureUniqueNameAsync(name, id);

        tag.TagName = name;
        tag.Color = request.Color.NullIfBlank();
        await _tags.UpdateAsync(tag);

        var usage = await _tags.CountTasksUsingAsync(id);
        return new TagUsageDto { TagId = tag.TagId, TagName = tag.TagName, Color = tag.Color, TaskCount = usage };
    }

    public async Task DeleteAsync(int id)
    {
        var tag = await _tags.GetByIdAsync(id)
            ?? throw new NotFoundException($"Tag {id} was not found.");

        var usage = await _tags.CountTasksUsingAsync(id);
        if (usage > 0)
        {
            throw new BusinessRuleException(
                $"Cannot delete tag '{tag.TagName}' because it is used by {usage} task(s).");
        }

        await _tags.DeleteAsync(tag);
    }

    private async Task EnsureUniqueNameAsync(string name, int? excludeId)
    {
        if (await _tags.NameExistsAsync(name, excludeId))
        {
            throw new ValidationFailedException("TagName", "A tag with this name already exists.");
        }
    }
}
