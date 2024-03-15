using System;
using JetBrains.Annotations;
using MultiplayerMod.Core.Dependency;
using MultiplayerMod.Multiplayer.Players;
using Steamworks;

namespace MultiplayerMod.Platform.Steam;

[Dependency, UsedImplicitly]
[RequirePlatform(PlatformKind.Steam)]
public class SteamPlayerProfileProvider : IPlayerProfileProvider {

    private readonly Lazy<PlayerProfile> profile = new(
        () => new PlayerProfile(
            SteamFriends.GetPersonaName()
        )
    );

    public PlayerProfile GetPlayerProfile() => profile.Value;

}
