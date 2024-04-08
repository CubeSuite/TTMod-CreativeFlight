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
using EquinoxsModUtils;
using UnityEngine.Windows;
using BepInEx;

namespace CreativeFlight.Patches
{
    internal class PlayerFirstPersonControllerPatch
    {
        // Objects & Variables
        private static bool ascend => UnityInput.Current.GetKey(KeyCode.Space);
        private static bool descend => UnityInput.Current.GetKey(KeyCode.C);
        private static float hThrust => CreativeFlightPlugin.HorizontalThrust.Value;
        private static float vThrust => CreativeFlightPlugin.VerticalThrust.Value;
        private static float friction => CreativeFlightPlugin.Friction.Value;

        [HarmonyPatch(typeof(PlayerFirstPersonController), "CalculateHorizontalMovement")]
        [HarmonyPrefix]
        private static bool RecalcHorizontalMovement(PlayerFirstPersonController __instance) {
            if (ShouldUseDefault(__instance)) return true;

            float speed = (float)ModUtils.GetPrivateField("m_HorizontalSpeed", Player.instance.fpcontroller);
            speed *= 1 - friction;

            if (UnityInput.Current.GetKey(KeyCode.W) || UnityInput.Current.GetKey(KeyCode.S) ||
                UnityInput.Current.GetKey(KeyCode.A) || UnityInput.Current.GetKey(KeyCode.D)) {
                speed += hThrust;
            }

            Vector3 forward = __instance.cam.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector2 moveAxes = InputHandler.instance.MoveAxes;
            Vector3 hMove = Quaternion.LookRotation(forward, Vector3.up) * new Vector3(moveAxes.x, 0, moveAxes.y) * speed;
            ModUtils.SetPrivateField("m_DesiredHorizontalVelocity", Player.instance.fpcontroller, hMove);
            return false;
        }

        [HarmonyPatch(typeof(PlayerFirstPersonController), "CalculateVerticalMovement")]
        [HarmonyPrefix]
        private static bool RecalcVerticalMovement(PlayerFirstPersonController __instance) {
            if (ShouldUseDefault(__instance)) return true;

            ModUtils.SetPrivateField("_active", Player.instance.equipment.hoverPack, false);

            float speed = (float)ModUtils.GetPrivateField("m_VerticalSpeed", Player.instance.fpcontroller);
            speed *=  1 - friction;

            if (ascend) speed += vThrust * Time.deltaTime;
            if (descend) speed -= vThrust * Time.deltaTime;
            
            ModUtils.SetPrivateField("m_VerticalSpeed", Player.instance.fpcontroller, speed);
            return false;
        }

        private static bool ShouldUseDefault(PlayerFirstPersonController __instance) {
            if (!CreativeFlightPlugin.isEnabled) return true;
            PlayerFirstPersonController.ControlState controlState = (PlayerFirstPersonController.ControlState)ModUtils.GetPrivateField("curControls", __instance);
            if (controlState == PlayerFirstPersonController.ControlState.RAIL_RUNNER) return true;
            if (!Jetpack.isFlying) return true;

            return false;
        }

        // Blocked Functions

        [HarmonyPatch(typeof(PlayerFirstPersonController), "AutoCrouch")]
        [HarmonyPrefix]
        private static bool BlockAutoCrouch() {
            if (ShouldUseDefault(Player.instance.fpcontroller)) return true;
            return false;
        }

        [HarmonyPatch(typeof(PlayerFirstPersonController), "UpdateHoverPackStatus")]
        [HarmonyPrefix]
        public static bool AdjustStiltsHeight(PlayerFirstPersonController __instance) {
            if (!CreativeFlightPlugin.isEnabled) return true;
            return false;
        }

        [HarmonyPatch(typeof(PlayerFirstPersonController), "DeactivateHoverPack")]
        [HarmonyPrefix]
        public static bool BlockDeactivateHoverpack() {
            if (!CreativeFlightPlugin.isEnabled) return true;
            return false;
        }
    }
}
