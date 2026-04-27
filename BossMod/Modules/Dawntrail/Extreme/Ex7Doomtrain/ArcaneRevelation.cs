namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

[SkipLocalsInit]
sealed class HailOfThunder(BossModule module) : Components.GenericAOEs(module)
{
    private int _advance;
    private int _lastPos;
    private DateTime _nextActivation;

    private static readonly WDir[] Sources = [new(0f, -10f), new(-4.8f, 0f), new(0f, 10f), new(4.8f, 0f)];
    private static readonly AOEShapeCircle Circle = new(16f);

    private AOEInstance[] _next = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _next;

    public override void Update()
    {
        if (_advance == 0)
            return;

        var indicators = Module.Enemies((uint)OID.ArcaneRevelationIndicator);
        if (indicators.Count == 0)
        {
            ReportError("Indicator not found, can't predict AOE");
            _advance = 0;
            return;
        }

        var indicator = indicators[0];
        if (indicator.LastFrameMovement == default)
            return;

        var sign = MathF.Sign(indicator.LastFrameMovement.OrthoL().Dot(indicator.DirectionTo(Arena.Center)));
        _lastPos += _advance * sign;
        if (_lastPos < 0)
            _lastPos += 4;
        _lastPos %= 4;

        var origin = Arena.Center + Sources[_lastPos];
        _next = [new(Circle, origin, default, _nextActivation, shapeDistance: Circle.Distance(origin, default))];
        _advance = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HailOfThunderVisual2: // short
                _advance = 2;
                _nextActivation = WorldState.FutureTime(7.5d);
                break;
            case (uint)AID.HailOfThunderVisual3: // medium
                _advance = 3;
                _nextActivation = WorldState.FutureTime(10.5d);
                break;
            case (uint)AID.HailOfThunderVisual1: // long
                _advance = 4;
                _nextActivation = WorldState.FutureTime(13.4d);
                break;
            case (uint)AID.HailOfThunder1:
                _advance = 0;
                _next = [];
                ++NumCasts;
                break;
        }
    }
}

[SkipLocalsInit]
sealed class DesignatedConductor(BossModule module) : Components.GenericStackSpread(module)
{
    private BitMask _conductors;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DesignatedConductor)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _conductors.Set(slot);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DesignatedConductor)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _conductors.Clear(slot);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HailOfThunderVisual1: // long
                Predict(13.4d);
                break;
            case (uint)AID.HailOfThunderVisual2: // short
                Predict(7.5d);
                break;
            case (uint)AID.HailOfThunderVisual3: // medium
                Predict(10.5d);
                break;
            case (uint)AID.HyperconductivePlasma:
                if (Stacks.Count != 0)
                    Stacks.RemoveAt(0);
                break;
        }
    }

    private void Predict(double advance)
    {
        var act = WorldState.FutureTime(advance);
        var party = Raid.WithSlot(false, true, true);
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var ip = ref party[i];
            if (_conductors[ip.Item1])
                Stacks.Add(new(ip.Item2, 13f, minSize: 3, activation: act));
        }
    }
}
