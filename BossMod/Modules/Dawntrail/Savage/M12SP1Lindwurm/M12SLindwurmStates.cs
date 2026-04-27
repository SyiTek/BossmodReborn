namespace BossMod.Dawntrail.Savage.M12SLindwurm;

sealed class M12SLindwurmStates : StateMachineBuilder
{
    public M12SLindwurmStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        // Opening Raidwide
        Cast(id, (uint)AID.TheFixer, 10.2f, 5, "Raidwide")
            .ActivateOnEnter<TheFixer>()
            .DeactivateOnExit<TheFixer>()
            .SetHint(StateMachine.StateHint.Raidwide);

        MortalSlayer(id + 0x100, 13.3f);
        GrotesquerieAct1(id + 0x10000, 17.3f);
        GrotesquerieAct2(id + 0x20000, 9.2f);
        GrotesquerieAct3(id + 0x30000, 6.1f);
        GrotesquerieCurtainCall(id + 0x40000, 9.1f);
        MortalSlayer(id + 0x50000, 13.8f);
        Slaughtershed(id + 0x60000, 12.2f);
        Slaughtershed(id + 0x70000, 3.6f);
        Slaughtershed(id + 0x80000, 3.8f);

        // Enrage
        Cast(id + 0x90000, (uint)AID.RefreshingOverkill0, 8.1f, 9.7f, "Enrage")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<RefreshingOverkill1>()
            .ActivateOnEnter<RefreshingOverkill2>()
            .DeactivateOnExit<RefreshingOverkill1>()
            .DeactivateOnExit<RefreshingOverkill2>();
    }

    private void MortalSlayer(uint id, float delay)
    {
        Cast(id, (uint)AID.MortalSlayer0, delay, 11.7f)
            .ActivateOnEnter<MortalSlayer>();

        ComponentCondition<MortalSlayer>(id + 0x10, 0.1f, m => m.NumCasts >= 2, "Orbs 1");
        ComponentCondition<MortalSlayer>(id + 0x11, 3, m => m.NumCasts >= 4, "Orbs 2");
        ComponentCondition<MortalSlayer>(id + 0x12, 3, m => m.NumCasts >= 6, "Orbs 3");
        ComponentCondition<MortalSlayer>(id + 0x13, 3, m => m.NumCasts >= 8, "Orbs 4")
            .DeactivateOnExit<MortalSlayer>();
    }

    private void GrotesquerieAct1(uint id, float delay)
    {
        Cast(id, (uint)AID.GrotesquerieAct1, delay, 2.7f)
            .ActivateOnEnter<BurstingGrotesquerieAct1>()
            .ActivateOnEnter<SharedGrotesquerieAct1>()
            .ActivateOnEnter<DirectedGrotesquerieAct1>()
            .ActivateOnEnter<PhagocyteSpotlight0>()
            .ActivateOnEnter<RavenousReach1>()
            .ActivateOnEnter<DramaticLysis0>()
            .ActivateOnEnter<FourthWallFusion0>();

        // Phagocyte spotlight puddles
        ComponentCondition<PhagocyteSpotlight0>(id + 0x10, 6.3f, p => p.NumCasts > 0, "Puddles start");

        // Head AOE (Ravenous Reach cone)
        ComponentCondition<RavenousReach1>(id + 0x20, 11.7f, r => r.NumCasts > 0, "Head AOE")
            .DeactivateOnExit<PhagocyteSpotlight0>()
            .DeactivateOnExit<DirectedGrotesquerieAct1>()
            .DeactivateOnExit<RavenousReach1>();

        // Stack/spread
        ComponentCondition<SharedGrotesquerieAct1>(id + 0x21, 0.3f, g => !g.Active, "Stack/spread/cones")
            .DeactivateOnExit<SharedGrotesquerieAct1>();

        // Big puddles (Burst)
        ComponentCondition<Burst>(id + 0x30, 8.7f, b => b.NumCasts > 0, "Big puddles")
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<VisceralBurst>()
            .DeactivateOnExit<Burst>();

        // Tankbuster spreads
        ComponentCondition<VisceralBurst>(id + 0x31, 0.5f, v => v.NumFinishedSpreads > 0, "Stack + tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<VisceralBurst>()
            .DeactivateOnExit<BurstingGrotesquerieAct1>();

        // Raidwide cleanup
        Cast(id + 0x100, (uint)AID.TheFixer, 5.1f, 5, "Raidwide")
            .ActivateOnEnter<TheFixer>()
            .DeactivateOnExit<TheFixer>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void GrotesquerieAct2(uint id, float delay)
    {
        Cast(id, (uint)AID.GrotesquerieAct2, delay, 2.7f)
            .ActivateOnEnter<GrotesquerieAct2>()
            .ActivateOnEnter<CruelCoil>()
            .ActivateOnEnter<Constrictor>()
            .ActivateOnEnter<PhagocyteSpotlight1>()
            .ActivateOnEnter<DramaticLysis2>()
            .ActivateOnEnter<unk_46194>();

        // Cruel Coil cast
        CastMulti(id + 0x100, [(uint)AID.CruelCoil0, (uint)AID.CruelCoil1, (uint)AID.CruelCoil2], 12.1f, 2.7f);

        // Tower / chain phase — no specific count fields exposed in Reborn, use NumCasts on towers
        ComponentCondition<GrotesquerieAct2>(id + 0x110, 11.9f, c => c.NumCasts >= 1, "Tower/Chain 1")
            .DeactivateOnExit<PhagocyteSpotlight1>();
        ComponentCondition<GrotesquerieAct2>(id + 0x111, 5, c => c.NumCasts >= 2, "Tower/Chain 2");
        ComponentCondition<GrotesquerieAct2>(id + 0x112, 4.2f, c => c.NumCasts >= 3, "Tower/Chain 3");
        ComponentCondition<GrotesquerieAct2>(id + 0x113, 4.2f, c => c.NumCasts >= 4, "Tower/Chain 4")
            .DeactivateOnExit<GrotesquerieAct2>();

        // Center AOE (Constrictor)
        ComponentCondition<Constrictor>(id + 0x200, 9, c => c.NumCasts > 0, "Center AOE")
            .DeactivateOnExit<CruelCoil>()
            .DeactivateOnExit<Constrictor>()
            .DeactivateOnExit<DramaticLysis2>();

        // Unmitigated Explosion raidwide tag
        Cast(id + 0x210, (uint)AID.unk_46194, 0.1f, 0.1f, "Raidwide")
            .DeactivateOnExit<unk_46194>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void GrotesquerieAct3(uint id, float delay)
    {
        // Splattershed raidwide
        Cast(id, (uint)AID.Splattershed1, delay, 2.7f)
            .ActivateOnEnter<Splattershed2>();
        ComponentCondition<Splattershed2>(id + 2, 2.5f, s => s.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<Splattershed2>()
            .SetHint(StateMachine.StateHint.Raidwide);

        // Act 3 main
        Cast(id + 0x100, (uint)AID.GrotesquerieAct3, 14.8f, 2.7f)
            .ActivateOnEnter<GrotesquerieAct3>()
            .ActivateOnEnter<GrandEntrance0>()
            .ActivateOnEnter<GrandEntrance1>()
            .ActivateOnEnter<GrandEntrance2>()
            .ActivateOnEnter<GrandEntrance3>()
            .ActivateOnEnter<BringDownTheHouse0>()
            .ActivateOnEnter<BringDownTheHouse1>()
            .ActivateOnEnter<BringDownTheHouse2>()
            .ActivateOnEnter<DramaticLysis1>();

        Targetable(id + 0x102, false, 3.1f, "Boss disappears");

        // Platforms drop
        ComponentCondition<BringDownTheHouse0>(id + 0x110, 7, b => b.NumCasts > 0, "Platforms")
            .DeactivateOnExit<BringDownTheHouse0>()
            .DeactivateOnExit<BringDownTheHouse1>()
            .DeactivateOnExit<BringDownTheHouse2>()
            .DeactivateOnExit<GrandEntrance0>()
            .DeactivateOnExit<GrandEntrance1>()
            .DeactivateOnExit<GrandEntrance2>()
            .DeactivateOnExit<GrandEntrance3>();

        // Spreads (Dramatic Lysis 1)
        ComponentCondition<DramaticLysis1>(id + 0x120, 2.6f, d => d.NumCasts > 0, "Spreads")
            .DeactivateOnExit<DramaticLysis1>();

        // Towers (Grotesquerie Act 3 component handles them)
        ComponentCondition<GrotesquerieAct3>(id + 0x130, 0.2f, g => g.NumCasts > 0, "Towers")
            .ActivateOnEnter<SplitScourge1>()
            .DeactivateOnExit<GrotesquerieAct3>();

        Targetable(id + 0x140, true, 2.5f, "Boss reappears");

        // Line tankbusters
        ComponentCondition<SplitScourge1>(id + 0x200, 7.4f, s => s.NumCasts > 0, "Line tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<SplitScourge1>();

        // East/west baits — no VenomousScourge component in Reborn, use timed gap
        // Raidwide cleanup
        Cast(id + 0x300, (uint)AID.TheFixer, 6.7f, 5, "Raidwide")
            .ActivateOnEnter<TheFixer>()
            .DeactivateOnExit<TheFixer>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void GrotesquerieCurtainCall(uint id, float delay)
    {
        Cast(id, (uint)AID.GrotesquerieCurtainCall, delay, 2.7f)
            .ActivateOnEnter<GrotesquerieCurtainCall>()
            .ActivateOnEnter<RavenousReach1>()
            .ActivateOnEnter<PhagocyteSpotlight0>();

        // Puddles
        ComponentCondition<PhagocyteSpotlight0>(id + 0x10, 1, p => p.NumCasts > 0, "Puddles start");

        // Head AOE (Ravenous Reach Inverted -> still RavenousReach1 in Reborn)
        ComponentCondition<RavenousReach1>(id + 0x20, 13.8f, r => r.NumCasts > 0, "Head AOE")
            .DeactivateOnExit<PhagocyteSpotlight0>()
            .DeactivateOnExit<RavenousReach1>();

        // Spreads (Curtain Call stack/spread)
        ComponentCondition<GrotesquerieCurtainCall>(id + 0x30, 0.4f, c => !c.Active, "Spreads")
            .ActivateOnEnter<Burst>();

        // Chains appear (no dedicated component in Reborn, use timed gap)
        // Safe corners (Burst goes off)
        ComponentCondition<Burst>(id + 0x50, 9.7f, b => b.NumCasts > 0, "Safe corners")
            .DeactivateOnExit<Burst>()
            .DeactivateOnExit<GrotesquerieCurtainCall>();

        // Splattershed raidwide
        Cast(id + 0x100, (uint)AID.Splattershed0, 3.7f, 2.7f)
            .ActivateOnEnter<Splattershed2>();
        ComponentCondition<Splattershed2>(id + 0x102, 2.5f, s => s.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<Splattershed2>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Slaughtershed(uint id, float delay)
    {
        // Boss Slaughtershed1 or Slaughtershed2 begins the phase
        CastMulti(id, [(uint)AID.Slaughtershed1, (uint)AID.Slaughtershed2], delay, 2.7f)
            .ActivateOnEnter<Slaughtershed>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<FourthWallFusion1>()
            .ActivateOnEnter<DramaticLysis>()
            .ActivateOnEnter<DramaticLysis4>()
            .ActivateOnEnter<SerpentineScourge>()
            .ActivateOnEnter<RaptorKnuckles>()
            .ActivateOnEnter<SerpentineScourge2>()
            .ActivateOnEnter<RaptorKnuckles2>();

        ComponentCondition<Slaughtershed>(id + 2, 2.5f, s => s.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<Slaughtershed>()
            .SetHint(StateMachine.StateHint.Raidwide);

        // Stack + spread
        ComponentCondition<FourthWallFusion1>(id + 3, 8.7f, f => f.NumFinishedStacks > 0, "Stack + spread")
            .DeactivateOnExit<FourthWallFusion1>()
            .DeactivateOnExit<DramaticLysis>()
            .DeactivateOnExit<DramaticLysis4>();

        // Safe corners (Burst)
        ComponentCondition<Burst>(id + 4, 0.5f, b => b.NumCasts > 0, "Safe corners")
            .DeactivateOnExit<Burst>();

        // Left/right cleaves + knockbacks
        ComponentCondition<SerpentineScourge2>(id + 0x10, 6.1f, s => s.NumCasts > 0, "Left/right 1")
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<SerpentineScourge2>(id + 0x11, 4.6f, s => s.NumCasts > 1, "Left/right 2")
            .SetHint(StateMachine.StateHint.Knockback)
            .DeactivateOnExit<SerpentineScourge>()
            .DeactivateOnExit<RaptorKnuckles>()
            .DeactivateOnExit<SerpentineScourge2>()
            .DeactivateOnExit<RaptorKnuckles2>();
    }
}

/*
sealed class M12SLindwurmStates : StateMachineBuilder
{
    public M12SLindwurmStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheFixer>()
            .ActivateOnEnter<MortalSlayer>()
            .ActivateOnEnter<BurstingGrotesquerieAct1>()
            .ActivateOnEnter<SharedGrotesquerieAct1>()
            .ActivateOnEnter<DirectedGrotesquerieAct1>()
            .ActivateOnEnter<PhagocyteSpotlight0>()
            .ActivateOnEnter<RavenousReach1>()
            .ActivateOnEnter<DramaticLysis0>()
            .ActivateOnEnter<FourthWallFusion0>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<VisceralBurst>()
            .ActivateOnEnter<FourthWallFusion2>()
            .ActivateOnEnter<GrotesquerieAct2>()
            .ActivateOnEnter<PhagocyteSpotlight1>()
            .ActivateOnEnter<CruelCoil>()
            .ActivateOnEnter<unk_46194>()
            .ActivateOnEnter<DramaticLysis2>()
            .ActivateOnEnter<UnmitigatedExplosion>()
            .ActivateOnEnter<Constrictor>()
            .ActivateOnEnter<Splattershed2>()
            .ActivateOnEnter<GrotesquerieAct3>()
            .ActivateOnEnter<GrandEntrance1>()
            .ActivateOnEnter<GrandEntrance3>()
            .ActivateOnEnter<GrandEntrance0>()
            .ActivateOnEnter<BringDownTheHouse0>()
            .ActivateOnEnter<BringDownTheHouse1>()
            .ActivateOnEnter<DramaticLysis1>()
            .ActivateOnEnter<SplitScourge1>()
            .ActivateOnEnter<GrandEntrance2>()
            .ActivateOnEnter<BringDownTheHouse2>()
            .ActivateOnEnter<GrotesquerieCurtainCall>()
            //TODO: SplitScourge1 and VenomousScourge baits
            .ActivateOnEnter<Slaughtershed0>()
            .ActivateOnEnter<DramaticLysis4>()
            .ActivateOnEnter<FourthWallFusion1>()
            .ActivateOnEnter<RaptorKnuckles2>()
            .ActivateOnEnter<SerpentineScourge2>()
            .ActivateOnEnter<RefreshingOverkill2>()
            .ActivateOnEnter<RefreshingOverkill1>()
            .ActivateOnEnter<unk_48028>()
            .ActivateOnEnter<unk_46395>()
            .ActivateOnEnter<ArcadiaAflame>()
            .ActivateOnEnter<TopTierSlam1>()
            .ActivateOnEnter<WingedScourge2>()
            .ActivateOnEnter<MightyMagic1>()
            .ActivateOnEnter<EsotericFinisher>()
            .ActivateOnEnter<FirefallSplash1>()
            .ActivateOnEnter<ManaBurst1>()
            .ActivateOnEnter<HeavySlam0>()
            .ActivateOnEnter<UnmitigatedImpact>()
            .ActivateOnEnter<HeavySlam2>()
            .ActivateOnEnter<SerpentineScourge>()
            .ActivateOnEnter<RaptorKnuckles>();
    }
}
*/
