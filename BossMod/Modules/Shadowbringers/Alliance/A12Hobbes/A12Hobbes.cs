namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

class LaserResistanceTest(BossModule module) : Components.RaidwideCast(module, (uint)AID.LaserResistanceTest);

class ShockingDischarge(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.ShockingDischarge, GetVoidzones, 1.03d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.ShockingDischargeVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];
        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class VariableCombatTestCone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.VariableCombatConeSlow, (uint)AID.VariableCombatConeFast], new AOEShapeCone(20f, 30f.Degrees()));
class VariableCombatTestCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.VariableCombatCircleSlow, (uint)AID.VariableCombatCircleFast], 2f);
class VariableCombatTestDonut(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.VariableCombatDonutSlow, (uint)AID.VariableCombatDonutFast], new AOEShapeDonut(7f, 19f));

class RingLaser(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeDonut(15f, 20f), new AOEShapeDonut(10f, 15f), new AOEShapeDonut(5f, 10f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RingLaser1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var ix = spell.Action.ID switch
        {
            (uint)AID.RingLaser1 => 0,
            (uint)AID.RingLaser2 => 1,
            (uint)AID.RingLaser3 => 2,
            _ => -1
        };
        AdvanceSequence(ix, caster.Position, WorldState.FutureTime(2.1d));
    }
}

class LaserSight(BossModule module) : Components.GenericWildCharge(module, 4f, default, 60f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LaserSightTarget)
        {
            var slot = Raid.FindSlot(spell.MainTargetID);
            if (slot >= 0)
            {
                Source = Module.PrimaryActor;
                PlayerRoles[slot] = PlayerRole.Target;
                for (var i = 0; i < PlayerRoles.Length; ++i)
                    if (PlayerRoles[i] == PlayerRole.Ignore)
                        PlayerRoles[i] = PlayerRole.Share;
            }
        }

        if (spell.Action.ID == (uint)AID.LaserSightStack)
        {
            ++NumCasts;
            Array.Fill(PlayerRoles, PlayerRole.Ignore);
        }
    }
}

class ShortRangeMissile(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ShortRangeMissile, 8f)
{
    private readonly UnwillingCargo? _cargo = module.FindComponent<UnwillingCargo>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_cargo?.IsAffected(actor) == true)
            return;

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_cargo?.IsAffected(actor) == true)
            return;

        base.AddHints(slot, actor, hints);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9143)] //9144 and 9145 also listed as Hobbes
public class A12Hobbes(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, CircleBounds)
{
    public static readonly WPos ArenaCenter = new(-805f, -240f);
    public static readonly WDir[] PlatformOffsets = [new(-26f, 15f), new(0f, -30f), new(26f, 15f)];
    public static readonly WPos[] PlatformCenters = [.. PlatformOffsets.Select(d => ArenaCenter + d)];
    public static readonly ArenaBoundsCustom CircleBounds = new([.. PlatformOffsets.Select(p => new Polygon(ArenaCenter + p, 20f, 64))]);
}
