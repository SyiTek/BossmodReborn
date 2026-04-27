namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

[ConfigDisplay(Order = 0x130, Parent = typeof(DawntrailConfig))]
public sealed class M08SHowlingBladeConfig() : ConfigNode()
{
    [PropertyDisplay("Show platform numbers")]
    public bool ShowPlatformNumbers = true;

    [PropertyDisplay("Platform number colors:")]
    public Color[] PlatformNumberColors = [new(0xffffffff), new(0xffffffff), new(0xffffffff), new(0xffffffff), new(0xffffffff)];

    [PropertyDisplay("Platform number font size")]
    [PropertySlider(0.1f, 100, Speed = 1)]
    public float PlatformNumberFontSize = 22;

    public enum ReignStrategy
    {
        [PropertyDisplay("Show both safespots for current role")]
        Any,
        [PropertyDisplay("Assume G1 left, G2 right when looking at boss from arena center")]
        Standard,
        [PropertyDisplay("Assume G1 right, G2 left when looking at boss from arena center")]
        Inverse,
        [PropertyDisplay("None")]
        Disabled
    }

    [PropertyDisplay("Revolutionary/Eminent Reign positioning hints")]
    public ReignStrategy ReignHints = ReignStrategy.Standard;

    [PropertyDisplay("Show Rinon/Toxic Friends tower spots for Lone Wolf's Lament")]
    public bool LoneWolfsLamentHints = true;

    public enum TerrestrialRageStrategy
    {
        [PropertyDisplay("No hints")]
        None,
        [PropertyDisplay("Clocks - stack marker goes to N/NE safe spot, spreads adjust")]
        Clock,
    }

    [PropertyDisplay("Terrestrial Rage")]
    public TerrestrialRageStrategy TRHints = TerrestrialRageStrategy.None;

    [PropertyDisplay("Windfang/Stonefang clock spots", tooltip: "Only used by AI")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("Default", [0, 4, 6, 2, 5, 3, 7, 1])]
    public GroupAssignmentUnique WindfangStonefangSpots = new() { Assignments = [0, 4, 6, 2, 5, 3, 7, 1] };
}
