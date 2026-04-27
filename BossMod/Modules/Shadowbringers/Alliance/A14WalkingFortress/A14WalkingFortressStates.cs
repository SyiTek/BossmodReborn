namespace BossMod.Shadowbringers.Alliance.A14WalkingFortress;

class A14WalkingFortressStates : StateMachineBuilder
{
    public A14WalkingFortressStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LaserSaturation>()
            .ActivateOnEnter<LaserTurret>()
            .ActivateOnEnter<BallisticImpact>()
            .ActivateOnEnter<BallisticImpactSpread>()
            .ActivateOnEnter<LaserSuppression>()
            .ActivateOnEnter<MarxImpact>()
            .ActivateOnEnter<BallisticExaImpact>()
            .ActivateOnEnter<BallisticFloor>()
            .ActivateOnEnter<Neutralization>()
            .ActivateOnEnter<BossInvincible>()
            .ActivateOnEnter<GoliathTank>()
            .ActivateOnEnter<GoliathTankLaserTurret>()
            .ActivateOnEnter<ConvenientSelfDestruction>()
            .ActivateOnEnter<ConvenientSelfDestruction2>()
            .ActivateOnEnter<SerialJointedServiceModel>()
            .ActivateOnEnter<ClangingBlow>()
            .ActivateOnEnter<ShrapnelImpact>()
            .ActivateOnEnter<DeployDefenses>();
    }
}
