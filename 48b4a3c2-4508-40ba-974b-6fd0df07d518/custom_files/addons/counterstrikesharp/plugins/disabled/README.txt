DISABLED FOLDER - CounterStrikeSharp does NOT auto-load plugins in this folder.

To DISABLE a plugin (example: Parachute):
  1) custom_files/addons/counterstrikesharp/plugins/Parachute
       -> custom_files/addons/counterstrikesharp/plugins/disabled/Parachute
  2) game/csgo/addons/counterstrikesharp/plugins/Parachute
       -> game/csgo/addons/counterstrikesharp/plugins/disabled/Parachute
  3) Restart the server and verify with "css_plugins list" in the console.

To ENABLE it again: do the same two moves in reverse.

WARNING: Always move it in BOTH places!
  - Only in custom_files -> the old copy in game/csgo keeps loading.
  - Only in game/csgo    -> on startup custom_files copies it back into plugins/.

ExamplePlugin is an empty placeholder, feel free to delete it.
