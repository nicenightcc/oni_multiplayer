using System.Collections.Generic;
using MultiplayerMod.Core.Dependency;
using MultiplayerMod.Core.Events;
using MultiplayerMod.Core.Extensions;
using MultiplayerMod.Core.Unity;
using MultiplayerMod.Multiplayer.Players.Events;
using UnityEngine;

namespace MultiplayerMod.Multiplayer.Components;

public class MultiplayerPlayerNotifier : MultiplayerMonoBehaviour {

    [InjectDependency]
    private readonly EventDispatcher dispatcher = null!;

    private const float notificationTimeout = 120f;
    private readonly LinkedList<Notification> notifications = new();
    private EventSubscription subscription = null!;
    private bool removalPending;

    protected override void Awake() {
        base.Awake();
        subscription = dispatcher.Subscribe<PlayerJoinedEvent>(OnPlayerJoin).Subscribe<PlayerLeftEvent>(OnPlayerLeft);
        NotificationManager.Instance.notificationRemoved += OnNotificationRemoved;
    }

    private void OnDestroy() {
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.notificationRemoved -= OnNotificationRemoved;
        subscription.Cancel();
    }

    private void OnPlayerJoin(PlayerJoinedEvent @event) {
        var playerName = @event.Player.Profile.PlayerName;
        var message = string.Format(Core.Strings.NETWORK.PLAYER.JOINED_NOTIFY, playerName);
        var description = string.Format(Core.Strings.NETWORK.PLAYER.JOINED_NOTIFY, playerName);
        AddNotification(message, description, NotificationType.Good);
    }

    private void OnPlayerLeft(PlayerLeftEvent @event) {
        var playerName = @event.Player.Profile.PlayerName;
        var message = string.Format(Core.Strings.NETWORK.PLAYER.LEFT_NOTIFY, playerName);
        var description = string.Format(@event.Gracefully ? Core.Strings.NETWORK.PLAYER.LEFT_NOTIFY : Core.Strings.NETWORK.PLAYER.DISCONN_NOTIFY, playerName);
        AddNotification(message, description, NotificationType.BadMinor);
    }

    private void AddNotification(string message, string tooltip, NotificationType type) {
        var notification = new Notification(
            message,
            type,
            tooltip: (_, _) => tooltip,
            expires: false,
            clear_on_click: true,
            show_dismiss_button: true
        ) {
            GameTime = Time.unscaledTime,
            Time = KTime.Instance.UnscaledGameTime,
            Delay = -Time.unscaledTime
        };
        NotificationManager.Instance.AddNotification(notification);
        notifications.AddLast(notification);
    }

    private void OnNotificationRemoved(Notification notification) {
        if (!removalPending)
            notifications.Remove(notification);
    }

    private void Update() {
        notifications.ForEach(
            (notification, node) => {
                if (Time.unscaledTime - notification.GameTime < notificationTimeout)
                    return;

                removalPending = true;
                NotificationManager.Instance.RemoveNotification(notification);
                notifications.Remove(node);
                removalPending = false;
            }
        );
    }

}
