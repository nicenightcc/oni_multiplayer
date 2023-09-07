﻿using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MultiplayerMod.Game;
using MultiplayerMod.ModRuntime;
using MultiplayerMod.Multiplayer;
using MultiplayerMod.Multiplayer.Configuration;
using MultiplayerMod.Multiplayer.Players;
using MultiplayerMod.Multiplayer.Players.Events;
using MultiplayerMod.Network;
using MultiplayerMod.Test.Environment;
using MultiplayerMod.Test.Environment.Network;
using MultiplayerMod.Test.Environment.Patches;
using MultiplayerMod.Test.Environment.Unity;
using NUnit.Framework;

namespace MultiplayerMod.Test.Multiplayer;

[TestFixture]
public class MultiplayerConnectionTests {

    [Test]
    public void HostSelfConnection() {
        UnityTestRuntime.Install();

        var runtime = MultiplayerTools.CreateTestRuntime(MultiplayerMode.Host, "Test player");
        runtime.Activate();

        var server = runtime.Dependencies.Get<TestMultiplayerServer>();
        var client = runtime.Dependencies.Get<TestMultiplayerClient>();
        server.EnablePendingActions = true;
        client.EnablePendingActions = true;

        typeof(GameEvents).RaiseEvent(nameof(GameEvents.GameStarted), runtime);

        Assert.AreEqual(expected: MultiplayerServerState.Stopped, actual: server.State);

        Assert.IsTrue(server.RunPendingAction());
        Assert.AreEqual(expected: MultiplayerServerState.Preparing, actual: server.State);

        Assert.IsTrue(server.RunPendingAction());
        Assert.AreEqual(expected: MultiplayerServerState.Starting, actual: server.State);

        Assert.IsFalse(client.RunPendingAction());
        Assert.AreEqual(expected: MultiplayerClientState.Disconnected, actual: client.State);

        Assert.IsTrue(server.RunPendingAction());
        Assert.AreEqual(expected: MultiplayerServerState.Started, actual: server.State);

        Assert.IsTrue(client.RunPendingAction());
        Assert.AreEqual(expected: MultiplayerClientState.Connecting, actual: client.State);

        Assert.AreEqual(expected: 0, actual: runtime.Multiplayer.Players.Count());

        Assert.IsTrue(client.RunPendingAction());
        Assert.AreEqual(expected: MultiplayerClientState.Connected, actual: client.State);

        Assert.IsTrue(client.RunPendingAction());
        Assert.IsFalse(client.RunPendingAction());

        Assert.AreEqual(expected: 1, actual: runtime.Multiplayer.Players.Count());
        Assert.AreEqual(
            expected: PlayerRole.Host,
            actual: runtime.Multiplayer.Players.Current.Role
        );

        UnityTestRuntime.Uninstall();
    }

    [Test]
    public void EstablishingTwoPlayersConnection() {
        var harmony = SetupEnvironment();

        var hostRuntime = MultiplayerTools.CreateTestRuntime(MultiplayerMode.Host, "Host player");
        var clientRuntime = MultiplayerTools.CreateTestRuntime(MultiplayerMode.Client, "Client player");

        // Event: host starts a game
        hostRuntime.StartGame();

        // Host player list must have a single ready player
        Assert.AreEqual(expected: 1, actual: hostRuntime.Multiplayer.Players.Count());
        var hostPlayer = hostRuntime.Multiplayer.Players.Current;
        Assert.AreEqual(expected: PlayerRole.Host, actual: hostPlayer.Role);
        Assert.AreEqual(expected: PlayerState.Ready, actual: hostPlayer.State);

        // A client connects to the host
        clientRuntime.ConnectTo(hostRuntime);

        // Lists must be equal and contain two players: the host player (ready) and a client player (loading)
        AssertPlayersAreEqual(hostRuntime, clientRuntime);
        Assert.AreEqual(expected: 2, actual: hostRuntime.Multiplayer.Players.Count());

        var clientPlayer = clientRuntime.Multiplayer.Players.Current;
        Assert.AreEqual(expected: PlayerRole.Client, actual: clientPlayer.Role);
        Assert.AreEqual(expected: PlayerState.Loading, actual: clientPlayer.State);
        Assert.IsFalse(hostRuntime.Multiplayer.Players.Ready);

        // Event: the client finished loading and the game is started
        clientRuntime.StartGame();

        // Lists must be equal and the client player must be ready
        AssertPlayersAreEqual(hostRuntime, clientRuntime);
        Assert.AreEqual(expected: PlayerState.Ready, actual: clientPlayer.State);
        Assert.IsTrue(hostRuntime.Multiplayer.Players.Ready);

        DisposeEnvironment(harmony);
    }

    [Test]
    public void PlayerConnectedAndGracefullyLeft() => PlayerConnectedAndDisconnected(true);

    [Test]
    public void PlayerConnectedAndManuallyDisconnected() => PlayerConnectedAndDisconnected(false);

    private static void PlayerConnectedAndDisconnected(bool gracefully) {
        var harmony = SetupEnvironment();
        PlayerLeftEvent? playerLeftEvent = null;

        var hostRuntime = MultiplayerTools.CreateTestRuntime(MultiplayerMode.Host, "Host player");
        var clientRuntime = MultiplayerTools.CreateTestRuntime(MultiplayerMode.Client, "Client player");
        hostRuntime.EventDispatcher.Subscribe<PlayerLeftEvent>(it => playerLeftEvent = it);

        hostRuntime.StartGame();
        clientRuntime.ConnectTo(hostRuntime);
        clientRuntime.StartGame();
        clientRuntime.Activate();

        if (gracefully)
            clientRuntime.Dependencies.Get<PlayerConnectionManager>().LeaveGame();
        else
            clientRuntime.Dependencies.Get<IMultiplayerClient>().Disconnect();

        Assert.AreEqual(expected: 0, clientRuntime.Multiplayer.Players.Count());
        Assert.AreEqual(expected: 1, hostRuntime.Multiplayer.Players.Count());
        Assert.AreEqual(expected: PlayerRole.Host, hostRuntime.Multiplayer.Players.First().Role);

        Assert.NotNull(playerLeftEvent);

        if (gracefully)
            Assert.IsTrue(playerLeftEvent?.Gracefully);
        else
            Assert.IsFalse(playerLeftEvent?.Gracefully);

        DisposeEnvironment(harmony);
    }

    private static void AssertPlayersAreEqual(Runtime runtimeA, Runtime runtimeB) {
        Assert.AreEqual(runtimeA.Multiplayer.Players.Count(), runtimeB.Multiplayer.Players.Count());
        var collection = runtimeA.Multiplayer.Players.Zip(runtimeB.Multiplayer.Players, (a, b) => (a, b));
        foreach (var (playerA, playerB) in collection) {
            Assert.AreEqual(playerA.Id, playerB.Id);
            Assert.AreEqual(playerA.Role, playerB.Role);
            Assert.AreEqual(playerA.State, playerB.State);
            Assert.AreEqual(playerA.Profile.PlayerName, playerB.Profile.PlayerName);
        }
    }

    private static Harmony SetupEnvironment() {
        UnityTestRuntime.Install();
        var harmony = new Harmony("MultiplayerConnectionTests");
        PatchesSetup.Install(harmony, new List<Type> { typeof(WorldManagerPatch) });
        return harmony;
    }

    private static void DisposeEnvironment(Harmony harmony) {
        PatchesSetup.Uninstall(harmony);
        UnityTestRuntime.Uninstall();
    }

}