namespace BossMod.RealmReborn.Alliance.A11BoneDragon;

class Apocalypse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Apocalypse, 6);
class EvilEye(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EvilEye, new AOEShapeCone(105f, 60f.Degrees()));
class Stone(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Stone);
class Level5Petrify(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Level5Petrify, new AOEShapeCone(7.8f, 45f.Degrees()));
class MiasmaBreath(BossModule module) : Components.Cleave(module, (uint)AID.MiasmaBreath, new AOEShapeCone(13f, 45f.Degrees()), activeWhileCasting: false);

class Platinal(BossModule module) : Components.Adds(module, (uint)OID.Platinal, 1);

class Poison(BossModule module) : BossComponent(module)
{
    private int _poisonStage;

    private static bool Safe(WPos center, int poisonStage, WPos p)
    {
        if (poisonStage == 0 || p.InCircle(center, 8f))
            return true;

        for (var i = 0; i < 8; ++i)
        {
            var deg = (45 * i).Degrees();

            // long platforms
            if (poisonStage == 1)
            {
                if (p.InRect(center + new WDir(0f, 31.87f).Rotate(deg), deg, 17.675f, 17.675f, 3.9f))
                    return true;
            }

            if (poisonStage == 2)
            {
                if (p.InRect(center + new WDir(0f, 18.02f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f))
                    return true;
                if (p.InRect(center + new WDir(0f, 32.13f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f))
                    return true;
                if (p.InRect(center + new WDir(0f, 45.745f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f))
                    return true;
            }
        }

        return false;
    }

    private bool Safe(WPos p) => Safe(Arena.Center, _poisonStage, p);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_poisonStage > 0)
        {
            Arena.ZoneCircle(Arena.Center, 8f, Colors.SafeFromAOE);

            for (var i = 0; i < 8; ++i)
            {
                var deg = (45 * i).Degrees();

                if (_poisonStage == 1)
                {
                    Arena.ZoneRect(Arena.Center + new WDir(0f, 31.87f).Rotate(deg), deg, 17.675f, 17.675f, 3.9f, Colors.SafeFromAOE);
                }

                if (_poisonStage == 2)
                {
                    Arena.ZoneRect(Arena.Center + new WDir(0f, 18.02f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f, Colors.SafeFromAOE);
                    Arena.ZoneRect(Arena.Center + new WDir(0f, 32.13f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f, Colors.SafeFromAOE);
                    Arena.ZoneRect(Arena.Center + new WDir(0f, 45.745f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f, Colors.SafeFromAOE);
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_poisonStage > 0)
        {
            var safeShapes = BuildSafeShapes(Arena.Center, _poisonStage);
            if (safeShapes.Length != 0)
                hints.AddForbiddenZone(new SDInvertedUnion(safeShapes), DateTime.MaxValue);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Safe(actor.Position))
            hints.Add("GTFO from poison!");
    }

    public override void OnLegacyMapEffect(byte seq, byte param, byte[] data)
    {
        if (data[0] == 3 && data[2] == 0x10)
            _poisonStage = 1;

        if (data[0] == 3 && data[2] == 0x20)
            _poisonStage = 2;

        if (data[0] == 3 && data[2] == 0x80)
            _poisonStage = 0;
    }

    private static ShapeDistance[] BuildSafeShapes(WPos center, int poisonStage)
    {
        var shapes = new List<ShapeDistance>(25)
        {
            new SDCircle(center, 8f)
        };

        for (var i = 0; i < 8; ++i)
        {
            var deg = (45 * i).Degrees();

            if (poisonStage == 1)
            {
                shapes.Add(new SDRect(center + new WDir(0f, 31.87f).Rotate(deg), deg, 17.675f, 17.675f, 3.9f));
            }
            else if (poisonStage == 2)
            {
                shapes.Add(new SDRect(center + new WDir(0f, 18.02f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f));
                shapes.Add(new SDRect(center + new WDir(0f, 32.13f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f));
                shapes.Add(new SDRect(center + new WDir(0f, 45.745f).Rotate(deg), deg, 2.95f, 2.95f, 2.95f));
            }
        }

        return [.. shapes];
    }
}

class BossDeathTracker(BossModule module) : BossComponent(module)
{
    public bool Dead { get; private set; }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x80000001 && param1 == 0xB4)
            Dead = true;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 706)]
public class A11BoneDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-451.2f, 23.93f), new ArenaBoundsCircle(49.4f, MapResolution: 1f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Platinal));
        Arena.Actors(Enemies((uint)OID.RottingEye));
    }
}
