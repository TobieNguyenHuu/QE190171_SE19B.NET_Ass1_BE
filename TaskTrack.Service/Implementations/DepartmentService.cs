using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Exceptions;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.Service.Implementations;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departments;

    public DepartmentService(IDepartmentRepository departments)
    {
        _departments = departments;
    }

    public async Task<List<DepartmentDto>> GetAllAsync() =>
        (await _departments.GetActiveAsync()).Select(d => d.ToDto()).ToList();

    public async Task<DepartmentDetailDto> GetByIdAsync(int id)
    {
        var department = await _departments.GetWithProjectsAsync(id);
        if (department is null || !department.IsActive)
        {
            throw new NotFoundException($"Department {id} was not found.");
        }

        return department.ToDetailDto();
    }

    public async Task<List<DepartmentDto>> SearchAsync(string? name) =>
        (await _departments.SearchAsync(name)).Select(d => d.ToDto()).ToList();

    public async Task<DepartmentDto> CreateAsync(DepartmentRequest request)
    {
        var department = new Department
        {
            DepartmentName = request.DepartmentName.Trim(),
            DepartmentDescription = request.DepartmentDescription.Trim(),
            IsActive = request.IsActive
        };

        await _departments.AddAsync(department);
        return department.ToDto();
    }

    public async Task<DepartmentDto> UpdateAsync(int id, DepartmentRequest request)
    {
        var department = await _departments.GetByIdAsync(id)
            ?? throw new NotFoundException($"Department {id} was not found.");

        department.DepartmentName = request.DepartmentName.Trim();
        department.DepartmentDescription = request.DepartmentDescription.Trim();
        department.IsActive = request.IsActive;

        await _departments.UpdateAsync(department);

        var reloaded = await _departments.GetWithProjectsAsync(id);
        return (reloaded ?? department).ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var department = await _departments.GetByIdAsync(id)
            ?? throw new NotFoundException($"Department {id} was not found.");

        var linkedProjects = await _departments.CountProjectsAsync(id);
        if (linkedProjects > 0)
        {
            throw new BusinessRuleException(
                $"Cannot delete department '{department.DepartmentName}' because it still has {linkedProjects} linked project(s).");
        }

        await _departments.DeleteAsync(department);
    }
}
