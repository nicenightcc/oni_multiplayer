using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using LiteNetLib;
using LiteNetLib.Utils;

namespace MultiplayerMod.Platform.Direct.Network.Utils;
public struct NetworkingMessage {
    public IntPtr Pointer;
    public uint Size;
    public uint ConnectionId;
}
public struct StatusInfo {
    public uint ConnectionId;
    public IPEndPoint Address;
    public ConnectionState ConnectionState;
}

public class SocketUtils : IDisposable {

    public enum Result {
        OK = 1,
        Fail = 2,
    }
    const string VERIFY_KEY = "ONIMP";
    public Action<StatusInfo>? ConnectionStatusChanged;
    public Action<NetworkingMessage>? MessageReceived;
    private EventBasedNetListener? listener;
    private NetManager? socket;
    private NetPeer? peer;
    private Dictionary<uint, NetPeer> clients = new Dictionary<uint, NetPeer>();

    public void CreateListenSocket(int port) {
        listener = new EventBasedNetListener();
        socket = new NetManager(listener);
        socket.Start(port);

        listener.ConnectionRequestEvent += request => {
            if (request.Data.TryGetString(out string key)) {
                if (key.Equals(VERIFY_KEY)) {
                    if (request.Data.TryGetUInt(out uint id)) {
                        var peer = request.Accept();
                        clients.Add(id, peer);
                        peer.Tag = id;
                        return;
                    }
                }
            }
            request.Reject();
        };

        listener.PeerConnectedEvent += peer => ConnectionStatusChanged?.Invoke(new StatusInfo { ConnectionId = (uint) peer.Tag, Address = new IPEndPoint(peer.Address, peer.Port), ConnectionState = peer.ConnectionState });
        listener.PeerDisconnectedEvent += (peer, _) => ConnectionStatusChanged?.Invoke(new StatusInfo { ConnectionId = (uint) peer.Tag, Address = new IPEndPoint(peer.Address, peer.Port), ConnectionState = peer.ConnectionState });
        listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
    }

    public void Connect(string address, int port, uint id) {
        listener = new EventBasedNetListener();
        socket = new NetManager(listener);
        socket.Start();
        var writer = new NetDataWriter();
        writer.Put(VERIFY_KEY);
        writer.Put(id);
        peer = socket.Connect(address, port, writer);
        peer.Tag = id;
        listener.PeerConnectedEvent += peer => ConnectionStatusChanged?.Invoke(new StatusInfo { ConnectionId = (uint) peer.Tag, ConnectionState = peer.ConnectionState });
        listener.PeerDisconnectedEvent += (peer, _) => ConnectionStatusChanged?.Invoke(new StatusInfo { ConnectionId = (uint) peer.Tag, ConnectionState = peer.ConnectionState });
        listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
    }


    private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod) {
        GCHandle handle = GCHandle.Alloc(reader.RawData, GCHandleType.Pinned);
        IntPtr Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(reader.RawData, reader.UserDataOffset);
        int Size = reader.UserDataSize;
        MessageReceived?.Invoke(new NetworkingMessage { Pointer = Pointer, ConnectionId = (uint) peer.Tag, Size = (uint) Size });
        handle.Free();
        reader.Recycle();
    }

    public bool CloseConnection(uint connection) {
        if (socket == null || !this.clients.ContainsKey(connection))
            return false;
        var peer = this.clients[connection];
        this.clients.Remove(connection);
        socket.DisconnectPeer(peer);
        return true;
    }

    public Result SendMessageToServer(IntPtr point, uint size) {
        if (peer != null && peer.ConnectionState == ConnectionState.Connected) {
            SendMessage(peer, point, (int) size);
            return Result.OK;
        }
        return Result.Fail;
    }

    public Result SendMessageToConnection(uint connection, IntPtr point, uint size) {
        if (socket == null || !this.clients.ContainsKey(connection))
            return Result.Fail;
        var peer = this.clients[connection];
        if (peer.ConnectionState != ConnectionState.Connected)
            return Result.Fail;
        SendMessage(peer, point, (int) size);
        return Result.OK;
    }

    private unsafe void SendMessage(NetPeer peer, IntPtr point, int size) {
        byte[] data = new byte[size];
        Marshal.Copy(point, data, 0, size);
        peer.Send(data, 0, size, DeliveryMethod.ReliableOrdered);
    }

    public void RunCallbacks() {
        socket?.PollEvents();
    }

    public void Dispose() {
        if (listener != null) {
            listener.ClearConnectionRequestEvent();
            listener.ClearPeerConnectedEvent();
            listener.ClearPeerDisconnectedEvent();
            listener.ClearNetworkReceiveEvent();
        }
        socket?.Stop();
        clients.Clear();
    }
}
