# Modified copy

This is a modified copy of [Franc1sco/CS2-Parachute](https://github.com/Franc1sco/CS2-Parachute)
(main, commit `c3be1bef3074ff4140e2fda97e13420dec5f47de`) by Franc1sco. The upstream project has not published a
license: all rights stay with its author. It is kept here, with credit, only to document a bug fix that I want to
send upstream as a pull request. Author: if you want it removed, please open an issue.
Modified by Kerem Ketenci for the Ghost Territory CS2 server.

| Date | File | Change |
|---|---|---|
| 2026-10-30 | `src/Parachute.cs` | `InitPlayer()` helper; players added on every load (not only hot reload), no `Dictionary.Add` on connect, `OnTick` never reads an unknown player (fixes `KeyNotFoundException` after `css_plugins load`) |

Every change is marked with `[LOCAL PATCH]`. Details: `build/patches/Parachute.md`, diff: `build/patches/Parachute.patch`.
