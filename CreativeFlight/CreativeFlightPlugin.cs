using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CreativeFlight.Patches;
using EquinoxsDebuggingTools;
using EquinoxsModUtils;
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
        public static ConfigEntry<bool> LandToEndFlight;

        // Forces
        public static ConfigEntry<float> HorizontalThrust;
        public static ConfigEntry<float> VerticalThrust;
        public static ConfigEntry<float> Friction;
        public static ConfigEntry<int> SprintSpeed;

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
            if (!ModUtils.hasGameLoaded) return;
            
            sSinceLastAscendPress += Time.deltaTime;
            sSinceLastDescendPress += Time.deltaTime;

            StartStopIfDoubleTap();

            if (LandToEndFlight.Value && Jetpack.isFlying && CheckIfNearGround()) {
                Jetpack.StopFlight();
            }
        }

        // Private Functions

        private void CreateConfigEntries() {
            DoubleTapThreshold = Config.Bind("General", "Double Tap Threshold", 300, new ConfigDescription("The time interval in milliseconds during which two key presses are registered as a double tap", new AcceptableValueRange<int>(0, 500)));
            DescendKey = Config.Bind("General", "Descend Key", KeyCode.C, new ConfigDescription("The key to press to descend or double tap to end flight. Left Control is not recommended if you are using Blueprints."));
            LandToEndFlight = Config.Bind("General", "Land To End Flight", false, new ConfigDescription("When enabled, touching the ground will end flight."));

            HorizontalThrust = Config.Bind("Forces", "Horizontal Thrust", 10f, new ConfigDescription("Controls horizontal acceleration while flying", new AcceptableValueRange<float>(0f, float.MaxValue)));
            VerticalThrust = Config.Bind("Forces", "Vertical Thrust", 150f, new ConfigDescription("Controls vertical acceleration while flying", new AcceptableValueRange<float>(0f, float.MaxValue)));
            Friction = Config.Bind("Forces", "Friction", 0.15f, new ConfigDescription("Controls how quickly you slow to a stop while flying", new AcceptableValueRange<float>(0.01f, 0.9f)));
            SprintSpeed = Config.Bind("Forces", "Sprint Speed", 150, new ConfigDescription("Sprint speed as a percentage of walking speed. E.g. default '150' means 50% faster than walking.", new AcceptableValueRange<int>(100, 200)));
        }

        private void ApplyPatches() {
            Harmony.CreateAndPatchAll(typeof(PlayerFirstPersonControllerPatch));
        }

        private void StartStopIfDoubleTap() {
            if (UnityInput.Current.GetKeyDown(KeyCode.Space)) {
                if (sSinceLastAscendPress < doubleTapThresholdSeconds) {
                    if (!Jetpack.isFlying) Jetpack.StartFlight();
                    else Jetpack.StopFlight();
                }
                else if (sSinceLastAscendPress < 1f) {
                    EDT.Log("Controls", $"Double space tap did not meet threshold | {sSinceLastAscendPress} > {doubleTapThresholdSeconds}");
                }

                sSinceLastAscendPress = 0;
            }
            else if (UnityInput.Current.GetKeyDown(DescendKey.Value) && !UnityInput.Current.GetKeyDown(KeyCode.LeftControl)) {
                if (sSinceLastDescendPress < doubleTapThresholdSeconds && Jetpack.isFlying) {
                    Jetpack.StopFlight();
                }
                else if (sSinceLastDescendPress < 1f && sSinceLastDescendPress > doubleTapThresholdSeconds) {
                    EDT.Log("Controls", $"Double {DescendKey.Value} tap did not meet threshold | {sSinceLastDescendPress} > {doubleTapThresholdSeconds}");
                }

                sSinceLastDescendPress = 0;
            }
        }

        private bool CheckIfNearGround() {
            PlayerFirstPersonController fpController = Player.instance.fpcontroller;
            LayerMask groundLayer = (LayerMask)ModUtils.GetPrivateField("groundLayer", fpController);
            
            if (Physics.Raycast(fpController.transform.position, Vector3.down, out RaycastHit raycastHit, 2.2f, groundLayer)) {
                float distance = fpController.transform.position.y - raycastHit.point.y;
                EDT.PacedLog("Landing", $"Raycast hit, user is {distance} away from ground");
                return distance < 0.2f;
            }
            else {
                EDT.PacedLog("Landing", "Raycast did not hit");
                return false;
            }
        }
    }
}
