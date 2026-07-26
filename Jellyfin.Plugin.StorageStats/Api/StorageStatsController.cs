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
    /// Server-rendered settings form, no client-side JavaScript. Anonymous because a plain
    /// HTML form POST can't carry Jellyfin's Authorization header. Only the thresholds and
    /// drive selection are exposed here, so the blast radius of that is negligible.
    /// </summary>
    [HttpGet("Config")]
    [AllowAnonymous]
    public ContentResult GetConfigForm([FromQuery] bool saved = false)
    {
        SetNoCache();
        var config = Plugin.Instance?.Configuration ?? new Configuration.PluginConfiguration();
        var availableDrives = _service.GetAvailableFixedDriveRoots();
        var html = StorageStatsConfigPageRenderer.Render(config, saved, availableDrives);
        return Content(html, "text/html");
    }

    [HttpPost("Config")]
    [AllowAnonymous]
    public IActionResult PostConfigForm(
        [FromForm] int amberThresholdPercent,
        [FromForm] int redThresholdPercent,
        [FromForm] bool autoDetectDrives,
        [FromForm] List<string>? selectedDrives)
    {
        var plugin = Plugin.Instance;
        if (plugin is not null)
        {
            plugin.Configuration.AmberThresholdPercent = Math.Clamp(amberThresholdPercent, 0, 100);
            plugin.Configuration.RedThresholdPercent = Math.Clamp(redThresholdPercent, 0, 100);
            plugin.Configuration.AutoDetectDrives = autoDetectDrives;
            plugin.Configuration.SelectedDrives = (selectedDrives ?? new List<string>())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
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
