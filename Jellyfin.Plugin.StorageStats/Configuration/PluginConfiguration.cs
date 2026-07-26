using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.StorageStats.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        AmberThresholdPercent = 20;
        RedThresholdPercent = 10;
        AutoDetectDrives = true;
        SelectedDrives = new List<string>();
    }

    /// <summary>
    /// Gets or sets the percent-free threshold below which the bar turns amber.
    /// </summary>
    public int AmberThresholdPercent { get; set; }

    /// <summary>
    /// Gets or sets the percent-free threshold below which the bar turns red.
    /// </summary>
    public int RedThresholdPercent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether drives should be auto-detected from library
    /// paths. When false, <see cref="SelectedDrives"/> is used instead.
    /// </summary>
    public bool AutoDetectDrives { get; set; }

    /// <summary>
    /// Gets or sets the drive roots (e.g. "E:\") the admin has explicitly chosen to monitor.
    /// Only used when <see cref="AutoDetectDrives"/> is false.
    /// </summary>
    public List<string> SelectedDrives { get; set; }
}
