namespace TaskTrack.Repo.Repositories;

internal static class LikeHelper
{
    /// <summary>Builds a "contains" pattern for ILIKE, escaping the wildcard characters.</summary>
    public static string Contains(string value)
    {
        var escaped = value.Trim()
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
        return $"%{escaped}%";
    }
}
