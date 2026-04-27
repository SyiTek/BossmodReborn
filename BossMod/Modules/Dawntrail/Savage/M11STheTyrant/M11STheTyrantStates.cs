namespace BossMod.Dawntrail.Savage.M11STheTyrant;

// Granular M11S state machine, ported from upstream awgil/ffxiv_bossmod (RM11STheTyrantStates.cs)
// AID enum names + component class names translated to Reborn's existing types.
// See translation map in commit message / project memory.
sealed class M11STheTyrantStates : StateMachineBuilder
{
    public M11STheTyrantStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChanges>();
    }

    private void SinglePhase(uint id)
    {
        TrophyWeapons(id, 5.2f);
        DanceOfDomination(id + 0x10000, 10.5f);
        UltimateTrophyWeapons(id + 0x20000, 10.3f);
        Meteorain(id + 0x30000, 7.2f);
        Flatliner(id + 0x40000, 9.2f);
        EclipticStampede(id + 0x50000, 6.3f);
        Heartbreaker(id + 0x60000, 8.7f);

        Cast(id + 0x70000, (uint)AID.EnrageCast, 5, 8, "Boss disappears (enrage)")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    // First TrophyWeapons block: Crown -> RawSteelTrophy -> 2x weapon-bait sequence with VoidStardust spread/stack -> Crown
    private void TrophyWeapons(uint id, float delay)
    {
        Cast(id, (uint)AID.CrownOfArcadia, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<CrownOfArcadia>()
            .DeactivateOnExit<CrownOfArcadia>();

        RawSteelTrophy(id + 0x10, 5.2f);

        // First weapon staging
        Cast(id + 0x100, (uint)AID.TrophyWeapons, 10.5f, 3)
            .ActivateOnEnter<AssaultWeaponTimeline>()
            .ActivateOnEnter<AssaultFloorPredictor>()
            .ActivateOnEnter<AssaultEvolvedSword>()
            .ActivateOnEnter<AssaultEvolvedAxeStack>()
            .ActivateOnEnter<AssaultEvolvedScythe>();

        Cast(id + 0x110, (uint)AID.AssaultEvolved, 3.4f, 6);
        ComponentCondition<AssaultFloorPredictor>(id + 0x112, 2.2f, t => t.NumCasts > 0, "Weapon 1");
        ComponentCondition<AssaultFloorPredictor>(id + 0x113, 5.2f, t => t.NumCasts > 1, "Weapon 2");
        ComponentCondition<AssaultFloorPredictor>(id + 0x114, 5.2f, t => t.NumCasts > 2, "Weapon 3")
            .DeactivateOnExit<AssaultEvolvedSword>()
            .DeactivateOnExit<AssaultEvolvedAxeStack>()
            .DeactivateOnExit<AssaultEvolvedScythe>();

        // Second weapon staging (interleaved with VoidStardust spread/stack)
        Cast(id + 0x200, (uint)AID.TrophyWeapons, 9.2f, 3)
            .ActivateOnEnter<AssaultEvolvedSword>()
            .ActivateOnEnter<AssaultEvolvedAxeStack>()
            .ActivateOnEnter<AssaultEvolvedScythe>();

        Cast(id + 0x210, (uint)AID.VoidStardust, 3.1f, 4)
            .ActivateOnEnter<Cometite>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<CrushingComet>();

        ComponentCondition<Cometite>(id + 0x212, 1.2f, c => c.NumCasts > 0, "Puddles start");

        ComponentCondition<Comet>(id + 0x214, 8.9f, c => c.NumFinishedSpreads + c.NumFinishedStacks > 0, "Stack/spread")
            .DeactivateOnExit<Comet>()
            .DeactivateOnExit<CrushingComet>();

        ComponentCondition<AssaultFloorPredictor>(id + 0x220, 5.1f, t => t.NumCasts > 0, "Weapon 1");
        ComponentCondition<AssaultFloorPredictor>(id + 0x221, 5.2f, t => t.NumCasts > 1, "Weapon 2");
        ComponentCondition<AssaultFloorPredictor>(id + 0x222, 5.2f, t => t.NumCasts > 2, "Weapon 3")
            .DeactivateOnExit<AssaultEvolvedSword>()
            .DeactivateOnExit<AssaultEvolvedAxeStack>()
            .DeactivateOnExit<AssaultEvolvedScythe>();

        // Second VoidStardust spread/stack pulse
        ComponentCondition<Comet>(id + 0x230, 11.2f, c => c.NumFinishedSpreads + c.NumFinishedStacks > 0, "Stack/spread")
            .ActivateOnEnter<Cometite>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<CrushingComet>()
            .DeactivateOnExit<Cometite>()
            .DeactivateOnExit<Comet>()
            .DeactivateOnExit<CrushingComet>();

        Cast(id + 0x240, (uint)AID.CrownOfArcadia, 1, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<CrownOfArcadia>()
            .DeactivateOnExit<CrownOfArcadia>();
    }

    // Raw Steel Trophy: random axe (tankbuster + spread) or scythe (tank cones + healer stack)
    private void RawSteelTrophy(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.RawSteelTrophyAxe, (uint)AID.RawSteelTrophyScythe], delay, 2)
            .ActivateOnEnter<RawSteelTrophyAxe>()
            .ActivateOnEnter<RawSteelTrophyScythe>();

        // RawSteelTrophyAxe.NumCasts increments on either axe-stack or scythe-cone resolution AID;
        // RawSteelTrophyScythe also tracks NumCasts on its own resolution. Use axe as canonical counter.
        Condition(id + 2, 7.5f, () =>
        {
            var axe = Module.FindComponent<RawSteelTrophyAxe>();
            var scythe = Module.FindComponent<RawSteelTrophyScythe>();
            return (axe?.NumCasts ?? 0) > 0 || (scythe?.NumCasts ?? 0) > 0;
        }, "Tankbuster + stack/spread")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<RawSteelTrophyAxe>()
            .DeactivateOnExit<RawSteelTrophyScythe>();
    }

    // Dance of Domination: 4-hit raidwide + Eye of the Hurricane stacks + RawSteelTrophy
    private void DanceOfDomination(uint id, float delay)
    {
        Cast(id, (uint)AID.DanceOfDominationTrophy, delay, 2)
            .ActivateOnEnter<DanceOfDomination>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<EyeOfTheHurricane>();

        ComponentCondition<DanceOfDomination>(id + 0x10, 6.6f, d => d.NumCasts > 0, "Raidwide 1")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DanceOfDomination>(id + 0x11, 4.1f, d => d.NumCasts >= 4, "Raidwide 4")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<DanceOfDomination>();

        ComponentCondition<EyeOfTheHurricane>(id + 0x20, 4.9f, e => e.NumFinishedStacks > 0, "Stacks")
            .DeactivateOnExit<EyeOfTheHurricane>()
            .DeactivateOnExit<Explosion>();

        RawSteelTrophy(id + 0x100, 4.2f);
    }

    // Charybdistopia (1HP) -> Ultimate Trophy Weapons (6 weapons) -> Maelstrom gust baits -> One And Only raidwide
    private void UltimateTrophyWeapons(uint id, float delay)
    {
        Cast(id, (uint)AID.Charybdistopia, delay, 5, "1 HP")
            .ActivateOnEnter<Charybdistopia>()
            .DeactivateOnExit<Charybdistopia>();

        Cast(id + 0x10, (uint)AID.UltimateTrophyWeapons, 2.6f, 3)
            .ActivateOnEnter<MaelstromVoidZones>()
            .ActivateOnEnter<MaelstromGustCones>()
            .ActivateOnEnter<AssaultWeaponTimeline>()
            .ActivateOnEnter<AssaultFloorPredictor>()
            .ActivateOnEnter<AssaultEvolvedSword>()
            .ActivateOnEnter<AssaultEvolvedAxeStack>()
            .ActivateOnEnter<AssaultEvolvedScythe>();

        ComponentCondition<AssaultFloorPredictor>(id + 0x20, 10, t => t.NumCasts > 0, "Weapon 1");
        ComponentCondition<AssaultFloorPredictor>(id + 0x21, 5.2f, t => t.NumCasts > 1, "Weapon 2");
        ComponentCondition<AssaultFloorPredictor>(id + 0x22, 5.2f, t => t.NumCasts > 2, "Weapon 3");
        ComponentCondition<AssaultFloorPredictor>(id + 0x23, 5.2f, t => t.NumCasts > 3, "Weapon 4");
        ComponentCondition<AssaultFloorPredictor>(id + 0x24, 5.2f, t => t.NumCasts > 4, "Weapon 5");
        ComponentCondition<AssaultFloorPredictor>(id + 0x25, 5.2f, t => t.NumCasts > 5, "Weapon 6")
            .DeactivateOnExit<AssaultEvolvedSword>()
            .DeactivateOnExit<AssaultEvolvedAxeStack>()
            .DeactivateOnExit<AssaultEvolvedScythe>()
            .DeactivateOnExit<AssaultWeaponTimeline>()
            .DeactivateOnExit<AssaultFloorPredictor>();

        // Tornado bait resolution (PowerfulGust event sets MaelstromGustCones._resolved)
        ComponentCondition<MaelstromGustCones>(id + 0x30, 6.3f, p => p._resolved, "Tornado baits")
            .DeactivateOnExit<MaelstromGustCones>();

        CastEnd(id + 0x40, 1).ActivateOnEnter<OneAndOnly>();
        Cast(id + 0x100, (uint)AID.OneAndOnly, 4.1f, 6);
        ComponentCondition<OneAndOnly>(id + 0x102, 3, o => o.NumCasts > 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<MaelstromVoidZones>()
            .DeactivateOnExit<OneAndOnly>();
    }

    // Great Wall Of Fire (line buster x2) -> OrbitalOmen + FireAndFury -> Meteorain (comets, cosmic kiss tethers, fearsome fireball line stack) -> TripleTyrannhilation LoS
    private void Meteorain(uint id, float delay)
    {
        Cast(id, (uint)AID.GreatWallOfFire, delay, 5)
            .ActivateOnEnter<GreatWallOfFire>()
            .ActivateOnEnter<GreatWallOfFireExplosion>();

        ComponentCondition<GreatWallOfFireExplosion>(id + 0x10, 0.3f, f => f.NumCasts > 0, "Line buster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<GreatWallOfFireExplosion>(id + 0x11, 3.2f, f => f.NumCasts > 1, "Line buster 2")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<GreatWallOfFire>();

        OrbitalOmen(id + 0x20, 5.1f);

        Cast(id + 0x30, (uint)AID.Meteorain, 7.5f, 5)
            .ActivateOnEnter<MeteorainComets>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<FearsomeFireball>();

        Cast(id + 0x40, (uint)AID.FearsomeFireball, 2.7f, 5);
        ComponentCondition<FearsomeFireball>(id + 0x42, 0.4f, f => f.NumCasts > 0, "Wild charge");
        ComponentCondition<CosmicKiss>(id + 0x43, 1.2f, c => c.NumFinishedSpreads > 0, "Meteors 1")
            .ActivateOnEnter<CometTethers>();
        ComponentCondition<CometTethers>(id + 0x44, 7.9f, f => f.NumCasts > 0, "Tethers");
        ComponentCondition<FearsomeFireball>(id + 0x45, 0.8f, f => f.NumCasts > 1, "Wild charge");

        ComponentCondition<CosmicKiss>(id + 0x46, 1.3f, c => c.NumFinishedSpreads > 2, "Meteors 2");
        ComponentCondition<CometTethers>(id + 0x47, 7.9f, f => f.NumCasts > 2, "Tethers");
        ComponentCondition<FearsomeFireball>(id + 0x48, 0.8f, f => f.NumCasts > 2, "Wild charge");

        ComponentCondition<CosmicKiss>(id + 0x49, 1.3f, c => c.NumFinishedSpreads > 4, "Meteors 3");
        ComponentCondition<CometTethers>(id + 0x4A, 7.9f, f => f.NumCasts > 4, "Tethers");
        ComponentCondition<FearsomeFireball>(id + 0x4B, 0.8f, f => f.NumCasts > 3, "Wild charge")
            .DeactivateOnExit<CosmicKiss>()
            .DeactivateOnExit<CometTethers>()
            .DeactivateOnExit<FearsomeFireball>();

        Cast(id + 0x100, (uint)AID.TripleTyrannhilation2, 5.6f, 7)
            .ActivateOnEnter<TripleTyrannhilation>()
            .ActivateOnEnter<Shockwave>();

        ComponentCondition<Shockwave>(id + 0x102, 1.1f, c => c.NumCasts > 0, "LoS 1");
        ComponentCondition<Shockwave>(id + 0x103, 3.2f, c => c.NumCasts > 2, "LoS 3")
            .DeactivateOnExit<TripleTyrannhilation>()
            .DeactivateOnExit<Shockwave>()
            .DeactivateOnExit<MeteorainComets>();
    }

    // Flatliner (arena split + KB) -> Majestic Meteor (towers, fire breath line baits, tether baits, mass meteor stacks, Arcadion Avalanche platform smashes) -> Crown raidwide
    private void Flatliner(uint id, float delay)
    {
        // Flatliner knockback is 15 units; tower knockback is 23 units
        Cast(id, (uint)AID.Flatliner, delay, 4)
            .SetHint(StateMachine.StateHint.Knockback)
            .ActivateOnEnter<Flatliner>()
            .ActivateOnEnter<FlatlinerKB>();

        ComponentCondition<Flatliner>(id + 0x10, 2, f => f.NumCasts > 0, "Arena split")
            .SetHint(StateMachine.StateHint.Knockback)
            .DeactivateOnExit<Flatliner>()
            .DeactivateOnExit<FlatlinerKB>();

        Cast(id + 0x20, (uint)AID.MajesticMeteor, 9.2f, 5)
            .ActivateOnEnter<ExplosionTowers>()
            .ActivateOnEnter<ExplosionTowerKnockback>()
            .ActivateOnEnter<FireBreath>()
            .ActivateOnEnter<MeteorainPortals>()
            .ActivateOnEnter<MeteorMechanicHints>()
            .ActivateOnEnter<Tether1>()
            .ActivateOnEnter<Tether2>()
            .ActivateOnEnter<MajesticMeteor>();

        ComponentCondition<ExplosionTowers>(id + 0x100, 13.1f, t => t.NumCasts > 0, "Towers 1")
            .SetHint(StateMachine.StateHint.Knockback)
            .DeactivateOnExit<ExplosionTowers>()
            .DeactivateOnExit<ExplosionTowerKnockback>();

        // Fire-breath / portal / tether bait sequence 1
        ComponentCondition<FireBreath>(id + 0x101, 6.1f, m => m.CurrentBaits.Count > 0, "Prey assigned");
        CastStart(id + 0x102, (uint)AID.FireBreath, 0.1f);
        ComponentCondition<MajesticMeteor>(id + 0x103, 2, m => m.NumCasts > 0, "Puddles 1");
        ComponentCondition<MajesticMeteor>(id + 0x104, 4, m => m.NumCasts >= 8, "Puddles 3");
        ComponentCondition<FireBreath>(id + 0x105, 2.8f, m => m.NumCasts > 0, "Baits + line meteors")
            .DeactivateOnExit<FireBreath>()
            .DeactivateOnExit<MeteorainPortals>()
            .DeactivateOnExit<MeteorMechanicHints>()
            .DeactivateOnExit<Tether1>()
            .DeactivateOnExit<Tether2>();

        // Tower set 2 + fire-breath / portal / tether bait sequence 2
        ComponentCondition<ExplosionTowers>(id + 0x200, 17, t => t.NumCasts > 0, "Towers 2")
            .SetHint(StateMachine.StateHint.Knockback)
            .ActivateOnEnter<ExplosionTowers>()
            .ActivateOnEnter<ExplosionTowerKnockback>()
            .ActivateOnEnter<FireBreath>()
            .ActivateOnEnter<MeteorainPortals>()
            .ActivateOnEnter<MeteorMechanicHints>()
            .ActivateOnEnter<Tether1>()
            .ActivateOnEnter<Tether2>();

        ComponentCondition<FireBreath>(id + 0x201, 6, m => m.CurrentBaits.Count > 0, "Prey assigned");
        CastStart(id + 0x202, (uint)AID.FireBreath, 0);
        ComponentCondition<MajesticMeteor>(id + 0x203, 2, m => m.NumCasts >= 9, "Puddles 1");
        ComponentCondition<MajesticMeteor>(id + 0x204, 4, m => m.NumCasts >= 16, "Puddles 3");
        ComponentCondition<FireBreath>(id + 0x205, 2.8f, m => m.NumCasts > 1, "Baits + line meteors")
            .DeactivateOnExit<FireBreath>()
            .DeactivateOnExit<MeteorainPortals>()
            .DeactivateOnExit<MeteorMechanicHints>()
            .DeactivateOnExit<Tether1>()
            .DeactivateOnExit<Tether2>()
            .DeactivateOnExit<MajesticMeteor>();

        // Massive Meteor 5-hit shared stacks
        CastStart(id + 0x300, (uint)AID.MassiveMeteor, 4.4f)
            .ActivateOnEnter<MassiveMeteor>();
        ComponentCondition<MassiveMeteor>(id + 0x301, 6.1f, m => m.NumFinishedStacks > 0, "Stacks 1");
        ComponentCondition<MassiveMeteor>(id + 0x302, 5.9f, m => m.NumFinishedStacks > 0, "Stacks 5")
            .DeactivateOnExit<MassiveMeteor>();

        // Arcadion Avalanche: pick + smash
        CastMulti(id + 0x310, [
            (uint)AID.ArcadionAvalanche_Pick1,
            (uint)AID.ArcadionAvalanche_Pick2,
            (uint)AID.ArcadionAvalanche_Pick3,
            (uint)AID.ArcadionAvalanche_Pick4,
        ], 1.2f, 6)
            .ActivateOnEnter<ArcadionAvalanche>()
            .ActivateOnEnter<ArcadionAvalancheSmash>();

        ComponentCondition<ArcadionAvalanche>(id + 0x312, 9.5f, r => r.NumCasts > 0, "Platform AOE")
            .DeactivateOnExit<ArcadionAvalanche>()
            .DeactivateOnExit<ArcadionAvalancheSmash>();

        Cast(id + 0x400, (uint)AID.CrownOfArcadia, 5.7f, 5, "Raidwide + restore arena")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<CrownOfArcadia>()
            .DeactivateOnExit<CrownOfArcadia>();
    }

    private void OrbitalOmen(uint id, float delay)
    {
        Cast(id, (uint)AID.OrbitalOmen, delay, 5)
            .ActivateOnEnter<OrbitalOmen>()
            .ActivateOnEnter<FireAndFury>()
            .DeactivateOnExit<GreatWallOfFireExplosion>();

        ComponentCondition<OrbitalOmen>(id + 2, 9.1f, o => o.NumCasts == 2, "Orbital start");
        ComponentCondition<FireAndFury>(id + 3, 0.6f, f => f.NumCasts > 0, "Sides safe")
            .DeactivateOnExit<FireAndFury>();
        ComponentCondition<OrbitalOmen>(id + 4, 4, o => o.NumCasts >= 8, "Orbital end")
            .DeactivateOnExit<OrbitalOmen>();
    }

    // Final phase: Great Wall + Orbital Omen + Crown -> Ecliptic Stampede (mammoth meteors, atomic impact spreads, towers, 2/4-way fireball baits) -> Crown raidwide
    private void EclipticStampede(uint id, float delay)
    {
        Cast(id, (uint)AID.GreatWallOfFire, delay, 5)
            .ActivateOnEnter<GreatWallOfFire>()
            .ActivateOnEnter<GreatWallOfFireExplosion>()
            .ActivateOnEnter<CrownOfArcadia>();

        ComponentCondition<GreatWallOfFireExplosion>(id + 0x10, 0.3f, f => f.NumCasts > 0, "Line buster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<GreatWallOfFireExplosion>(id + 0x11, 3.2f, f => f.NumCasts > 1, "Line buster 2")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<GreatWallOfFire>();

        OrbitalOmen(id + 0x20, 5.1f);

        Cast(id + 0x30, (uint)AID.CrownOfArcadia, 0, 4.2f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<CrownOfArcadia>();

        Cast(id + 0x100, (uint)AID.EclipticStampede, 7.4f, 5)
            .ActivateOnEnter<MammothMeteor>()
            .ActivateOnEnter<MajesticMeteorStorm>()
            .ActivateOnEnter<AtomicImpactVoidZones>()
            .ActivateOnEnter<AtomicImpact>();

        ComponentCondition<MammothMeteor>(id + 0x110, 7.1f, m => m.NumCasts > 0, "Proximity + puddles start")
            .ActivateOnEnter<CosmicKissTowers>()
            .ActivateOnEnter<WeightyImpactTowers>()
            .DeactivateOnExit<MammothMeteor>();

        ComponentCondition<CosmicKissTowers>(id + 0x120, 16, k => k.NumCasts > 0, "Towers")
            .ActivateOnEnter<Tether1>()
            .ActivateOnEnter<Tether2>()
            .DeactivateOnExit<MajesticMeteorStorm>()
            .DeactivateOnExit<CosmicKissTowers>()
            .DeactivateOnExit<WeightyImpactTowers>()
            .DeactivateOnExit<AtomicImpact>();

        // Fireball 2/4-way bait sequence
        CastStartMulti(id + 0x130, [(uint)AID.TwoWayFireballStart, (uint)AID.FourWayFireballStart], 7)
            .ActivateOnEnter<TwoWayFireball>()
            .ActivateOnEnter<FourWayFireball>();

        Condition(id + 0x131, 2.6f, () =>
        {
            var t1 = Module.FindComponent<Tether1>();
            var t2 = Module.FindComponent<Tether2>();
            return ((t1?.NumCasts ?? 0) + (t2?.NumCasts ?? 0)) > 0;
        }, "Tethers")
            .DeactivateOnExit<Tether1>()
            .DeactivateOnExit<Tether2>();

        Condition(id + 0x132, 4.3f, () =>
        {
            var two = Module.FindComponent<TwoWayFireball>();
            var four = Module.FindComponent<FourWayFireball>();
            return ((two?.NumCasts ?? 0) + (four?.NumCasts ?? 0)) > 0;
        }, "2/4 stack")
            .DeactivateOnExit<TwoWayFireball>()
            .DeactivateOnExit<FourWayFireball>()
            .DeactivateOnExit<AtomicImpactVoidZones>();

        Cast(id + 0x200, (uint)AID.CrownOfArcadia, 3.3f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<CrownOfArcadia>()
            .DeactivateOnExit<CrownOfArcadia>();
    }

    // Heartbreaker: 3 escalating waves of 8-soak towers (5/6/7 hits)
    private void Heartbreaker(uint id, float delay)
    {
        Cast(id, (uint)AID.HeartbreakKickTower, delay, 5)
            .ActivateOnEnter<HeartBreakerTower>();
        ComponentCondition<HeartBreakerTower>(id + 2, 1.2f, k => k.NumCasts > 0, "Tower 1");
        ComponentCondition<HeartBreakerTower>(id + 3, 8, k => k.NumCasts >= 5, "Tower 5");

        Cast(id + 0x100, (uint)AID.HeartbreakKickTower, 5, 5);
        ComponentCondition<HeartBreakerTower>(id + 0x102, 1.2f, k => k.NumCasts >= 6, "Tower 1");
        ComponentCondition<HeartBreakerTower>(id + 0x103, 10, k => k.NumCasts >= 11, "Tower 6");

        Cast(id + 0x200, (uint)AID.HeartbreakKickTower, 5, 5);
        ComponentCondition<HeartBreakerTower>(id + 0x202, 1.2f, k => k.NumCasts >= 12, "Tower 1");
        ComponentCondition<HeartBreakerTower>(id + 0x203, 12, k => k.NumCasts >= 18, "Tower 7")
            .DeactivateOnExit<HeartBreakerTower>();
    }
}
