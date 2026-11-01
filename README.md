# Ghost Territory CS2 server

Configuration and plugins of a CS2 server on Pterodactyl (KitsuneLab egg, CounterStrikeSharp + Metamod).
The game itself is not in this repository.

## Layout

| Path | What it is |
|---|---|
| `cs2-server/custom_files/` | Everything I add to the game: plugins, plugin configs, `cfg/` (modes, settings), `gamemodes_server.txt`. Copied over `game/csgo/` on every server start |
| `cs2-server/custom_files.sh` | Startup script that does that copy (egg variable `CUSTOM_FILES_ENABLED`) |
| `cs2-server/egg/configs/` | KitsuneLab egg configs (logging, cleanup, console filter) |
| `PANEL.md` | Pterodactyl panel settings (egg variables, startup command). They are not in any file, keep this up to date by hand |
| `build/plugins/` | Source of my own plugins (OnlineAdmins, CheatsGuard, MoveDebug, AllMaps, GrenadeRebuy) |
| `build/*-src/` | Source of the third-party plugins I build myself with local patches (each has a `MODIFICATIONS.md`) |
| `build/patches/` | **Read before updating a plugin**: what I changed in third-party plugins and why, with `.patch` files |
| `build/tools/plugins.json` | Installed plugins, versions, repos and the files to keep when updating. What to change in it after an update: `build/tools/README.md` |
| `build/tools/check-updates.mjs` | Update check (run `check-plugin-updates.bat`) |
| `build/tools/help-pages.mjs` | Generates the `!help` pages in CustomCommands' `Commands.json` |
| `sync-custom-files.bat` | Two-way WinSCP sync of `custom_files` with the server |
| `THIRD_PARTY_NOTICES.md`, `licenses/` | Authors, licenses and sources of every third-party plugin and config in this repository |

## Not in git

Game files, the server's live databases (SimpleAdmin, Retakes allocator), `backups/` (database copies),
`downloads/` (release zips, logs) and build outputs. See `.gitignore`.

## Building

- Own plugins and most self-built ones: .NET 8 SDK, `dotnet build -c Release` in the project folder.
- Plugins that target .NET 10 (Retakes): .NET 10 SDK.
- Each self-built plugin's build and install steps are in `build/patches/README.md`.

## Upstream contributions

The fixes I made to other people's plugins should go back to them. Plan: fork the upstream project, apply the
change (the step-by-step guides in `build/patches/<Plugin>.md` work for that too), open a pull request.

| Plugin | Change | Suggested way upstream | Status |
|---|---|---|---|
| GameModeManager | Server crash when changing to the workshop map that is already loaded (`ChangeMap()`) | Pull request (bug fix) | not sent yet |
| GameModeManager | Option to replay the current map when the match ends without a vote | Issue first, then a pull request if the maintainer wants it (opt-in option, default off) | not sent yet |
| MenuManagerAPI | E stops working after picking an option that closes the menu (`Choose()`) | Pull request (bug fix) | not sent yet |
| MenuManagerAPI | Build fails: PlayerSettingsAPI `HintPath` points to the drive root | Pull request (build fix) | not sent yet |
| MenuManagerAPI | Menus stuck after a map or round change | Already open upstream as [PR #4](https://github.com/nickj609/MenuManagerAPI/pull/4) by another contributor: comment there that it fixes the bug for me too, and suggest my extra pawn check in `CloseMenu()` | not sent yet |
| CS2 Parachute | `KeyNotFoundException` after loading the plugin mid-game | Pull request (bug fix); the project has had no commits since 2024 | not sent yet |
| CS2 Retakes | In-game convars to turn team scramble and balance on and off | Issue first (feature request) | not sent yet |

Update the status column when something is sent or merged. When upstream merges a fix, drop my patch
(`build/patches/README.md`, "When a new upstream version comes out").

## License and credits

My own code (the plugins in `build/plugins/`, scripts, tools, configs and docs written for this server) is under
the MIT license, see `LICENSE`.

Everything else (the other plugins, their source copies in `build/*-src/`, and configs based on other projects)
belongs to its authors and keeps its own license. The full list, with authors, licenses and links, is in
`THIRD_PARTY_NOTICES.md`. Big thanks to all of them: this server would not exist without their work.
