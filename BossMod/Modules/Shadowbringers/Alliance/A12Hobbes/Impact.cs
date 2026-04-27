namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

class Impact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Impact, 15f);

class Towerfall(BossModule module) : Components.GenericAOEs(module, (uint)AID.Towerfall)
{
    private readonly List<AOEInstance> _predicted = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_predicted);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            var ix = _predicted.FindIndex(p => p.Origin.AlmostEqual(caster.Position, 1f) && p.Rotation.AlmostEqual(spell.Rotation, 0.1f));
            if (ix < 0)
                ReportError($"missing cast for {spell.Rotation}");
            else
                _predicted.RemoveAt(ix);
        }

        var deg = spell.Action.ID switch
        {
            (uint)AID.ImpactRotation1 => 0f,
            (uint)AID.ImpactRotation2 => 67.5f,
            (uint)AID.ImpactRotation3 => 45f,
            (uint)AID.ImpactRotation4 => 22.5f,
            _ => -1f
        };

        if (deg >= 0f)
        {
            var rotation = deg.Degrees();
            for (var i = 0; i < 4; ++i)
            {
                _predicted.Add(new(new AOEShapeRect(20f, 4f), caster.Position, caster.Rotation + rotation, WorldState.FutureTime(3d)));
                rotation += 90f.Degrees();
            }
        }
    }
}

class ConvenientSelfDestruction(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(5f), (uint)TetherID.ConvenientSelfDestruction, (uint)AID.ConvenientSelfDestruction, centerAtTarget: true)
{
    public DateTime Activation { get; private set; }

    // with this much time remaining before explosion, treat it as a regular spread mechanic, since trying to pass tethers might kill more people
    public const float PanicTime = 1.2f;

    private IEnumerable<Bait> ImportantBaits(Actor a)
    {
        var plat = A12Hobbes.PlatformCenters.MinBy(p => (p - a.Position).LengthSq());
        return CurrentBaits.Where(b => b.Source.Position.InCircle(plat, 20f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.Impact)
            Activation = WorldState.FutureTime(9.5d);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (WorldState.FutureTime(PanicTime) > Activation)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        if (actor.Role == Role.Tank)
        {
            var playerBaits = ImportantBaits(actor).Where(b => b.Target.Role != Role.Tank).Select(b => (ShapeDistance)new SDRect(b.Source.Position, b.Target.Position, 1f)).ToList();
            if (playerBaits.Count > 0)
            {
                hints.AddForbiddenZone(new SDInvertedUnion([.. playerBaits]), Activation);
                return;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (WorldState.FutureTime(PanicTime) > Activation)
        {
            base.AddHints(slot, actor, hints);
            return;
        }

        if (actor.Role == Role.Tank)
        {
            if (ImportantBaits(actor).Any(b => b.Target.Role != Role.Tank))
            {
                hints.Add("Take tethers from party!");
                return;
            }
        }
        else
        {
            var haveTank = Raid.WithoutSlot(excludeAlliance: true, includeDead: false).Any(p => p.Role == Role.Tank);
            if (ActiveBaitsOn(actor).Any() && haveTank)
            {
                hints.Add("Pass tether to tank!");
                return;
            }
        }

        base.AddHints(slot, actor, hints);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => ActiveBaitsOn(pc).Any() && pc.Role != Role.Tank && player.Role == Role.Tank && playerSlot < 8
            ? PlayerPriority.Critical
            : base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
}
