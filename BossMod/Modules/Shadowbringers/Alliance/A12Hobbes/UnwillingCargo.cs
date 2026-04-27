namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

class UnwillingCargoRing(BossModule module) : Components.GenericAOEs(module)
{
    public bool Active { get; private set; }
    private static readonly AOEShapeDonut _shape = new(17.5f, 20f);
    private static readonly WPos _center = new(-805f, -270f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            return [];
        return new[] { new AOEInstance(_shape, _center) };
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 4)
        {
            if (state == 0x00020001u)
                Active = true;
            else if (state == 0x00080004u)
                Active = false;
        }
    }
}

class UnwillingCargo(BossModule module) : Components.GenericKnockback(module, (uint)AID.UnwillingCargo, stopAtWall: true)
{
    public DateTime Activation { get; private set; }

    // blue = knockback west, pink = knockback east
    enum Pattern
    {
        None,
        Blue,
        Pink,
        BluePink,
        PinkBlue
    }
    private Pattern _pattern;

    private static WDir ExpectedDirection(Pattern pattern, WPos p)
    {
        var stripe = MathF.Abs(p.Z + 270f) switch
        {
            < 3.5f => 1,
            < 10.5f => 0,
            < 17.5f => 1,
            _ => -1
        };
        if (stripe < 0)
            return default;

        var blue = new WDir(-15f, 0f);
        var pink = new WDir(15f, 0f);

        return pattern switch
        {
            Pattern.Blue => blue,
            Pattern.Pink => pink,
            Pattern.BluePink => stripe == 1 ? blue : pink,
            Pattern.PinkBlue => stripe == 1 ? pink : blue,
            _ => default
        };
    }

    public bool IsAffected(Actor actor) => Activation > WorldState.CurrentTime && MathF.Abs(actor.Position.Z + 270f) < 20f;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !pos.InCircle(new WPos(-805f, -270f), 17.5f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (IsImmune(slot, Activation))
            return [];

        var dir = ExpectedDirection(_pattern, actor.Position);
        if (dir == default)
            return [];
        return new[] { new Knockback(actor.Position, 15f, Activation, direction: Angle.FromDirection(dir), kind: Kind.DirForward) };
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 5)
        {
            switch (state)
            {
                case 0x00200020u:
                    SetPattern(Pattern.Blue);
                    break;
                case 0x01000100u:
                    SetPattern(Pattern.Pink);
                    break;
                case 0x08000800u:
                    SetPattern(Pattern.BluePink);
                    break;
                case 0x40004000u:
                    SetPattern(Pattern.PinkBlue);
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _pattern = Pattern.None;
        }
    }

    private void SetPattern(Pattern p)
    {
        _pattern = p;
        Activation = WorldState.FutureTime(6.1d);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_pattern == Pattern.None || IsImmune(slot, Activation))
            return;

        var pat = _pattern;
        hints.AddForbiddenZone(new SDCustomKnockback(pat), Activation);
    }

    private sealed class SDCustomKnockback(Pattern pattern) : ShapeDistance
    {
        private readonly Pattern _pattern = pattern;
        private static readonly WPos _center = new(-805f, -270f);

        public override float Distance(in WPos p)
        {
            var dir = ExpectedDirection(_pattern, p);
            if (dir == default)
                return 1f; // not affected, treat as safe
            var dest = p + dir;
            return dest.InCircle(_center, 17.5f) ? 1f : -1f;
        }
    }
}
