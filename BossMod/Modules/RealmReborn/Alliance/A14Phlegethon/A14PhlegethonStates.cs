namespace BossMod.RealmReborn.Alliance.A14Phlegethon;

class A14PhlegethonStates : StateMachineBuilder
{
    public A14PhlegethonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MegiddoFlame2>()
            .ActivateOnEnter<MegiddoFlame3>()
            .ActivateOnEnter<MegiddoFlame4>()
            .ActivateOnEnter<MegiddoFlame5>()
            .ActivateOnEnter<MoonfallSlash>()
            .ActivateOnEnter<VacuumSlash2>()
            .ActivateOnEnter<AbyssalSlash1>()
            .ActivateOnEnter<AbyssalSlash2>()
            .ActivateOnEnter<Pads>()
            .ActivateOnEnter<AncientFlareVoidzone>()
            .ActivateOnEnter<DynamicArenaBorder>()
            .ActivateOnEnter<Adds>();
    }
}
