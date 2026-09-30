using TaskTrack.Repo.Models;
using TaskTrack.Repo.Repositories;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Exceptions;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.Service.Implementations;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _tasks;
    private readonly IProjectRepository _projects;
    private readonly ITagRepository _tags;

    public TaskService(ITaskRepository tasks, IProjectRepository projects, ITagRepository tags)
    {
        _tasks = tasks;
        _projects = projects;
        _tags = tags;
    }

    public async Task<List<TaskDto>> GetAllAsync() =>
        (await _tasks.GetActiveAsync()).Select(t => t.ToDto()).ToList();

    public async Task<TaskDto> GetByIdAsync(int id)
    {
        var task = await _tasks.GetDetailAsync(id);
        if (task is null || !task.IsActive)
        {
            throw new NotFoundException($"Task {id} was not found.");
        }

        return task.ToDto();
    }

    public async Task<List<TaskDto>> GetByProjectAsync(int projectId)
    {
        var project = await _projects.GetByIdAsync(projectId);
        if (project is null)
        {
            throw new NotFoundException($"Project {projectId} was not found.");
        }

        return (await _tasks.GetByProjectAsync(projectId)).Select(t => t.ToDto()).ToList();
    }

    public async Task<List<TaskDto>> SearchAsync(string? title, int? status, int? priority, int? projectId, int? tagId) =>
        (await _tasks.SearchAsync(title, status, priority, projectId, tagId)).Select(t => t.ToDto()).ToList();

    public async Task<TaskDto> CreateAsync(TaskRequest request)
    {
        var project = await RequireProjectAsync(request.ProjectId);
        var tags = await ResolveTagsAsync(request.TagIds);

        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description.NullIfBlank(),
            Status = (short)request.Status,
            Priority = (short)request.Priority,
            DueDate = request.DueDate,
            ProjectId = project.ProjectId,
            IsActive = true,
            CreatedDate = Clock.Now,
            Tags = tags
        };

        await _tasks.AddAsync(task);

        var created = await _tasks.GetDetailAsync(task.TaskId);
        return (created ?? task).ToDto(project.ProjectName);
    }

    public async Task<TaskDto> UpdateAsync(int id, TaskRequest request)
    {
        var task = await _tasks.GetDetailAsync(id);
        if (task is null || !task.IsActive)
        {
            throw new NotFoundException($"Task {id} was not found.");
        }

        var project = await RequireProjectAsync(request.ProjectId);
        var tags = await ResolveTagsAsync(request.TagIds);

        task.Title = request.Title.Trim();
        task.Description = request.Description.NullIfBlank();
        task.Status = (short)request.Status;
        task.Priority = (short)request.Priority;
        task.DueDate = request.DueDate;
        task.ProjectId = project.ProjectId;
        task.Project = project;
        task.ModifiedDate = Clock.Now;

        // Replace the tag set.
        task.Tags.Clear();
        foreach (var tag in tags)
        {
            task.Tags.Add(tag);
        }

        await _tasks.UpdateAsync(task);
        return task.ToDto(project.ProjectName);
    }

    public async Task DeleteAsync(int id)
    {
        var task = await _tasks.GetDetailAsync(id);
        if (task is null || !task.IsActive)
        {
            throw new NotFoundException($"Task {id} was not found.");
        }

        // Soft delete: never remove the row.
        task.IsActive = false;
        task.ModifiedDate = Clock.Now;
        await _tasks.UpdateAsync(task);
    }

    private async Task<Project> RequireProjectAsync(int projectId)
    {
        var project = await _projects.GetByIdAsync(projectId);
        if (project is null || !project.IsActive)
        {
            throw new ValidationFailedException("ProjectId", "Project does not exist or is inactive.");
        }

        return project;
    }

    private async Task<List<Tag>> ResolveTagsAsync(List<int>? tagIds)
    {
        var ids = (tagIds ?? new List<int>()).Distinct().ToList();
        if (ids.Count == 0)
        {
            return new List<Tag>();
        }

        var tags = await _tags.GetByIdsAsync(ids);
        var missing = ids.Except(tags.Select(t => t.TagId)).ToList();
        if (missing.Count > 0)
        {
            throw new ValidationFailedException("TagIds", $"Tag(s) not found: {string.Join(", ", missing)}.");
        }

        return tags;
    }
}
