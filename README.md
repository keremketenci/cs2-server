# Ghost Territory CS2 server

Configuration and plugins of a CS2 server on Pterodactyl (KitsuneLab egg, CounterStrikeSharp + Metamod).
The game itself is not in this repository.

## Layout

| Path | What it is |
|---|---|
| `48b4a3c2-.../custom_files/` | Everything we add to the game: plugins, plugin configs, `cfg/` (modes, settings), `gamemodes_server.txt`. Copied over `game/csgo/` on every server start |
| `48b4a3c2-.../custom_files.sh` | Startup script that does that copy (egg variable `CUSTOM_FILES_ENABLED`) |
| `48b4a3c2-.../egg/configs/` | KitsuneLab egg configs (logging, cleanup, console filter) |
| `build/plugins/` | Source of our own plugins (OnlineAdmins, CheatsGuard, MoveDebug, AllMaps, GrenadeRebuy) |
| `build/*-src/` | Source of the third-party plugins we build ourselves with local patches |
| `build/patches/` | **Read before updating a plugin**: what we changed in third-party plugins and why, with `.patch` files |
| `build/tools/plugins.json` | Installed plugins, versions, repos and the files to keep when updating |
| `build/tools/check-updates.mjs` | Update check (run `check-plugin-updates.bat`) |
| `build/tools/help-pages.mjs` | Generates the `!help` pages in CustomCommands' `Commands.json` |
| `sync-custom-files.bat` | Two-way WinSCP sync of `custom_files` with the server |

## Not in git

Game files, the server's live databases (SimpleAdmin, Retakes allocator), `backups/` (database copies),
`downloads/` (release zips, logs) and build outputs. See `.gitignore`.

## Building

- Own plugins and most self-built ones: .NET 8 SDK, `dotnet build -c Release` in the project folder.
- Plugins that target .NET 10 (Retakes): .NET 10 SDK.
- Each self-built plugin's build and install steps are in `build/patches/README.md`.
