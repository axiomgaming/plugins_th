# Leche plugins!

Just copy into your THUD plugins directory the contents of the 'plugins' folder
here. To prevent conflicts the easiest thing to do is back up your old plugins
folder, rename it to 'plugins_backup_YYYYMMDD' or something like that,
then copy this whole plugins folder in fresh.

### Disabling

Remove a plugin's .cs file will remove it entirely, but you can also typically
disable a plugin by finding its .cs file, then editing to change the one line
that reads 'Enabled = true;' to 'Enabled = false;'
