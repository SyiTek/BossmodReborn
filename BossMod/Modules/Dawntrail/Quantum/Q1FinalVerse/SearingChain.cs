namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

[SkipLocalsInit]
sealed class SearingChain(BossModule module) : Components.Chains(module, (uint)TetherID._Gen_Tether_chn_hfchain1f, chainLength: 20f);

[SkipLocalsInit]
sealed class SearingChainCross(BossModule module) : Components.GenericBaitAway(module, (uint)AID.SearingChains, centerAtTarget: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count > 0)
        {
            var act = CurrentBaits[0].Activation;
            foreach (var player in Raid.WithoutSlot().Exclude(actor))
            {
                hints.AddForbiddenZone(new SDCross(player.Position, default, 50f, 3f), act);
            }

            hints.AddPredictedDamage(Raid.WithSlot().Mask(), act.AddSeconds(1.1d));
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_SearingChains)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeCross(50f, 3f), WorldState.FutureTime(5.6d), customRotation: default(Angle)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SearingChains)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}
