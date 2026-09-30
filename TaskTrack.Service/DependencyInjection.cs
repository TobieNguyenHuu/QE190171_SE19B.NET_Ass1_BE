using Microsoft.Extensions.DependencyInjection;
using TaskTrack.Service.Implementations;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ITagService, TagService>();
        return services;
    }
}
