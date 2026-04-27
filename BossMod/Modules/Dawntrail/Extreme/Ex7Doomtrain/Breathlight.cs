namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

// Breathlight: Boss telegraphs Headlight (air-based) or ThunderousBreath (ground-based) first.
// First AID hits, then 2.5s later second alternates. Player on the wrong level (high/low) takes damage.
// Reborn doesn't have CarGeometry portals/AirShape/GroundShape ported, so we render the resolved breath as a global hint and rely on actor-cast-detection on the Helper rect (70x10x70).
[SkipLocalsInit]
sealed class Breathlight(BossModule module) : Components.GenericAOEs(module)
{
    private enum Level { Ground, Air }
    private readonly List<(Level Level, DateTime Activation)> _casts = [];
    public bool Draw = true; // upstream renders AOE shapes on car geometry; preserved for state-machine compat but unused here

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ThunderousBreathFirstVisual:
                _casts.Add((Level.Ground, Module.CastFinishAt(spell)));
                _casts.Add((Level.Air, Module.CastFinishAt(spell, 2.5d)));
                break;
            case (uint)AID.HeadlightFirstVisual:
                _casts.Add((Level.Air, Module.CastFinishAt(spell)));
                _casts.Add((Level.Ground, Module.CastFinishAt(spell, 2.5d)));
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_casts.Count != 0)
        {
            var l = _casts[0].Level;
            hints.Add(l == Level.Ground ? "Next: platform safe" : "Next: ground safe");
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Headlight or (uint)AID.ThunderousBreath)
        {
            ++NumCasts;
            if (_casts.Count != 0)
                _casts.RemoveAt(0);
        }
    }
}
