namespace BossMod.Shadowbringers.Alliance.A14WalkingFortress;

class BallisticFloor(BossModule module) : Components.GenericAOEs(module, (uint)AID.BallisticImpactFloor)
{
    private sealed class Pattern
    {
        public List<WPos> Tiles = [];
        public DateTime Appear;
        public int Order;
        public DateTime Activate;
    }

    private readonly List<Pattern> _patterns = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BallisticImpactGroundSquare)
        {
            if (_patterns.Count > 0 && _patterns[^1].Appear.AddSeconds(2d) > WorldState.CurrentTime)
            {
                _patterns[^1].Tiles.Add(actor.Position);
            }
            else
            {
                _patterns.Add(new Pattern
                {
                    Tiles = [actor.Position],
                    Appear = WorldState.CurrentTime,
                    Order = _patterns.Count,
                    Activate = WorldState.FutureTime(6.1d)
                });
            }

            var startDelay = _patterns.Count switch
            {
                2 => 6.1d,
                3 => 9.2d,
                _ => 0d
            };

            if (startDelay > 0d)
            {
                var appear1 = _patterns[0].Appear.AddSeconds(startDelay);
                for (var i = 0; i < _patterns.Count; ++i)
                {
                    _patterns[i].Activate = appear1;
                    appear1 = appear1.AddSeconds(2.1d);
                }
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_patterns.Count == 0)
            return [];
        var p = _patterns[0];
        var tiles = p.Tiles;
        var count = tiles.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
            aoes[i] = new AOEInstance(new AOEShapeRect(7.5f, 7.5f, 7.5f), tiles[i], default, p.Activate);
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            if (_patterns.Count > 0)
            {
                _patterns[0].Tiles.RemoveAll(t => t.AlmostEqual(caster.Position, 1f));
                if (_patterns[0].Tiles.Count == 0)
                    _patterns.RemoveAt(0);
            }
        }
    }
}
