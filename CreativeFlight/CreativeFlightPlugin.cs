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
        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        // General
        public static ConfigEntry<int> DoubleTapThreshold;

        // Forces
        public static ConfigEntry<float> HorizontalThrust;
        public static ConfigEntry<float> VerticalThrust;
        public static ConfigEntry<float> Friction;

        // Objects & Variables
        public static bool shouldDebug = true;
        public static bool isEnabled = true;
        public static float sSinceLastAscendPress;
        public static float sSinceLastDescendPress;
        private static float doubleTapThresholdSeconds => 0.001f * DoubleTapThreshold.Value;

        private void Awake() {
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();

            DoubleTapThreshold = Config.Bind("General", "Double Tap Threshold", 300, new ConfigDescription("The time interval in milliseconds during which two key presses are registered as a double tap", new AcceptableValueRange<int>(0, 500)));
            HorizontalThrust = Config.Bind("Forces", "Horizontal Thrust", 10f, new ConfigDescription("Controls horizontal acceleration while flying", new AcceptableValueRange<float>(0f, float.MaxValue)));
            VerticalThrust = Config.Bind("Forces", "Vertical Thrust", 150f, new ConfigDescription("Controls vertical acceleration while flying", new AcceptableValueRange<float>(0f, float.MaxValue)));
            Friction = Config.Bind("Forces", "Friction", 0.15f, new ConfigDescription("Controls how quickly you slow to a stop while flying", new AcceptableValueRange<float>(0.01f, 0.9f)));

            Harmony.CreateAndPatchAll(typeof(PlayerFirstPersonControllerPatch));

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

                sSinceLastAscendPress = 0;
            }
            else if (UnityInput.Current.GetKeyDown(KeyCode.C)) {
                if(sSinceLastDescendPress < doubleTapThresholdSeconds && Jetpack.isFlying) {
                    Jetpack.StopFlight();
                }

                sSinceLastDescendPress = 0;
            }
        }
    }
}
