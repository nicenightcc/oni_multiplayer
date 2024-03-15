using System.Net;
using MultiplayerMod.Network;


namespace MultiplayerMod.Platform.Direct.Network;

public record DirectServerEndpoint(IPEndPoint endpoint) : IMultiplayerEndpoint {
    //public static readonly DirectServerEndpoint Self = new DirectServerEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Configuration.ServerPort));
}
