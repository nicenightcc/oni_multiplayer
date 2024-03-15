using MultiplayerMod.Core.Dependency;
using MultiplayerMod.Core.Unity;

// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace MultiplayerMod.Platform.Direct.Network.Components;

public class DirectServerComponent : MultiplayerMonoBehaviour
{

    [InjectDependency]
    private DirectServer server = null!;

    private void Update() => server.Tick();

}
