namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

[SkipLocalsInit]
sealed class CrimeAndPunishmentHint(BossModule module) : Components.CastHint(module, (uint)AID.CrimeAndPunishmentVisual, "")
{
    private readonly LightAndDark? _light = module.FindComponent<LightAndDark>();

    public override void AddGlobalHints(GlobalHints hints) { } // don't draw

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Active && _light != null && _light.LightBuff[slot])
        {
            hints.Add("Switch to dark debuff!");
        }
    }
}

[SkipLocalsInit]
sealed class SinBearer(BossModule module) : BossComponent(module)
{
    public Actor? Bearer;
    public int BearerStacks;
    public bool HaveOrder;
    public bool HaveValidOrder;
    public readonly int[] PassSlots = Utils.MakeArray(4, -1); // player slots sorted by when they should receive debuff
    public readonly int[] PassOrder = Utils.MakeArray(4, -1);

    public BitMask Immunes;

    private readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();
    private readonly Q1FinalVerseConfig _config = Service.Config.Get<Q1FinalVerseConfig>();

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_SinBearer && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            Bearer = actor;
            BearerStacks = status.Extra;

            if (!HaveOrder)
            {
                CalculateOrder(slot);
            }
        }

        if (status.ID == (uint)SID._Gen_WithoutSin && Raid.FindSlot(actor.InstanceID) is var slot2 && slot2 >= 0)
        {
            Immunes.Set(slot2);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_SinBearer && Immunes.NumSetBits() == 3)
        {
            Bearer = null;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!HaveValidOrder || Immunes[slot] || Immunes.NumSetBits() >= 3)
        {
            return;
        }

        var ix = PassSlots.IndexOf(slot);
        if (ix >= 0)
        {
            hints.Add($"Order: {ix + 1}", false);
        }

        if (actor == Bearer && BearerStacks >= 10 && ix < 3)
        {
            var stackedWithBuddy = false;
            var stackedWithOther = false;

            foreach (var (s, a) in Raid.WithSlot().InRadiusExcluding(actor, 4f))
            {
                var isBuddy = IsNextPass(slot, s);
                stackedWithBuddy |= isBuddy;
                stackedWithOther |= !isBuddy && !Immunes[s];
            }

            hints.Add("Stack with buddy!", !stackedWithBuddy);
            if (stackedWithOther)
            {
                hints.Add("GTFO from forbidden players!");
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Immunes[pcSlot])
        {
            return;
        }

        if (!HaveValidOrder)
        {
            if (Bearer is { } b)
            {
                if (MiniArena.Config.ShowOutlinesAndShadows)
                {
                    Arena.AddCircle(b.Position, 4f, 0xFF000000u, 2f);
                }
                Arena.AddCircle(b.Position, 4f, Colors.Danger);
            }
            return;
        }

        foreach (var (playerSlot, player) in Raid.WithSlot())
        {
            if (player == Bearer)
            {
                if (MiniArena.Config.ShowOutlinesAndShadows)
                {
                    Arena.AddCircle(player.Position, 4f, 0xFF000000u, 2f);
                }

                if (IsNextPass(playerSlot, pcSlot) || playerSlot == pcSlot)
                {
                    Arena.AddCircle(player.Position, 4f, Colors.Safe);
                }
                else
                {
                    Arena.AddCircle(player.Position, 4f, Colors.Danger);
                }
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var basePrio = base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);

        if (!HaveValidOrder || Immunes[pcSlot] || Immunes[playerSlot])
        {
            return basePrio;
        }

        if (IsNextPass(playerSlot, pcSlot))
        {
            return PlayerPriority.Interesting;
        }

        if (pc == Bearer)
        {
            return IsNextPass(pcSlot, playerSlot) ? PlayerPriority.Interesting : PlayerPriority.Danger;
        }

        if (player == Bearer)
        {
            return PlayerPriority.Danger;
        }

        return basePrio;
    }

    private bool IsNextPass(int player1Slot, int player2Slot) => PassOrder[player1Slot] + 1 == PassOrder[player2Slot];

    private void CalculateOrder(int first)
    {
        HaveOrder = true;

        var roles = _prc.EffectiveRolePerSlot(Raid);
        if (roles.Length == 0) // invalid assignments, can't calculate order
        {
            return;
        }

        switch (_config.SinBearerOrder)
        {
            case Q1FinalVerseConfig.SinBearer.None:
                return; // no order defined

            case Q1FinalVerseConfig.SinBearer.AccelFirst:
                try
                {
                    PassSlots[0] = first;
                    PassSlots[1] = Array.IndexOf(roles, PartnerMMRR(roles[PassSlots[0]]));
                    PassSlots[2] = Array.IndexOf(roles, PartnerRole(roles[PassSlots[1]]));
                    PassSlots[3] = Array.IndexOf(roles, PartnerMMRR(roles[PassSlots[2]]));
                    for (var i = 0; i < PassSlots.Length; ++i)
                    {
                        PassOrder[PassSlots[i]] = i;
                    }
                    HaveValidOrder = true;
                }
                catch (IndexOutOfRangeException) // assignments invalid somehow
                {
                    Array.Fill(PassSlots, -1);
                    Array.Fill(PassOrder, -1);
                    HaveValidOrder = false;
                }
                break;
        }
    }

    private static Role PartnerMMRR(Role r) => r switch
    {
        Role.Tank => Role.Melee,
        Role.Melee => Role.Tank,
        Role.Ranged => Role.Healer,
        Role.Healer => Role.Ranged,
        _ => Role.None
    };

    private static Role PartnerRole(Role r) => r switch
    {
        Role.Tank => Role.Healer,
        Role.Healer => Role.Tank,
        Role.Melee => Role.Ranged,
        Role.Ranged => Role.Melee,
        _ => Role.None
    };
}

[SkipLocalsInit]
sealed class Doom(BossModule module) : BossComponent(module)
{
    private Actor? _victim;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_Doom)
        {
            _victim = actor;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_Doom && _victim == actor)
        {
            _victim = null;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_victim is { } t && t.PendingDispels.Count == 0 && actor.Class.CanEsuna())
        {
            hints.Add($"Cleanse {t.Name}!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_victim != null && Raid.FindSlot(_victim.InstanceID) is var v && v >= 0)
        {
            hints.ShouldCleanse.Set(v);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => pc.Class.CanEsuna() && player == _victim ? PlayerPriority.Critical : PlayerPriority.Normal;
}
