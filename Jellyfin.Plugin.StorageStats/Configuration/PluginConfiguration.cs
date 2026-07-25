using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.StorageStats.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        AmberThresholdPercent = 20;
        RedThresholdPercent = 10;
    }

    /// <summary>
    /// Gets or sets the percent-free threshold below which the bar turns amber.
    /// </summary>
    public int AmberThresholdPercent { get; set; }

    /// <summary>
    /// Gets or sets the percent-free threshold below which the bar turns red.
    /// </summary>
    public int RedThresholdPercent { get; set; }
}
