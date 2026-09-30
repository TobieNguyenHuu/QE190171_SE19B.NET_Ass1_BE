using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;

namespace TaskTrack.Repo.Repositories;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<List<Department>> GetActiveAsync();
    Task<Department?> GetWithProjectsAsync(int id);
    Task<List<Department>> SearchAsync(string? name);
    Task<int> CountProjectsAsync(int departmentId);
}

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(TaskManagementContext context) : base(context)
    {
    }

    public Task<List<Department>> GetActiveAsync() =>
        _set.AsNoTracking()
            .Include(d => d.Projects.Where(p => p.IsActive))
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentId)
            .ToListAsync();

    public Task<Department?> GetWithProjectsAsync(int id) =>
        _set.AsNoTracking()
            .Include(d => d.Projects.Where(p => p.IsActive))
            .FirstOrDefaultAsync(d => d.DepartmentId == id);

    public Task<List<Department>> SearchAsync(string? name)
    {
        var query = _set.AsNoTracking()
            .Include(d => d.Projects.Where(p => p.IsActive))
            .Where(d => d.IsActive);

        if (!string.IsNullOrWhiteSpace(name))
        {
            var pattern = LikeHelper.Contains(name);
            query = query.Where(d => EF.Functions.ILike(d.DepartmentName, pattern));
        }

        return query.OrderBy(d => d.DepartmentId).ToListAsync();
    }

    public Task<int> CountProjectsAsync(int departmentId) =>
        _context.Projects.CountAsync(p => p.DepartmentId == departmentId);
}
