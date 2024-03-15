using System.Collections.Generic;
using System.Reflection;
using Epic.OnlineServices;
using HarmonyLib;
using KMod;
using MultiplayerMod.Core.Logging;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace MultiplayerMod.ModRuntime.Loader;

// ReSharper disable once UnusedType.Global
public class ModLoader : UserMod2 {

    private readonly Core.Logging.Logger log = LoggerFactory.GetLogger<ModLoader>();

    public override void OnLoad(Harmony harmony) {
        PUtil.InitLibrary(true);
        new POptions().RegisterOptions(this, typeof(Config));
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        log.Info($"Multiplayer mod version: {version}");
        harmony.CreateClassProcessor(typeof(LaunchInitializerPatch)).Patch();
    }

    public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods) {
        LaunchInitializerPatch.Loader = new DelayedModLoader(harmony, assembly, mods);
        log.Info("Delayed loader initialized");
    }

}
