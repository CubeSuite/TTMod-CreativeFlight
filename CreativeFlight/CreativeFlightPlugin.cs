using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CreativeFlight.Patches;
using EquinoxsDebuggingTools;
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
        private const string VersionString = "1.0.1";
        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        // General
        public static ConfigEntry<int> DoubleTapThreshold;
        public static ConfigEntry<KeyCode> DescendKey;

        // Forces
        public static ConfigEntry<float> HorizontalThrust;
        public static ConfigEntry<float> VerticalThrust;
        public static ConfigEntry<float> Friction;

        // Objects & Variables
        public static bool isEnabled = true;
        public static float sSinceLastAscendPress;
        public static float sSinceLastDescendPress;
        private static float doubleTapThresholdSeconds => 0.001f * DoubleTapThreshold.Value;

        private void Awake() {
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();

            CreateConfigEntries();
            ApplyPatches();

            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
            Log = Logger;
        }

        private void Update() {
            sSinceLastAscendPress += Time.deltaTime;
            sSinceLastDescendPress += Time.deltaTime;
            
            if (UnityInput.Current.GetKeyDown(KeyCode.Space)) {
                if(sSinceLastAscendPress < doubleTapThresholdSeconds && !Jetpack.isFlying) {
                    Jetpack.StartFlight();
                }
                else if (sSinceLastAscendPress < 1f && sSinceLastAscendPress > doubleTapThresholdSeconds) {
                    EDT.Log("Controls", $"Double space tap did not meet threshold | {sSinceLastAscendPress} > {doubleTapThresholdSeconds}");
                }

                sSinceLastAscendPress = 0;
            }
            else if (UnityInput.Current.GetKeyDown(DescendKey.Value) && !UnityInput.Current.GetKeyDown(KeyCode.LeftControl)) {
                if(sSinceLastDescendPress < doubleTapThresholdSeconds && Jetpack.isFlying) {
                    Jetpack.StopFlight();
                }
                else if (sSinceLastDescendPress < 1f && sSinceLastDescendPress > doubleTapThresholdSeconds) {
                    EDT.Log("Controls", $"Double {DescendKey.Value} tap did not meet threshold | {sSinceLastDescendPress} > {doubleTapThresholdSeconds}");
                }

                sSinceLastDescendPress = 0;
            }
        }

        // Private Functions

        private void CreateConfigEntries() {
            DoubleTapThreshold = Config.Bind("General", "Double Tap Threshold", 300, new ConfigDescription("The time interval in milliseconds during which two key presses are registered as a double tap", new AcceptableValueRange<int>(0, 500)));
            DescendKey = Config.Bind("General", "Descend Key", KeyCode.C, new ConfigDescription("The key to press to descend or double tap to end flight. Left Control is not recommended if you are using Blueprints."));
            
            HorizontalThrust = Config.Bind("Forces", "Horizontal Thrust", 10f, new ConfigDescription("Controls horizontal acceleration while flying", new AcceptableValueRange<float>(0f, float.MaxValue)));
            VerticalThrust = Config.Bind("Forces", "Vertical Thrust", 150f, new ConfigDescription("Controls vertical acceleration while flying", new AcceptableValueRange<float>(0f, float.MaxValue)));
            Friction = Config.Bind("Forces", "Friction", 0.15f, new ConfigDescription("Controls how quickly you slow to a stop while flying", new AcceptableValueRange<float>(0.01f, 0.9f)));
        }

        private void ApplyPatches() {
            Harmony.CreateAndPatchAll(typeof(PlayerFirstPersonControllerPatch));
        }
    }
}
