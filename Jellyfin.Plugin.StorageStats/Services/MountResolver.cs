using Jellyfin.Plugin.StorageStats.Models;

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

    /// <summary>
    /// Free-space readings of two mounts of the same filesystem are taken moments apart, so
    /// they can differ by a few bytes if anything writes in between.
    /// </summary>
    private const long FreeSpaceToleranceBytes = 16L * 1024 * 1024;

    /// <summary>
    /// Merges mounts that are really the same filesystem. Several bind mounts of one
    /// filesystem (typically Unraid, where /data/movies and /data/tvshows are both views of the
    /// same array) report identical totals and free space; summing them would double-count.
    /// Distinct disks essentially never match on type, exact size and free space together.
    /// </summary>
    public static List<VolumeInfo> MergeSameFilesystem(IEnumerable<VolumeInfo> volumes)
    {
        var merged = new List<VolumeInfo>();

        foreach (var volume in volumes.OrderBy(v => v.Path, StringComparer.Ordinal))
        {
            var existing = merged.FirstOrDefault(m =>
                string.Equals(m.Format, volume.Format, StringComparison.OrdinalIgnoreCase)
                && m.TotalBytes == volume.TotalBytes
                && Math.Abs(m.FreeBytes - volume.FreeBytes) <= FreeSpaceToleranceBytes);

            if (existing is null)
            {
                merged.Add(volume);
            }
            else
            {
                existing.AlsoCoveredPaths.Add(volume.Path);
            }
        }

        return merged;
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
