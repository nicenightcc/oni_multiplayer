using System;
using MultiplayerMod.ModRuntime;

namespace MultiplayerMod.Platform;

[AttributeUsage(AttributeTargets.Class)]
public class RequirePlatformAttribute : Attribute {
    public PlatformKind Platform { get; set; }
    public RequirePlatformAttribute(PlatformKind platform) {
        Platform = platform;
    }

    public bool CheckPlatform() =>
        PlatformSelector.Platform == Platform;
}
public enum PlatformKind {
    Steam,
    Direct
}
public static class PlatformSelector {
    private static Lazy<PlatformKind> platform = new Lazy<PlatformKind>(() =>
        Config.Instance.UseSteamNetwork ? PlatformKind.Steam : PlatformKind.Direct
    );
    public static PlatformKind Platform { get => platform.Value; }
}
