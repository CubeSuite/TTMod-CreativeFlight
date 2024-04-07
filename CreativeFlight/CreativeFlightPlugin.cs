using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CreativeFlight.Patches;
using HarmonyLib;
using Rewired;
using UnityEngine;

namespace CreativeFlight
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class CreativeFlightPlugin : BaseUnityPlugin
    {
        private const string MyGUID = "com.equinox.CreativeFlight";
        private const string PluginName = "CreativeFlight";
        private const string VersionString = "0.2.0";

        public static string VerticalSpeedKey = "Vertical Speed";
        public static ConfigEntry<float> VerticalSpeed;

        public static string AscendShortcutKey = "Ascend Shortcut";
        public static ConfigEntry<KeyboardShortcut> AscendShortcut;

        public static string DescendShortcutKey = "Descend Shortcut";
        public static ConfigEntry<KeyboardShortcut> DescendShortcut;

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        public static bool isEnabled = true;

        private void Awake() {
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();

            VerticalSpeed = Config.Bind("General", VerticalSpeedKey, 0.2f, new ConfigDescription("Speed of ascending and descending", new AcceptableValueRange<float>(-0f, 1.0f)));
            AscendShortcut = Config.Bind("General", AscendShortcutKey, new KeyboardShortcut(KeyCode.Space), new ConfigDescription("The key to press to ascend"));
            DescendShortcut = Config.Bind("General", DescendShortcutKey, new KeyboardShortcut(KeyCode.C), new ConfigDescription("The key to press to descend"));

            Harmony.CreateAndPatchAll(typeof(PlayerFirstPersonControllerPatch));

            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
            Log = Logger;
        }
    }
}
