[RU](README.md)
<h1 align="center">jellyfin-plugin-shikimori</h1>

# About

This plugin adds the metadata provider for [Shikimori](https://shikimori.io)

# Installation

This fork is published as **Shikimori Next** in the
[jellyfin-plugins](https://github.com/DevLn737/jellyfin-plugins) repository.

1. Go to Jellyfin's dashboard -> Plugins -> Repositories.
2. Add new repository with url: `https://devln737.github.io/jellyfin-plugins/manifest.json`.
3. Install Shikimori Next from the catalogue.

# Build

1. Make sure that you have [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) installed
2. Clone this repository
3. Build plugin with following command
```bash
dotnet publish Jellyfin.Plugin.Shikimori/Jellyfin.Plugin.Shikimori.csproj --configuration Release --output artifacts/publish
```
4. Create folder `shikimori` in jellyfin's plugins directory (it's located in [data directory](https://jellyfin.org/docs/general/administration/configuration/#data-directory))
5. Move `Jellyfin.Plugin.Shikimori.dll`, `Newtonsoft.Json.dll`, and `meta.json` from `artifacts/publish` to the directory created in the previous step. Other published DLLs belong to the host server and must not be packaged with the plugin.

# Shikimori Next and Jellyfin 12.2

Version 6.1.0.0 targets Jellyfin 12.2 and .NET 10. Since 6.1.0.0 the plugin has
its own identity, **Shikimori Next** with GUID `42730b4b-b30e-45dc-bee0-8f351eadb690`,
so it never collides with the original Shikimori plugin in a server's catalogue.
What a library depends on stays the same: the metadata provider is still called
`Shikimori`, items keep their `Shikimori` provider IDs, and settings stay in
`plugins/configurations/Jellyfin.Plugin.Shikimori.xml`.

Moving from the original plugin (or this fork's 6.0.0.0 and older): uninstall
Shikimori under Dashboard -> Plugins, restart Jellyfin, install Shikimori Next
from the catalogue, restart again. Library settings, saved IDs and the plugin
settings carry over. Do not run both at once: they register the same provider.

Verified test command:

```bash
dotnet test Jellyfin.Plugin.Shikimori.sln --configuration Release
```

Regression tests cover old configuration XML, provider ID matching, plugin
identity and configuration location, and Movie/Series metadata and searches that
retain TMDb, TVDB, AniList, AniDB, IMDb, and custom provider IDs.

# Releasing

The best way to release is to use
[JPRM](https://github.com/oddstr13/jellyfin-plugin-repository-manager), which
can package the plugin and add it to the `manifest.json`.
