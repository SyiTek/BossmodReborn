namespace BossMod.Shadowbringers.Alliance.A14WalkingFortress;

public enum OID : uint
{
    Boss = 0x2C74,
    Helper = 0x233C,
    Marx1 = 0x2C0B, // R0.700, x1
    Marx2 = 0x2C0C, // R0.700, x1
    FlightUnit = 0x2C75, // R2.800, x1
    MarxCrane = 0x2C78, // R12.000, x0 (spawn during fight)
    GoliathTank = 0x2C77, // R9.600, x0 (spawn during fight)
    SerialJointedServiceModel = 0x2C76, // R3.360, x0 (spawn during fight)
    TetherHelper1 = 0x2C86, // R1.000, x0 (spawn during fight)
    TetherHelper2 = 0x2C87, // R1.000, x0 (spawn during fight)
    TetherHelper3 = 0x2C88, // R1.000, x0 (spawn during fight)
    TetherHelper4 = 0x2CC7, // R1.000, x0 (spawn during fight)
    TetherHelper5 = 0x2CC8, // R1.000, x0 (spawn during fight)

    O2P = 0x2C66, // R0.500, x1
}

public enum AID : uint
{
    LaserSaturation = 18678, // Boss->self, 4.0s cast, range 85 circle
    AutoAttack = 18884, // Boss->player, no cast, single-target
    LaserTurretCast = 18679, // Boss->self, 4.0s cast, single-target
    LaserTurret = 19060, // Helper->self, 4.3s cast, range 90 width 8 rect
    GroundToGroundMissile = 18680, // Boss->self, no cast, single-target
    BallisticImpactLocation = 18804, // Helper->location, 3.5s cast, range 6 circle
    BallisticImpactSpread = 18681, // Helper->players, 5.0s cast, range 6 circle
    DualFlankCannons = 18654, // Boss->self, 5.0s cast, single-target
    ForeHindCannons = 18655, // Boss->self, 5.0s cast, single-target
    LaserSuppression = 18656, // Helper->self, 5.0s cast, range 60 90-degree cone

    EngageMarxSupport = 18643, // Boss->self, 3.0s cast, single-target
    MarxImpact = 18644, // MarxCrane->self, 5.0s cast, range 22 circle
    UndockCast = 19255, // Boss->self, 4.0s cast, single-target
    Undock = 19038, // Boss->self, no cast, single-target

    BallisticImpactExa = 18652, // Helper->self, no cast, range 15 width 20 rect
    BallisticImpactIndicator1 = 18649, // Helper->self, 1.0s cast, range 60 width 20 rect
    BallisticImpactIndicator2 = 18650, // Helper->self, 3.5s cast, range 60 width 20 rect
    BallisticImpactIndicator3 = 18651, // Helper->self, 6.0s cast, range 60 width 20 rect
    Neutralization = 18677, // Boss->player, 4.0s cast, single-target

    AntiPersonnelMissile2x = 18657, // Boss->self, 6.0s cast, single-target
    AntiPersonnelMissile3x = 18658, // Boss->self, 9.0s cast, single-target
    BallisticImpactFloor = 18660, // Helper->self, no cast, range 15 width 15 rect

    EngageGoliathTankSupport = 18661, // Boss->self, 3.0s cast, single-target
    LaserTurretTank = 18662, // GoliathTank->self, no cast, range 85 width 10 rect
    HackGoliathTank = 18663, // Boss->GoliathTank, 10.0s cast, single-target
    ConvenientSelfDestructionLOS = 18664, // GoliathTank->self, 10.0s cast, range 85 circle
    ConvenientSelfDestruction = 18665, // GoliathTank->self, 8.0s cast, range 22 circle

    ServiceModelAuto = 872, // SerialJointedServiceModel->player, no cast, single-target
    ClangingBlow = 18672, // SerialJointedServiceModel->player, 4.0s cast, single-target
    TotalAnnihilationManeuverCast = 18667, // Boss->self, 10.0s cast, single-target
    TotalAnnihilationManeuver = 18668, // Helper->self, 13.0s cast, range 80 circle
    ShrapnelImpact = 18675, // Helper->player, 5.0s cast, range 6 circle
}

public enum IconID : uint
{
    Spread = 139, // player->self
    Tankbuster = 198, // player->self
    BallisticImpactGroundSquare = 199, // Helper->self
    TurretLockon = 164, // player->self
    Hacking = 200, // GoliathTank->self
    Stack = 161, // player->self
}

public enum SID : uint
{
    Invincibility = 775, // none->Boss, extra=0x0
    VulnerabilityDown = 350, // none->player, extra=0x0
}
