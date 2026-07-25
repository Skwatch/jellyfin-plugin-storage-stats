# Storage Availability

A Jellyfin plugin that shows available disk space and rough library capacity
estimates through a custom tab, styled to match a violet/magenta
[Custom Tabs](https://github.com/IAmParadox27/jellyfin-plugin-custom-tabs)
dashboard.

It server-renders a complete HTML page with the numbers already baked into
the markup — no client-side JavaScript, no fetch calls, no DOM manipulation.
This is deliberate: pages injected via Custom Tabs are set through
`innerHTML`, which does not execute `<script>` tags, so any client-side
rendering would silently fail. Auto-refresh is done with
`<meta http-equiv="refresh">` instead of a JS timer.

## What it shows

- Free / used / total space for the drive(s) Jellyfin's libraries live on
  (auto-detected, or explicitly chosen in the plugin settings)
- A gradient progress bar that turns amber/red as free space drops below
  configurable thresholds
- A rough estimate of how many more episodes or movies will fit, based on
  the median episode/movie size in your library (median, not mean, so a
  handful of 4K remuxes don't skew the estimate)

## Requirements

- Jellyfin **10.11.x** (targets plugin ABI `10.11.0.0`, built against
  `net9.0`)

## Installation (via the Jellyfin plugin repository)

1. In Jellyfin, go to **Dashboard → Plugins → Repositories**.
2. Add a new repository with this manifest URL:

   ```
   https://raw.githubusercontent.com/Skwatch/jellyfin-plugin-storage-stats/main/manifest.json
   ```

3. Go to **Dashboard → Plugins → Catalog**, find **Storage Availability**
   under General, and install it.
4. Restart Jellyfin.

No manual file copying is required — updates show up in the Catalog like
any other plugin.

## Configuration

Under **Dashboard → Plugins → Storage Availability**:

- **Amber threshold (% free)** — default 20. Below this, the bar turns
  amber.
- **Red threshold (% free)** — default 10. Below this, the bar turns red
  and a warning line appears.
- **Drives to monitor** — a checklist of fixed drives detected on the
  server. Leave everything unchecked to auto-detect drives from your
  library paths instead (the default, and normally all you need on a
  single-drive server). Check specific drives if you want to include a
  drive that isn't part of any library, or exclude one that is.

## Adding the tab (Custom Tabs plugin)

This plugin only serves the page — you still need the
[Custom Tabs](https://github.com/IAmParadox27/jellyfin-plugin-custom-tabs)
plugin (and its File Transformation dependency) installed to actually
place it in Jellyfin's web UI as a tab. See `CUSTOM_TABS_SNIPPET.html` in
this repo for the exact HTML to paste into Custom Tabs' settings.

## Endpoints

- `GET /StorageStats/Data` — JSON diagnostic endpoint (anonymous)
- `GET /StorageStats/Page` — the rendered HTML page (anonymous)
- `GET /StorageStats/Drives` — lists fixed drives for the config page's
  drive picker (authenticated; used by the dashboard only)

`/Data` and `/Page` are intentionally anonymous: an iframe navigation
carries no `Authorization` header, so an authenticated endpoint would
render a blank frame. The only data exposed is free disk space and
aggregate library size — nothing else Jellyfin knows about the server or
its libraries.

## Building from source

Requires the .NET 9 SDK.

```bash
dotnet build Jellyfin.Plugin.StorageStats/Jellyfin.Plugin.StorageStats.csproj -c Release
```

The plugin DLL is at
`Jellyfin.Plugin.StorageStats/bin/Release/net9.0/Jellyfin.Plugin.StorageStats.dll`.
Copy it into your Jellyfin `plugins` folder (on Windows,
`%ProgramData%\Jellyfin\Server\plugins\Storage Availability\`) and restart
Jellyfin.

## Releasing

Tag a commit on `main` as `vX.Y.Z.W` (a full 4-part version, e.g.
`v1.0.0.0`) and push the tag. The `Release` GitHub Actions workflow will
build, zip just the plugin DLL, compute its MD5 checksum, publish a GitHub
Release with the zip attached, and append the new version to
`manifest.json` on `main`.

## License

GPLv3 — see [LICENSE](LICENSE).
