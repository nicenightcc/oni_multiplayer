using System;
using MultiplayerMod.Multiplayer.UI.Overlays;

namespace MultiplayerMod.Multiplayer.Commands.World;

[Serializable]
[MultiplayerCommand(Type = MultiplayerCommandType.System)]
public class NotifyWorldSavePreparing : MultiplayerCommand {

    public override void Execute(MultiplayerCommandContext context) {
        MultiplayerStatusOverlay.Show(Core.Strings.UI.OVERLAY.WAITING_WORLD);
    }

}
