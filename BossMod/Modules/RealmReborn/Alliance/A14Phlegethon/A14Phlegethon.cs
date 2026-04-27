namespace BossMod.RealmReborn.Alliance.A14Phlegethon;

class MegiddoFlame2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MegiddoFlame2, 3f);
class MegiddoFlame3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MegiddoFlame3, 4f);
class MegiddoFlame4(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MegiddoFlame4, 5f);
class MegiddoFlame5(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MegiddoFlame5, 6f);
class MoonfallSlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MoonfallSlash, new AOEShapeCone(15f, 60f.Degrees()));
class VacuumSlash2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VacuumSlash2, new AOEShapeCone(80f, 26f.Degrees()));
class AbyssalSlash1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalSlash2, new AOEShapeDonutSector(2.2f, 7.5f, 90f.Degrees()));
class AbyssalSlash2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalSlash3, new AOEShapeDonutSector(12.5f, 17.5f, 90f.Degrees()));

class Pads(BossModule module) : Components.GenericTowers(module)
{
    private static readonly WPos[] _pads = [new(-148.65f, 191.975f), new(-110f, 221.59f), new(-71.35f, 191.975f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AncientFlare1)
        {
            var activation = Module.CastFinishAt(spell);
            for (var i = 0; i < _pads.Length; ++i)
                Towers.Add(new(_pads[i], 4.2f, minSoakers: 6, maxSoakers: 8, activation: activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AncientFlare2)
            Towers.Clear();
    }
}

class AncientFlareVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(32.45f);
    private DateTime _activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            return new AOEInstance[1] { new(_shape, Arena.Center, default, _activation) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AncientFlare1)
        {
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1E8885 && state == 0x00110022)
            _activation = default;
    }
}

class DynamicArenaBorder(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut _shape = new(32.45f, 100f);
    private bool _active;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
            return new AOEInstance[1] { new(_shape, Arena.Center) };
        return [];
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1E8894)
        {
            if (state == 0x00040008)
                _active = true;
            if (state == 0x00010002)
                _active = false;
        }
    }
}

class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.IronGiant, (uint)OID.IronClaws]);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 732)]
public class A14Phlegethon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-110f, 181.6f), PhlegBounds)
{
    private static readonly ArenaBoundsCustom PhlegBounds = new(
        [
            new Polygon(new(-110f, 181.6f), 32.45f, 60),
            new ConeHA(new(-110f, 181.6f), 44.5f, default, 97.5f.Degrees())
        ],
        MapResolution: 1f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.IronClaws));
        Arena.Actors(Enemies((uint)OID.IronGiant));
    }
}
