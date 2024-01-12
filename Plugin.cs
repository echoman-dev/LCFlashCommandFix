using BepInEx;
using BepInEx.Logging;
using LCFlashCommandFix.Patches;
using HarmonyLib;

namespace LCFlashCommandFix
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInProcess("Lethal Company.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "io.github.echoman-dev.LCFlashCommandFix";
        private const string modName = "Lethal Company Flash Command Fix";
        private const string modVersion = "0.0.1";

        private readonly Harmony harmony = new Harmony(modGUID);
        internal static ManualLogSource log;

        void Awake()
        {
            log = Logger;

            log.LogInfo(modName + " loaded.");

            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(TerminalPatch));
        }
    }
}
