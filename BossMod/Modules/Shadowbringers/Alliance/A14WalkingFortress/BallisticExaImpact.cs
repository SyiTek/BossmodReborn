namespace BossMod.Shadowbringers.Alliance.A14WalkingFortress;

class BallisticExaImpact(BossModule module) : Components.Exaflare(module, new AOEShapeRect(15f, 10f))
{
    private readonly List<Line> _toAdd = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var delay = spell.Action.ID switch
        {
            (uint)AID.BallisticImpactIndicator1 => 1d,
            (uint)AID.BallisticImpactIndicator2 => 3.5d,
            (uint)AID.BallisticImpactIndicator3 => 6d,
            _ => 0d
        };
        if (delay > 0d)
        {
            _toAdd.Add(new Line(
                next: caster.Position,
                advance: new WDir(0f, 8.7f).Rotate(caster.Rotation),
                nextExplosion: Module.CastFinishAt(spell),
                timeToMove: 0.7d,
                explosionsLeft: 7,
                maxShownExplosions: 4,
                rotation: caster.Rotation));
        }
    }

    public override void Update()
    {
        for (var i = _toAdd.Count - 1; i >= 0; --i)
        {
            if (_toAdd[i].NextExplosion < WorldState.FutureTime(3d))
            {
                Lines.Add(_toAdd[i]);
                _toAdd.RemoveAt(i);
            }
        }
        base.Update();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BallisticImpactExa)
        {
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1f));
            if (ix >= 0)
                AdvanceLine(Lines[ix], caster.Position);
        }
    }
}
