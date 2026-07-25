namespace Jellyfin.Plugin.StorageStats.Models;

public class VolumeInfo
{
    public string Path { get; set; } = string.Empty;

    public long TotalBytes { get; set; }

    public long FreeBytes { get; set; }
}

public class StorageStatsData
{
    public long TotalBytes { get; set; }

    public long FreeBytes { get; set; }

    public long UsedBytes { get; set; }

    public double PercentUsed { get; set; }

    public List<VolumeInfo> Volumes { get; set; } = new();

    public long LibrarySizeBytes { get; set; }

    public int EpisodeCount { get; set; }

    public int MovieCount { get; set; }

    public long? MedianEpisodeSizeBytes { get; set; }

    public long? MedianMovieSizeBytes { get; set; }

    public long? EstimatedEpisodesFit { get; set; }

    public long? EstimatedMoviesFit { get; set; }

    public int ItemsWithUsableSizeCount { get; set; }
}
