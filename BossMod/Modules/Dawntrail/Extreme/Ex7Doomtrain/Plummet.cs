namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

[SkipLocalsInit]
sealed class Plummet(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Plummet, 8f);
