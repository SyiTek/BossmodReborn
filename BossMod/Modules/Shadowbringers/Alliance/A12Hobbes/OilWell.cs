namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

class OilWell(BossModule module) : Components.GenericAOEs(module, (uint)AID.OilWell)
{
    public DateTime Activation { get; private set; }
    private bool _inverted;
    private static readonly List<WPos> Platforms = MakePlatforms();

    private static List<WPos> MakePlatforms()
    {
        var centers = CurveApprox.Rect(new(8f, 0f), new(0f, 8f));
        WPos platformCenter = new(-779f, -225f);
        return [.. centers.Select(c => platformCenter + c.Rotate(-120f.Degrees()))];
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Activation == default)
            return [];
        var count = Platforms.Count;
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            aoes[i] = new AOEInstance(new AOEShapeCircle(6f), Platforms[i], default, Activation, _inverted ? Colors.SafeFromAOE : default, !_inverted);
        }
        return aoes;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation != default && Relevant(actor))
        {
            var platformZones = new ShapeDistance[Platforms.Count];
            for (var i = 0; i < Platforms.Count; ++i)
                platformZones[i] = new SDCircle(Platforms[i], 6f);

            // when inverted, players must STAY on a platform (forbidden = outside any platform)
            // when not inverted, players must AVOID platforms (forbidden = inside any platform)
            ShapeDistance forbidden = _inverted ? new SDOutsideOfUnion(platformZones) : new SDUnion(platformZones);
            hints.AddForbiddenZone(forbidden, Activation);
        }
    }

    static bool Relevant(Actor a) => a.Position.InCircle(new(-779f, -225f), 20f);
    static bool OnPlatform(WPos a) => Platforms.Any(p => p.InCircle(a, 6f));

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (Activation != default && _inverted && Relevant(actor) && !OnPlatform(actor.Position))
            hints.Add("Go to platform!");
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 6)
        {
            switch (state)
            {
                case 0x01000080u:
                    Activation = WorldState.FutureTime(4.2d);
                    _inverted = false;
                    break;
                case 0x00200010u:
                    Activation = WorldState.FutureTime(4.2d);
                    _inverted = true;
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            Activation = default;
        }
    }
}
