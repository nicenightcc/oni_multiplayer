using JetBrains.Annotations;
using MultiplayerMod.Core.Dependency;
using MultiplayerMod.Core.Events;
using MultiplayerMod.Multiplayer.CoreOperations.Events;

namespace MultiplayerMod.Multiplayer.UI;

[Dependency, UsedImplicitly]
public class Notifications {

    public Notifications(EventDispatcher events) {
        events.Subscribe<ConnectionLostEvent>(OnConnectionLost);
    }

    private void OnConnectionLost(ConnectionLostEvent @event) {
        var screen = (InfoDialogScreen) GameScreenManager.Instance.StartScreen(
            ScreenPrefabs.Instance.InfoDialogScreen.gameObject,
            GameScreenManager.Instance.ssOverlayCanvas.gameObject
        );
        screen.SetHeader(Core.Strings.UI.MULTIPLAYER);
        screen.AddPlainText(Core.Strings.NETWORK.ERROR.CONNECTION_LOST);
        screen.AddOption(
           STRINGS.UI.CONFIRMDIALOG.OK,
            _ => PauseScreen.Instance.OnQuitConfirm()
        );
    }
}
