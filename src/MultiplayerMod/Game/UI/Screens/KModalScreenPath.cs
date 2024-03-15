using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MultiplayerMod.Core.Extensions;
using MultiplayerMod.ModRuntime.Context;

namespace MultiplayerMod.Game.UI.Screens;

[HarmonyPatch]
public static class KModalScreenPath {

    private static readonly Type[] screenClasses = {
            typeof(ResearchScreen),
            typeof(SkillsScreen),
            typeof(ImmigrantScreen)
        };

    private static IEnumerable<MethodBase> TargetMethods()
        => screenClasses.Select(type => AccessTools.Method(type, "OnShow", new Type[] { typeof(bool) })).NotNull();

    [HarmonyPrefix]
    [RequireExecutionLevel(ExecutionLevel.Game)]
    public static void CancelOnShowPause(bool show, ref bool ___pause) {
        ___pause = false;
    }
}
