using System;

namespace MultiplayerMod.Platform.Direct.Network;

public class NetworkPlatformException : PlatformException
{
    public NetworkPlatformException(string message) : base(message) { }
    public NetworkPlatformException(string message, Exception cause) : base(message, cause) { }
}
