namespace Jellyfin.Plugin.StorageStats.Models;

public class DriveOption
{
    public string Path { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public long TotalBytes { get; set; }

    public long FreeBytes { get; set; }

    public bool IsSelected { get; set; }

    public bool IsAutoDetected { get; set; }
}
