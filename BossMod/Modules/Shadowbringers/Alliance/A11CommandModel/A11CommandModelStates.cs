namespace BossMod.Shadowbringers.Alliance.A11CommandModel;

class A11CommandModelStates : StateMachineBuilder
{
    public A11CommandModelStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ForcefulImpact>()
            .ActivateOnEnter<ClangingBlow>()
            .ActivateOnEnter<EnergyBomb>()
            .ActivateOnEnter<HighPoweredLaser>()
            .ActivateOnEnter<EnergyBombardment>()
            .ActivateOnEnter<SidestrikingSpin>()
            .ActivateOnEnter<CentrifugalSpin>()
            .ActivateOnEnter<EnergyAssault>()
            .ActivateOnEnter<AirToSurfaceEnergy>()
            .ActivateOnEnter<EnergyRing>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<HighCaliberLaser>();
    }
}
