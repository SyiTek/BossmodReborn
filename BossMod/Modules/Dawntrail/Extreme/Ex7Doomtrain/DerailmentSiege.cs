namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

[SkipLocalsInit]
sealed class DerailmentSiege(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.DerailmentSiegeVisual5 or (uint)AID.DerailmentSiegeVisual6 or (uint)AID.DerailmentSiegeVisual7 or (uint)AID.DerailmentSiegeVisual8)
        {
            Towers.Add(new(caster.Position, 5f, maxSoakers: 8, activation: Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.DerailmentSiegeVisual5 or (uint)AID.DerailmentSiegeVisual6 or (uint)AID.DerailmentSiegeVisual7 or (uint)AID.DerailmentSiegeVisual8)
        {
            ++NumCasts;
            Towers.Clear();
        }
        else if (id == (uint)AID.DerailmentSiege1)
        {
            ++NumCasts;
        }
    }
}
