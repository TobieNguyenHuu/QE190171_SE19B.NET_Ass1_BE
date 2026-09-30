using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public interface ITagRepository : IGenericRepository<Tag>
{
    /// <summary>All tags, each with its linked tasks loaded (used to count usage).</summary>
    Task<List<Tag>> GetAllWithUsageAsync();
    Task<List<Tag>> GetByIdsAsync(IReadOnlyCollection<int> ids);
    Task<bool> NameExistsAsync(string name, int? excludeId);
    Task<int> CountTasksUsingAsync(int tagId);
}

public class TagRepository : GenericRepository<Tag>, ITagRepository
{
    public TagRepository(TaskManagementContext context) : base(context)
    {
    }

    public Task<List<Tag>> GetAllWithUsageAsync() =>
        _set.AsNoTracking()
            .Include(t => t.Tasks)
            .OrderBy(t => t.TagName)
            .ToListAsync();

    public Task<List<Tag>> GetByIdsAsync(IReadOnlyCollection<int> ids) =>
        _set.Where(t => ids.Contains(t.TagId)).ToListAsync();

    public Task<bool> NameExistsAsync(string name, int? excludeId)
    {
        var lowered = name.Trim().ToLower();
        return _set.AnyAsync(t => t.TagName.ToLower() == lowered && (excludeId == null || t.TagId != excludeId));
    }

    public Task<int> CountTasksUsingAsync(int tagId) =>
        _context.Tasks.CountAsync(t => t.Tags.Any(g => g.TagId == tagId));
}
