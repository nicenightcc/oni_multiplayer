﻿using System;
using MultiplayerMod.Game.UI.Screens;
using MultiplayerMod.ModRuntime;
using MultiplayerMod.Multiplayer.Objects.Reference;

namespace MultiplayerMod.Multiplayer.Commands.Gameplay;

[Serializable]
public class RejectDelivery : MultiplayerCommand {

    private ComponentReference<Telepad> reference;

    public RejectDelivery(ComponentReference<Telepad> reference) {
        this.reference = reference;
    }

    public override void Execute(Runtime runtime) {
        reference.GetComponent().RejectAll();
        ImmigrantScreenPatch.Deliverables = null;
    }

}