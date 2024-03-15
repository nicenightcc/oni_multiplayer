using System;
using System.Net;
using MultiplayerMod.Core.Dependency;
using MultiplayerMod.Core.Events;
using MultiplayerMod.Core.Logging;
using MultiplayerMod.Core.Unity;
using MultiplayerMod.Game.UI;
using MultiplayerMod.ModRuntime.StaticCompatibility;
using MultiplayerMod.Multiplayer.Players.Events;
using PeterHan.PLib.UI;
using Steamworks;
using TMPro;
using UnityEngine;

// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace MultiplayerMod.Platform.Direct.Network.Components;

public class DirectJoinDialogComponent : MultiplayerMonoBehaviour {

    private readonly Core.Logging.Logger log = LoggerFactory.GetLogger<DirectJoinDialogComponent>();

    [InjectDependency]
    private EventDispatcher eventDispatcher = null!;

    protected override void Awake() {
        base.Awake();

        string text = string.Empty;
        var dialog = new PDialog("ip") {
            Title = Core.Strings.UI.DIALOG.JOIN_TITLE, Size = new Vector2(360.0f, 160.0f),
            DialogClosed = action => {
                switch (action) {
                    case "ok":
                        log.Info("DialogClosed: " + text);
                        ConnectTo(text);
                        break;
                }
                Closed();
            }
        };

        var input = new PTextField() {
            PlaceholderText = Core.Strings.UI.DIALOG.JOIN_TOOLTIP,
            ToolTip = Core.Strings.UI.DIALOG.JOIN_TOOLTIP,
            MinWidth = 320,
            Type = PTextField.FieldType.Text,
            TextAlignment = TextAlignmentOptions.Left,
            OnTextChanged = (_, t) => text = t,
        };
        input.TextStyle.fontSize = 20;
        input.OnRealize += (go) => { go.GetComponent<TMP_InputField>().ActivateInputField(); };
        dialog.Body.AddChild(input);

        dialog.AddButton("cancel", STRINGS.UI.CONFIRMDIALOG.CANCEL, null);
        dialog.AddButton("ok", Core.Strings.UI.DIALOG.JOIN_BUTTON, null);

        dialog.Show();
    }

    public void ConnectTo(string s) {
        if (string.IsNullOrWhiteSpace(s) || s.StartsWith(".") || s.EndsWith("."))
            return;
        IPAddress? address = null;
        if (!IPAddress.TryParse(s, out address)) {
            address = Dns.GetHostAddresses(s)?[0];
        }
        if (address != null) {
            var endpoint = new IPEndPoint(address, Configuration.SERVER_PORT);
            eventDispatcher.Dispatch(new MultiplayerJoinRequestedEvent(new DirectServerEndpoint(endpoint), Core.Strings.NETWORK.DIRECT.HOST_NAME));
        }
    }

    public void Closed() {
        UnityObject.Destroy(gameObject);
    }

}
