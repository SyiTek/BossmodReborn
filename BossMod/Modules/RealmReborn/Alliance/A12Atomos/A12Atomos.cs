using System.Collections.ObjectModel;

namespace BossMod.RealmReborn.Alliance.A12Atomos;

public enum OID : uint
{
    Boss = 0x963, // R4.000, x3
    Dira = 0x966, // R2.000, x0-2 (spawn during fight)
    Valefor = 0x964, // R5.000, x0-2 (spawn during fight)
    GreaterDemon = 0x965, // R2.000, x0-2 (spawn during fight)

    RingA = 0x1E8F7D,
    RingB = 0x1E8F7E,
    RingC = 0x1E8F7F,

    PlatformA = 0x1E8F80,
    PlatformB = 0X1E8F81,
    PlatformC = 0x1E8F82,
}

public enum AID : uint
{
    AutoAttack = 1461, // GreaterDemon/Valefor/Dira->player, no cast, single-target
    DarkOrb = 911, // GreaterDemon->player, no cast, single-target
    TheLook = 1791, // Valefor->self, no cast, range 6+R 120?-degree cone
    VoidFireII = 1829, // Dira->location, 3.0s cast, range 5 circle
}

class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Dira, (uint)OID.Valefor, (uint)OID.GreaterDemon]);
class VoidFireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidFireII, 5);
class Ring(BossModule module) : Components.GenericInvincible(module)
{
    private BitMask _vulnerable;
    private readonly List<Actor> _forbidden = [];

    protected override ReadOnlySpan<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        _forbidden.Clear();
        var platform = A12Atomos.GetPlatform(actor);

        foreach (var e in Module.Enemies((uint)OID.Dira).Concat(Module.Enemies((uint)OID.Valefor)).Concat(Module.Enemies((uint)OID.GreaterDemon)))
            if (platform != A12Atomos.GetPlatform(e))
                _forbidden.Add(e);

        var m = (A12Atomos)Module;
        if (m.AtomosA != null && (platform != 0 || !_vulnerable[0]))
            _forbidden.Add(m.AtomosA);
        if (m.AtomosB != null && (platform != 1 || !_vulnerable[1]))
            _forbidden.Add(m.AtomosB);
        if (m.AtomosC != null && (platform != 2 || !_vulnerable[2]))
            _forbidden.Add(m.AtomosC);

        return CollectionsMarshal.AsSpan(_forbidden);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        var id = (OID)actor.OID switch
        {
            OID.RingA => 0,
            OID.RingB => 1,
            OID.RingC => 2,
            _ => -1
        };

        if (id >= 0)
        {
            if (state == 0x10042200)
                _vulnerable.Clear(id);
            else if (state == 0x04400880)
                _vulnerable.Set(id);
        }
    }
}

class Pad(BossModule module) : BossComponent(module)
{
    private readonly Actor?[] _pads = new Actor?[3];

    private BitMask _activePads;

    public override void Update()
    {
        if (_pads[0] == null)
        {
            _pads[0] = Module.Enemies((uint)OID.PlatformA).FirstOrDefault();
            _pads[1] = Module.Enemies((uint)OID.PlatformB).FirstOrDefault();
            _pads[2] = Module.Enemies((uint)OID.PlatformC).FirstOrDefault();
        }
    }

    // pad activate = 00400080
    // pad deactivate = 00040200
    public override void OnActorEAnim(Actor actor, uint state)
    {
        var ix = (int)actor.OID - (int)OID.PlatformA;
        if (state == 0x00400080)
            _activePads.Set(ix);
        else if (state == 0x00040200)
            _activePads.Clear(ix);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var myPlatform = A12Atomos.GetPlatform(pc);

        for (var i = 0; i < 3; i++)
        {
            if (_pads[i] is not { } pad)
                continue;

            var color = GetPadBoss(i)?.IsDeadOrDestroyed == true
                ? Colors.Border
                : _activePads[i]
                    ? Colors.Safe
                    : Colors.Danger;
            var width = i == myPlatform ? 2 : 1;

            Arena.AddCircle(pad.Position, 4, color, width);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var platform = A12Atomos.GetPlatform(actor);
        if (_pads[platform] is not { } myPad)
            return;

        if (GetPadBoss(platform)?.IsDeadOrDestroyed == true)
            return;

        if (Raid.WithoutSlot().InRadius(myPad.Position, 4).Count() < 4)
            hints.Add("Stand on pad!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var platform = A12Atomos.GetPlatform(actor);
        if (_pads[platform] is not { } myPad)
            return;

        if (GetPadBoss(platform)?.IsDeadOrDestroyed == true)
            return;

        var padCount = Raid.WithoutSlot().InRadius(myPad.Position, 4).Exclude(actor);
        if (padCount.Count() < 4)
            hints.AddForbiddenZone(new SDDonut(myPad.Position, 4, 500), DateTime.MaxValue);
    }

    private Actor? GetPadBoss(int platform) => ((A12Atomos)Module).Bosses[(platform + 1) % 3];
}

class A12AtomosStates : StateMachineBuilder
{
    public A12AtomosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<Ring>()
            .ActivateOnEnter<Pad>()
            .ActivateOnEnter<VoidFireII>()
            .Raw.Update = () => module.Enemies((uint)OID.Boss).All(b => b.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 1872)]
public class A12Atomos(WorldState ws, Actor primary) : BossModule(ws, primary, new(232.5f, 280), CustomBounds)
{
    public static int GetPlatform(Actor a)
    {
        var zdist = a.Position.Z - 280;

        return MathF.Abs(zdist) < 15 ? 1 : zdist < 0 ? 0 : 2;
    }

    // Simplified arena bounds (Reborn's PolygonClipper API differs from upstream).
    // Uses 3 stacked rectangular platforms approximating the original 3-platform layout.
    // Original upstream had cutouts for the rings between platforms; omitted here for simplicity.
    public static readonly ArenaBoundsCustom CustomBounds = new(
        [
            new Rectangle(default, 37.65f, 12.3f),
            new Rectangle(new WPos(0, 35.2f), 37.65f, 12.3f),
            new Rectangle(new WPos(0, -35.2f), 37.65f, 12.3f),
        ]);

    public Actor? AtomosA { get; private set; }
    public Actor? AtomosB { get; private set; }
    public Actor? AtomosC { get; private set; }

    public ReadOnlyCollection<Actor?> Bosses => (new Actor?[] { AtomosA, AtomosB, AtomosC }).AsReadOnly();

    protected override void UpdateModule()
    {
        AtomosA ??= Enemies((uint)OID.Boss).FirstOrDefault(b => b.Position.InCircle(new WPos(253, 244), 5));
        AtomosB ??= Enemies((uint)OID.Boss).FirstOrDefault(b => b.Position.InCircle(new WPos(253, 279), 5));
        AtomosC ??= Enemies((uint)OID.Boss).FirstOrDefault(b => b.Position.InCircle(new WPos(253, 315), 5));
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss), Colors.Enemy);
    }
}
