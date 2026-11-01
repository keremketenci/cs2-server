# CS2 Parachute: redo our changes by hand

Use this after downloading a new Parachute version, when `Parachute.patch` does not apply
or you want to do it yourself. Background and reasons: `README.md#cs2-parachute`.

1 change in 1 file (`src/Parachute.cs`), done in 4 small steps. Search for the "Find" text with Ctrl+F
(line numbers change between versions).

## 0. Before you start

1. Download the new source (zip or `git clone https://github.com/Franc1sco/CS2-Parachute`).
2. Check the changelog: if `!pon` (loading the plugin mid-game) no longer spams `KeyNotFoundException`,
   skip all of this.
3. Rename the old `build/parachute-src` (e.g. `parachute-src-old`) and put the new source in `build/parachute-src`.
   Delete the old folder at the end, when everything works.

## Step 1: add the `InitPlayer` helper

Find:

```csharp
private void StopPara(CCSPlayerController player)
```

Add this method directly above it:

```csharp
    // [LOCAL PATCH] one place that creates (or resets) a player's entries, overwriting instead of Add
    private void InitPlayer(int index)
    {
        bUsingPara[index] = false;
        gParaTicks[index] = 0;
        gParaModel[index] = null;
    }

```

## Step 2: `Load()`, add every current player, not only on hot reload

Find:

```csharp
        if (hotReload)
        {
            Utilities.GetPlayers().ForEach(player =>
            {
                bUsingPara.Add((int)player.Index, false);
                gParaTicks.Add((int)player.Index, 0);
                gParaModel.Add((int)player.Index, null);
            });
        }
```

Replace the whole block with:

```csharp
        // [LOCAL PATCH] was "if (hotReload)": css_plugins load mid-game passes hotReload=false,
        // so players already on the server were never added and OnTick threw KeyNotFoundException until they rejoined.
        Utilities.GetPlayers().ForEach(player => InitPlayer((int)player.Index));
```

## Step 3: `EventPlayerConnectFull`, no `Dictionary.Add`

Find (inside `RegisterEventHandler<EventPlayerConnectFull>`, in the `else` branch):

```csharp
                bUsingPara.Add((int)player.Index, false);
                gParaTicks.Add((int)player.Index, 0);
                gParaModel.Add((int)player.Index, null);
```

Replace with:

```csharp
                // [LOCAL PATCH] was Dictionary.Add, which throws when the slot is already known (map change)
                InitPlayer((int)player.Index);
```

## Step 4: `OnTick`, never read a player that is not in the dictionaries

Find (inside `RegisterListener<Listeners.OnTick>`):

```csharp
                    var buttons = player.Buttons;
```

Add directly above it:

```csharp
                    // [LOCAL PATCH] never index the dictionaries with a player we have not seen yet
                    if (!bUsingPara.ContainsKey((int)player.Index))
                        InitPlayer((int)player.Index);

```

Check: after these 4 steps a search for `.Add((int)player.Index` in `Parachute.cs` finds nothing.

## Build and install

```sh
cd build/parachute-src/src
dotnet build -c Release
```

Copy `bin/Release/net7.0/Parachute.dll`, `.pdb` and `.deps.json` to
`custom_files/addons/counterstrikesharp/plugins/disabled/Parachute/`
(it stays in `disabled/`, `!pon` loads it). If the new version targets another .NET, the `net7.0` folder name changes.

## Test in game

With players on the server: `!pon`, then jump and hold E. The parachute opens for everyone without rejoining,
and the server console shows no `KeyNotFoundException` from `Parachute.cs`. Change the map and try again.

## Afterwards

Update `README.md` ("Built from" column), `COMMIT.txt` in `build/parachute-src`, `installed` in
`build/tools/plugins.json`, and regenerate `Parachute.patch` (step 4 in `README.md`).
