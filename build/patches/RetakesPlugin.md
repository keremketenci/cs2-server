# Retakes: redo my changes by hand

Use this after downloading a new Retakes version, when `RetakesPlugin.patch` does not apply
or you want to do it yourself. Background and reasons: `README.md#retakes`.

1 change in 2 files. Search for the "Find" text with Ctrl+F (line numbers change between versions).

## 0. Before you start

1. Download the new source (zip or `git clone https://github.com/B3none/cs2-retakes`).
2. Check the changelog: if upstream now has convars or commands to turn team scramble and balance
   on and off in game, use those instead and update the `!scron` / `!roton` commands in CustomCommands.
3. Rename the old `build/retakes-src` (e.g. `retakes-src-old`) and put the new source in `build/retakes-src`.
   Delete the old folder at the end, when everything works.

## Step 1: `RetakesPlugin/RetakesPlugin.cs`, add the two convars

Find:

```csharp
public FakeConVar<bool> RetakesEnabledConVar = new("retakes_enabled",
```

Below that line (and the `_lastEnabledState` line that follows it), before `#endregion`, add:

```csharp

    // [LOCAL PATCH] in-game toggles for TeamSettings.IsScrambleEnabled and IsBalanceEnabled (read every round)
    public FakeConVar<bool> ScrambleEnabledConVar = new("retakes_scramble_enabled", "Scramble the teams after RoundsToScramble T wins in a row.", true);
    public FakeConVar<bool> BalanceEnabledConVar = new("retakes_balance_enabled", "Move the best CTs to T after a CT win and balance the team sizes.", true);
```

The convar names `retakes_scramble_enabled` and `retakes_balance_enabled` must stay exactly like this:
CustomCommands uses them.

## Step 2: `RetakesPlugin/RetakesPlugin.cs`, start values from the config

Find the method:

```csharp
public void OnConfigParsed(BaseConfigs config)
```

Directly after its `Config = config;` line, add:

```csharp
        // [LOCAL PATCH] the config only sets the start values, the convars below can change them in game
        ScrambleEnabledConVar.Value = Config.Team.IsScrambleEnabled;
        BalanceEnabledConVar.Value = Config.Team.IsBalanceEnabled;
```

If the build says `Config.Team` does not exist, open the config class and use the property that holds
`IsScrambleEnabled` / `IsBalanceEnabled` (`"TeamSettings"` in `RetakesPlugin.json`).

## Step 3: `RetakesPlugin/Managers/GameManager.cs`, read the convars every round

### 3a. Fields

Find:

```csharp
    private readonly bool _isScrambleEnabled;
```

Replace with:

```csharp
    // [LOCAL PATCH] read from the plugin convars so they can be toggled in game
    private bool _isScrambleEnabled => _plugin.ScrambleEnabledConVar.Value;
```

Find:

```csharp
    private readonly bool _isBalanceEnabled;
```

Replace with:

```csharp
    private bool _isBalanceEnabled => _plugin.BalanceEnabledConVar.Value;
```

### 3b. Constructor

In the `public GameManager(...)` constructor, delete these two lines (they would not compile any more):

```csharp
        _isScrambleEnabled = isScrambleEnabled ?? true;
        _isBalanceEnabled = isBalanceEnabled ?? true;
```

Leave the constructor parameters as they are, only the two assignments go.
If the class has no `_plugin` field in the new version, keep the plugin passed to the constructor in one
(`private readonly RetakesPlugin _plugin;` + `_plugin = plugin;`).

## Build and install

Needs the .NET 10 SDK (user-local, not on PATH):

```sh
cd build/retakes-src/RetakesPlugin
"%LOCALAPPDATA%\dotnet10\dotnet.exe" build -c Release
```

Output: `build/retakes-src/build-output/addons/counterstrikesharp/plugins/RetakesPlugin`.
Install the new release files first (deps.json, lang ...), then copy only my `RetakesPlugin.dll` and `.pdb` over them in
`custom_files/addons/counterstrikesharp/plugins/disabled/RetakesPlugin/`.

## Config that goes with it (already in custom_files, just check it is still there)

- `configs/plugins/RetakesPlugin/RetakesPlugin.json`: `"IsScrambleEnabled": true`, `"IsBalanceEnabled": true`
  (start values every time the Retake mode loads).
- `plugins/CustomCommands/Commands.json`: `!scron` / `!scroff` (`retakes_scramble_enabled 1/0`)
  and `!roton` / `!rotoff` (`retakes_balance_enabled 1/0`), flag `@css/config`. Listed on `!hretake`.

## Test in game

Load the Retake mode. In the server console `retakes_scramble_enabled` and `retakes_balance_enabled` print `true`.
`!scroff` / `!rotoff` set them to `false` and scramble / rotation stop from the next round;
`!scron` / `!roton` turn them back on.

## Afterwards

Update `README.md` ("Built from" column), `BUILT_FROM_COMMIT.txt` in `build/retakes-src`, `installed` in
`build/tools/plugins.json`, and regenerate `RetakesPlugin.patch` (step 4 in `README.md`).
