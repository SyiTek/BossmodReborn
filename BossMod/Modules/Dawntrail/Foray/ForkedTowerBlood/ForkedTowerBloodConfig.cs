using Dalamud.Bindings.ImGui;

namespace BossMod.Dawntrail.Foray.ForkedTowerBlood;

[ConfigDisplay(Order = 0x300, Parent = typeof(DawntrailConfig))]
public sealed class ForkedTowerBloodConfig : ConfigNode
{
    public enum Alliance
    {
        [PropertyDisplay("None - only show generic hints")]
        None,
        A,
        B,
        C,
        [PropertyDisplay("D/1")]
        D1,
        [PropertyDisplay("E/2")]
        E2,
        [PropertyDisplay("F/3")]
        F3
    }

    [PropertyDisplay("Alliance assignment for hints")]
    public Alliance PlayerAlliance = Alliance.None;

    [PropertyDisplay("Enable config overlay while inside Forked Tower")]
    public bool DrawOverlay = true;
}

public static class ForkedTowerBloodConfigExtensions
{
    public static int Group2(this ForkedTowerBloodConfig.Alliance a) => a switch
    {
        ForkedTowerBloodConfig.Alliance.A or ForkedTowerBloodConfig.Alliance.B or ForkedTowerBloodConfig.Alliance.C => 1,
        ForkedTowerBloodConfig.Alliance.D1 or ForkedTowerBloodConfig.Alliance.E2 or ForkedTowerBloodConfig.Alliance.F3 => 2,
        _ => 0
    };

    public static int Group3(this ForkedTowerBloodConfig.Alliance a) => a switch
    {
        ForkedTowerBloodConfig.Alliance.A or ForkedTowerBloodConfig.Alliance.D1 => 1,
        ForkedTowerBloodConfig.Alliance.B or ForkedTowerBloodConfig.Alliance.E2 => 2,
        ForkedTowerBloodConfig.Alliance.C or ForkedTowerBloodConfig.Alliance.F3 => 3,
        _ => 0
    };
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 1018u)]
public sealed class FTBAllianceSelector(WorldState ws) : ZoneModule(ws)
{
    private readonly ForkedTowerBloodConfig _config = Service.Config.Get<ForkedTowerBloodConfig>();

    public override bool WantDrawExtra() => _config.DrawOverlay && World.Party.Player()?.PosRot.Y < -200;

    public override void DrawExtra()
    {
        var a = _config.PlayerAlliance;

        if (UICombo.Enum("Alliance assignment for hints", ref a))
        {
            _config.PlayerAlliance = a;
            _config.Modified.Fire();
        }

        if (ImGui.Button("Hide this window"))
        {
            _config.DrawOverlay = false;
            _config.Modified.Fire();
        }
    }
}
