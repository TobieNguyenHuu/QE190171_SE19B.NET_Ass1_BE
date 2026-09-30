using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public interface IProjectRepository : IGenericRepository<Project>
{
    Task<List<Project>> GetActiveAsync();
    Task<Project?> GetDetailAsync(int id);
    Task<List<Project>> GetByDepartmentAsync(int departmentId);
    Task<List<Project>> SearchAsync(string? name, int? status, int? departmentId);
    Task<int> CountTasksAsync(int projectId);
}

public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    public ProjectRepository(TaskManagementContext context) : base(context)
    {
    }

    private IQueryable<Project> ListQuery() =>
        _set.AsNoTracking()
            .Include(p => p.Department)
            .Include(p => p.Tasks.Where(t => t.IsActive))
            .Where(p => p.IsActive);

    public Task<List<Project>> GetActiveAsync() =>
        ListQuery().OrderBy(p => p.ProjectId).ToListAsync();

    /// <summary>Project + department + its active tasks (with tags). Tracked, so it can also be used for updates.</summary>
    public Task<Project?> GetDetailAsync(int id) =>
        _set.Include(p => p.Department)
            .Include(p => p.Tasks.Where(t => t.IsActive))
                .ThenInclude(t => t.Tags)
            .FirstOrDefaultAsync(p => p.ProjectId == id);

    public Task<List<Project>> GetByDepartmentAsync(int departmentId) =>
        ListQuery().Where(p => p.DepartmentId == departmentId).OrderBy(p => p.ProjectId).ToListAsync();

    public Task<List<Project>> SearchAsync(string? name, int? status, int? departmentId)
    {
        var query = ListQuery();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var pattern = LikeHelper.Contains(name);
            query = query.Where(p => EF.Functions.ILike(p.ProjectName, pattern));
        }

        if (status.HasValue)
        {
            var s = (short)status.Value;
            query = query.Where(p => p.Status == s);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(p => p.DepartmentId == departmentId.Value);
        }

        return query.OrderBy(p => p.ProjectId).ToListAsync();
    }

    public Task<int> CountTasksAsync(int projectId) =>
        _context.Tasks.CountAsync(t => t.ProjectId == projectId);
}
