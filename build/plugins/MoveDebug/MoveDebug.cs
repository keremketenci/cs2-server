using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory;

namespace MoveDebug;

// !movedebug (root only): prints your pawn's movement state to chat and the movement cvars to the server console.
// Found the AWP noclip problem: awp_bhop_rocket sets sv_maxspeed 10000 through its own map commands.
public class MoveDebug : BasePlugin
{
    public override string ModuleName => "Move Debug";
    public override string ModuleVersion => "1.1.0";
    public override string ModuleAuthor => "Ghost Territory";
    public override string ModuleDescription => "!movedebug: movement state and movement cvars, for debugging";

    [ConsoleCommand("css_movedebug", "Prints your pawn's movement state")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    [RequiresPermissions("@css/root")]
    public void OnMoveDebug(CCSPlayerController? player, CommandInfo command)
    {
        var pawn = player?.PlayerPawn.Value;
        if (player == null || pawn == null || !pawn.IsValid)
            return;

        var actual = Schema.GetSchemaValue<byte>(pawn.Handle, "CBaseEntity", "m_nActualMoveType");
        var line1 = $"MoveType={pawn.MoveType} Actual={actual} Flags={pawn.Flags} Alive={player.PawnIsAlive}";
        // Only members that also exist in the server's CounterStrikeSharp (1.0.379 removed CBaseEntity.Speed)
        var vel = pawn.AbsVelocity;
        var line2 = $"VelMod={pawn.VelocityModifier} Gravity={pawn.GravityScale} sv_cheats={ConVar.Find("sv_cheats")?.GetPrimitiveValue<bool>()}";
        var line3 = $"Vel=({vel.X:0},{vel.Y:0},{vel.Z:0}) |{vel.Length():0}| Parent={pawn.CBodyComponent?.SceneNode?.PParent?.Owner?.DesignerName ?? "none"}";

        // Game state: "can look around but not move, no respawn" fits a stuck freeze time or a pause
        var rules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules;
        var line4 = rules == null
            ? "GameRules=none"
            : $"Freeze={rules.FreezePeriod} Warmup={rules.WarmupPeriod} WaitResume={rules.MatchWaitingForResume} " +
              $"TechPause={rules.TechnicalTimeOut} TPause={rules.TerroristTimeOutActive} CTPause={rules.CTTimeOutActive} " +
              $"Phase={rules.GamePhase} RoundTime={rules.RoundTime} Now={Server.CurrentTime:0.0} RoundStart={rules.RoundStartTime:0.0} Restart={rules.RestartRoundTime:0.0}";
        var line5 = $"Team={player.Team} Observer={pawn.ObserverServices?.ObserverMode} " +
                    $"mp_freezetime={ConVar.Find("mp_freezetime")?.StringValue} mp_warmup_pausetimer={ConVar.Find("mp_warmup_pausetimer")?.StringValue}";

        foreach (var line in new[] { line1, line2, line3, line4, line5 })
        {
            player.PrintToChat($" [movedebug] {line}");
            Console.WriteLine($"[movedebug] {player.PlayerName}: {line}");
        }

        // Movement cvars (console only, too long for chat): compare a stuck noclip with a working one
        string[] cvars =
        [
            "sv_noclipspeed", "sv_noclipaccelerate", "sv_maxspeed", "sv_friction", "sv_stopspeed", "sv_accelerate",
            "sv_accelerate_use_weapon_speed", "sv_airaccelerate", "sv_air_max_wishspeed", "sv_maxvelocity", "sv_gravity",
            "sv_jump_impulse", "sv_enablebunnyhopping", "sv_autobunnyhopping", "sv_staminamax", "sv_staminajumpcost",
            "sv_staminalandcost", "sv_staminarecoveryrate", "sv_standable_normal", "sv_walkable_normal",
            "sv_timebetweenducks", "sv_jump_precision_enable", "sv_ladder_scale_speed", "sv_wateraccelerate",
            "sv_water_slow_amount", "sv_falldamage_scale", "sv_cheats",
        ];
        var values = cvars.Select(name => $"{name}={ConVar.Find(name)?.StringValue ?? "missing"}");
        Console.WriteLine($"[movedebug] {player.PlayerName} cvars: {string.Join(" ", values)}");
        player.PrintToChat(" [movedebug] cvars written to the server console");
    }

}
