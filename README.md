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

- Free / used / total space for the drive(s) Jellyfin's libraries live on,
  auto-detected from your library paths. If your libraries span more than
  one drive, each is shown as its own block instead of a summed total.
- A gradient progress bar that turns amber/red as free space drops below
  configurable thresholds
- A rough estimate of how many more episodes or movies will fit, based on
  the median episode/movie size in your library (median, not mean, so a
  handful of 4K remuxes don't skew the estimate)

## Requirements

- Jellyfin **10.11.x** (targets plugin ABI `10.11.0.0`, built against
  `net9.0`)
- [Custom Tabs](https://github.com/IAmParadox27/jellyfin-plugin-custom-tabs)
  (and its File Transformation dependency), installed and working. This
  plugin only serves the data and the page — Custom Tabs is what actually
  puts a tab in Jellyfin's web UI. Without it, the endpoints below still
  work, but there's nowhere in the Jellyfin UI to see them.

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

Clicking **Storage Availability** under Dashboard → Plugins redirects to a
plain, server-rendered settings form (no client-side JavaScript). Saving
does a full page reload rather than an in-place update. This is a
deliberate design choice, not a workaround for broken script execution —
Jellyfin's dashboard does run plugin config page scripts fine as long as
the content is wrapped in the standard
`<div class="page type-interior pluginConfigurationPage">` container; a
plain form was just simpler than reintroducing the authenticated
`ApiClient` JS pattern.

- **Amber threshold (% free)** — default 20. Below this, the bar turns
  amber.
- **Red threshold (% free)** — default 10. Below this, the bar turns red
  and a warning line appears.

Drives are always auto-detected from your library paths; there's no manual
drive picker.

## Adding the tab (Custom Tabs plugin)

This plugin only serves the page — you still need the
[Custom Tabs](https://github.com/IAmParadox27/jellyfin-plugin-custom-tabs)
plugin (and its File Transformation dependency) installed to actually
place it in Jellyfin's web UI as a tab. See `CUSTOM_TABS_SNIPPET.html` in
this repo for the exact HTML to paste into Custom Tabs' settings.

**Known quirk:** on at least one setup, opening the tab from the Jellyfin
server's own machine via `http://localhost:8096/...` showed a blank card,
while the same tab displayed correctly for remote users connecting through
a public hostname (e.g. a DuckDNS address). If your tab looks blank, try
it from a different device or a non-localhost address before assuming
something's broken.

## Endpoints

- `GET /StorageStats/Data` — JSON diagnostic endpoint (anonymous)
- `GET /StorageStats/Page` — the rendered HTML page (anonymous)
- `GET /StorageStats/Config` — the settings form (anonymous)
- `POST /StorageStats/Config` — saves the two thresholds (anonymous)

All four are anonymous. `/Data` and `/Page` need to be: an iframe
navigation carries no `Authorization` header, so an authenticated endpoint
would render a blank frame. `/Config`'s GET/POST are anonymous for a
different reason — Jellyfin's web client doesn't execute the JavaScript a
plugin config page would normally use to call the authenticated
configuration API, so this had to become a plain HTML form post instead,
which can't carry that header either. In all cases the only thing exposed
or mutable is free disk space, aggregate library size, and two threshold
percentages — nothing else Jellyfin knows about the server or its
libraries.

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
