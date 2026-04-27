namespace BossMod.Shadowbringers.Alliance.A11CommandModel;

class ForcefulImpact(BossModule module) : Components.RaidwideCast(module, (uint)AID.ForcefulImpact);
class ClangingBlow(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ClangingBlow);
class CentrifugalSpin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CentrifugalSpin1, new AOEShapeRect(30f, 4f));
class Shockwave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Shockwave, 15f);
class HighCaliberLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HighCaliberLaser, new AOEShapeRect(70f, 12f))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var baseAoes = base.ActiveAOEs(slot, actor);
        var len = baseAoes.Length;
        if (len == 0)
            return [];

        var group1 = DateTime.MinValue;
        var result = new AOEInstance[len];
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref baseAoes[i];
            if (group1 == DateTime.MinValue)
                group1 = aoe.Activation.AddSeconds(0.5d);
            var inFirstGroup = aoe.Activation < group1;
            result[i] = aoe with { Color = inFirstGroup ? Colors.Danger : Colors.AOE, Risky = inFirstGroup };
        }
        return result;
    }
}

class EnergyBomb(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _balls = [];
    private static readonly AOEShapeCircle _shape = new(2f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _balls.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
            aoes[i] = new(_shape, _balls[i].Position.Quantized());
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.EnergyBomb)
            _balls.Remove(caster);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.Turret2)
        {
            if (id == 0x11D2)
                _balls.Add(actor);
            if (id == 0x11E7)
                _balls.Remove(actor);
        }
    }
}

// there's no good way to figure out which player is baiting which turret so we treat them like regular AOEs
class HighPoweredLaser(BossModule module) : Components.GenericAOEs(module, (uint)AID.HighPoweredLaser)
{
    public readonly List<Actor> Turrets = [];
    public DateTime Activation { get; private set; }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Activation == default)
            return [];
        var count = Turrets.Count;
        var aoes = new AOEInstance[count];
        var risky = WorldState.FutureTime(1d) > Activation;
        for (var i = 0; i < count; ++i)
        {
            var t = Turrets[i];
            aoes[i] = new AOEInstance(new AOEShapeRect(40f, 2f), t.Position, t.Rotation, Activation, risky: risky);
        }
        return aoes;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.Turret1 && id == 0x1E43)
            Turrets.Add(actor);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LockOn && Activation == default)
            Activation = WorldState.FutureTime(6.6d);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            Turrets.Remove(caster);
        }
    }
}

class SidestrikingSpin(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SidestrikingSpin1, (uint)AID.SidestrikingSpin2], new AOEShapeRect(30f, 6f));
class EnergyBombardment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EnergyBombardment1, 4f);

class EnergyAssault(BossModule module) : Components.GenericAOEs(module, (uint)AID.EnergyAssault1)
{
    private readonly List<(Actor caster, DateTime activation)> Casters = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var c = Casters[i];
            aoes[i] = new AOEInstance(new AOEShapeCone(30f, 45f.Degrees()), c.caster.Position, c.caster.CastInfo?.Rotation ?? c.caster.Rotation, c.activation);
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EnergyAssault)
            Casters.Add((caster, Module.CastFinishAt(spell, 2.2f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            if (NumCasts >= 5)
                Casters.Clear();
        }
    }
}

class AirToSurfaceEnergy(BossModule module) : Components.GenericAOEs(module, (uint)AID.AirToSurfaceEnergy)
{
    private static readonly List<WPos> _centers = [.. CurveApprox.Rect(new(12f, 0f), new(0f, 12f)).Select(c => new WPos(-500f, 0f) + c)];
    private readonly List<AOEInstance> _predicted = [];

    public int NumStarts;
    public const int AOEsToShow = 5;

    public static readonly List<WDir> PatternOut = [
        new(10f, -4.6f),
        new(10f, -7.8f),
        new(9.1f, -10f),
        new(5.9f, -10f),
        new(2.7f, -10f),
        new(-0.5f, -10f),
        new(-3.7f, -10f),
        new(-7f, -10f),
        new(-10f, -10f),
        new(-10f, -6.8f),
        new(-10f, -3.6f),
        new(-10f, -0.4f),
        new(-10f, 2.8f),
        new(-10f, 6f),
        new(-10f, 9.2f)
    ];
    public static readonly List<WDir> PatternIn = [
        new(2.4f, 4f),
        new(4f, 2.5f),
        new(4f, -0.7f),
        new(4f, -3.9f),
        new(1.2f, -4f),
        new(-2f, -4f),
        new(-4f, -2.9f),
        new(-4f, 0.3f),
        new(-4f, 3.5f),
        new(-1.4f, 4f),
        new(1.7f, 4f),
        new(4f, 3.2f),
        new(4f, 0f),
        new(4f, -3.2f),
        new(1.9f, -4f)
    ];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _predicted.Count;
        if (count == 0)
            return [];
        var max = Math.Min(count, AOEsToShow * 8);
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            var p = _predicted[i];
            aoes[max - 1 - i] = p with { Color = i < 8 ? Colors.Danger : Colors.AOE };
        }
        return aoes;
    }

    public override void Update()
    {
        // remove garbage from minimap if the component screws up
        _predicted.RemoveAll(p => p.Activation.AddSeconds(5d) < WorldState.CurrentTime);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AirToSurfaceAppear)
        {
            ++NumStarts;
            var pivot = _centers.MinBy(c => (c - caster.Position).Length());
            var close = caster.Position.AlmostEqual(pivot, 8f);
            var patternBase = close ? PatternIn : PatternOut;
            var pattern = NumStarts < 9 ? patternBase.Take(10) : patternBase;

            (Angle closeRot, Angle farRot) = Angle.FromDirection(caster.Position - pivot).Deg switch
            {
                > 90f => (-90f.Degrees(), 90f.Degrees()),
                > 0f => (180f.Degrees(), default(Angle)),
                > -90f => (90f.Degrees(), -90f.Degrees()),
                _ => (default(Angle), 180f.Degrees())
            };

            var start = WorldState.FutureTime(12.2d);
            foreach (var p in pattern)
            {
                _predicted.Add(new AOEInstance(new AOEShapeCircle(5f), pivot + p.Rotate(close ? closeRot : farRot), default, start));
                start = start.AddSeconds(1.1d);
            }
            _predicted.Sort((a, b) => a.Activation.CompareTo(b.Activation));
        }

        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            var ix = _predicted.FindIndex(p => p.Origin.AlmostEqual(caster.Position, 0.5f));
            if (ix < 0)
                ReportError($"missing predicted cast for {caster} at {caster.Position}");
            else
                _predicted.RemoveAt(ix);
        }
    }
}

class EnergyRing(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(12f), new AOEShapeDonut(12f, 24f), new AOEShapeDonut(24f, 36f), new AOEShapeDonut(36f, 48f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EnergyRing1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = spell.Action.ID switch
        {
            (uint)AID.EnergyRing1 => 0,
            (uint)AID.EnergyRing2 => 1,
            (uint)AID.EnergyRing3 => 2,
            (uint)AID.EnergyRing4 => 3,
            _ => -1
        };
        AdvanceSequence(order, caster.Position, WorldState.FutureTime(2d));
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9141)] //Other service models 9142 and 9155, nonservice model 9923
public class A11CommandModel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-500f, 0f), new ArenaBoundsSquare(23.5f));
