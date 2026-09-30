using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Exceptions;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.Service.Implementations;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projects;
    private readonly IDepartmentRepository _departments;

    public ProjectService(IProjectRepository projects, IDepartmentRepository departments)
    {
        _projects = projects;
        _departments = departments;
    }

    public async Task<List<ProjectDto>> GetAllAsync() =>
        (await _projects.GetActiveAsync()).Select(p => p.ToDto()).ToList();

    public async Task<ProjectDetailDto> GetByIdAsync(int id)
    {
        var project = await _projects.GetDetailAsync(id);
        if (project is null || !project.IsActive)
        {
            throw new NotFoundException($"Project {id} was not found.");
        }

        return project.ToDetailDto();
    }

    public async Task<List<ProjectDto>> GetByDepartmentAsync(int departmentId)
    {
        var department = await _departments.GetByIdAsync(departmentId);
        if (department is null)
        {
            throw new NotFoundException($"Department {departmentId} was not found.");
        }

        return (await _projects.GetByDepartmentAsync(departmentId)).Select(p => p.ToDto()).ToList();
    }

    public async Task<List<ProjectDto>> SearchAsync(string? name, int? status, int? departmentId) =>
        (await _projects.SearchAsync(name, status, departmentId)).Select(p => p.ToDto()).ToList();

    public async Task<ProjectDto> CreateAsync(ProjectRequest request)
    {
        var department = await RequireDepartmentAsync(request.DepartmentId);

        var project = new Project
        {
            ProjectName = request.ProjectName.Trim(),
            Description = request.Description.NullIfBlank(),
            StartDate = request.StartDate!.Value,
            EndDate = request.EndDate,
            Status = (short)request.Status,
            DepartmentId = department.DepartmentId,
            IsActive = request.IsActive,
            CreatedDate = Clock.Now
        };

        await _projects.AddAsync(project);

        var created = await _projects.GetDetailAsync(project.ProjectId);
        return (created ?? project).ToDto(department.DepartmentName);
    }

    public async Task<ProjectDto> UpdateAsync(int id, ProjectRequest request)
    {
        var project = await _projects.GetByIdAsync(id)
            ?? throw new NotFoundException($"Project {id} was not found.");
        var department = await RequireDepartmentAsync(request.DepartmentId);

        project.ProjectName = request.ProjectName.Trim();
        project.Description = request.Description.NullIfBlank();
        project.StartDate = request.StartDate!.Value;
        project.EndDate = request.EndDate;
        project.Status = (short)request.Status;
        project.DepartmentId = department.DepartmentId;
        project.IsActive = request.IsActive;

        await _projects.UpdateAsync(project);

        var updated = await _projects.GetDetailAsync(id);
        return (updated ?? project).ToDto(department.DepartmentName);
    }

    public async Task DeleteAsync(int id)
    {
        var project = await _projects.GetByIdAsync(id)
            ?? throw new NotFoundException($"Project {id} was not found.");

        var linkedTasks = await _projects.CountTasksAsync(id);
        if (linkedTasks > 0)
        {
            throw new BusinessRuleException(
                $"Cannot delete project '{project.ProjectName}' because it still has {linkedTasks} linked task(s).");
        }

        await _projects.DeleteAsync(project);
    }

    private async Task<Department> RequireDepartmentAsync(int departmentId)
    {
        var department = await _departments.GetByIdAsync(departmentId);
        if (department is null || !department.IsActive)
        {
            throw new ValidationFailedException("DepartmentId", "Department does not exist or is inactive.");
        }

        return department;
    }
}
