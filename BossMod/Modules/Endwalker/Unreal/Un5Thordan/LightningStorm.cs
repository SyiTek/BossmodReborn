namespace BossMod.Endwalker.Unreal.Un5Thordan;

// note: we don't use simple 'spread from cast targets', because casts are staggered a bit, which is ugly
class LightningStorm(BossModule module) : Components.UniformStackSpread(module, 0f, 5f, alwaysShowSpreads: true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LightningStorm)
            AddSpread(actor, WorldState.FutureTime(4d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LightningStormAOE)
            Spreads.Clear();
    }
}
