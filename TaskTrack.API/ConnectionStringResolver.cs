using Npgsql;

namespace TaskTrack.API;

/// <summary>
/// Reads the PostgreSQL connection string. Priority:
///   1. DATABASE_URL environment variable (Render / production) - accepts a postgres:// URL or a key=value string
///   2. ConnectionStrings:DefaultConnection from appsettings.json (local development)
/// </summary>
public static class ConnectionStringResolver
{
    public static string Resolve(IConfiguration configuration)
    {
        var raw = configuration["DATABASE_URL"];
        if (string.IsNullOrWhiteSpace(raw))
        {
            raw = configuration.GetConnectionString("DefaultConnection");
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException(
                "No database connection string found. Set DATABASE_URL or ConnectionStrings:DefaultConnection.");
        }

        if (raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return FromUrl(raw);
        }

        return raw;
    }

    private static string FromUrl(string url)
    {
        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
            // Render's external URL requires SSL; the internal one does not. "Prefer" works for both.
            SslMode = SslMode.Prefer,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }
}
