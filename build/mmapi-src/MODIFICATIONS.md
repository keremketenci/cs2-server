# Modified copy

This is a modified copy of [nickj609/MenuManagerAPI](https://github.com/nickj609/MenuManagerAPI)
(tag v1.0.3, commit `79c307ef516d2b3de707f2e0e7a037923beb2cb5`), MIT license (`LICENSE`, unchanged).
Modified by Kerem Ketenci for the Ghost Territory CS2 server.

| Date | File | Change |
|---|---|---|
| 2026-10-30 | `MenuManagerAPI/Core/PlayerInfo.cs` | `Choose()`: reset `SelectionProcessing` when the menu was closed by the option |
| 2026-10-30 | `MenuManagerAPI/MenuManagerAPI.csproj` | PlayerSettingsAPI `HintPath` relative to the project (build fix) |
| 2026-10-31 | `MenuManagerAPI/Plugin.cs`, `MenuManagerAPI/Core/PlayerInfo.cs` | full menu close on map start, round end and hot reload (same as upstream PR #4), pawn check in `CloseMenu()` |

Every change is marked with `[LOCAL PATCH]`. Details: `build/patches/MenuManagerAPI.md`, diff: `build/patches/MenuManagerAPI.patch`.
