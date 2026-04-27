namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

[SkipLocalsInit]
sealed class BladeStillnessFireCounter(BossModule module) : BossComponent(module)
{
    public int NumCasts;

    private int _expectedBlades;
    private int _fireballBaits;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BallOfFire:
                ++_fireballBaits;
                // fireballs double up on one person if there is a death so we can always count on 4
                if (_fireballBaits % 4 == 0)
                {
                    ++NumCasts;
                }
                break;
            case (uint)AID.BladeOfFirstLightVisual2:
            case (uint)AID.BladeOfFirstLightVisual4:
                _expectedBlades = 2;
                break;
            case (uint)AID.BladeOfFirstLightVisual1:
            case (uint)AID.BladeOfFirstLightVisual3:
                _expectedBlades = 1;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BladeOfFirstLight1:
            case (uint)AID.BladeOfFirstLight2:
                --_expectedBlades;
                if (_expectedBlades <= 0)
                {
                    ++NumCasts;
                }
                break;
            case (uint)AID.ChainsOfCondemnation1:
            case (uint)AID.ChainsOfCondemnation2:
                ++NumCasts;
                break;
        }
    }
}

[SkipLocalsInit]
sealed class ChainsOfCondemnation(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChainsOfCondemnation1 or (uint)AID.ChainsOfCondemnation2)
        {
            Array.Fill(PlayerStates, new PlayerState(Requirement.Stay2, Module.CastFinishAt(spell)));
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_ChainsOfCondemnation && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            var ps = new PlayerState(Requirement.Stay2, WorldState.CurrentTime);
            SetState(slot, ref ps);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_ChainsOfCondemnation && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            ClearState(slot);
        }
    }
}

[SkipLocalsInit]
sealed class BladeOfFirstLight(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BladeOfFirstLight1, (uint)AID.BladeOfFirstLight2], new AOEShapeRect(30f, 7.5f));

[SkipLocalsInit]
sealed class BallOfFireBait(BossModule module) : BossComponent(module)
{
    private bool _active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BallOfFireVisual1:
            case (uint)AID.BallOfFireVisual2:
                _active = true;
                break;
            case (uint)AID.BallOfFire:
                _active = false;
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_active)
        {
            Arena.AddCircle(pc.Position, 6f, Colors.Danger);
        }
    }
}
