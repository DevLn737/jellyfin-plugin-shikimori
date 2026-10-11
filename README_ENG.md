[RU](README.md)
<h1 align="center">jellyfin-plugin-shikimori</h1>

# About

This plugin adds the metadata provider for [Shikimori](https://shikimori.io)

# Installation

1. Go to Jellyfin's dashboard -> Plugins -> Repositories.
2. Add new repository with url: `https://raw.githubusercontent.com/DevLn737/jellyfin-plugin-shikimori/main/manifest.json`.
3. Install Shikimori plugin from the catalogue.

# Build

1. Make sure that you have [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) installed
2. Clone this repository
3. Build plugin with following command
```bash
dotnet publish Jellyfin.Plugin.Shikimori/Jellyfin.Plugin.Shikimori.csproj --configuration Release --output artifacts/publish
```
4. Create folder `shikimori` in jellyfin's plugins directory (it's located in [data directory](https://jellyfin.org/docs/general/administration/configuration/#data-directory))
5. Move `Jellyfin.Plugin.Shikimori.dll`, `Newtonsoft.Json.dll`, and `meta.json` from `artifacts/publish` to the directory created in the previous step. Other published DLLs belong to the host server and must not be packaged with the plugin.

# Jellyfin 12.2 compatibility

Version 6.0.0.0 targets Jellyfin 12.2 and .NET 10. It preserves the plugin GUID,
saved configuration keys, `Jellyfin.Plugin.Shikimori.xml` filename, ID matching,
and preservation of external provider IDs introduced in 5.0.1.0. Stop Jellyfin
before replacing binaries, keep the XML file under `plugins/configurations`, and
remove the old Shikimori binary directory.

The public `manifest.json` lists only published releases. Install a verified local
6.0.0.0 package manually until its ZIP has been published and its download address
and checksum have been verified before updating the public catalogue.

Verified test command:

```bash
dotnet test Jellyfin.Plugin.Shikimori.sln --configuration Release
```

Regression tests cover old configuration XML, provider ID matching, stable plugin
identity and configuration location, and Movie/Series metadata and searches that
retain TMDb, TVDB, AniList, AniDB, IMDb, and custom provider IDs.

# Releasing

The best way to release is to use
[JPRM](https://github.com/oddstr13/jellyfin-plugin-repository-manager), which
can package the plugin and add it to the `manifest.json`.
