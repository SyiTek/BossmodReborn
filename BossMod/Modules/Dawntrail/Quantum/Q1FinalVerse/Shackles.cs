namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

[SkipLocalsInit]
sealed class ShackleSpreadHint(BossModule module) : Components.GenericStackSpread(module, raidwideOnResolve: false)
{
    public bool Shackles { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShacklesOfGreaterSanctity)
        {
            var act = Module.CastFinishAt(spell);
            foreach (var player in Raid.WithoutSlot())
            {
                if (player.Role == Role.Healer)
                {
                    Spreads.Add(new(player, 21f, act));
                }
                else if (player.Role != Role.Tank)
                {
                    Spreads.Add(new(player, 8f, act));
                }
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_ShackledHealing)
        {
            Shackles = true;
            Spreads.Clear();
        }
    }
}

// only draw spread on healer to reduce visual clutter
// DPS will naturally avoid healer (because of defam) and tank (because of jail)
[SkipLocalsInit]
sealed class ShackleHint(BossModule module) : BossComponent(module)
{
    private Actor? Healer;
    public bool Expired;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_ShackledHealing)
        {
            Healer = actor;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_ShackledHealing)
        {
            Healer = null;
            Expired = true;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Healer != null)
        {
            Arena.AddCircle(Healer.Position, 21f, Colors.Danger);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == Healer)
        {
            var count = Raid.WithoutSlot().InRadiusExcluding(actor, 21f).Count();
            hints.Add($"Allies in radius: {count}", false);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => player == Healer ? PlayerPriority.Interesting : base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
}

[SkipLocalsInit]
sealed class ArcaneFont(BossModule module) : Components.Adds(module, (uint)OID.ArcaneFont, forbidDots: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var jail = actor.FindStatus((uint)SID._Gen_HellishEarth) != null;

        foreach (var add in ActiveActors)
        {
            if (hints.FindEnemy(add) is { } enemy)
            {
                enemy.Priority = jail ? AIHints.Enemy.PriorityInvincible : 0;
                enemy.ForbidDOTs = true;
            }
        }
    }
}

[SkipLocalsInit]
sealed class HellishEarthPull(BossModule module) : Components.GenericKnockback(module)
{
    private Actor? Caster;
    private Actor? Target;
    private readonly List<Knockback> _kbs = new(8);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _kbs.Clear();
        if (Caster != null && Caster.CastInfo != null)
        {
            var activation = Module.CastFinishAt(Caster.CastInfo);

            if (actor == Target)
            {
                _kbs.Add(new(Caster.Position, 60f, activation, kind: Kind.TowardsOrigin, ignoreImmunes: true));
            }
            else if (!PlayerImmunes[slot].ImmuneAt(activation))
            {
                _kbs.Add(new(Caster.Position, 10f, activation, kind: Kind.TowardsOrigin, ignoreImmunes: true));
            }
        }
        return CollectionsMarshal.AsSpan(_kbs);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == Target)
        {
            if (actor.Role != Role.Tank)
            {
                hints.Add("Too far from boss!");
            }
        }
        else if (Target != null && actor.Role == Role.Tank)
        {
            hints.Add("Go far to bait tether!");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HellishEarth2)
        {
            Caster = caster;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HellishEarth1 or (uint)AID.HellishEarth2)
        {
            Target = null;
            Caster = null;
            ++NumCasts;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID._Gen_Tether_chn_fire001f && WorldState.Actors.Find(tether.Target) is { } tar)
        {
            Target = tar;
        }
    }
}

[SkipLocalsInit]
sealed class ManifoldLashingsTower(BossModule module) : Components.GenericTowers(module, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ManifoldLashingsFirst)
        {
            Towers.Add(new(caster.Position, 2f, forbiddenSoakers: Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask(), activation: Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ManifoldLashingsFirst or (uint)AID.ManifoldLashingsRest)
        {
            ++NumCasts;
        }
    }
}

[SkipLocalsInit]
sealed class ManifoldLashingsTail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManifoldLashingsEnd, new AOEShapeRect(42f, 4.5f));
