using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using GameModeManager.Shared;
using MenuManagerAPI.Shared;

namespace AllMaps;

public class AllMapsConfig : BasePluginConfig
{
    [JsonPropertyName("Permission")] public string Permission { get; set; } = "@css/changemap";
}

// !maps2 / !allmaps: one page per GameModeManager mode (same list and order as !modes) with that mode's maps
// (its map groups in GameModeManager.json). Picking a map keeps the current mode.
// The map change goes through GameModeManager's API (ChangeMap), so the current mode stays and workshop maps
// load with host_workshop_map (our GameModeManager patch).
// !modes2 / !allmodes: switch the mode but stay on the current map (GameModeManager's !modes loads the
// mode's default map).
public class AllMaps : BasePlugin, IPluginConfig<AllMapsConfig>
{
    public override string ModuleName => "All Maps";
    public override string ModuleVersion => "1.2.1";
    public override string ModuleAuthor => "Ghost Territory";
    public override string ModuleDescription => "!maps2 / !allmaps: any mode's maps, same mode. !modes2 / !allmodes: any mode, same map";

    public AllMapsConfig Config { get; set; } = new();
    private readonly PluginCapability<IMenuAPI> _menuCapability = new("menu:api");
    private readonly PluginCapability<IGameModeApi?> _gameModeCapability = new("game_mode:api");

    public void OnConfigParsed(AllMapsConfig config) => Config = config;

    public override void Load(bool hotReload)
    {
        AddCommand("css_maps2", "Menu with the maps of every game mode", OnAllMapsCommand);
        AddCommand("css_allmaps", "Menu with the maps of every game mode", OnAllMapsCommand);
        AddCommand("css_modes2", "Switch the game mode and stay on the current map", OnAllModesCommand);
        AddCommand("css_allmodes", "Switch the game mode and stay on the current map", OnAllModesCommand);
    }

    private void OnAllModesCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (!AdminManager.PlayerHasPermissions(player, Config.Permission))
        {
            command.ReplyToCommand($" {ChatColors.Red}You do not have access to this command.");
            return;
        }

        var api = _menuCapability.Get();
        var game = _gameModeCapability.Get();
        if (api == null || game == null)
        {
            command.ReplyToCommand(" Menu system or GameModeManager is not loaded.");
            return;
        }

        var current = game.State.CurrentMap;
        var menu = api.GetMenu($"Mode on {current.DisplayName}");
        foreach (var mode in game.State.Modes.Values)
        {
            var isCurrent = mode.Name == game.State.CurrentMode.Name;
            menu.AddMenuOption(isCurrent ? $"{mode.Name} (current)" : mode.Name, (p, _) =>
            {
                api.CloseMenu(p);
                ChangeModeKeepMap(p, game, mode);
            }, isCurrent);
        }
        api.OpenMenu(menu, player);
    }

    private void ChangeModeKeepMap(CCSPlayerController player, IGameModeApi game, GameModeManager.Shared.Models.IMode mode)
    {
        var current = game.State.CurrentMap;

        // The map has to be one GameModeManager knows from gamemodes_server.txt: a workshop map whose real name
        // differs from its list name has no workshop id here and could not be loaded again.
        if (!game.State.Maps.TryGetValue(current.Name, out var map))
        {
            Server.PrintToChatAll($" {ChatColors.Gold}[Server] {ChatColors.Lime}{player.PlayerName}{ChatColors.Default} changes the mode to {ChatColors.Lime}{mode.Name}{ChatColors.Default} (this map is not in the map list, loading the mode's default map)");
            game.Control.ChangeMode(mode);
            return;
        }

        Server.PrintToChatAll($" {ChatColors.Gold}[Server] {ChatColors.Lime}{player.PlayerName}{ChatColors.Default} changes the mode to {ChatColors.Lime}{mode.Name}{ChatColors.Default} on {ChatColors.Lime}{map.DisplayName}");
        // Same as GameModeManager's ChangeMode, but reloads the current map instead of the mode's default map.
        // The mode cfg runs css_gamemode, which only switches GameModeManager's current mode; the map reload then
        // runs the new mode's *_settings.cfg. The cfg runs next frame, the map changes after the countdown.
        // The map change goes through the API, not "css_map": MatchZy (loaded by the FACEIT/Utility cfgs) has its own css_map.
        Server.ExecuteCommand($"exec {mode.Config}");
        game.Control.ChangeMap(map, MapChangeDelay);
    }

    // Seconds, same as GameModeManager's Maps.Delay in our config
    private const int MapChangeDelay = 5;

    private void OnAllMapsCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid)
            return;

        if (!AdminManager.PlayerHasPermissions(player, Config.Permission))
        {
            command.ReplyToCommand($" {ChatColors.Red}You do not have access to this command.");
            return;
        }

        var api = _menuCapability.Get();
        var game = _gameModeCapability.Get();
        if (api == null || game == null)
        {
            command.ReplyToCommand(" Menu system or GameModeManager is not loaded.");
            return;
        }

        OpenModes(api, player, game);
    }

    // Pages = GameModeManager's modes, maps = the maps of each mode's map groups. Read from GameModeManager on every
    // open, so adding a map to a mode's map group in gamemodes_server.txt is enough.
    private void OpenModes(IMenuAPI api, CCSPlayerController player, IGameModeApi game)
    {
        var menu = api.GetMenu("All maps");
        foreach (var mode in game.State.Modes.Values)
        {
            // Same order as GameModeManager's !maps: map groups in the mode's order, maps in gamemodes_server.txt order
            var maps = mode.Maps.DistinctBy(m => m.Name).ToList();
            if (maps.Count == 0)
                continue;

            menu.AddMenuOption($"{mode.Name} ({maps.Count})", (p, _) => OpenMaps(api, p, game, mode.Name, maps));
        }
        api.OpenMenu(menu, player);
    }

    private void OpenMaps(IMenuAPI api, CCSPlayerController player, IGameModeApi game, string title,
        List<GameModeManager.Shared.Models.IMap> maps)
    {
        var menu = api.GetMenu(title, p => OpenModes(api, p, game));
        foreach (var map in maps)
        {
            menu.AddMenuOption(map.DisplayName, (p, _) =>
            {
                api.CloseMenu(p);
                Server.PrintToChatAll($" {ChatColors.Gold}[Server] {ChatColors.Lime}{p.PlayerName}{ChatColors.Default} changes the map to {ChatColors.Lime}{map.DisplayName}");
                // Through GameModeManager's API: keeps the current mode, workshop maps load with host_workshop_map
                // (our GameModeManager patch), and no clash with MatchZy's css_map in FACEIT/Utility
                game.Control.ChangeMap(map, MapChangeDelay);
            });
        }
        api.OpenMenu(menu, player);
    }
}
