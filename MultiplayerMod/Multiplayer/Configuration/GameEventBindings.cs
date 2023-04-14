﻿using System;
using MultiplayerMod.Core.Dependency;
using MultiplayerMod.Core.Logging;
using MultiplayerMod.Game.Events;
using MultiplayerMod.Game.Events.Tools;
using MultiplayerMod.Multiplayer.Commands.GameTools;
using MultiplayerMod.Multiplayer.Commands.Speed;
using MultiplayerMod.Multiplayer.Commands.State;
using MultiplayerMod.Multiplayer.Commands.Tools;
using MultiplayerMod.Multiplayer.Patches;
using MultiplayerMod.Multiplayer.Tools;
using MultiplayerMod.Network;

namespace MultiplayerMod.Multiplayer.Configuration;

public class GameEventBindings {

    private readonly Core.Logging.Logger log = LoggerFactory.GetLogger<GameEventBindings>();

    private readonly IMultiplayerClient client = Container.Get<IMultiplayerClient>();

    private readonly CommandRateThrottle throttle10Hz = new(rate: 10);

    private bool bound;

    public void Bind() {
        if (bound)
            return;

        log.Debug("Binding game events");

        GameSpeedControlEvents.GamePaused += () => client.Send(new PauseGame());
        GameSpeedControlEvents.GameResumed += () => client.Send(new ResumeGame());
        GameSpeedControlEvents.SpeedChanged += speed => client.Send(new ChangeGameSpeed(speed));

        // TODO: Cursor update may be ignored if MouseMoved isn't triggered after the rate period.
        // TODO: Will be changed later (probably with current / last sent positions check).
        InterfaceToolEvents.MouseMoved += position => throttle10Hz.Run<UpdateCursorPosition>(
            () => client.Send(new UpdateCursorPosition(client.Player, position))
        );

        BindTools();

        DragToolEvents.DragComplete += (sender, args) => {
            var x = args.Parameters != null ? string.Join(", ", args.Parameters) : "n/a";
            log.Debug(
                $"Drag complete: {sender.GetType().Name}, " +
                $"cursor up: {args.CursorUp}, " +
                $"cursor down: {args.CursorDown}, " +
                $"parameters: [{x}], " +
                $"priority: {args.Priority.priority_class}:{args.Priority.priority_value}, " +
                $"cells: [{string.Join(", ", args.Cells)}]."
            );

            // @formatter:off
            switch (sender) {
                case DigTool: client.Send(new Dig(args)); break;
                case CancelTool: client.Send(new Cancel(args)); break;
                case DeconstructTool: client.Send(new Deconstruct(args)); break;
                case PrioritizeTool: client.Send(new Prioritize(args)); break;
                case DisinfectTool: client.Send(new Disinfect(args)); break;
                case ClearTool: client.Send(new Sweep(args)); break;
                case AttackTool: client.Send(new Commands.Tools.Attack(args)); break;
                case MopTool: client.Send(new Mop(args)); break;
                case CaptureTool: client.Send(new Wrangle(args)); break;
                case HarvestTool: client.Send(new Harvest(args)); break;
                case EmptyPipeTool: client.Send(new EmptyPipe(args)); break;
                case DisconnectTool: client.Send(new Disconnect(args)); break;
            }
            // @formatter:on
        };

        bound = true;
    }

    [Obsolete("For payload-based compatibility")]
    private void BindTools() {
        DragToolPatches.BaseUtilityBuildToolPatch.OnUtilityTool +=
            p => client.Send(new UseTool(GameToolType.UtilityBuild, p));
        DragToolPatches.BaseUtilityBuildToolPatch.OnWireTool +=
            p => client.Send(new UseTool(GameToolType.WireBuild, p));
        DragToolPatches.BuildToolPatch.OnDragTool += p => client.Send(new UseTool(GameToolType.Build, p));
        DragToolPatches.CopySettingsToolPatch.OnDragTool += p => client.Send(new UseTool(GameToolType.CopySettings, p));
        DragToolPatches.DebugToolPatch.OnDragTool += p => client.Send(new UseTool(GameToolType.Debug, p));
        DragToolPatches.PlaceToolPatch.OnDragTool += p => client.Send(new UseTool(GameToolType.Place, p));
    }

}