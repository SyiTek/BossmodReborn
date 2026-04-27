namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class CrownOfArcadia(BossModule module) : Components.RaidwideCast(module, (uint)AID.CrownOfArcadia);
sealed class UltimateTrophyWeapons(BossModule module) : Components.CastHint(module, (uint)AID.UltimateTrophyWeapons, "Ultimate Trophy Weapons");

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
StatesType = typeof(M11STheTyrantStates),
ConfigType = typeof(M11STheTyrantConfig),
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = typeof(TetherID),
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.Boss,
Contributors = "Topas, upstream port",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Savage,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1073u,
NameID = 14305u,
SortOrder = 1,
PlanLevel = 100)]

public sealed class M11STheTyrant(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.ArenaCenter, ArenaChanges.InitialBounds);