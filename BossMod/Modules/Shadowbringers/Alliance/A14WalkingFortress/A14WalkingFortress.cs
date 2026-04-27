namespace BossMod.Shadowbringers.Alliance.A14WalkingFortress;

class LaserSaturation(BossModule module) : Components.RaidwideCast(module, (uint)AID.LaserSaturation);
class LaserTurret(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LaserTurret, new AOEShapeRect(90f, 4f));
class BallisticImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BallisticImpactLocation, 6f);
class BallisticImpactSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BallisticImpactSpread, 6f);
class LaserSuppression(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LaserSuppression, new AOEShapeCone(60f, 45f.Degrees()));
class MarxImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarxImpact, 22f);
class Neutralization(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Neutralization);
class BossInvincible(BossModule module) : Components.InvincibleStatus(module, (uint)SID.Invincibility);
class GoliathTank(BossModule module) : Components.Adds(module, (uint)OID.GoliathTank);

class GoliathTankLaserTurret(BossModule module) : Components.GenericAOEs(module, (uint)AID.LaserTurretTank)
{
    private readonly List<(Actor caster, Angle direction, DateTime activation)> _predicted = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.TurretLockon && Module.Enemies((uint)OID.GoliathTank).Closest(actor.Position) is { } tank)
            _predicted.Add((tank, tank.AngleTo(actor), WorldState.FutureTime(3.3d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _predicted.RemoveAll(p => p.caster == caster);
        }
    }

    public override void Update()
    {
        _predicted.RemoveAll(p => !p.caster.IsTargetable);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _predicted.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var p = _predicted[i];
            aoes[i] = new AOEInstance(new AOEShapeRect(85f, 5f), p.caster.Position, p.direction, p.activation);
        }
        return aoes;
    }
}

// actual radius (9.6 units) is too wide
class ConvenientSelfDestruction(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.ConvenientSelfDestructionLOS, 85f, false, false, true)
{
    private readonly List<Actor> _blockers = [];

    public override ReadOnlySpan<Actor> BlockerActors()
    {
        _blockers.Clear();
        var enemies = Module.Enemies((uint)OID.GoliathTank);
        var count = enemies.Count;
        for (var i = 0; i < count; ++i)
        {
            var t = enemies[i];
            if (t.CastInfo == null)
                _blockers.Add(t);
        }
        return CollectionsMarshal.AsSpan(_blockers);
    }
}

class ConvenientSelfDestruction2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ConvenientSelfDestruction, 22f);
class SerialJointedServiceModel(BossModule module) : Components.Adds(module, (uint)OID.SerialJointedServiceModel);
class ClangingBlow(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ClangingBlow);

class DeployDefenses(BossModule module) : Components.GenericAOEs(module, (uint)AID.TotalAnnihilationManeuver, warningText: "Go to safe spot!")
{
    private Actor? _shield;
    private Actor? _caster;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster is { } c && _shield is { } s && c.CastInfo != null)
            return new[] { new AOEInstance(new AOEShapeDonut(6f, 80f), s.Position, default, Module.CastFinishAt(c.CastInfo)) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _caster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == 18671u)
            _shield = caster;

        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _caster = null;
            _shield = null;
        }
    }
}

class ShrapnelImpact(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ShrapnelImpact, 6f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9153)]
public class A14WalkingFortress(WorldState ws, Actor primary) : BossModule(ws, primary, new(900f, 427f), new ArenaBoundsSquare(30f));
