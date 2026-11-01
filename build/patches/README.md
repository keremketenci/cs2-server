# Self-built plugins and local patches

Plugins we build ourselves instead of using the release zip. Read this before updating any of them:
a plain update from the release would silently drop our fixes.

Every code change is marked with a `[LOCAL PATCH]` comment in the source, so
`grep -rn "LOCAL PATCH" build/*-src` lists all of them.

Each plugin also has a step-by-step guide to redo its changes by hand after an update
(find this text, replace it with that code, build, install, test): the `<Plugin>.md` files in this folder,
linked in the "Manual guide" column below. Keep the guide in sync whenever a patch changes.

## Overview

| Plugin | Built from | Source | Patch file | Manual guide | Code changes |
|---|---|---|---|---|---|
| GameModeManager | `nickj609/GameModeManager` main, commit `0f354d1d569b91ea9fd9b7e37a54d0b3a5dd2e70` | `build/gmm-src` | `GameModeManager.patch` | [GameModeManager.md](GameModeManager.md) | 1 feature patch + 1 crash fix, see below (built from main because release v1.0.63 never loaded menus with Votes.Enabled and never refreshed !maps) |
| MenuManagerAPI | `nickj609/MenuManagerAPI` tag v1.0.3, commit `79c307ef516d2b3de707f2e0e7a037923beb2cb5` | `build/mmapi-src` | `MenuManagerAPI.patch` | [MenuManagerAPI.md](MenuManagerAPI.md) | 2 bug fixes + 1 build fix, see below |
| CS2 Parachute | `Franc1sco/CS2-Parachute` main, commit `c3be1bef3074ff4140e2fda97e13420dec5f47de` (5 commits after release 1.5.2) | `build/parachute-src` | `Parachute.patch` | [Parachute.md](Parachute.md) | 1 bug fix, see below |
| Retakes | `B3none/cs2-retakes` tag 3.1.1, commit `2fde4a9cc21e791d6909b98bacc833aa5c54bdad` | `build/retakes-src` | `RetakesPlugin.patch` | [RetakesPlugin.md](RetakesPlugin.md) | 1 feature patch (in-game toggles), see below |

## GameModeManager

Still actively maintained upstream. Patched anyway at the owner's explicit request (2026-10-30), so this patch
has to be re-applied after every update until upstream offers the same option.

### Patch 1: replay the current map when nobody voted (feature)

- Files: `GameModeManager/Config.cs` (new `RTV.RepeatCurrentMapWhenNoVote`, default `false` = upstream behavior)
  and `GameModeManager/CrossCutting/ServerManager.cs`, method `TriggerRotation()`.
- Why: with `EndOfMapVote: false` and `ChangeImmediately: false`, a match that ends without an !rtv vote made
  GameModeManager pick a random map or mode. The owner wants the same map and mode to start again instead.
- Fix: new `else if` branch before the random pick: when the option is on, GameModeManager does nothing
  (only logs it) and the engine restarts the match on the same map because `cfg/settings/map_voting.cfg` sets
  `mp_match_end_restart 1` (after `mp_match_restart_delay`, 25 s). A finished !rtv vote still wins: GameModeManager
  changes the map 5 s after the match ends, before the engine restart.
- First version (2026-10-30) reloaded `_pluginState.Game.CurrentMap` with `ChangeMap`. That failed on workshop maps:
  when the map's real name differs from its `gamemodes_server.txt` name, CurrentMap has no workshop id, the change
  command fails and the engine loaded the next map instead. Do not go back to that.
- Our config sets `"RepeatCurrentMapWhenNoVote": true` in `configs/plugins/GameModeManager/GameModeManager.json`
  and `mp_match_end_restart 1` in `cfg/settings/map_voting.cfg` (run by aim, awp, deathmatch and gungame settings).

### Patch 2: server crash when changing to the workshop map that is already loaded (bug fix)

- File: `GameModeManager/CrossCutting/ServerManager.cs`, method `ChangeMap()`.
- Symptom: on awp_bhop_rocket, picking "AWP Only" again in !modes killed the server. Console:
  `changelevel "awp_bhop_rocket"`, `Unmounting addon '3142070597'`,
  `Failed to mount world vpk file maps/awp_bhop_rocket.vpk`, `NETWORK_DISCONNECT_CLIENT_NO_MAP`, then the process exits.
