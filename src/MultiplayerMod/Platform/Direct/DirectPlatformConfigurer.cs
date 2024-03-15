using JetBrains.Annotations;
using MultiplayerMod.Core.Dependency;
using MultiplayerMod.Core.Logging;
using MultiplayerMod.Core.Unity;
using MultiplayerMod.ModRuntime.Loader;
using MultiplayerMod.Platform.Direct.Network.Components;

namespace MultiplayerMod.Platform.Direct;

[UsedImplicitly]
[ModComponentOrder(ModComponentOrder.Platform)]
public class DirectPlatformConfigurer : IModComponentConfigurer {

    private readonly Core.Logging.Logger log = LoggerFactory.GetLogger<DirectPlatformConfigurer>();

    public void Configure(DependencyContainerBuilder builder) {
        //var steam = DistributionPlatform.Inst.Platform == "Steam";
        //if (!steam)
        //    return;
        if (PlatformSelector.Platform != PlatformKind.Direct)
            return;

        log.Info("Direct platform detected");

        //builder.ContainerCreated += _ => UnityObject.CreateStaticWithComponent<LobbyJoinRequestComponent>();
    }

}
