# Dual Theme Frontend Update

This frontend keeps the existing ASP.NET MVC functionality and changes only the UI layer.

## Themes

- `default`: teal + white clinical theme.
- `vampire`: spooky supernatural medical theme with visible blood stains, top spider webs, hanging spiders, bats, red glow, and dark UI surfaces.

## Main files changed

- `Views/Shared/_Layout.cshtml`
  - Added SVG definitions for webs, bats, spiders, blood splatter clip paths.
  - Added the background decoration layer.
  - Updated the theme toggle button.

- `wwwroot/css/site.css`
  - Added theme variables and overrides for `default` and `vampire`.
  - Added visible top spider webs, blood splashes/drips, bats, hanging spiders, and spooky red glow.
  - Preserved original layout classes and component behavior.

- `wwwroot/js/site.js`
  - Changed theme toggle from light/dark to default/vampire.
  - Saves user preference in `localStorage` as `hms-theme`.
  - Keeps the existing sidebar, photo preview, file picker preview, and password toggle behavior.

## Note

The environment used to package this update does not have the .NET SDK installed, so `dotnet build` could not be run here. The code changes are limited to Razor/CSS/JS UI files and should not affect backend functionality.