- Cause: `ChangeMap` tried `Server.IsMapValid(name)` before the workshop id. For the workshop map that is loaded right
  now IsMapValid is true, so it used `changelevel`, which unmounts the map's own addon first.
- Fix: use `host_workshop_map <id>` whenever the map has a workshop id, `changelevel` only for maps without one.

### Build and install

```sh
cd build/gmm-src
dotnet build GameModeManager.sln -c Release
```

Copy `GameModeManager/bin/Release/net8.0/GameModeManager.dll` (+ `.pdb`) to
`custom_files/addons/counterstrikesharp/plugins/GameModeManager/`. Never copy the shared/ DLLs into the plugin folder.
The unpatched build is kept in `backups/GameModeManager-0f354d1-unpatched/`.

## MenuManagerAPI

Project state when patched (2026-10-30): last release v1.0.3 on 2026-02-15, last commit 2026-02-16,
PR #4 (another menu-close fix) open and unanswered since 2026-08-22.

### Patch 1: E stops working after picking a menu option (bug fix)

- File: `MenuManagerAPI/Core/PlayerInfo.cs`, method `Choose()`
- Symptom: open `!settings`, pick a setting, open `!settings` again: E does nothing until the player rejoins.
  Same for every GameModeManager menu whose option closes the menu (!maps, !modes, !rtv vote, !nominate).
- Cause: `Choose()` sets `SelectionProcessing = true`, runs the option, then resets the flag inside
  `switch (MainMenu?.PostSelectAction)`. GameModeManager closes the menu inside the option callback,
  which sets `MainMenu = null`, so no `case` matches and the flag stays `true` forever.
  Every later `Choose()` returns at `if (SelectionProcessing ...) return;`.
- Fix: a `default:` case in that switch that also sets `SelectionProcessing = false`.

### Patch 2: build fix (no behavior change)

- File: `MenuManagerAPI/MenuManagerAPI.csproj`
- Upstream `HintPath` for PlayerSettingsAPI is `\lib\shared\PlayerSettingsAPI.dll` (drive root), so the build fails.
  Changed to `lib\shared\PlayerSettingsApi\PlayerSettingsApi.dll` (the copy shipped in the repo).

### Patch 3: menus stuck after a mode change or round end (bug fix, 2026-10-31)

- Files: `MenuManagerAPI/Plugin.cs` (map start, round end and hot reload handlers) and
  `MenuManagerAPI/Core/PlayerInfo.cs`, method `CloseMenu()`.
- Symptom: right after a mode change (or when a round ends) an open button menu (`!game`, `!settings` ...) freezes
  on screen, the player stays frozen, and every later menu does nothing until the player rejoins.
- Cause: those handlers called `ButtonMenuManager.CloseMenu(info)`, which only stops the key handling. The player
  was not unfrozen and `PlayerInfo.MainMenu` stayed set, so the next `OpenMenu` took the sub-menu branch and never
  registered the player for key handling again. After a mode change GameModeManager runs `mp_restartgame 1`
  0.5 s after map start, which ends a round, so a menu opened at that moment always broke.
- Fix: same as upstream PR #4 (`playerInfo.CloseMenu()` in all three places). Extra: `CloseMenu()` only unfreezes
  when the controller and pawn are still valid (on map start they can be gone; `player.Pawn.Value!` would throw).
- Drop this patch when upstream merges PR #4 (keep the pawn check if upstream does not have one).

### Build and install

```sh
cd build/mmapi-src
dotnet build MenuManagerAPI.sln -c Release
```

Copy only `bin/counterstrikesharp/plugins/MenuManagerAPI/MenuManagerAPI.dll` (+ `.pdb`) to
`custom_files/addons/counterstrikesharp/plugins/MenuManagerAPI/`.
Do NOT copy the `shared/` or `plugins/PlayerSettings` folders from `bin/`: the server has newer versions of those.
The original release DLL is kept in `backups/MenuManagerAPI-1.0.3-release/`.

