using System;
using System.Collections.Generic;
using Jellyfin.Plugin.StorageStats.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.StorageStats;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public static readonly Guid PluginGuid = new("71fd4f14-4e8c-4dfe-97ee-21ffffddfde1");

    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public override string Name => "Storage Availability";

    public override Guid Id => PluginGuid;

    public static Plugin? Instance { get; private set; }

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = "storagestats",
                EmbeddedResourcePath = string.Format("{0}.Configuration.configPage.html", GetType().Namespace)
            }
        };
    }
}
