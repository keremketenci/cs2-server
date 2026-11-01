using System.Reflection;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace OnlineAdmins;

public class StaffRole
{
    [JsonPropertyName("Name")] public string Name { get; set; } = "";
    [JsonPropertyName("Color")] public string Color { get; set; } = "Default";
    // A player gets the role if they are in this admin group ("" = skip the group check) ...
    [JsonPropertyName("Group")] public string Group { get; set; } = "";
    // ... or have this flag ("" = skip the flag check).
    [JsonPropertyName("Flag")] public string Flag { get; set; } = "";
}

public class OnlineAdminsConfig : BasePluginConfig
{
    // Checked top to bottom, the first matching role is shown. Players without a role are not listed.
    [JsonPropertyName("Roles")]
    public List<StaffRole> Roles { get; set; } =
    [
        new() { Name = "OWNER", Color = "Red", Flag = "@css/root" },
        new() { Name = "ADMIN", Color = "Blue", Group = "#admin", Flag = "@css/config" },
        new() { Name = "MODERATOR", Color = "Orange", Group = "#moderator", Flag = "@css/ban" },
    ];
}

public class OnlineAdmins : BasePlugin, IPluginConfig<OnlineAdminsConfig>
{
    public override string ModuleName => "Online Admins";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Ghost Territory";
    public override string ModuleDescription => "!admins: lists online staff with their role (replaces the abandoned Admin List plugin)";

    public OnlineAdminsConfig Config { get; set; } = new();

    public void OnConfigParsed(OnlineAdminsConfig config) => Config = config;

    [ConsoleCommand("css_admins", "Lists the staff that is online")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnAdminsCommand(CCSPlayerController? caller, CommandInfo command)
    {
        var staff = Utilities.GetPlayers()
            .Where(p => p is { IsValid: true, IsBot: false, IsHLTV: false })
            .Select(p => (Player: p, Role: FindRole(p)))
            .Where(x => x.Role != null)
            .OrderBy(x => Config.Roles.IndexOf(x.Role!))
            .ThenBy(x => x.Player.PlayerName)
            .ToList();

        Reply(caller, $" {ChatColors.Gold}========== {ChatColors.Blue}ONLINE STAFF {ChatColors.Gold}==========");

        if (staff.Count == 0)
        {
            Reply(caller, $" {ChatColors.Grey}No staff online right now.");
            return;
        }

        foreach (var (player, role) in staff)
        {
            var color = ParseColor(role!.Color);
            Reply(caller, $" {color}[{role.Name}] {ChatColors.Default}{player.PlayerName}");
        }
    }

    private StaffRole? FindRole(CCSPlayerController player)
    {
        foreach (var role in Config.Roles)
        {
            bool inGroup = role.Group.Length > 0 && AdminManager.PlayerInGroup(player, role.Group);
            bool hasFlag = role.Flag.Length > 0 && AdminManager.PlayerHasPermissions(player, role.Flag);
            if (inGroup || hasFlag)
                return role;
        }
        return null;
    }

    private static char ParseColor(string name)
    {
        var field = typeof(ChatColors).GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
        return field?.GetValue(null) is char c ? c : ChatColors.Default;
    }

    private static void Reply(CCSPlayerController? caller, string message)
    {
        if (caller == null)
            Server.PrintToConsole(message);
        else
            caller.PrintToChat(message);
    }
}
