using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using GameModeManager.Shared;
using MenuManagerAPI.Shared;
using Microsoft.Extensions.Logging;

namespace AllMaps;

public class Category
{
    public string Name { get; set; } = "";
    // Map groups from gamemodes_server.txt whose maps are listed in this category
    public List<string> MapGroups { get; set; } = new();
}

public class AllMapsConfig : BasePluginConfig
{
    [JsonPropertyName("Permission")] public string Permission { get; set; } = "@css/changemap";
    [JsonPropertyName("MapGroupFile")] public string MapGroupFile { get; set; } = "gamemodes_server.txt";
    [JsonPropertyName("Categories")]
    public List<Category> Categories { get; set; } = new()
    {
        new() { Name = "Valve maps", MapGroups = new() { "mg_valve_casual", "mg_rush" } },
        new() { Name = "Wingman maps", MapGroups = new() { "mg_valve_wingman" } },
        new() { Name = "Arms Race maps", MapGroups = new() { "mg_valve_arms_race" } },
        new() { Name = "Aim maps", MapGroups = new() { "mg_aim" } },
        new() { Name = "AWP maps", MapGroups = new() { "mg_awp" } },
        new() { Name = "Gun Game / fun maps", MapGroups = new() { "mg_gg" } },
        new() { Name = "Utility practice maps", MapGroups = new() { "mg_utility" } },
    };
}

// !maps2 / !allmaps: every map of the server's map groups, by category, in any mode.
// The map change goes through GameModeManager's API (ChangeMap), so the current mode stays and workshop maps
// load with host_workshop_map (our GameModeManager patch).
// !modes2 / !allmodes: switch the mode but stay on the current map (GameModeManager's !modes loads the
// mode's default map).
public class AllMaps : BasePlugin, IPluginConfig<AllMapsConfig>
{
    public override string ModuleName => "All Maps";
    public override string ModuleVersion => "1.1.1";
    public override string ModuleAuthor => "Ghost Territory";
    public override string ModuleDescription => "!maps2 / !allmaps: any map, same mode. !modes2 / !allmodes: any mode, same map";

    public AllMapsConfig Config { get; set; } = new();
    private readonly PluginCapability<IMenuAPI> _menuCapability = new("menu:api");
    private readonly PluginCapability<IGameModeApi?> _gameModeCapability = new("game_mode:api");

    // Display = second value in gamemodes_server.txt (e.g. "de_poseidon (wingman)"), the map name when empty
    private record MapEntry(string Name, string? WorkshopId, string Display);

    public void OnConfigParsed(AllMapsConfig config) => Config = config;

    public override void Load(bool hotReload)
    {
        AddCommand("css_maps2", "Menu with every server map, by category", OnAllMapsCommand);
        AddCommand("css_allmaps", "Menu with every server map, by category", OnAllMapsCommand);
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
        if (api == null)
        {
            command.ReplyToCommand(" Menu system (MenuManagerAPI) is not loaded.");
            return;
        }

        var groups = ReadMapGroups();
        if (groups == null)
        {
            command.ReplyToCommand($" Could not read {Config.MapGroupFile}, see the server console.");
            return;
        }

        OpenCategories(api, player, groups);
    }

    private void OpenCategories(IMenuAPI api, CCSPlayerController player, Dictionary<string, List<MapEntry>> groups)
    {
        var menu = api.GetMenu("All maps");
        foreach (var category in Config.Categories)
        {
            var maps = category.MapGroups
                .SelectMany(g => groups.TryGetValue(g, out var list) ? list : new List<MapEntry>())
                .DistinctBy(m => m.Name)
                .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (maps.Count == 0)
                continue;

            menu.AddMenuOption($"{category.Name} ({maps.Count})", (p, _) => OpenMaps(api, p, groups, category.Name, maps));
        }
        api.OpenMenu(menu, player);
    }

    private void OpenMaps(IMenuAPI api, CCSPlayerController player, Dictionary<string, List<MapEntry>> groups,
        string title, List<MapEntry> maps)
    {
        var menu = api.GetMenu(title, p => OpenCategories(api, p, groups));
        foreach (var map in maps)
        {
            menu.AddMenuOption(map.Display, (p, _) =>
            {
                api.CloseMenu(p);
                Server.PrintToChatAll($" {ChatColors.Gold}[Server] {ChatColors.Lime}{p.PlayerName}{ChatColors.Default} changes the map to {ChatColors.Lime}{map.Display}");
                // Through GameModeManager's API when it knows the map (no clash with MatchZy's css_map in FACEIT/Utility)
                var game = _gameModeCapability.Get();
                if (game != null && game.State.Maps.TryGetValue(map.Name, out var known))
                    game.Control.ChangeMap(known, MapChangeDelay);
                else
                    Server.ExecuteCommand(map.WorkshopId == null ? $"css_map {map.Name}" : $"css_map {map.Name} {map.WorkshopId}");
            });
        }
        api.OpenMenu(menu, player);
    }

