using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MultiplayerMod.Core.Dependency;
using MultiplayerMod.Core.Logging;
using MultiplayerMod.ModRuntime.Loader;
using MultiplayerMod.Platform.Direct;

namespace MultiplayerMod.Core.I18n;

[UsedImplicitly]
public class LocalizationConfigurer : IModComponentConfigurer {

    private readonly Logging.Logger log = LoggerFactory.GetLogger<DirectMultiplayerOperations>();
    private const string TRANSLATIONS_EXT = ".po";
    private const string TRANSLATIONS_RES_PATH = "MultiplayerMod.translations.";

    public void Configure(DependencyContainerBuilder builder) {
        Localization.RegisterForTranslation(typeof(Core.Strings));

        Localization.Locale locale = Localization.GetLocale();
        string? locCode = locale?.Code;
        if (string.IsNullOrEmpty(locCode))
            locCode = Localization.GetCurrentLanguageCode();

        if (!string.IsNullOrEmpty(locCode)) {
            LoadTranslation(locCode!);
        }
    }

    private void LoadTranslation(string locCode) {
        log.Info($"Try to load {locCode} localization");
        try {
            var path = TRANSLATIONS_RES_PATH + locCode + TRANSLATIONS_EXT;
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream? stream = assembly.GetManifestResourceStream(path);
            if (stream != null) {
                List<string> lines = new List<string>();
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                    while (!reader.EndOfStream)
                        lines.Add(reader.ReadLine());

                Localization.LoadTranslation(lines.ToArray());
                return;
            }

            path = Path.Combine(Path.GetDirectoryName(assembly.Location), locCode + TRANSLATIONS_EXT);
            if (File.Exists(path)) {
                Localization.LoadTranslation(File.ReadAllLines(path));
            }
        } catch (Exception e) {
            log.Warning($"Failed to load {locCode} localization");
        }
    }
}

