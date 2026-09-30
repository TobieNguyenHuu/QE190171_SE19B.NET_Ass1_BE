using TaskTrack.Repo.Models;
using TaskTrack.Service.DTOs;

namespace TaskTrack.Service;

/// <summary>Human readable names for the numeric status / priority codes.</summary>
public static class Labels
{
    public static string ProjectStatus(int value) => value switch
    {
        0 => "Not Started",
        1 => "In Progress",
        2 => "Completed",
        3 => "On Hold",
        _ => "Unknown"
    };

    public static string TaskState(int value) => value switch
    {
        0 => "To Do",
        1 => "In Progress",
        2 => "Done",
        3 => "Cancelled",
        _ => "Unknown"
    };

    public static string Priority(int value) => value switch
    {
        0 => "Low",
        1 => "Medium",
        2 => "High",
        3 => "Critical",
        _ => "Unknown"
    };
}

/// <summary>
/// The Task/Project tables use "timestamp without time zone": store UTC with an Unspecified kind.
/// </summary>
public static class Clock
{
    public static DateTime Now => DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
}

public static class Mappers
{
    public static TagDto ToDto(this Tag t) => new()
    {
        TagId = t.TagId,
        TagName = t.TagName,
        Color = t.Color
    };

    public static TagUsageDto ToUsageDto(this Tag t) => new()
    {
        TagId = t.TagId,
        TagName = t.TagName,
        Color = t.Color,
        TaskCount = t.Tasks.Count
    };

    public static TaskDto ToDto(this TaskItem t, string? projectName = null) => new()
    {
        TaskId = t.TaskId,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        StatusName = Labels.TaskState(t.Status),
        Priority = t.Priority,
        PriorityName = Labels.Priority(t.Priority),
        DueDate = t.DueDate,
        ProjectId = t.ProjectId,
        ProjectName = t.Project?.ProjectName ?? projectName ?? string.Empty,
        IsActive = t.IsActive,
        CreatedDate = t.CreatedDate,
        ModifiedDate = t.ModifiedDate,
        Tags = t.Tags.OrderBy(g => g.TagName).Select(g => g.ToDto()).ToList()
    };

    public static ProjectDto ToDto(this Project p, string? departmentName = null) => new()
    {
        ProjectId = p.ProjectId,
        ProjectName = p.ProjectName,
        Description = p.Description,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Status = p.Status,
        StatusName = Labels.ProjectStatus(p.Status),
        DepartmentId = p.DepartmentId,
        DepartmentName = p.Department?.DepartmentName ?? departmentName ?? string.Empty,
        IsActive = p.IsActive,
        CreatedDate = p.CreatedDate,
        TaskCount = p.Tasks.Count(t => t.IsActive)
    };

    public static ProjectDetailDto ToDetailDto(this Project p) => new()
    {
        ProjectId = p.ProjectId,
        ProjectName = p.ProjectName,
        Description = p.Description,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Status = p.Status,
        StatusName = Labels.ProjectStatus(p.Status),
        DepartmentId = p.DepartmentId,
        DepartmentName = p.Department?.DepartmentName ?? string.Empty,
        IsActive = p.IsActive,
        CreatedDate = p.CreatedDate,
        TaskCount = p.Tasks.Count(t => t.IsActive),
        Tasks = p.Tasks.Where(t => t.IsActive).OrderBy(t => t.TaskId).Select(t => t.ToDto(p.ProjectName)).ToList()
    };

    public static DepartmentDto ToDto(this Department d) => new()
    {
        DepartmentId = d.DepartmentId,
        DepartmentName = d.DepartmentName,
        DepartmentDescription = d.DepartmentDescription,
        IsActive = d.IsActive,
        ProjectCount = d.Projects.Count(p => p.IsActive)
    };

    public static DepartmentDetailDto ToDetailDto(this Department d) => new()
    {
        DepartmentId = d.DepartmentId,
        DepartmentName = d.DepartmentName,
        DepartmentDescription = d.DepartmentDescription,
        IsActive = d.IsActive,
        ProjectCount = d.Projects.Count(p => p.IsActive),
        Projects = d.Projects.Where(p => p.IsActive).OrderBy(p => p.ProjectId)
            .Select(p => p.ToDto(d.DepartmentName)).ToList()
    };

    public static string? NullIfBlank(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
