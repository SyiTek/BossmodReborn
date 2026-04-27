namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

class FireResistanceTest(BossModule module) : Components.GenericAOEs(module)
{
    private int _platform; // 0-2
    enum Pattern
    {
        None,
        Inside,
        Outside,
        CW,
        CCW
    }
    private Pattern _pattern;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        WPos center = _platform switch
        {
            0 => new(-831f, -225f),
            1 => new(-805f, -270f),
            2 => new(-779f, -225f),
            _ => default
        };
        switch (_pattern)
        {
            case Pattern.CW:
                return new AOEInstance[] { new(new AOEShapeRect(22f, 21f, 2f), center, Angle.FromDirection(Arena.Center - center) + 85f.Degrees()) };
            case Pattern.CCW:
                return new AOEInstance[] { new(new AOEShapeRect(22f, 21f, 2f), center, Angle.FromDirection(Arena.Center - center) - 85f.Degrees()) };
            case Pattern.Inside:
                {
                    var dir = (Arena.Center - center).Normalized() * 15f;
                    return new AOEInstance[] { new(new AOEShapeCone(70f, 15f.Degrees()), Arena.Center + dir, Angle.FromDirection(center - Arena.Center)) };
                }
            case Pattern.Outside:
                {
                    var platformEdge = (Arena.Center - center).Normalized() * 20f + center;
                    var angle = Angle.FromDirection(center - Arena.Center);
                    return new AOEInstance[]
                    {
                        new(new AOEShapeCone(70f, 30f.Degrees()), platformEdge, angle + 55f.Degrees()),
                        new(new AOEShapeCone(70f, 30f.Degrees()), platformEdge, angle - 55f.Degrees())
                    };
                }
            default:
                return [];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        var plat = index switch
        {
            7 => 0,
            9 => 1,
            10 => 2,
            _ => -1
        };
        if (plat >= 0)
        {
            _platform = plat;
            switch (state)
            {
                case 0x00100010u:
                    _pattern = Pattern.Inside;
                    break;
                case 0x00800080u:
                    _pattern = Pattern.Outside;
                    break;
                case 0x04000400u:
                    _pattern = Pattern.CCW;
                    break;
                case 0x20002000u:
                    _pattern = Pattern.CW;
                    break;
                case 0x00400004u:
                case 0x02000004u:
                case 0x10000004u:
                case 0x80000004u:
                    // mapeffect is generally close enough to the actual cast that we can just use it as an indicator for when the voidzone disappears
                    _pattern = Pattern.None;
                    _platform = 0;
                    break;
            }
        }
    }
}
