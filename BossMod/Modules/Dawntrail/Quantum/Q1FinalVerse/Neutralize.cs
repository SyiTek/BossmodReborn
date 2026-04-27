namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

[SkipLocalsInit]
sealed class Neutralize(BossModule module) : BossComponent(module)
{
    private enum Color { None, Light, Dark }

    private readonly Color[] _colors = new Color[PartyState.MaxAllies];
    public int NumIcons { get; private set; }

    // random guess
    private DateTime _resolve;

    public const float Radius = 1.5f; // todo verify

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var color = iconID switch
        {
            (uint)IconID.Light => Color.Light,
            (uint)IconID.Dark => Color.Dark,
            _ => Color.None
        };

        if (_resolve == default)
        {
            _resolve = WorldState.FutureTime(5.1d);
        }

        if (color != Color.None && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _colors[slot] = color;
            ++NumIcons;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_colors[slot] != Color.None)
        {
            var stacked = Raid.WithSlot().InRadiusExcluding(actor, Radius).ToList();

            if (stacked.Any(s => _colors[s.Item1] == _colors[slot]))
            {
                hints.Add("GTFO from same color!");
            }
            else
            {
                hints.Add("Stack with opposite color!", !stacked.Any(s => _colors[s.Item1] != _colors[slot]));
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_colors[slot] != Color.None)
        {
            var partners = new List<ShapeDistance>();

            foreach (var (s, a) in Raid.WithSlot().Exclude(actor))
            {
                if (_colors[s] != _colors[slot] && _colors[s] != Color.None)
                {
                    partners.Add(new SDDonut(a.Position, Radius, 60f));
                }
                else if (_colors[s] == _colors[slot])
                {
                    hints.AddForbiddenZone(new SDCircle(a.Position, Radius), _resolve);
                }
            }

            if (partners.Count > 0)
            {
                hints.AddForbiddenZone(new SDIntersection([.. partners]), _resolve);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_colors[pcSlot] != Color.None)
        {
            Arena.AddCircle(pc.Position, Radius, Colors.Safe);

            foreach (var (slot, player) in Raid.WithSlot().Exclude(pc))
            {
                if (_colors[slot] == _colors[pcSlot])
                {
                    Arena.AddCircle(player.Position, Radius, Colors.Danger);
                }
            }
        }
    }

    public override void Update()
    {
        if (_resolve != default && _resolve <= WorldState.CurrentTime)
        {
            _resolve = default;
            Array.Fill(_colors, Color.None);
            NumIcons = 0;
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _colors[pcSlot] == Color.None ? PlayerPriority.Normal : _colors[pcSlot] == _colors[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Interesting;
}
