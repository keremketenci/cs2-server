using System.Runtime.InteropServices;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory;

namespace GrenadeRebuy;

// CS2 counts grenade purchases per round (CCSPlayer_ActionTrackingServices.m_weaponPurchasesThisRound) and blocks
// buying more than ammo_grenade_limit_* of a type in one round. In Free Play the round never ends, so after
// 2 flashbangs a player could never buy one again. With grenade_rebuy_enabled 1, a thrown grenade gives its
// purchase back: the carry limits still apply, but a used grenade can be bought again.
// Turned on by cfg/modes/freeplay_settings.cfg, turned off by cfg/modes/reset.cfg.
public class GrenadeRebuy : BasePlugin
{
    public override string ModuleName => "Grenade Rebuy";
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "Ghost Territory";
    public override string ModuleDescription => "grenade_rebuy_enabled: thrown grenades can be bought again in the same round";

    public FakeConVar<bool> Enabled = new("grenade_rebuy_enabled", "A thrown grenade can be bought again in the same round", false);

    // Item definition indexes of the grenades
    private static readonly Dictionary<string, ushort> DefIndexes = new()
    {
        ["flashbang"] = 43,
        ["hegrenade"] = 44,
        ["smokegrenade"] = 45,
        ["molotov"] = 46,
        ["decoy"] = 47,
        ["incgrenade"] = 48,
    };

    [GameEventHandler]
    public HookResult OnGrenadeThrown(EventGrenadeThrown @event, GameEventInfo info)
    {
        if (!Enabled.Value)
            return HookResult.Continue;

        var player = @event.Userid;
        var pawn = player?.PlayerPawn.Value;
        if (player == null || !player.IsValid || pawn == null || !pawn.IsValid)
            return HookResult.Continue;

        var weapon = @event.Weapon.Replace("weapon_", "");
        if (!DefIndexes.TryGetValue(weapon, out var defIndex))
            return HookResult.Continue;

        var purchases = pawn.ActionTrackingServices?.WeaponPurchasesThisRound.WeaponPurchases;
        if (purchases == null || purchases.Handle == IntPtr.Zero)
            return HookResult.Continue;

        // CSS's NetworkedVector indexer only supports CHandle elements, so the elements are read directly:
        // the vector is a CUtlVector { int m_Size; T* m_pMemory (offset 8); ... }, element size from the schema.
        var count = purchases.Count;
        var elementSize = Schema.GetClassSize("WeaponPurchaseCount_t");
        var elements = Marshal.ReadIntPtr(purchases.Handle, 8);
        if (count <= 0 || count > 128 || elementSize <= 0 || elementSize > 256 || elements == IntPtr.Zero)
            return HookResult.Continue;

        for (var i = 0; i < count; i++)
        {
            var entry = new WeaponPurchaseCount_t(elements + i * elementSize);
            if (entry.ItemDefIndex == defIndex && entry.Count > 0)
            {
                entry.Count--;
                break;
            }
        }
        return HookResult.Continue;
    }
}
