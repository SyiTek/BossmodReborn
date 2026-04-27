namespace BossMod.RealmReborn.Alliance.A11BoneDragon;

class A11BoneDragonStates : StateMachineBuilder
{
    public A11BoneDragonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Apocalypse>()
            .ActivateOnEnter<EvilEye>()
            .ActivateOnEnter<Stone>()
            .ActivateOnEnter<Level5Petrify>()
            .ActivateOnEnter<MiasmaBreath>()
            .ActivateOnEnter<Platinal>()
            .ActivateOnEnter<Poison>()
            .ActivateOnEnter<BossDeathTracker>()
            .Raw.Update = () => Module.FindComponent<BossDeathTracker>()!.Dead || Module.PrimaryActor.IsDestroyed;
    }
}
