# MenuManagerAPI: redo our changes by hand

Use this after downloading a new MenuManagerAPI version, when `MenuManagerAPI.patch` does not apply
or you want to do it yourself. Background and reasons: `README.md#menumanagerapi`.

3 changes, 3 files. Search for the "Find" text with Ctrl+F (line numbers change between versions).

## 0. Before you start

1. Download the new source (zip or `git clone https://github.com/nickj609/MenuManagerAPI`).
2. Check the changelog / merged PRs:
   - If PR #4 is merged (menus closed with `playerInfo.CloseMenu()` in `Plugin.cs`), skip change 3
     but keep the pawn check in `CloseMenu()` if upstream does not have one.
   - If `Choose()` already resets `SelectionProcessing` when `MainMenu` is null, skip change 1.
3. Rename the old `build/mmapi-src` (e.g. `mmapi-src-old`) and put the new source in `build/mmapi-src`.
   Delete the old folder at the end, when everything works.

## Change 1: E stops working after picking a menu option

### `MenuManagerAPI/Core/PlayerInfo.cs`, method `Choose()`

Find:

```csharp
switch (MainMenu?.PostSelectAction)
```

At the end of that switch there is:

```csharp
                case PostSelectAction.Nothing:
                    SelectionProcessing = false;
                    break;
```

Add a `default` case right after it (before the switch's closing `}`):

```csharp
                // [LOCAL PATCH] OnChoose may close the menu (MainMenu == null), then no case matches
                // and SelectionProcessing stayed true forever: E did nothing until the player rejoined.
                default:
                    SelectionProcessing = false;
                    break;
```

## Change 2: build fix

### `MenuManagerAPI/MenuManagerAPI.csproj`

Find:

```xml
<HintPath>\lib\shared\PlayerSettingsAPI.dll</HintPath>
```

Replace with:

```xml
        <!-- [LOCAL PATCH] upstream HintPath was \lib\shared\PlayerSettingsAPI.dll (drive root), the build could not find it -->
        <HintPath>lib\shared\PlayerSettingsApi\PlayerSettingsApi.dll</HintPath>
```

Check the file really is at `MenuManagerAPI/lib/shared/PlayerSettingsApi/PlayerSettingsApi.dll` in the new source.
If upstream fixed the path, skip this change.

## Change 3: menus stuck after a mode change or round end

### 3a. `MenuManagerAPI/Plugin.cs`, method `Load()`: 3 places

Find every:

```csharp
ButtonMenuManager.CloseMenu(
```

There are 3 hits in `Plugin.cs` (in `RegisterListener<OnMapStart>`, in `RegisterEventHandler<EventRoundEnd>`
and in the `if (hotReload)` block). Replace each call so the menu is closed through the PlayerInfo object:

| Find | Replace with |
|---|---|
| `ButtonMenuManager.CloseMenu(playerInfo);` (map start) | `playerInfo.CloseMenu();` |
| `ButtonMenuManager.CloseMenu(playerInfo);` (round end) | `playerInfo.CloseMenu();` |
| `ButtonMenuManager.CloseMenu(infoObj);` (hot reload) | `infoObj.CloseMenu();` |

Put this comment above each of them:

```csharp
// [LOCAL PATCH] full close (upstream PR #4): ButtonMenuManager.CloseMenu only stopped the key handling,
// the player stayed frozen and PlayerInfo kept the menu, so later menus never worked until rejoin
```

Do NOT change the `ButtonMenuManager.CloseMenu(this);` call inside `PlayerInfo.CloseMenu()` itself
(that would make it call itself forever).

### 3b. `MenuManagerAPI/Core/PlayerInfo.cs`, method `CloseMenu()`

Find:

```csharp
                else
                {
                    PlayerExtensions.Unfreeze(player.Pawn.Value!);
                }
```

Replace with:

```csharp
                // [LOCAL PATCH] on map start the controller or pawn can already be gone, skip unfreezing then
                else if (player.IsValid && player.Pawn.Value is { IsValid: true } pawn)
                {
                    PlayerExtensions.Unfreeze(pawn);
                }
```

## Build and install

```sh
cd build/mmapi-src
dotnet build MenuManagerAPI.sln -c Release
```

Copy only `bin/counterstrikesharp/plugins/MenuManagerAPI/MenuManagerAPI.dll` and `.pdb` to
`custom_files/addons/counterstrikesharp/plugins/MenuManagerAPI/`.
Do NOT copy the `shared/` or `plugins/PlayerSettings` folders from `bin/`: the server has newer versions of those.

## Test in game

1. Change 1: open `!settings`, pick something, open `!settings` again: E still selects.
   Same with `!maps` / `!modes`.
2. Change 3: open `!game` and keep it open while the mode changes (or the round ends): the menu closes,
   you can move, and `!game` opens and works again without rejoining.

## Afterwards

Update `README.md` ("Built from" column), `COMMIT.txt` in `build/mmapi-src`, `installed` in `build/tools/plugins.json`
(and remove its `reviewedMain` line there, or set it to the main commit you built from), and regenerate `MenuManagerAPI.patch` (step 4 in `README.md`).
