# Leche plugins!

Just copy into your THUD plugins directory the contents of the `plugins` folder
here. To prevent conflicts the easiest thing to do is back up your old plugins
folder, rename it to `plugins_backup_YYYYMMDD` or something like that,
then copy this whole plugins folder in fresh.

### Disabling

Remove a plugin's `.cs` file will remove it entirely, but you can also typically
disable a plugin by finding its `.cs` file, then editing to change the one line
that reads `Enabled = true;` to `Enabled = false;`

### Changelog

2020-03-29: Cleaned up extant plugins, minor tweaks to Spirit Barrage-related plugins.
- Fixed files to be named `.cs` (not `.txt`), reenabling a handful of
plugins that weren't working before (Shrine announcer, mob group lines, players
skill bars).
- SB circles in pink, wider, for better visibility.
- SB countdown icon larger, moved slightly.

2020-03-28: Created git repo.
- Player health arcs showing health/resource/shield at feet.
- Materials widget at top right.
