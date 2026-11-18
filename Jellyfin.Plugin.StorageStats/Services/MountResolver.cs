namespace Jellyfin.Plugin.StorageStats.Services;

/// <summary>
/// Resolves which mount point / drive a path lives on, by longest-prefix match.
/// <c>Path.GetPathRoot</c> is not enough on Linux (it is always "/"), which would make every
/// library path resolve to the container's root filesystem instead of the disk it is stored on.
/// </summary>
public static class MountResolver
{
    public static string? PickContainingRoot(string path, IEnumerable<string> roots, StringComparison comparison)
    {
        string? best = null;
        int bestLength = -1;
        var normalizedPath = WithTrailingSeparator(path);

        foreach (var root in roots)
        {
            if (string.IsNullOrEmpty(root))
            {
                continue;
            }

            var normalizedRoot = WithTrailingSeparator(root);
            if (normalizedPath.StartsWith(normalizedRoot, comparison) && normalizedRoot.Length > bestLength)
            {
                best = root;
                bestLength = normalizedRoot.Length;
            }
        }

        return best;
    }

    private static string WithTrailingSeparator(string value)
    {
        if (value.EndsWith('/') || value.EndsWith('\\'))
        {
            return value;
        }

        return value + (value.Contains('\\') ? '\\' : '/');
    }
}
