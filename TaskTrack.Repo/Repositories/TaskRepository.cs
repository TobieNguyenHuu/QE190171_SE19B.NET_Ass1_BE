using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public interface ITaskRepository : IGenericRepository<TaskItem>
{
    Task<List<TaskItem>> GetActiveAsync();
    Task<TaskItem?> GetDetailAsync(int id);
    Task<List<TaskItem>> GetByProjectAsync(int projectId);
    Task<List<TaskItem>> SearchAsync(string? title, int? status, int? priority, int? projectId, int? tagId);
}

public class TaskRepository : GenericRepository<TaskItem>, ITaskRepository
{
    public TaskRepository(TaskManagementContext context) : base(context)
    {
    }

    private IQueryable<TaskItem> ListQuery() =>
        _set.AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.Tags)
            .Where(t => t.IsActive);

    public Task<List<TaskItem>> GetActiveAsync() =>
        ListQuery().OrderBy(t => t.TaskId).ToListAsync();

    /// <summary>Task + project + tags. Tracked, so it can also be used for updates.</summary>
    public Task<TaskItem?> GetDetailAsync(int id) =>
        _set.Include(t => t.Project)
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.TaskId == id);

    public Task<List<TaskItem>> GetByProjectAsync(int projectId) =>
        ListQuery().Where(t => t.ProjectId == projectId).OrderBy(t => t.TaskId).ToListAsync();

    public Task<List<TaskItem>> SearchAsync(string? title, int? status, int? priority, int? projectId, int? tagId)
    {
        var query = ListQuery();

        if (!string.IsNullOrWhiteSpace(title))
        {
            var pattern = LikeHelper.Contains(title);
            query = query.Where(t => EF.Functions.ILike(t.Title, pattern));
        }

        if (status.HasValue)
        {
            var s = (short)status.Value;
            query = query.Where(t => t.Status == s);
        }

        if (priority.HasValue)
        {
            var p = (short)priority.Value;
            query = query.Where(t => t.Priority == p);
        }

        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        if (tagId.HasValue)
        {
            var id = tagId.Value;
            query = query.Where(t => t.Tags.Any(g => g.TagId == id));
        }

        return query.OrderBy(t => t.TaskId).ToListAsync();
    }
}
