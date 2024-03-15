using System;
using JetBrains.Annotations;
using MultiplayerMod.Core.Dependency;
using MultiplayerMod.Multiplayer.Players;
using MultiplayerMod.Platform.Direct.Network;
using Steamworks;

namespace MultiplayerMod.Platform.Direct;

[Dependency, UsedImplicitly]
[RequirePlatform(PlatformKind.Direct)]
public class DirectPlayerProfileProvider : IPlayerProfileProvider {

    private readonly Lazy<PlayerProfile> profile = new(
        () => new PlayerProfile(
            SteamFriends.GetPersonaName()
        )
    );

    public PlayerProfile GetPlayerProfile() => profile.Value;


    private readonly Lazy<DirectMultiplayerClientId> clientId = new(
        () => new DirectMultiplayerClientId(
            (uint) SteamUser.GetSteamID().m_SteamID
        )
    );

    public DirectMultiplayerClientId GetClientId() => clientId.Value;
}
