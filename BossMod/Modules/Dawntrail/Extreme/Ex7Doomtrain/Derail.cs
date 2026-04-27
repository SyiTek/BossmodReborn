namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

[SkipLocalsInit]
sealed class Derail(BossModule module) : Components.CastCounter(module, (uint)AID.Derail1);

[SkipLocalsInit]
sealed class Launchpad(BossModule module) : BossComponent(module)
{
    private WPos? _pad;
    private const float Radius = 2f;

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u)
        {
            switch (index)
            {
                case 0x0A:
                    _pad = new(100f, 212.5f);
                    break;
                case 0x0B:
                    _pad = new(100f, 262.5f);
                    break;
                case 0x0C:
                    _pad = new(100f, 312.5f);
                    break;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_pad is WPos pad && pc.Position.Z < pad.Z + Radius)
            Arena.ZoneCircle(pad, Radius, Colors.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_pad is WPos pad && actor.Position.Z < pad.Z + Radius)
            hints.Add("Get to launchpad!", !actor.Position.InCircle(pad, Radius));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_pad is WPos pad && actor.Position.Z < pad.Z + Radius)
            hints.AddForbiddenZone(new SDInvertedCircle(pad, Radius));
    }
}
