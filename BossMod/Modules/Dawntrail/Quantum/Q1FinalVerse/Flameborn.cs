namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

[SkipLocalsInit]
sealed class Flameborn(BossModule module) : Components.Adds(module, (uint)OID.Flameborn)
{
    public int NumSpawned;

    private readonly Dictionary<Actor, Actor> _tethers = [];
    private readonly Q1FinalVerseConfig.FlamebornAssignment _assignments = Service.Config.Get<Q1FinalVerseConfig>().FlamebornAssignments;

    private record class Add(Actor Actor, int Index, int Owner);

    private readonly List<Add> _untetheredAdds = [];

    private static readonly WPos[] _spawns = [new(-617f, -312f), new(-583f, -312f), new(-600f, -300f), new(-617f, -288f), new(-583f, -288f)];

    public override void OnActorTargetable(Actor actor)
    {
        if (actor.OID == (uint)OID.Flameborn)
        {
            ++NumSpawned;
            var spawnIdx = Array.FindIndex(_spawns, s => actor.Position.AlmostEqual(s, 1f));
            if (spawnIdx >= 0)
            {
                var assignedRole = _assignments.AsArray()[spawnIdx];
                var assignedSlot = Raid.WithSlot().WhereActor(a => a.Role == assignedRole).Select(a => a.Item1).DefaultIfEmpty(-1).First();

                _untetheredAdds.Add(new(actor, spawnIdx, assignedSlot));
            }
            else
            {
                ReportError($"Flameborn spawned at unexpected position: {actor.Position}");
                _untetheredAdds.Add(new(actor, -1, -1));
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Flameborn && WorldState.Actors.Find(tether.Target) is { } tar)
        {
            _tethers[source] = tar;
            _untetheredAdds.RemoveAll(t => t.Actor == source);
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        _tethers.Remove(source);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var add in ActiveActors)
        {
            var en = hints.FindEnemy(add);
            if (en == null)
            {
                continue;
            }

            if (_untetheredAdds.FirstOrDefault(a => a.Actor == add) is { } t)
            {
                // add belongs to us, prioritize tethering it before anything else
                var prio = t.Owner == slot
                    ? 1
                    // add belongs to someone else, we must not attack it
                    : t.Owner != slot && t.Owner >= 0
                        ? AIHints.Enemy.PriorityForbidden
                        // no valid assignments, add is a normal target that we can't kill but can build gauge on
                        : AIHints.Enemy.PriorityPointless;
                en.Priority = prio;
            }
            else
            {
                en.Priority = -1;
            }
            en.ForbidDOTs = true;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        foreach (var (a, b) in _tethers)
        {
            Arena.AddLine(a.Position, b.Position, Colors.Danger);
        }

        foreach (var add in _untetheredAdds.Where(a => a.Owner == pcSlot))
        {
            Arena.AddCircle(add.Actor.Position, 1.5f, Colors.Safe);
        }
    }
}

// TODO: need another recording to figure out AOE size
[SkipLocalsInit]
sealed class FlamebornAura(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<Actor> Adds = [];
    private readonly List<AOEInstance> _aoes = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        var count = Adds.Count;
        for (var i = 0; i < count; ++i)
        {
            var a = Adds[i];
            _aoes.Add(new(new AOEShapeCircle(4f * (a.HitboxRadius / 2.6f)), a.Position));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnActorTargetable(Actor actor)
    {
        if (actor.OID == (uint)OID.Flameborn)
        {
            Adds.Add(actor);
        }
    }

    public override void OnActorUntargetable(Actor actor)
    {
        Adds.Remove(actor);
    }
}

[SkipLocalsInit]
sealed class SelfDestruct(BossModule module) : Components.RaidwideCast(module, (uint)AID.SelfDestruct);
