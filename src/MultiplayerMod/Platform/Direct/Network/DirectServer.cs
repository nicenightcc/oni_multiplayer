using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MultiplayerMod.Core.Collections;
using MultiplayerMod.Core.Dependency;
using MultiplayerMod.Core.Extensions;
using MultiplayerMod.Core.Logging;
using MultiplayerMod.Core.Scheduling;
using MultiplayerMod.Core.Unity;
using MultiplayerMod.Multiplayer.Commands;
using MultiplayerMod.Multiplayer.Commands.Registry;
using MultiplayerMod.Network;
using MultiplayerMod.Platform.Direct.Network.Components;
using MultiplayerMod.Platform.Direct.Network.Messaging;
using MultiplayerMod.Platform.Direct.Network.Utils;
using UnityEngine;

namespace MultiplayerMod.Platform.Direct.Network;

[Dependency, UsedImplicitly]
[RequirePlatform(PlatformKind.Direct)]
public class DirectServer : IMultiplayerServer {

    public MultiplayerServerState State { private set; get; } = MultiplayerServerState.Stopped;

    public IMultiplayerEndpoint Endpoint {
        get {
            if (State != MultiplayerServerState.Started)
                throw new NetworkPlatformException("Server isn't started");

            return new DirectServerEndpoint(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Configuration.SERVER_PORT));
        }
    }

    public List<IMultiplayerClientId> Clients => new(clients.Select(it => it.Key));

    public event Action<MultiplayerServerState>? StateChanged;
    public event Action<IMultiplayerClientId>? ClientConnected;
    public event Action<IMultiplayerClientId>? ClientDisconnected;
    public event Action<IMultiplayerClientId, IMultiplayerCommand>? CommandReceived;

    private readonly Core.Logging.Logger log = LoggerFactory.GetLogger<DirectServer>();

    private SocketUtils? server;

    private readonly NetworkMessageProcessor messageProcessor = new();
    private readonly NetworkMessageFactory messageFactory = new();

    private readonly Dictionary<IMultiplayerClientId, uint> clients = new();
    private readonly IMultiplayerClientId currentPlayer = new DirectMultiplayerClientId(0);

    private readonly UnityTaskScheduler scheduler;
    private readonly MultiplayerCommandRegistry commands;

    private GameObject? gameObject;

    public DirectServer(UnityTaskScheduler scheduler, MultiplayerCommandRegistry commands, DirectPlayerProfileProvider profileProvider) {
        this.scheduler = scheduler;
        this.commands = commands;
        this.currentPlayer = profileProvider.GetClientId();
    }

    public void Start() {
        //if (!SteamManager.Initialized)
        //    throw new NetworkPlatformException("Steam API is not initialized");

        log.Debug("Starting...");
        SetState(MultiplayerServerState.Preparing);
        try {
            Initialize();
        } catch (Exception) {
            Reset();
            SetState(MultiplayerServerState.Error);
            throw;
        }
        gameObject = UnityObject.CreateStaticWithComponent<DirectServerComponent>();
    }

    public void Stop() {
        if (State <= MultiplayerServerState.Stopped)
            throw new NetworkPlatformException("Server isn't started");

        log.Debug("Stopping...");
        if (gameObject != null)
            UnityObject.Destroy(gameObject);
        Reset();
        SetState(MultiplayerServerState.Stopped);
    }

    public void Tick() {
        switch (State) {
            case MultiplayerServerState.Starting:
            case MultiplayerServerState.Started:
                server?.RunCallbacks();
                break;
        }
    }

    public void Send(IMultiplayerClientId clientId, IMultiplayerCommand command) {
        var connections = new SingletonCollection<uint>(clients[clientId]);
        SendCommand(command, MultiplayerCommandOptions.None, connections);
    }

    public void Send(IMultiplayerCommand command, MultiplayerCommandOptions options) {
        IEnumerable<KeyValuePair<IMultiplayerClientId, uint>> recipients = clients;
        if (options.HasFlag(MultiplayerCommandOptions.SkipHost))
            recipients = recipients.Where(entry => !entry.Key.Equals(currentPlayer));

        SendCommand(command, options, recipients.Select(it => it.Value));
    }

    private void SetState(MultiplayerServerState state) {
        State = state;
        StateChanged?.Invoke(state);
    }

    private void Initialize() {

        server = new SocketUtils();

        server.ConnectionStatusChanged += HandleConnectionStatusChanged;
        server.MessageReceived += ReceiveMessages;

        server.CreateListenSocket(Configuration.SERVER_PORT);

        SetState(MultiplayerServerState.Starting);

        log.Debug("CreateListenSocket: " + Configuration.SERVER_PORT);

        scheduler.Run(() => { SetState(MultiplayerServerState.Started); });
    }

    private void Reset() {
        server?.Dispose();
    }


    //private void OnLobbyCreated() {
    //    SteamMatchmaking.SetLobbyData(lobby.Id, "server.name", $"{SteamFriends.GetPersonaName()}");
    //}


    private void ReceiveMessages(NetworkingMessage netMessage) {
        var message = messageProcessor.Process(netMessage.ConnectionId, new NetworkMessageHandle(netMessage.Pointer, netMessage.Size));
        if (message != null) {
            IMultiplayerClientId id = new DirectMultiplayerClientId(netMessage.ConnectionId);
            if (!"UpdatePlayerCursorPosition".Equals(message.Command.GetType().Name))
                log.Debug("Message received from - id: " + id + " cmd: " + message.Command.GetType().Name + " connection: " + netMessage.ConnectionId + ", Data length: " + netMessage.Size);

            var configuration = commands.GetCommandConfiguration(message.Command.GetType());
            if (configuration.ExecuteOnServer) {
                CommandReceived?.Invoke(id, message.Command);
            } else {
                var connections = clients.Where(it => !it.Key.Equals(id)).Select(it => it.Value);
                SendCommand(message.Command, message.Options, connections);
            }
            //log.Debug("Message received from - ID: " + netMessage.connection + ", Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
        }
    }

    private void SendCommand(
        IMultiplayerCommand command,
        MultiplayerCommandOptions options,
        IEnumerable<uint> connections
    ) {
        var sequence = messageFactory.Create(command, options);
        sequence.ForEach(handle => connections.ForEach(connection => Send(handle, connection)));
    }

    private void Send(INetworkMessageHandle handle, uint connection) {
        var result = server?.SendMessageToConnection(connection, handle.Pointer, handle.Size);

        if (result != SocketUtils.Result.OK)
            log.Error($"Failed to send message, result: {result}");
    }

    private void HandleConnectionStatusChanged(StatusInfo info) {
        var clientId = new DirectMultiplayerClientId(info.ConnectionId);

        switch (info.ConnectionState) {
            case LiteNetLib.ConnectionState.Connected:
                log.Debug("Client connected - ID: " + info.ConnectionId + ", IP: " + info.Address.ToString());
                AcceptConnection(info.ConnectionId, clientId);
                break;


            case LiteNetLib.ConnectionState.Disconnected:
            case LiteNetLib.ConnectionState.ShutdownRequested:
                CloseConnection(info.ConnectionId, clientId);
                log.Debug("Client disconnected - ID: " + info.ConnectionId + ", IP: " + info.Address.ToString());
                break;
        }
    }

    private void AcceptConnection(uint connection, DirectMultiplayerClientId clientId) {
        if (clients.ContainsKey(clientId)) {
            clients[clientId] = connection;
        } else {
            clients.Add(clientId, connection);
        }
        ClientConnected?.Invoke(clientId);

        log.Debug($"Connection accepted from {clientId}");
    }

    private void CloseConnection(uint connection, DirectMultiplayerClientId clientId) {
        ClientDisconnected?.Invoke(clientId);
        server?.CloseConnection(connection);
        clients.Remove(clientId);
        log.Debug($"Connection closed for {clientId}");
    }

}
