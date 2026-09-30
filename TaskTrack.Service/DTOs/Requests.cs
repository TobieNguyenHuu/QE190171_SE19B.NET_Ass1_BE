using System.ComponentModel.DataAnnotations;

namespace TaskTrack.Service.DTOs;

public class DepartmentRequest
{
    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(100, ErrorMessage = "Department name must be at most 100 characters.")]
    public string DepartmentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department description is required.")]
    [StringLength(300, ErrorMessage = "Description must be at most 300 characters.")]
    public string DepartmentDescription { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class ProjectRequest : IValidatableObject
{
    [Required(ErrorMessage = "Project name is required.")]
    [StringLength(200, ErrorMessage = "Project name must be at most 200 characters.")]
    public string ProjectName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Start date is required.")]
    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [Range(0, 3, ErrorMessage = "Status must be 0 (Not Started), 1 (In Progress), 2 (Completed) or 3 (On Hold).")]
    public int Status { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Department is required.")]
    public int DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && EndDate.HasValue && EndDate.Value < StartDate.Value)
        {
            yield return new ValidationResult(
                "End date must be on or after the start date.", new[] { nameof(EndDate) });
        }
    }
}

public class TaskRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(300, ErrorMessage = "Title must be at most 300 characters.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, 3, ErrorMessage = "Status must be 0 (To Do), 1 (In Progress), 2 (Done) or 3 (Cancelled).")]
    public int Status { get; set; } = 0;

    [Range(0, 3, ErrorMessage = "Priority must be 0 (Low), 1 (Medium), 2 (High) or 3 (Critical).")]
    public int Priority { get; set; } = 1;

    public DateOnly? DueDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Project is required.")]
    public int ProjectId { get; set; }

    /// <summary>Optional. On update, the task's tags are replaced by this list.</summary>
    public List<int>? TagIds { get; set; }
}

public class TagRequest
{
    [Required(ErrorMessage = "Tag name is required.")]
    [StringLength(50, ErrorMessage = "Tag name must be at most 50 characters.")]
    public string TagName { get; set; } = string.Empty;

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a hex code such as #3B82F6.")]
    public string? Color { get; set; }
}
