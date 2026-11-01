# Modified copy

This is a modified version of [B3none/cs2-retakes](https://github.com/B3none/cs2-retakes)
(tag 3.1.1, commit `2fde4a9cc21e791d6909b98bacc833aa5c54bdad`), licensed under the GNU GPL v3 (`LICENSE`, unchanged).
This modified version is distributed under the same license.
Modified by Kerem Ketenci for the Ghost Territory CS2 server.

| Date | File | Change |
|---|---|---|
| 2026-10-31 | `RetakesPlugin/RetakesPlugin.cs` | new convars `retakes_scramble_enabled` and `retakes_balance_enabled`, start values from the config |
| 2026-10-31 | `RetakesPlugin/Managers/GameManager.cs` | scramble / balance read from those convars every round instead of readonly fields |

Every change is marked with `[LOCAL PATCH]`. Details: `build/patches/RetakesPlugin.md`, diff: `build/patches/RetakesPlugin.patch`.
