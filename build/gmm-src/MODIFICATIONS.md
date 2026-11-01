# Modified copy

This is a modified copy of [nickj609/GameModeManager](https://github.com/nickj609/GameModeManager)
(commit `0f354d1d569b91ea9fd9b7e37a54d0b3a5dd2e70`), MIT license (`LICENSE`, unchanged).
Modified by Kerem Ketenci for the Ghost Territory CS2 server.

| Date | File | Change |
|---|---|---|
| 2026-10-30 / 2026-10-31 | `GameModeManager/Config.cs` | new option `RTV.RepeatCurrentMapWhenNoVote` (default off) |
| 2026-10-31 | `GameModeManager/CrossCutting/ServerManager.cs` | `TriggerRotation()`: keep the current map when nobody voted (if the option is on); `ChangeMap()`: workshop id before `changelevel` (server crash fix) |

Every change is marked with `[LOCAL PATCH]`. Details: `build/patches/GameModeManager.md`, diff: `build/patches/GameModeManager.patch`.
