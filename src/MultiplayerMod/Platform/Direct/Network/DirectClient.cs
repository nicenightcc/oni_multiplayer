using System;
using JetBrains.Annotations;
using MultiplayerMod.Core.Dependency;
using MultiplayerMod.Core.Extensions;
using MultiplayerMod.Core.Logging;
using MultiplayerMod.Core.Scheduling;
using MultiplayerMod.Core.Unity;
using MultiplayerMod.Multiplayer.Commands;
using MultiplayerMod.Network;
using MultiplayerMod.Platform.Direct.Network.Components;
using MultiplayerMod.Platform.Direct.Network.Messaging;
using MultiplayerMod.Platform.Direct.Network.Utils;
using UnityEngine;

namespace MultiplayerMod.Platform.Direct.Network;

[Dependency, UsedImplicitly]
[RequirePlatform(PlatformKind.Direct)]
public class DirectClient : IMultiplayerClient {
    public IMultiplayerClientId Id { get => playerContainer; }
    public MultiplayerClientState State { get; private set; } = MultiplayerClientState.Disconnected;
    public event Action<MultiplayerClientState>? StateChanged;
    public event Action<IMultiplayerCommand>? CommandReceived;

    private readonly Core.Logging.Logger log = LoggerFactory.GetLogger<DirectClient>();

    private readonly DirectMultiplayerClientId playerContainer;

    private readonly NetworkMessageProcessor messageProcessor = new();
    private readonly NetworkMessageFactory messageFactory = new();
    private SocketUtils? client;


    private GameObject gameObject = null!;
    private readonly UnityTaskScheduler scheduler;

    public DirectClient(UnityTaskScheduler scheduler, DirectPlayerProfileProvider profileProvider) {
        this.scheduler = scheduler;
        this.playerContainer = profileProvider.GetClientId();
    }

    public void Connect(IMultiplayerEndpoint endpoint) {
        //if (!SteamManager.Initialized)
        //    return;
        DirectServerEndpoint server = (DirectServerEndpoint) endpoint;

        SetState(MultiplayerClientState.Connecting);

        client = new SocketUtils();

        client.MessageReceived += ReceiveCommands;
        client.ConnectionStatusChanged += HandleConnectionStatusChanged;

        var id = this.playerContainer.Id;

        client.Connect(server.endpoint.Address.ToString(), server.endpoint.Port, id);

        log.Debug($"Connecting to {server.endpoint.ToString()} using id: {id}");

        //SetRichPresence();

        gameObject = UnityObject.CreateStaticWithComponent<DirectClientComponent>();

    }

    public void Disconnect() {
        if (State == MultiplayerClientState.Disconnected)
            throw new NetworkPlatformException("Client not connected");

        UnityObject.Destroy(gameObject);
        client?.Dispose();
        SetState(MultiplayerClientState.Disconnected);
        //SteamFriends.ClearRichPresence();
    }

    public void Tick() {
        switch (State) {
            case MultiplayerClientState.Connecting:
            case MultiplayerClientState.Connected:
                client?.RunCallbacks();
                break;
        }
    }

    public void Send(IMultiplayerCommand command, MultiplayerCommandOptions options = MultiplayerCommandOptions.None) {
        if (State != MultiplayerClientState.Connected)
            throw new NetworkPlatformException("Client not connected");

        messageFactory.Create(command, options).ForEach(
            handle => {
                var result = client?.SendMessageToServer(handle.Pointer, handle.Size);

                if (result != SocketUtils.Result.OK) {
                    log.Error($"Failed to send {command}: {result}");
                    SetState(MultiplayerClientState.Error);
                }
            }
        );
    }

    private void SetState(MultiplayerClientState status) {
        State = status;
        StateChanged?.Invoke(status);
    }


    private void HandleConnectionStatusChanged(StatusInfo info) {
        if (info.ConnectionState == LiteNetLib.ConnectionState.Connected) {
            log.Debug($"Connected to Server ID: {info.ConnectionId}");
            //if (this.lobby.Host && this.lobby.Id != null) {
            //    this.Id = this.lobby.Id;
            //} else {
            //this.Id = new DirectMultiplayerClientId(info.ConnectionId);
            //}
            this.scheduler.Run(() => {
                SetState(MultiplayerClientState.Connected);
            });
        }
    }

    //private void SetRichPresence() {
    //    SteamFriends.SetRichPresence("connect", $"+connect_lobby {lobby.Id}");
    //}

    //private SteamNetworkingIdentity GetNetworkingIdentity(CSteamID steamId) {
    //    var identity = new SteamNetworkingIdentity();
    //    identity.SetSteamID(steamId);
    //    return identity;
    //}

    private void ReceiveCommands(NetworkingMessage netMessage) {
        var message = messageProcessor.Process(netMessage.ConnectionId, new NetworkMessageHandle(netMessage.Pointer, netMessage.Size));
        if (message != null) {
            if (!"UpdatePlayerCursorPosition".Equals(message.Command.GetType().Name))
                log.Debug("Message received from - server -  cmd: " + message.Command.GetType().Name + " connection: " + netMessage.ConnectionId + ", Data length: " + netMessage.Size);

            CommandReceived?.Invoke(message.Command);
        }

    }

}
