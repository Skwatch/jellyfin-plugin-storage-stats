using Jellyfin.Plugin.StorageStats.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        return _service.GetStats();
    }

    [HttpGet("Page")]
    [AllowAnonymous]
    public ContentResult GetPage()
    {
        var data = _service.GetStats();
        var config = Plugin.Instance?.Configuration ?? new Configuration.PluginConfiguration();
        var html = StorageStatsPageRenderer.Render(data, config);
        return Content(html, "text/html");
    }

    /// <summary>
    /// Lists fixed drives on the machine, for the admin config page's drive picker.
    /// Requires an authenticated session (loaded only from the Jellyfin dashboard), unlike
    /// Data/Page which must be anonymous for iframe embedding.
    /// </summary>
    [HttpGet("Drives")]
    [Produces("application/json")]
    public ActionResult<List<Models.DriveOption>> GetDrives()
    {
        return _service.GetDriveOptions();
    }
}
