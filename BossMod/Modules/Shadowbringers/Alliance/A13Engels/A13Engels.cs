namespace BossMod.Shadowbringers.Alliance.A13Engels;

class DemolishStructureArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect square = new(5f, 5f, 5f, invertForbiddenZone: true);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DemolishStructure2 && Arena.Bounds == A13MarxEngels.StartingBounds)
        {
            _aoe = [new(square, A13MarxEngels.TransitionSpot, color: Colors.SafeFromAOE)];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0B && state == 0x00020001u)
        {
            Arena.Center = A13MarxEngels.SecondArenaCenter;
            Arena.Bounds = A13MarxEngels.StartingBounds;
            _aoe = [];
        }
    }
}

class MarxSmash1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash1, new AOEShapeRect(60, 15));
class MarxSmash2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash2, new AOEShapeRect(60, 15));
class MarxSmash3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash3, new AOEShapeRect(60, 15));
class MarxSmash4(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash4, new AOEShapeRect(30, 30));
class MarxSmash5(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash5, new AOEShapeRect(35, 30));
class MarxSmash6(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash6, new AOEShapeRect(60, 10));
class MarxSmash7(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxSmash7, new AOEShapeRect(60, 10));

class MarxCrush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxCrush, new AOEShapeRect(15, 15));
class MarxThrust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxThrust2, new AOEShapeRect(30f, 10f));
class CrushingWheel(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrushingWheel2, new AOEShapeRect(20f, 15f));
class IncendiarySaturationBombing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IncendiarySaturationBombing2, new AOEShapeRect(30f, 30f));
class DemolishStructure(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DemolishStructure2, new AOEShapeCircle(25f));
class ArmLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArmLaser, new AOEShapeCone(30f, 45f.Degrees()));

class PrecisionGuidedMissile2(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.PrecisionGuidedMissile2, 6);
class GuidedMissile2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GuidedMissile2, 6);
class IncendiaryBombing2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IncendiaryBombing2, 8);

class IncendiaryBombing1(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.IncendiaryBombing2, 8f, 9.1d)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == SpreadAction)
            Spreads.Clear();
    }
}

class IncendiaryBombingVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 8f, (uint)AID.IncendiaryBombing2, GetVoidzones, 0.1d)
{
    private static List<Actor> GetVoidzones(BossModule module)
    {
        var bombs = module.Enemies((uint)OID.IncendiaryBomb);
        var count = bombs.Count;
        List<Actor> result = new(count);
        for (var i = 0; i < count; ++i)
        {
            var b = bombs[i];
            if (b.EventState != 7)
                result.Add(b);
        }
        return result;
    }
}

class DiffuseLaser(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiffuseLaser);
class SurfaceMissile2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurfaceMissile2, 6);

class GuidedMissile(BossModule module) : Components.StandardChasingAOEs(module, 6f, (uint)AID.GuidedMissile2, (uint)AID.GuidedMissile3, 5.5f, 1d, 4, true, (uint)IconID.GuidedMissile);

class GuidedMissileBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6f), (uint)IconID.GuidedMissile, (uint)AID.GuidedMissile2, 5.1d, centerAtTarget: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var baits = ActiveBaitsOn(actor);
        if (baits.Count != 0)
        {
            var bait = baits[0];
            hints.AddForbiddenZone(new AOEShapeRect(25f, 25f, 25f), Arena.Center, activation: bait.Activation);
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GuidedMissile2)
            CurrentBaits.Clear();
    }
}

class LaserSight1(BossModule module) : Components.GenericAOEs(module, (uint)AID.LaserSight1)
{
    private readonly List<Actor> _casters = [];
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(100f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        _aoes.Clear();
        var count = _casters.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = _casters[i];
            _aoes.Add(new(rect, c.Position, c.Rotation, Module.CastFinishAt(c.CastInfo)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction || spell.Action.ID == (uint)AID.LaserSight2)
        {
            ++NumCasts;
            if (NumCasts >= 5)
            {
                _casters.Clear();
                NumCasts = 0;
            }
        }
    }
}

class EnergyDispersal(BossModule module) : Components.GenericTowers(module, (uint)AID.EnergyDispersal)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.EnergyDispersalTower)
            Towers.Add(new(actor.Position, 4f, maxSoakers: int.MaxValue, activation: WorldState.FutureTime(12.1d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1f));
        }
    }
}

class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.SmallBiped, (uint)OID.ReverseJointedGoliath]);
class AddsArms(BossModule module) : Components.AddsMulti(module, [(uint)OID.MarxR, (uint)OID.MarxL]);

class IncendiarySaturationBombingVoidzone(BossModule module) : BossComponent(module)
{
    private WPos _oldCenter;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.IncendiarySaturationBombing2)
        {
            _oldCenter = Arena.Center;
            Arena.Center += new WDir(default, -15f);
            Arena.Bounds = new ArenaBoundsRect(30f, 15f);
        }
        if (spell.Action.ID == (uint)AID.MarxCrush)
            Arena.Bounds = new ArenaBoundsSquare(15f);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0C && state == 0x00080004u)
        {
            Arena.Center = _oldCenter;
            Arena.Bounds = new ArenaBoundsSquare(30f);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9147)]
public class A13MarxEngels(WorldState ws, Actor primary) : BossModule(ws, primary, StartingArenaCenter, StartingBounds)
{
    public static readonly WPos TransitionSpot = new(900, 697);
    public static readonly WPos StartingArenaCenter = new(900, 670);
    public static readonly WPos SecondArenaCenter = new(900, 785);
    public static readonly ArenaBoundsSquare StartingBounds = new(30);
}