## CS2 Parachute

Project state when patched (2026-10-30): last release 1.5.2 on 2024-04-02, last commit 2024-05-03.
We build from main because it has 5 fixes after 1.5.2 (dead-player chute, unhandled exceptions, hostage option).

### Patch 1: parachute broken after !pon until the player rejoins (bug fix)

- File: `src/Parachute.cs`
- Symptom: after `!pon` (`css_plugins load` from `plugins/disabled`) the console spams
  `KeyNotFoundException: The given key '1' was not present in the dictionary` at `Parachute.cs` OnTick,
  and the parachute does not work right until the player reconnects.
- Cause: players are added to the plugin's dictionaries only on `player_connect_full`, or in `Load` when
  `hotReload` is true. `css_plugins load` passes `hotReload = false`, so players already on the server
  were never added. Also `Dictionary.Add` on connect throws when the slot is already known (map change).
- Fix: new `InitPlayer(index)` helper that sets the entries with the indexer (no Add);
  `Load` calls it for every current player regardless of `hotReload`; `player_connect_full` calls it;
  `OnTick` calls it for any player not seen yet before reading the dictionaries.

### Build and install

```sh
cd build/parachute-src/src
dotnet build -c Release
```

Copy `bin/Release/net7.0/Parachute.dll`, `.pdb` and `.deps.json` to
`custom_files/addons/counterstrikesharp/plugins/disabled/Parachute/` (it stays in disabled/, `!pon` loads it).
The original release files are kept in `backups/Parachute-1.5.2-release/`.

## When a new upstream version comes out

1. Read its changelog / diff: if the bug is fixed upstream, drop our patch and use the release as is
   (then remove the plugin's row above and its `BUILT_FROM_COMMIT.txt`).
2. Otherwise download the new source into `build/<name>-src`, then apply the patch from the repo root:
   `git apply --ignore-whitespace build/patches/<Plugin>.patch`
   (or `patch -p1 -l < ...`). If it does not apply cleanly, redo the changes by hand with the plugin's guide
   (`build/patches/<Plugin>.md`).
3. Build, install, update the "Built from" column, `BUILT_FROM_COMMIT.txt` in the plugin folder
   and `installed` in `build/tools/plugins.json`.
4. Regenerate the patch file: `diff -ruN -x bin -x obj <pristine>/<Plugin> build/<name>-src/<Plugin> > build/patches/<Plugin>.patch`
   (paths inside the patch must start with `a/<Plugin>/` and `b/<Plugin>/`).

## Retakes

Actively maintained upstream. Patched at the owner's explicit request (2026-10-31): re-apply after every update.
Build needs the .NET 10 SDK (user-local install in `%LOCALAPPDATA%\dotnet10`, not on PATH):
`"%LOCALAPPDATA%\dotnet10\dotnet.exe" build -c Release` in `build/retakes-src/RetakesPlugin`, output in
`build/retakes-src/build-output/addons/counterstrikesharp/plugins/RetakesPlugin`. Copy only `RetakesPlugin.dll` and
`.pdb` over the release files (deps.json and lang are the same). Release DLL kept in `backups/RetakesPlugin-3.1.1-release`.

### Patch 1: toggle team scramble and round-end rotation in game (feature)

- Files: `RetakesPlugin/RetakesPlugin.cs` (two FakeConVars, start values set in `OnConfigParsed`) and
  `RetakesPlugin/Managers/GameManager.cs` (`_isScrambleEnabled` / `_isBalanceEnabled` read the convars every round
  instead of readonly fields filled from the config).
- Why: `TeamSettings.IsScrambleEnabled` / `IsBalanceEnabled` are only read when the plugin loads, upstream has no command.
- New server convars: `retakes_scramble_enabled` (scramble after `RoundsToScramble` T wins in a row) and
  `retakes_balance_enabled` (best CTs move to T after a CT win, team size balance). Start value = config value
  (both true), so every time the Retake mode loads they are on again.
- Chat commands (CustomCommands, `@css/config`): `!scron` / `!scroff`, `!roton` / `!rotoff`. Shown on `!hretake`.

