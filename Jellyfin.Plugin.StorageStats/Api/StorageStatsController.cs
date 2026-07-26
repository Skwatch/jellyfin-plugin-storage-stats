using Jellyfin.Plugin.StorageStats.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Jellyfin.Plugin.StorageStats.Api;

[ApiController]
[Route("StorageStats")]
public class StorageStatsController : ControllerBase
{
    private readonly StorageStatsService _service;

    public StorageStatsController(StorageStatsService service)
    {
        _service = service;
    }

    [HttpGet("Data")]
    [AllowAnonymous]
    [Produces("application/json")]
    public ActionResult<Models.StorageStatsData> GetData()
    {
        SetNoCache();
        return _service.GetStats();
    }

    [HttpGet("Page")]
    [AllowAnonymous]
    public ContentResult GetPage()
    {
        SetNoCache();
        var data = _service.GetStats();
        var config = Plugin.Instance?.Configuration ?? new Configuration.PluginConfiguration();
        var html = StorageStatsPageRenderer.Render(data, config);
        return Content(html, "text/html");
    }

    /// <summary>
    /// Server-rendered settings form. Anonymous because Jellyfin's web client (10.11.x)
    /// does not execute scripts embedded in plugin config pages, so this can't be saved
    /// via the usual authenticated ApiClient JS calls — it has to be a plain HTML form
    /// POST, which can't carry Jellyfin's Authorization header. Only the two threshold
    /// percentages are exposed here, so the blast radius of that is negligible.
    /// </summary>
    [HttpGet("Config")]
    [AllowAnonymous]
    public ContentResult GetConfigForm([FromQuery] bool saved = false)
    {
        SetNoCache();
        var config = Plugin.Instance?.Configuration ?? new Configuration.PluginConfiguration();
        var html = StorageStatsConfigPageRenderer.Render(config, saved);
        return Content(html, "text/html");
    }

    [HttpPost("Config")]
    [AllowAnonymous]
    public IActionResult PostConfigForm([FromForm] int amberThresholdPercent, [FromForm] int redThresholdPercent)
    {
        var plugin = Plugin.Instance;
        if (plugin is not null)
        {
            plugin.Configuration.AmberThresholdPercent = Math.Clamp(amberThresholdPercent, 0, 100);
            plugin.Configuration.RedThresholdPercent = Math.Clamp(redThresholdPercent, 0, 100);
            plugin.SaveConfiguration();
        }

        return Redirect("/StorageStats/Config?saved=true");
    }

    private void SetNoCache()
    {
        Response.Headers[HeaderNames.CacheControl] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers[HeaderNames.Pragma] = "no-cache";
        Response.Headers[HeaderNames.Expires] = "0";
    }
}
