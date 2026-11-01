# GameModeManager: redo my changes by hand

Use this after downloading a new GameModeManager version, when `GameModeManager.patch` does not apply
or you want to do it yourself. Background and reasons: `README.md#gamemodemanager`.

2 changes, 2 files. Search for the "Find" text with Ctrl+F (line numbers change between versions).

## 0. Before you start

1. Download the new source (zip or `git clone https://github.com/nickj609/GameModeManager`).
2. Check the changelog: if upstream now has a "keep the map when nobody voted" option, or `ChangeMap()` already
   tries the workshop id before `IsMapValid`, skip that change.
3. Rename the old `build/gmm-src` (e.g. `gmm-src-old`) and put the new source in `build/gmm-src`.
   Delete the old folder at the end, when everything works.

## Change 1: replay the current map when nobody voted

### 1a. `GameModeManager/Config.cs`

Find (inside the RTV settings class):

```csharp
public int TriggerSecondsBeforeEnd { get; set; } = 120;
```

Add right below it:

```csharp
        // [LOCAL PATCH] When the match ends without a vote, keep the current map (same mode) instead of a random option.
        // Needs mp_match_end_restart 1 so the engine restarts the match.
        public bool RepeatCurrentMapWhenNoVote { get; set; } = false;
```

### 1b. `GameModeManager/CrossCutting/ServerManager.cs`, method `TriggerRotation()`

Find the comment:

```csharp
// If no vote occurred, pick a random option based on RTV settings
```

It is the first line inside an `else { ... }` block. Directly above that `else`, add a new branch, so it reads:

```csharp
                // [LOCAL PATCH] No vote happened: keep the current map (and mode) instead of a random option.
                // Do not change the map here: workshop maps often do not match their gamemodes_server.txt name,
                // so CurrentMap has no workshop id and changing to it fails. The engine restarts the match
                // on the same map instead (mp_match_end_restart 1 in cfg/settings/map_voting.cfg).
                else if (_config.RTV.RepeatCurrentMapWhenNoVote)
                {
                    _logger.LogInformation("RTV: Match ended without a vote, the engine restarts it on the same map.");
                }
                else
                {
                    // If no vote occurred, pick a random option based on RTV settings
```

The block above the new `else if` must be the one that starts with
`if (_pluginState.RTV.EofVoteHappened && !_pluginState.RTV.ChangeImmediately)`.

Do NOT call `ChangeMap(_pluginState.Game.CurrentMap ...)` here: it breaks on workshop maps (see README).

## Change 2: server crash when changing to the workshop map that is already loaded

### `GameModeManager/CrossCutting/ServerManager.cs`, method `ChangeMap()`

Find (inside the `CountdownTimer` callback):

```csharp
                if (Server.IsMapValid(nextMap.Name))
                    Server.ExecuteCommand($"changelevel \"{nextMap.Name}\"");
                else if (nextMap.WorkshopId != -1)
                    Server.ExecuteCommand($"host_workshop_map \"{nextMap.WorkshopId}\"");
```

Replace with (workshop id first, changelevel second):

```csharp
                // [LOCAL PATCH] Workshop id first. IsMapValid is also true for the workshop map that is loaded
                // right now, and changelevel to it unmounts its addon, then the map file is missing and the server
                // dies (NETWORK_DISCONNECT_CLIENT_NO_MAP), e.g. picking AWP Only again while on awp_bhop_rocket.
                if (nextMap.WorkshopId > 0)
                    Server.ExecuteCommand($"host_workshop_map \"{nextMap.WorkshopId}\"");
                else if (Server.IsMapValid(nextMap.Name))
                    Server.ExecuteCommand($"changelevel \"{nextMap.Name}\"");
```

Leave the final `else ... ds_workshop_changelevel ...` line as it is.

## Build and install

```sh
cd build/gmm-src
dotnet build GameModeManager.sln -c Release
```

Copy `GameModeManager/bin/Release/net8.0/GameModeManager.dll` and `.pdb` to
`custom_files/addons/counterstrikesharp/plugins/GameModeManager/`.
Never copy the `shared/` DLLs into the plugin folder.

## Config that goes with it (already in custom_files, just check it is still there)

- `configs/plugins/GameModeManager/GameModeManager.json`, inside `"RTV"`: `"RepeatCurrentMapWhenNoVote": true`.
  If the new version regenerates the config, add the line again.
- `cfg/settings/map_voting.cfg`: `mp_match_end_restart 1` and `mp_match_restart_delay 25`.

## Test in game

1. Change 1: play a map to the end without `!rtv`: the same map and mode start again after ~25 s.
   With a finished `!rtv` vote the voted map loads instead.
2. Change 2: on `awp_bhop_rocket`, pick "AWP Only" again in `!modes`: the map reloads, the server does not crash.

## Afterwards

Update `README.md` ("Built from" column), `COMMIT.txt` in `build/gmm-src`, `installed` in `build/tools/plugins.json`,
and regenerate `GameModeManager.patch` (step 4 in `README.md`).
