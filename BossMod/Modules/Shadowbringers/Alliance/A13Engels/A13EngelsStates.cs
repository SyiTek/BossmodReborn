namespace BossMod.Shadowbringers.Alliance.A13Engels;

class A13MarxEngelsStates : StateMachineBuilder
{
    public A13MarxEngelsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DemolishStructureArenaChange>()
            .ActivateOnEnter<PrecisionGuidedMissile2>()
            .ActivateOnEnter<DiffuseLaser>()
            .ActivateOnEnter<LaserSight1>()
            .ActivateOnEnter<GuidedMissile2>()
            .ActivateOnEnter<IncendiaryBombing1>()
            .ActivateOnEnter<IncendiaryBombing2>()
            .ActivateOnEnter<IncendiaryBombingVoidzone>()
            .ActivateOnEnter<GuidedMissile>()
            .ActivateOnEnter<GuidedMissileBait>()
            .ActivateOnEnter<MarxSmash1>()
            .ActivateOnEnter<MarxSmash2>()
            .ActivateOnEnter<MarxSmash3>()
            .ActivateOnEnter<MarxSmash4>()
            .ActivateOnEnter<MarxSmash5>()
            .ActivateOnEnter<MarxSmash6>()
            .ActivateOnEnter<MarxSmash7>()
            .ActivateOnEnter<MarxCrush>()
            .ActivateOnEnter<MarxThrust>()
            .ActivateOnEnter<CrushingWheel>()
            .ActivateOnEnter<IncendiarySaturationBombing>()
            .ActivateOnEnter<IncendiarySaturationBombingVoidzone>()
            .ActivateOnEnter<DemolishStructure>()
            .ActivateOnEnter<ArmLaser>()
            .ActivateOnEnter<EnergyDispersal>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<AddsArms>()
            .ActivateOnEnter<SurfaceMissile2>();
    }
}
