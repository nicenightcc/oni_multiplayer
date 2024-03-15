using System;
using MultiplayerMod.Platform.Steam;
using PeterHan.PLib.Options;

namespace MultiplayerMod.ModRuntime;

[RestartRequired]
[Serializable]
public class Config : SingletonOptions<Config> {
    public Config() {
        var steam = DistributionPlatform.Inst.Platform == "Steam";
        if (steam) {
            this.NickName = new SteamPlayerProfileProvider().GetPlayerProfile().PlayerName;
            this.UseSteamNetwork = true;
        } else {
            this.NickName = "Player";
            this.UseSteamNetwork = false;
        }
    }

    [Option("UseSteamNetwork ", "If checked then use steam network, else use direct network.", "MultiplayerMod")]
    public bool UseSteamNetwork { get; set; }

    [Option("PlayerName", "Player Name", "MultiplayerMod")]
    public string NickName { get; set; }

}

