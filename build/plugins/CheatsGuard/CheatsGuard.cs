using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;

namespace CheatsGuard;

// Lets cfg files turn sv_cheats on for a few cheat-protected cvars without switching off cheats
// an admin enabled with !ch1. Usage in a cfg:
//   cheats_push   (remember sv_cheats, set it to 1)
//   ...cvars...
//   cheats_pop    (put sv_cheats back to the remembered value)
// The cvar is set directly (not through the command buffer), so it changes in order with the cfg lines.
public class CheatsGuard : BasePlugin
{
    public override string ModuleName => "Cheats Guard";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Ghost Territory";
    public override string ModuleDescription => "cheats_push / cheats_pop: cfg files restore sv_cheats instead of always turning it off";

    // Value before the outermost cheats_push; null when nothing is pushed
    private bool? _saved;

    [ConsoleCommand("cheats_push", "Remembers sv_cheats and sets it to 1")]
    [CommandHelper(whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnCheatsPush(CCSPlayerController? player, CommandInfo command)
    {
        var cheats = ConVar.Find("sv_cheats");
        if (cheats == null)
            return;

        // A nested push (cfg executing another cfg) keeps the value from before the first push
        _saved ??= cheats.GetPrimitiveValue<bool>();
        cheats.SetValue(true);
    }

    [ConsoleCommand("cheats_pop", "Puts sv_cheats back to the value saved by cheats_push")]
    [CommandHelper(whoCanExecute: CommandUsage.SERVER_ONLY)]
    public void OnCheatsPop(CCSPlayerController? player, CommandInfo command)
    {
        var cheats = ConVar.Find("sv_cheats");
        if (cheats == null || _saved == null)
            return;

        cheats.SetValue(_saved.Value);
        _saved = null;
    }
}
