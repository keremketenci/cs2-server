# Third-party software

This server is built on the work of many plugin authors. Thank you all!

Only my own code is under this repository's MIT license (`LICENSE`): the plugins in `build/plugins/`
(OnlineAdmins, CheatsGuard, MoveDebug, AllMaps, GrenadeRebuy), the scripts and tools, and my own configs and docs.
Everything listed below belongs to its authors and stays under its own license. Using it here does not change that.

The game, CounterStrikeSharp and Metamod:Source are not in this repository (the server's egg installs them).

## Plugins I modified

My changes are bug fixes and small options for my server. Each one is marked with a `[LOCAL PATCH]` comment in
the source, described in `build/patches/README.md` and kept as a `.patch` file, and I want to send them back
upstream as pull requests (see "Upstream contributions" in `README.md`).

| Plugin | Author | License | My source copy | What I changed |
|---|---|---|---|---|
| [GameModeManager](https://github.com/nickj609/GameModeManager) | Nickj609 | MIT | `build/gmm-src` | `build/patches/GameModeManager.md` |
| [MenuManagerAPI](https://github.com/nickj609/MenuManagerAPI) | Nickj609 | MIT | `build/mmapi-src` | `build/patches/MenuManagerAPI.md` |
| [CS2 Retakes](https://github.com/B3none/cs2-retakes) | B3none | GPL-3.0 | `build/retakes-src` | `build/patches/RetakesPlugin.md` |
| [CS2 Parachute](https://github.com/Franc1sco/CS2-Parachute) | Franc1sco | none published | `build/parachute-src` | `build/patches/Parachute.md` |

The full source of every modified plugin I run is in its `build/*-src` folder, with the original license file
where upstream has one, and a `MODIFICATIONS.md` that lists the changed files and the dates.

## Plugins and libraries included as released

The DLLs under `48b4a3c2-.../custom_files/addons/` are the authors' release builds, unchanged.
Installed versions are in `build/tools/plugins.json`.

| Component | Author | License | Source |
|---|---|---|---|
| MatchZy (+ `cfg/MatchZy`, `gamedata/matchzy.json`) | Shobhit Pathak | MIT | https://github.com/shobhit-pathak/MatchZy |
| CS2-SimpleAdmin (+ FunCommands, StealthModule, `shared/CS2-SimpleAdminApi`) | daffyyyy | GPL-3.0 | https://github.com/daffyyyy/CS2-SimpleAdmin |
| CS2-Tags | daffyyyy | GPL-3.0 | https://github.com/daffyyyy/CS2-Tags |
| Inventory Simulator (+ `gamedata/inventory-simulator.json`) | Ian Lucas | MIT | https://github.com/ianlucas/cs2-css-inventory-simulator |
| CustomCommands | HerrMagic | MIT | https://github.com/HerrMagiic/CSS-CreateCustomCommands |
| Custom Votes (+ `shared/CS2-CustomVotes.Shared`) | imi-tat0r | MIT | https://github.com/imi-tat0r/CS2-CustomVotes |
| CSSharpUtils (`shared/CSSharpUtils`) | imi-tat0r | MIT | https://github.com/imi-tat0r/CSSharpUtils |
| Deathmatch (+ `shared/DeathmatchAPI`) | NockyCZ | MIT | https://github.com/NockyCZ/CS2-Deathmatch |
| GunGame (+ `shared/GunGameAPI`, `cfg/gungame`) | ssypchenko | MIT | https://github.com/ssypchenko/cs2-gungame |
| CS2_ExecAfter | kus | GPL-3.0 | https://github.com/kus/CS2_ExecAfter |
| CS2 Remove Map Weapons | kus | GPL-3.0 | https://github.com/kus/CS2-Remove-Map-Weapons |
| K4-DamageInfo | K4ryuu | GPL-3.0 | https://github.com/K4ryuu/K4-DamageInfo |
| Instadefuse | B3none | GPL-3.0 | https://github.com/B3none/cs2-instadefuse |
| MultiAddonManager (`addons/multiaddonmanager`, `addons/metamod/multiaddonmanager.vdf`) | Source2ZE | GPL-3.0 | https://github.com/Source2ZE/MultiAddonManager |
| Retakes shared API (`shared/RetakesPluginShared`) and map configs | B3none | GPL-3.0 | https://github.com/B3none/cs2-retakes |
| Advertisement | partiusfabaa | none published | https://github.com/partiusfabaa/cs2-advertisement |
| MenuManager [Core] (+ `shared/MenuManagerApi`) | NickFox007 | none published | https://github.com/NickFox007/MenuManagerCS2 |
| PlayerSettings [Core] (+ `shared/PlayerSettingsApi`) | NickFox007 | none published | https://github.com/NickFox007/PlayerSettingsCS2 |
| AnyBaseLib (`shared/AnyBaseLib`) | NickFox007 | none published | https://github.com/NickFox007/AnyBaseLibCS2 |
| ShowPlayersInfo | jockii | none published | https://github.com/jockii/cs2_ShowPlayersInfo |
| MutualScoringPlayers | qstage | none published | https://github.com/qstage/CS2-MutualScoringPlayers |
| Retakes Weapon Allocator | Ravid-A | none published | https://github.com/Ravid-A/cs2-retakes-weapon-allocator |
| Clutch Announce | B3none | none published | https://github.com/B3none/cs2-clutch-announce |

**GPL-3.0 components:** the complete source code of each one is available from its repository above; the exact
version I use is the tag in `build/tools/plugins.json`. The license text is in `licenses/GPL-3.0.txt`.

**MIT components:** copyright notices, as given in each project's LICENSE:
Copyright (c) 2023 Shobhit Pathak; Copyright (c) 2024 - present Ian Lucas; Copyright (c) 2024 HerrMagic;
Copyright (c) 2024 imi-tat0r (CS2-CustomVotes, CSSharpUtils); Copyright (c) 2024 Nickj609 (GameModeManager);
Copyright (c) 2025 Nickj609 (MenuManagerAPI); Copyright (c) 2026 NockyCZ; Copyright (c) 2023 ssypchenko.
Each is licensed under the MIT license (same text as this repository's `LICENSE`, with the author's own copyright line).

**"None published":** these authors have not published a license. Their files are included only so this server
setup is complete, with full credit to them, and all rights stay with them. If you are one of these authors and
want your files removed from this repository, please open an issue and they will be removed.

## Configs and other material

| What | Based on | License |
|---|---|---|
| `gamemodes_server.txt` map groups and several `cfg/modes/*_settings.cfg` (aim, awp, deathmatch, gungame; marked in the file) | [kus/cs2-modded-server](https://github.com/kus/cs2-modded-server) by kus | LGPL-3.0 (`licenses/LGPL-3.0.txt`), the derived files stay under it |
| `cfg/settings/bots_buy.cfg`, `bots_dont_buy.cfg` | [gist by qstage](https://gist.github.com/qstage/f81fdfc338f2d3cb9aa3644d6f53e87d) | credited in the file |
| `cfg/cs2-retakes` | CS2 Retakes example configs | GPL-3.0 |
| Workshop maps in `gamemodes_server.txt` | their Steam Workshop authors | referenced by Workshop id only, not included |
