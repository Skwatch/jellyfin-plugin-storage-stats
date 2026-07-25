using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.StorageStats.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        AmberThresholdPercent = 20;
        RedThresholdPercent = 10;
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
    /// Gets or sets the drive roots (e.g. "D:\") the admin has explicitly chosen to monitor.
    /// When empty, the plugin falls back to auto-detecting volumes from library paths.
    /// </summary>
    public List<string> SelectedDrives { get; set; }
}
