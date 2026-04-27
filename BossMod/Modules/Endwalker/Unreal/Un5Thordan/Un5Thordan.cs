namespace BossMod.Endwalker.Unreal.Un5Thordan;

class AscalonsMight(BossModule module) : Components.Cleave(module, (uint)AID.AscalonsMight, new AOEShapeCone(8f + 3.8f, 45f.Degrees()));
class Meteorain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeteorainAOE, 6f);
class AscalonsMercy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AscalonsMercy, new AOEShapeCone(34.8f, 10f.Degrees()));
class AscalonsMercyHelper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AscalonsMercyAOE, new AOEShapeCone(34.5f, 10f.Degrees()));
class DragonsRage(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DragonsRage, 6f, 6);
class Heavensflame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavensflameAOE, 6f);
class Conviction(BossModule module) : Components.CastTowers(module, (uint)AID.ConvictionAOE, 3f);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, (uint)AID.HolyChain);
class SerZephirin(BossModule module) : Components.Adds(module, (uint)OID.Zephirin);
class LightOfAscalon(BossModule module) : Components.CastCounter(module, (uint)AID.LightOfAscalon); // TODO: show knockback 3 from [-0.8, -16.3]
class UltimateEnd(BossModule module) : Components.CastCounter(module, (uint)AID.UltimateEndAOE);
class HeavenswardLeap(BossModule module) : Components.CastCounter(module, (uint)AID.HeavenswardLeap);
class PureOfSoul(BossModule module) : Components.CastCounter(module, (uint)AID.PureOfSoul);
class AbsoluteConviction(BossModule module) : Components.CastCounter(module, (uint)AID.AbsoluteConviction);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 963, NameID = 3632, PlanLevel = 90)]
public class Un5Thordan(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(21f));
