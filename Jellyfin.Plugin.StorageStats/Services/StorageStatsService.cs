using Jellyfin.Data.Enums;
using Jellyfin.Plugin.StorageStats.Models;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.StorageStats.Services;

public class StorageStatsService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);
    private const int MinimumSampleSize = 5;

    private readonly ILibraryManager _libraryManager;
    private readonly IApplicationPaths _applicationPaths;
    private readonly ILogger<StorageStatsService> _logger;

    private StorageStatsData? _cachedData;
    private DateTime _cachedAt;
    private readonly object _cacheLock = new();

    public StorageStatsService(ILibraryManager libraryManager, IApplicationPaths applicationPaths, ILogger<StorageStatsService> logger)
    {
        _libraryManager = libraryManager;
        _applicationPaths = applicationPaths;
        _logger = logger;
    }

    public StorageStatsData GetStats()
    {
        lock (_cacheLock)
        {
            if (_cachedData is not null && DateTime.UtcNow - _cachedAt < CacheDuration)
            {
                return _cachedData;
            }

            var data = Compute();
            _cachedData = data;
            _cachedAt = DateTime.UtcNow;
            return data;
        }
    }

    private StorageStatsData Compute()
    {
        var volumes = GetVolumes();

        long totalBytes = volumes.Sum(v => v.TotalBytes);
        long freeBytes = volumes.Sum(v => v.FreeBytes);
        long usedBytes = totalBytes - freeBytes;
        double percentUsed = totalBytes > 0 ? (double)usedBytes / totalBytes * 100.0 : 0;

        var episodeSizes = GetSizes(BaseItemKind.Episode);
        var movieSizes = GetSizes(BaseItemKind.Movie);

        long librarySizeBytes = episodeSizes.Sum() + movieSizes.Sum();
        int usableItemCount = episodeSizes.Count + movieSizes.Count;

        long? medianEpisodeSize = episodeSizes.Count >= MinimumSampleSize ? Median(episodeSizes) : null;
        long? medianMovieSize = movieSizes.Count >= MinimumSampleSize ? Median(movieSizes) : null;

        long? estimatedEpisodesFit = medianEpisodeSize is > 0 ? freeBytes / medianEpisodeSize.Value : null;
        long? estimatedMoviesFit = medianMovieSize is > 0 ? freeBytes / medianMovieSize.Value : null;

        return new StorageStatsData
        {
            TotalBytes = totalBytes,
            FreeBytes = freeBytes,
            UsedBytes = usedBytes,
            PercentUsed = percentUsed,
            Volumes = volumes,
            LibrarySizeBytes = librarySizeBytes,
            EpisodeCount = episodeSizes.Count,
            MovieCount = movieSizes.Count,
            MedianEpisodeSizeBytes = medianEpisodeSize,
            MedianMovieSizeBytes = medianMovieSize,
            EstimatedEpisodesFit = estimatedEpisodesFit,
            EstimatedMoviesFit = estimatedMoviesFit,
            ItemsWithUsableSizeCount = usableItemCount
        };
    }

    private List<VolumeInfo> GetVolumes()
    {
        var config = Plugin.Instance?.Configuration;
        var manualDrives = config?.SelectedDrives?.Where(d => !string.IsNullOrWhiteSpace(d)).ToList() ?? new List<string>();

        var roots = config is not null && !config.AutoDetectDrives && manualDrives.Count > 0
            ? new HashSet<string>(manualDrives, StringComparer.OrdinalIgnoreCase)
            : GetAutoDetectedRoots();

        if (roots.Count == 0)
        {
            var fallbackRoot = TryGetDriveRoot(_applicationPaths.ProgramDataPath);
            if (fallbackRoot is not null)
            {
                roots.Add(fallbackRoot);
            }
        }

        var volumes = new List<VolumeInfo>();
        foreach (var root in roots)
        {
            try
            {
                var drive = new DriveInfo(root);
                volumes.Add(new VolumeInfo
                {
                    Path = root,
                    TotalBytes = drive.TotalSize,
                    FreeBytes = drive.AvailableFreeSpace
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read DriveInfo for {Root}", root);
            }
        }

        return volumes;
    }

    private HashSet<string> GetAutoDetectedRoots()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var internalDataPath = NormalizeForPrefixCheck(_applicationPaths.DataPath);

        try
        {
            foreach (var folder in _libraryManager.GetVirtualFolders())
            {
                foreach (var location in folder.Locations)
                {
                    // Jellyfin auto-creates internal virtual folders (e.g. "Collections", "Playlists")
                    // stored under its own data directory; these aren't real media libraries and
                    // shouldn't count toward which drives the user's media actually lives on.
                    if (IsUnderPath(location, internalDataPath))
                    {
                        continue;
                    }

                    var root = TryGetDriveRoot(location);
                    if (root is not null)
                    {
                        roots.Add(root);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to enumerate library virtual folders for drive detection");
        }

        return roots;
    }

    private static bool IsUnderPath(string candidate, string? basePath)
    {
        if (string.IsNullOrEmpty(basePath))
        {
            return false;
        }

        var normalizedCandidate = NormalizeForPrefixCheck(candidate);
        return normalizedCandidate.StartsWith(basePath, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeForPrefixCheck(string path)
    {
        try
        {
            var full = Path.GetFullPath(path);
            return full.EndsWith(Path.DirectorySeparatorChar) ? full : full + Path.DirectorySeparatorChar;
        }
        catch
        {
            return path;
        }
    }

    /// <summary>
    /// Lists fixed, ready drives on the machine for the settings page's drive dropdowns.
    /// </summary>
    public List<string> GetAvailableFixedDriveRoots()
    {
        var roots = new List<string>();

        foreach (var drive in DriveInfo.GetDrives())
        {
            try
            {
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed)
                {
                    continue;
                }

                roots.Add(drive.RootDirectory.FullName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read DriveInfo while listing available drives for {DriveName}", drive.Name);
            }
        }

        return roots;
    }

    private static string? TryGetDriveRoot(string path)
    {
        try
        {
            return Path.GetPathRoot(Path.GetFullPath(path));
        }
        catch
        {
            return null;
        }
    }

    private List<long> GetSizes(BaseItemKind kind)
    {
        try
        {
            var query = new InternalItemsQuery
            {
                IncludeItemTypes = new[] { kind },
                Recursive = true,
                IsVirtualItem = false
            };

            var items = _libraryManager.GetItemList(query);

            return items
                .Select(i => i.Size)
                .Where(size => size.HasValue && size.Value > 0)
                .Select(size => size!.Value)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to query library items of type {Kind}", kind);
            return new List<long>();
        }
    }

    private static long Median(List<long> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        int mid = sorted.Count / 2;
        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2
            : sorted[mid];
    }
}
