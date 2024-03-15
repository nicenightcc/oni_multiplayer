using JetBrains.Annotations;
using MultiplayerMod.Core.Dependency;
using MultiplayerMod.Core.Unity;
using MultiplayerMod.Multiplayer;
using MultiplayerMod.Platform.Direct.Network.Components;

namespace MultiplayerMod.Platform.Direct;

[Dependency, UsedImplicitly]
[RequirePlatform(PlatformKind.Direct)]
public class DirectMultiplayerOperations : IMultiplayerOperations {

    public void Join() => UnityObject.CreateStaticWithComponent<DirectJoinDialogComponent>();

}