    // Map group name -> maps, from the "mapgroups" section of gamemodes_server.txt.
    // Workshop entries look like "workshop/<id>/<name>".
    private Dictionary<string, List<MapEntry>>? ReadMapGroups()
    {
        var path = Path.Combine(Server.GameDirectory, "csgo", Config.MapGroupFile);
        try
        {
            var root = KeyValues.Parse(File.ReadAllText(path));
            var mapGroups = KeyValues.FindSection(root, "mapgroups");
            if (mapGroups == null)
            {
                Logger.LogError("No \"mapgroups\" section in {Path}", path);
                return null;
            }

            var result = new Dictionary<string, List<MapEntry>>(StringComparer.OrdinalIgnoreCase);
            foreach (var (groupName, groupValue) in mapGroups)
            {
                if (groupValue is not List<(string, object)> group)
                    continue;
                var maps = group.FirstOrDefault(kv => kv.Item1.Equals("maps", StringComparison.OrdinalIgnoreCase)).Item2
                    as List<(string, object)>;
                if (maps == null)
                    continue;

                result[groupName] = maps.Select(kv =>
                {
                    var parts = kv.Item1.Split('/');
                    var workshop = parts.Length == 3 && parts[0] == "workshop";
                    var name = workshop ? parts[2] : kv.Item1;
                    var display = kv.Item2 is string value && value.Length > 0 ? value : name;
                    return new MapEntry(name, workshop ? parts[1] : null, display);
                }).ToList();
            }
            return result;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Could not read {Path}", path);
            return null;
        }
    }
}

// Minimal Valve KeyValues reader: quoted keys and values, { } sections, // comments.
// A section is a List<(string, object)> (keys can repeat), a value is a string.
internal static class KeyValues
{
    public static List<(string, object)> Parse(string text)
    {
        var pos = 0;
        return ReadSection(text, ref pos, topLevel: true);
    }

    public static List<(string, object)>? FindSection(List<(string, object)> section, string key)
    {
        foreach (var (k, v) in section)
        {
            if (v is not List<(string, object)> child)
                continue;
            if (k.Equals(key, StringComparison.OrdinalIgnoreCase))
                return child;
            var found = FindSection(child, key);
            if (found != null)
                return found;
        }
        return null;
    }

    private static List<(string, object)> ReadSection(string text, ref int pos, bool topLevel)
    {
        var items = new List<(string, object)>();
        while (true)
        {
            var token = NextToken(text, ref pos);
            if (token == null)
            {
                if (topLevel)
                    return items;
                throw new FormatException("Unexpected end of file, missing }");
            }
            if (token == "}")
            {
                if (topLevel)
                    throw new FormatException($"Unexpected }} at {pos}");
                return items;
            }

            var key = token;
            var next = NextToken(text, ref pos) ?? throw new FormatException($"Missing value for \"{key}\"");
            if (next == "{")
                items.Add((key, ReadSection(text, ref pos, topLevel: false)));
            else
                items.Add((key, next));
        }
    }

    // Returns "{", "}", the content of a quoted string, or null at the end
    private static string? NextToken(string text, ref int pos)
    {
        while (pos < text.Length)
        {
            var c = text[pos];
            if (char.IsWhiteSpace(c))
            {
                pos++;
            }
            else if (c == '/' && pos + 1 < text.Length && text[pos + 1] == '/')
            {
                while (pos < text.Length && text[pos] != '\n')
                    pos++;
            }
            else if (c == '{' || c == '}')
            {
                pos++;
                return c.ToString();
            }
            else if (c == '"')
            {
                var end = text.IndexOf('"', pos + 1);
                if (end < 0)
                    throw new FormatException($"Unclosed quote at {pos}");
                var value = text.Substring(pos + 1, end - pos - 1);
                pos = end + 1;
                return value;
            }
            else
            {
                // Unquoted token (rare in this file)
                var start = pos;
                while (pos < text.Length && !char.IsWhiteSpace(text[pos]) && text[pos] != '{' && text[pos] != '}' && text[pos] != '"')
                    pos++;
                return text.Substring(start, pos - start);
            }
        }
        return null;
    }
}
