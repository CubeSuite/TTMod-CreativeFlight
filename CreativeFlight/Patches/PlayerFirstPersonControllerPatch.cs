using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Rewired;

namespace CreativeFlight.Patches
{
    internal class PlayerFirstPersonControllerPatch
    {
        public static DateTime lastDescendPress = DateTime.Now;

        [HarmonyPatch(typeof(PlayerFirstPersonController), "UpdateHoverPackStatus")]
        [HarmonyPrefix]
        public static void AdjustStiltsHeight(PlayerFirstPersonController __instance) {
            if (!CreativeFlightPlugin.isEnabled) return;
            Type playerType = typeof(PlayerFirstPersonController);
            PropertyInfo hoverPackActiveProperty = playerType.GetProperty("hoverPackActive", BindingFlags.NonPublic | BindingFlags.Instance);

            Stilts hoverPack = Player.instance.equipment.hoverPack;
            Type hoverPackType = hoverPack.GetType();
            FieldInfo stiltsHeightField = hoverPackType.GetField("_stiltHeight", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo stiltThresholdCloseField = hoverPackType.GetField("_stiltThresholdClose", BindingFlags.Instance | BindingFlags.NonPublic);
            stiltThresholdCloseField.SetValue(hoverPack, 0);

            if (hoverPackActiveProperty != null && (bool)hoverPackActiveProperty.GetValue(__instance, null)) {
                float speed = CreativeFlightPlugin.VerticalSpeed.Value;

                if (CreativeFlightPlugin.AscendShortcut.Value.IsPressed()) {
                    Vector3 position = __instance.transform.position;
                    position.y += speed;
                    __instance.transform.position = position;

                    if (stiltsHeightField != null) {
                        float newStiltsHeightValue = (float)stiltsHeightField.GetValue(hoverPack) + speed;
                        stiltsHeightField.SetValue(hoverPack, newStiltsHeightValue);
                    }
                    else {
                        Debug.Log("_stiltsHeight field not found");
                    }
                }
                else if (CreativeFlightPlugin.DescendShortcut.Value.IsPressed()) {
                    Vector3 position = __instance.transform.position;
                    position.y -= speed;
                    __instance.transform.position = position;

                    if (stiltsHeightField != null) {
                        float newStiltsHeightValue = (float)stiltsHeightField.GetValue(hoverPack) - speed;
                        if (newStiltsHeightValue < 3) newStiltsHeightValue = 3;
                        stiltsHeightField.SetValue(hoverPack, newStiltsHeightValue);
                    }
                    else {
                        Debug.Log("_stiltsHeight field not found");
                    }
                }
                
                if (CreativeFlightPlugin.DescendShortcut.Value.IsDown()) {
                    Debug.Log("Descend Pressed");
                    double timeBetweenPresses = (DateTime.Now - lastDescendPress).TotalMilliseconds;
                    Debug.Log($"Time between presses: {timeBetweenPresses}");
                    if (timeBetweenPresses < 200) {
                        hoverPack.DeactivateStilts();
                        stiltsHeightField.SetValue(hoverPack, 3);
                    }

                    lastDescendPress = DateTime.Now;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerFirstPersonController), "DeactivateHoverPack")]
        [HarmonyPrefix]
        public static bool BlockDeactivateHoverpack() {
            if (!CreativeFlightPlugin.isEnabled) return true;
            return false;
        }
    }
}
