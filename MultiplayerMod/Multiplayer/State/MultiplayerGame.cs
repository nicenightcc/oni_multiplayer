﻿using MultiplayerMod.Multiplayer.Objects;

namespace MultiplayerMod.Multiplayer.State;

public static class MultiplayerGame {

    public static MultiplayerRole Role { get; set; }
    public static bool WorldSpawned { get; set; }
    public static MultiplayerState State { get; set; } = new();
    public static MultiplayerObjects Objects { get; set; } = new();

    public static void Reset() {
        Role = MultiplayerRole.None;
        WorldSpawned = false;
        State = new MultiplayerState();
        Objects = new MultiplayerObjects();
    }

}