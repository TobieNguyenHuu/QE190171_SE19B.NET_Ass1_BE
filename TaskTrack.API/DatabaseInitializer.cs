using Microsoft.EntityFrameworkCore;
using TaskTrack.Repo.Models;

namespace TaskTrack.API;

public static class DatabaseInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TaskManagementContext>();

        try
        {
            var conn = context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            // Check if Department table exists
            bool tableExists = false;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'Department');";
                var result = await cmd.ExecuteScalarAsync();
                tableExists = result is bool b && b;
            }

            if (!tableExists)
            {
                Console.WriteLine("--> Database tables not found. Seeding database from SQL script...");
                
                // Locate TaskManagementDB_Postgres.sql
                string[] potentialPaths = [
                    Path.Combine(AppContext.BaseDirectory, "db", "TaskManagementDB_Postgres.sql"),
                    Path.Combine(Directory.GetCurrentDirectory(), "db", "TaskManagementDB_Postgres.sql"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "db", "TaskManagementDB_Postgres.sql"),
                    @"C:\Users\DELL\Downloads\QE190171_SE19B.NET_Ass1_BE\db\TaskManagementDB_Postgres.sql"
                ];

                string? sqlPath = potentialPaths.FirstOrDefault(File.Exists);
                if (sqlPath != null)
                {
                    string sql = await File.ReadAllTextAsync(sqlPath);
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    await cmd.ExecuteNonQueryAsync();
                    Console.WriteLine("--> Database created and seeded successfully on Render PostgreSQL!");
                }
                else
                {
                    Console.WriteLine("--> Could not locate TaskManagementDB_Postgres.sql script.");
                }
            }
            else
            {
                Console.WriteLine("--> Database already initialized with tables.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Note on Database Initialization: {ex.Message}");
        }
    }
}
