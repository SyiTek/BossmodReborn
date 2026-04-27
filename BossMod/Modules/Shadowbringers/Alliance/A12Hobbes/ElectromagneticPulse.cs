namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

class ElectromagneticPulse(BossModule module) : Components.GenericAOEs(module, (uint)AID.ElectromagneticPulse)
{
    enum Pattern
    {
        None,
        Odd,
        Even
    }
    private Pattern _pattern;

    public DateTime Activation { get; private set; }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var center = new WPos(-831f, -225f);
        var angleToCenter = 120f.Degrees();
        var startPos = _pattern switch
        {
            Pattern.Odd => center + angleToCenter.ToDirection() * 20f,
            Pattern.Even => center + angleToCenter.ToDirection() * 15f,
            _ => default
        };
        if (startPos == default)
            return [];

        var aoes = new AOEInstance[5];
        for (var i = 0; i < 5; ++i)
        {
            aoes[i] = new AOEInstance(new AOEShapeRect(5f, 20f), startPos, -60f.Degrees(), Activation);
            startPos += 300f.Degrees().ToDirection() * 10f;
        }
        return aoes;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 3)
        {
            if (state == 0x00400020u)
            {
                _pattern = Pattern.Even;
                Activation = WorldState.FutureTime(4.2d);
            }
            else if (state == 0x02000100u)
            {
                _pattern = Pattern.Odd;
                Activation = WorldState.FutureTime(4.2d);
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
}
