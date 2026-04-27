namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

[SkipLocalsInit]
sealed class Shockwave(BossModule module) : Components.RaidwideInstant(module, (uint)AID.Shockwave, 5.7d)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.ShockwaveVisual)
            Activation = WorldState.FutureTime(Delay);
    }
}
