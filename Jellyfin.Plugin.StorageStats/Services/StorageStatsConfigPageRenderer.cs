using System.Net;
using System.Text;
using Jellyfin.Plugin.StorageStats.Configuration;

namespace Jellyfin.Plugin.StorageStats.Services;

/// <summary>
/// Renders the admin settings form server-side, with no client-side JavaScript.
/// The form posts directly to StorageStatsController's anonymous Config endpoint.
/// </summary>
public static class StorageStatsConfigPageRenderer
{
    private const int MinDriveRows = 2;
    private const int MaxDriveRows = 8;

    public static string Render(PluginConfiguration config, bool saved, List<string> availableDrives)
    {
        var selectedDrives = config.SelectedDrives.Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
        int rowCount = Math.Min(MaxDriveRows, Math.Max(MinDriveRows, selectedDrives.Count + 1));

        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html>\n<html><head>\n<meta charset=\"utf-8\">\n<title>Storage Availability settings</title>\n");
        sb.Append("<style>\n").Append(Css).Append("\n</style>\n");
        sb.Append("</head><body>\n");
        sb.Append("<div class=\"card\">\n");
        sb.Append("<div class=\"title\">Storage Availability settings</div>\n");
        sb.Append("<p class=\"description\">These thresholds control when the storage bar on the custom tab switches to a warning colour, based on percent free space remaining.</p>\n");

        if (saved)
        {
            sb.Append("<div class=\"saved-banner\">Settings saved.</div>\n");
        }

        sb.Append("<form method=\"post\" action=\"/StorageStats/Config\">\n");
        sb.Append("<label class=\"field-label\" for=\"AmberThresholdPercent\">Amber threshold (% free)</label>\n");
        sb.Append($"<input class=\"field-input\" id=\"AmberThresholdPercent\" name=\"AmberThresholdPercent\" type=\"number\" min=\"0\" max=\"100\" value=\"{WebUtility.HtmlEncode(config.AmberThresholdPercent.ToString())}\" />\n");
        sb.Append("<div class=\"field-description\">Below this percentage of free space, the bar turns amber.</div>\n");

        sb.Append("<label class=\"field-label\" for=\"RedThresholdPercent\">Red threshold (% free)</label>\n");
        sb.Append($"<input class=\"field-input\" id=\"RedThresholdPercent\" name=\"RedThresholdPercent\" type=\"number\" min=\"0\" max=\"100\" value=\"{WebUtility.HtmlEncode(config.RedThresholdPercent.ToString())}\" />\n");
        sb.Append("<div class=\"field-description\">Below this percentage of free space, the bar turns red and a warning line is shown.</div>\n");

        sb.Append("<div class=\"drives-section\">\n");
        sb.Append("<label class=\"checkbox-label\"><input type=\"checkbox\" name=\"autoDetectDrives\" value=\"true\"");
        if (config.AutoDetectDrives)
        {
            sb.Append(" checked");
        }

        sb.Append(" /> Auto-detect drives from library paths</label>\n");
        sb.Append("<div class=\"field-description\">When checked, the drives below are ignored and the drive(s) your library paths live on are used automatically. Uncheck this to choose specific drives instead.</div>\n");

        sb.Append("<label class=\"field-label\">Drives to monitor</label>\n");

        for (int i = 0; i < rowCount; i++)
        {
            string? current = i < selectedDrives.Count ? selectedDrives[i] : null;
            sb.Append("<select class=\"field-input\" name=\"SelectedDrives\">\n");
            sb.Append("<option value=\"\">-- Not used --</option>\n");

            foreach (var drive in availableDrives)
            {
                string selectedAttr = string.Equals(drive, current, StringComparison.OrdinalIgnoreCase) ? " selected" : string.Empty;
                sb.Append($"<option value=\"{WebUtility.HtmlEncode(drive)}\"{selectedAttr}>{WebUtility.HtmlEncode(drive)}</option>\n");
            }

            sb.Append("</select>\n");
        }

        sb.Append("<div class=\"field-description\">Only used when auto-detect is unchecked above. There's always one spare slot below your current picks — fill it in and save to add another drive.</div>\n");
        sb.Append("</div>\n");

        sb.Append("<button class=\"save-button\" type=\"submit\">Save</button>\n");
        sb.Append("</form>\n");
        sb.Append("</div>\n");
        sb.Append("</body></html>\n");

        return sb.ToString();
    }

    private const string Css = @"
        * { box-sizing: border-box; }
        html, body {
            margin: 0;
            padding: 0;
            background: #101010;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            display: flex;
            justify-content: center;
            padding-top: 40px;
            padding-bottom: 40px;
        }
        .card {
            width: 90%;
            max-width: 480px;
            padding: 28px 24px;
            border-radius: 10px;
            background: linear-gradient(135deg, #4b1a91 0%, #8e1fb0 50%, #c2189d 100%);
            box-shadow: 0 0 12px 2px rgba(194, 24, 157, 0.35), 0 8px 24px rgba(0, 0, 0, 0.5);
        }
        .title {
            color: #fff;
            font-weight: 800;
            font-size: 18px;
            margin-bottom: 12px;
        }
        .description {
            color: #ddd;
            font-size: 13px;
            line-height: 1.5;
            margin: 0 0 20px;
        }
        .saved-banner {
            background: rgba(76, 175, 80, 0.2);
            border: 1px solid #4caf50;
            color: #dfffe0;
            font-weight: 600;
            font-size: 13px;
            padding: 8px 12px;
            border-radius: 6px;
            margin-bottom: 18px;
        }
        .field-label {
            display: block;
            color: #fff;
            font-weight: 600;
            font-size: 13px;
            margin-bottom: 6px;
        }
        .field-input {
            width: 100%;
            padding: 8px 10px;
            font-size: 14px;
            border-radius: 6px;
            border: 1px solid rgba(255, 255, 255, 0.3);
            background: rgba(0, 0, 0, 0.35);
            color: #fff;
            margin-bottom: 8px;
        }
        select.field-input {
            background: #2a1a45;
        }
        .field-description {
            color: #ccc;
            font-size: 12px;
            margin-bottom: 18px;
        }
        .drives-section {
            border-top: 1px solid rgba(255, 255, 255, 0.2);
            padding-top: 18px;
            margin-top: 4px;
        }
        .checkbox-label {
            display: flex;
            align-items: center;
            gap: 8px;
            color: #fff;
            font-weight: 600;
            font-size: 13px;
            margin-bottom: 6px;
        }
        .save-button {
            background: #00a4dc;
            color: #fff;
            border: none;
            border-radius: 6px;
            padding: 10px 20px;
            font-size: 14px;
            font-weight: 700;
            cursor: pointer;
        }
        .save-button:hover {
            background: #0088b8;
        }
    ";
}
