namespace BossMod.Shadowbringers.Alliance.A11CommandModel;

public enum OID : uint
{
    Boss = 0x2C61,
    Helper = 0x233C,
    Turret1 = 0x2C63, // R2.400, x12
    Turret2 = 0x2C65, // R1.000, x0 (spawn during fight)
}

public enum IconID : uint
{
    Tankbuster = 198, // player->self
    LockOn = 164, // player->self
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    SystematicSiege = 18610, // Boss->self, 2.5s cast, single-target
    Unk0 = 19249, // Turret1->self, no cast, single-target
    Unk1 = 18611, // Turret1->self, 1.0s cast, single-target
    ClangingBlow = 18638, // Boss->player, 4.0s cast, single-target
    EnergyBomb = 18612, // Turret2->player/2P, no cast, single-target
    EnergyBombardment = 18615, // Boss->self, 3.0s cast, single-target
    EnergyBombardment1 = 18616, // Helper->location, 3.0s cast, range 4 circle
    ForcefulImpact = 18639, // Boss->self, 4.0s cast, range 100 circle
    EnergyAssault = 18613, // Boss->self, 5.0s cast, single-target
    EnergyAssault1 = 18614, // Helper->self, no cast, range 30 90-degree cone
    Unk2 = 18960, // Boss->self, no cast, single-target
    SystematicTargeting = 18628, // Boss->self, 2.5s cast, single-target
    HighPoweredLaser = 18629, // Turret1->self, no cast, range 70 width 4 rect
    SidestrikingSpin = 18634, // Boss->self, 6.0s cast, single-target
    SidestrikingSpin1 = 18635, // Helper->self, 6.3s cast, range 30 width 12 rect
    SidestrikingSpin2 = 18636, // Helper->self, 6.3s cast, range 30 width 12 rect
    CentrifugalSpin = 18632, // Boss->self, 6.0s cast, single-target
    CentrifugalSpin1 = 18633, // Helper->self, 6.3s cast, range 30 width 8 rect
    SystematicAirstrike = 18617, // Boss->self, 2.5s cast, single-target
    AirToSurfaceAppear = 19250, // Turret1->self, no cast, single-target
    AirToSurfaceEnergy = 18618, // Helper->self, no cast, range 5 circle
    Shockwave = 18627, // Boss->self, 5.0s cast, range 100 circle
    EnergyRingCast1 = 18619, // Boss->self, 3.5s cast, single-target
    EnergyRingCast2 = 18621, // Boss->self, no cast, single-target
    EnergyRingCast3 = 18623, // Boss->self, no cast, single-target
    EnergyRingCast4 = 18625, // Boss->self, no cast, single-target
    EnergyRing1 = 18620, // Helper->self, 4.7s cast, range 12 circle
    EnergyRing2 = 18622, // Helper->self, 6.7s cast, range 12-24 donut
    EnergyRing3 = 18624, // Helper->self, 8.7s cast, range 24-36 donut
    EnergyRing4 = 18626, // Helper->self, 10.7s cast, range 36-48 donut
    SystematicSuppression = 18630, // Boss->self, 2.5s cast, single-target
    HighCaliberLaserCast = 18631, // Turret1->self, 7.0s cast, single-target
    HighCaliberLaser = 18682, // Helper->self, 7.0s cast, range 70 width 24 rect
}
