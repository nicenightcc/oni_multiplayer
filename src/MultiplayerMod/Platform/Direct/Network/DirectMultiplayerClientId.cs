using System;
using MultiplayerMod.Network;

namespace MultiplayerMod.Platform.Direct.Network;

[Serializable]
public record DirectMultiplayerClientId(uint Id) : IMultiplayerClientId {
    //public static DirectMultiplayerClientId Current { get => new DirectMultiplayerClientId(SocketUtils.Id); }

    //public static DirectMultiplayerClientId Host = new DirectMultiplayerClientId(new NetworkingIdentity());

    public bool Equals(IMultiplayerClientId other) {
        return other is DirectMultiplayerClientId player && player.Id.Equals(this.Id);
    }

}
