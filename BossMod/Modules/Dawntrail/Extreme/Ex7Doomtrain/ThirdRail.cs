namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

// ThirdRail spawns puddles at either ground level (Y < 4) or platform level (Y > 4).
// Players only take damage if they're on the same height as the puddle, so AOEs at the other height are non-risky.
[SkipLocalsInit]
sealed class ThirdRail(BossModule module) : Components.GenericAOEs(module, (uint)AID.ThirdRail)
{
    private readonly List<(AOEInstance AOE, bool High)> _casters = [];
    private static readonly AOEShapeCircle Circle = new(4f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        var actorHigh = actor.PosRot.Y > 4f;
        var span = CollectionsMarshal.AsSpan(_casters);
        for (var i = 0; i < count; ++i)
        {
            ref var c = ref span[i];
            var aoe = c.AOE;
            aoe.Risky = c.High == actorHigh;
            aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var origin = spell.LocXZ;
            var aoe = new AOEInstance(Circle, origin, default, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: Circle.Distance(origin, default));
            _casters.Add((aoe, spell.Location.Y > 4f));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            var id = caster.InstanceID;
            var span = CollectionsMarshal.AsSpan(_casters);
            var count = _casters.Count;
            for (var i = 0; i < count; ++i)
            {
                if (span[i].AOE.ActorID == id)
                {
                    _casters.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

[SkipLocalsInit]
sealed class ThirdRailBait(BossModule module) : BossComponent(module)
{
    public bool Active = true;
    private const float Radius = 4f;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Active)
            Arena.AddCircle(pc.Position, Radius, Colors.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ThirdRail)
            Active = false;
    }
}
