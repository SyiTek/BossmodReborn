namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

[SkipLocalsInit]
sealed class Spinelash(BossModule module) : Components.GenericWildCharge(module, 4f, (uint)AID.Spinelash, 60f)
{
    private Actor? _target;

    public BitMask IntactWindows = BitMask.Build(0, 1, 2);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID._Gen_Icon_lockon5_t0h && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _target = actor;
            Activation = WorldState.FutureTime(10.5d);
            PlayerRoles[slot] = actor.Role == Role.Tank ? PlayerRole.Target : PlayerRole.TargetNotFirst;
            foreach (var (s, p) in Raid.WithSlot().Exclude(actor))
            {
                PlayerRoles[s] = p.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;
            }

            // TODO: refactor GenericWildCharge so this isn't necessary :(
            var pr = Module.PrimaryActor.PosRot;
            pr.X = actor.Position.X;
            Source = new Actor(1uL, 0u, 819, 0u, "fake actor", 0u, ActorType.Enemy, Class.ACN, 1, pr);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (PlayerRoles[pcSlot] is PlayerRole.Target or PlayerRole.TargetNotFirst)
        {
            var col = AimedAtWindow(pc.Position) ? Colors.Safe : Colors.Danger;
            foreach (var bit in IntactWindows.SetBits())
            {
                Arena.AddLine(new WPos(WindowCenterX(bit) + 7f, -284f), new WPos(WindowCenterX(bit) - 7f, -284f), col, 2f);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (PlayerRoles[slot] is PlayerRole.Target or PlayerRole.TargetNotFirst)
        {
            hints.Add("Aim for window!", !AimedAtWindow(actor.Position));
        }
    }

    private static float WindowCenterX(int windowBit) => -614f + windowBit * 14f;

    private bool AimedAtWindow(WPos pos)
    {
        foreach (var win in IntactWindows.SetBits())
        {
            if (MathF.Abs(pos.X - WindowCenterX(win)) < 7f)
            {
                return true;
            }
        }
        return false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Spinelash)
        {
            ++NumCasts;
            Source = null;
            _target = null;
            Array.Fill(PlayerRoles, PlayerRole.Ignore);
        }
    }

    public override void Update()
    {
        if (_target != null && Source != null)
        {
            var pr = Source.PosRot;
            pr.X = _target.Position.X;
            Source.PosRot = pr;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        var window = index - 0x18;
        if (window is >= 0 and <= 2 && state == 0x00020001u)
        {
            IntactWindows.Clear(window);
        }
    }
}
