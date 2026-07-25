using System.Globalization;
using System.Text;
using Jellyfin.Plugin.StorageStats.Configuration;
using Jellyfin.Plugin.StorageStats.Models;

namespace Jellyfin.Plugin.StorageStats.Services;

public static class StorageStatsPageRenderer
{
    private const double BytesPerGiB = 1024.0 * 1024.0 * 1024.0;

    public static string Render(StorageStatsData data, PluginConfiguration config)
    {
        double percentFree = 100.0 - data.PercentUsed;
        string barClass = "bar-normal";
        string? warningLine = null;

        if (percentFree < config.RedThresholdPercent)
        {
            barClass = "bar-red";
            warningLine = "⚠ DANGER — DON'T KILL MY HARD DRIVE! Free up space now.";
        }
        else if (percentFree < config.AmberThresholdPercent)
        {
            barClass = "bar-amber";
            warningLine = "⚠ Getting tight in here — keep an eye on it.";
        }

        string availableGb = FormatGb(data.FreeBytes);
        string totalGb = FormatGb(data.TotalBytes);
        string percentUsedLabel = data.PercentUsed.ToString("0", CultureInfo.InvariantCulture);
        string fillWidth = Math.Clamp(data.PercentUsed, 0, 100).ToString("0.0", CultureInfo.InvariantCulture);

        string capacityLine = BuildCapacityLine(data);

        var warningHtml = warningLine is null
            ? string.Empty
            : $"<div class=\"warning-line\">{warningLine}</div>";

        var capacityHtml = string.IsNullOrEmpty(capacityLine)
            ? string.Empty
            : $"<div class=\"capacity-line\">{capacityLine}</div>";

        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html>\n");
        sb.Append("<html><head>\n");
        sb.Append("<meta charset=\"utf-8\">\n");
        sb.Append("<meta http-equiv=\"refresh\" content=\"300\">\n");
        sb.Append("<title>Storage Availability</title>\n");
        sb.Append("<style>\n");
        sb.Append(Css);
        sb.Append("\n</style>\n");
        sb.Append("</head><body>\n");
        sb.Append("<div class=\"card\">\n");
        sb.Append("<div class=\"title\">Total Storage</div>\n");
        sb.Append($"<div class=\"hero\"><span class=\"hero-value\">{availableGb} GB</span><span class=\"hero-sub\">available of {totalGb} GB</span></div>\n");
        sb.Append("<div class=\"bar-track\">\n");
        sb.Append($"<div class=\"bar-fill {barClass}\" style=\"width:{fillWidth}%\"></div>\n");
        sb.Append("</div>\n");
        sb.Append($"<div class=\"percent-label\">{percentUsedLabel}% used</div>\n");
        sb.Append(capacityHtml);
        sb.Append(warningHtml);
        sb.Append("</div>\n");
        sb.Append("</body></html>\n");

        return sb.ToString();
    }

    private static string BuildCapacityLine(StorageStatsData data)
    {
        if (data.EstimatedEpisodesFit.HasValue && data.EstimatedMoviesFit.HasValue)
        {
            return $"Room for roughly {data.EstimatedEpisodesFit} episodes, or {data.EstimatedMoviesFit} movies";
        }

        if (data.EstimatedEpisodesFit.HasValue)
        {
            return $"Room for roughly {data.EstimatedEpisodesFit} episodes";
        }

        if (data.EstimatedMoviesFit.HasValue)
        {
            return $"Room for roughly {data.EstimatedMoviesFit} movies";
        }

        return string.Empty;
    }

    private static string FormatGb(long bytes)
    {
        return (bytes / BytesPerGiB).ToString("0.0", CultureInfo.InvariantCulture);
    }

    private const string Css = @"
        * { box-sizing: border-box; }
        html, body {
            margin: 0;
            padding: 0;
            background: transparent;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
        }
        .card {
            position: relative;
            width: 90%;
            max-width: 420px;
            margin: 20px auto;
            padding: 28px 24px;
            border-radius: 10px;
            background: linear-gradient(135deg, #4b1a91 0%, #8e1fb0 50%, #c2189d 100%);
            box-shadow: 0 0 12px 2px rgba(194, 24, 157, 0.35), 0 8px 24px rgba(0, 0, 0, 0.5);
            text-align: center;
        }
        .card::before {
            content: '';
            position: absolute;
            top: 10px; left: 10px; right: 10px; bottom: 10px;
            pointer-events: none;
            background-repeat: no-repeat;
            background-image:
                linear-gradient(#00a4dc, #00a4dc), linear-gradient(#00a4dc, #00a4dc),
                linear-gradient(#00a4dc, #00a4dc), linear-gradient(#00a4dc, #00a4dc),
                linear-gradient(#00a4dc, #00a4dc), linear-gradient(#00a4dc, #00a4dc),
                linear-gradient(#00a4dc, #00a4dc), linear-gradient(#00a4dc, #00a4dc);
            background-size:
                2px 20px, 20px 2px,
                2px 20px, 20px 2px,
                2px 20px, 20px 2px,
                2px 20px, 20px 2px;
            background-position:
                top left, top left,
                top right, top right,
                bottom right, bottom right,
                bottom left, bottom left;
            opacity: 0.85;
        }
        .title {
            color: #fff;
            font-weight: 800;
            font-size: 14px;
            letter-spacing: 3px;
            text-transform: uppercase;
            margin-bottom: 18px;
        }
        .hero {
            margin-bottom: 20px;
        }
        .hero-value {
            display: block;
            color: #fff;
            font-weight: 800;
            font-size: 48px;
            line-height: 1.1;
            text-shadow: 0 0 20px rgba(255, 255, 255, 0.55), 0 0 40px rgba(217, 37, 172, 0.6);
        }
        .hero-sub {
            display: block;
            color: #ccc;
            font-size: 14px;
            font-weight: 400;
            margin-top: 4px;
        }
        .bar-track {
            width: 100%;
            height: 22px;
            border-radius: 11px;
            background: rgba(0, 0, 0, 0.45);
            box-shadow: inset 0 2px 5px rgba(0, 0, 0, 0.6);
            overflow: hidden;
        }
        .bar-fill {
            height: 100%;
            border-radius: 11px;
            background: linear-gradient(90deg, #7b2ff7, #d925ac);
            box-shadow: 0 0 14px 2px rgba(217, 37, 172, 0.8);
            transition: none;
        }
        .bar-fill.bar-amber {
            background: linear-gradient(90deg, #ffb300, #ff7a00);
            box-shadow: 0 0 14px 2px rgba(255, 179, 0, 0.8);
        }
        .bar-fill.bar-red {
            background: linear-gradient(90deg, #ff3b3b, #b3001b);
            box-shadow: 0 0 14px 2px rgba(255, 59, 59, 0.85);
        }
        .percent-label {
            color: #aaa;
            font-size: 13px;
            margin-top: 8px;
        }
        .capacity-line {
            color: #aaa;
            font-size: 14px;
            margin-top: 16px;
        }
        .warning-line {
            color: #ffd166;
            font-weight: 700;
            font-size: 13px;
            margin-top: 14px;
        }
    ";
}
