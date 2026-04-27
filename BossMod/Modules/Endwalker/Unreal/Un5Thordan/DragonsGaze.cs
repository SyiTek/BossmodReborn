namespace BossMod.Endwalker.Unreal.Un5Thordan;

class DragonsGaze(BossModule module) : Components.GenericGaze(module)
{
    private readonly List<Actor> _casters = [];
    private WPos _posHint;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
            return [];
        var eyes = new Eye[count];
        for (var i = 0; i < count; ++i)
        {
            var c = _casters[i];
            eyes[i] = new(c.Position, Module.CastFinishAt(c.CastInfo!));
        }
        return eyes;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_posHint != default)
            Arena.AddCircle(_posHint, 1f, Colors.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DragonsGaze or (uint)AID.DragonsGlory)
        {
            _casters.Add(caster);
            _posHint = default;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DragonsGaze or (uint)AID.DragonsGlory)
            _casters.Remove(caster);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008u && actor.OID is >= (uint)OID.DragonEyeN and <= (uint)OID.DragonEyeNW)
        {
            var index = actor.OID - (uint)OID.DragonEyeN; // 0 = N, then CW
            _posHint = Module.Center + 19f * (180f - (int)index * 45f).Degrees().ToDirection();
        }
    }
}
