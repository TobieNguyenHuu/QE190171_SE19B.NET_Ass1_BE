using Microsoft.OpenApi.Models;
using TaskTrack.API;
using TaskTrack.API.Middleware;
using TaskTrack.Repo;
using TaskTrack.Service;

// Our tables use "timestamp without time zone"; keep Npgsql lenient about DateTime.Kind.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Render (and most PaaS hosts) tell the app which port to listen on through PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// ---- Services -------------------------------------------------------------
builder.Services.AddRepositories(ConnectionStringResolver.Resolve(builder.Configuration));
builder.Services.AddServices();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskTrack API",
        Version = "v1",
        Description = "PRN232 - Assignment 1: Task & Team Management (public CRUD, no authentication)."
    });
});

// CORS: origins come from CORS_ALLOWED_ORIGINS (comma separated, e.g. your Vercel URL) and/or Cors:AllowedOrigins.
var allowedOrigins = (builder.Configuration["CORS_ALLOWED_ORIGINS"] ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Concat(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
    .Select(origin => origin.TrimEnd('/'))
    .Where(origin => origin.Length > 0)
    .Distinct()
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

// ---- Pipeline -------------------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger is always on in Development. Set Swagger:Enabled=true (default in appsettings.json)
// to keep it available on the deployed Render URL, as the assignment asks.
var swaggerEnabled = app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled");
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskTrack API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("Frontend");

app.MapGet("/", () => Results.Redirect(swaggerEnabled ? "/swagger" : "/health")).ExcludeFromDescription();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).ExcludeFromDescription();
app.MapControllers();

await DatabaseInitializer.SeedAsync(app.Services);

app.Run();
