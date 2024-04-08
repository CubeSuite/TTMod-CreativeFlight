using EquinoxsModUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CreativeFlight
{
    public static class Jetpack
    {
        // Objects & Variables
        public static bool shouldDebug => CreativeFlightPlugin.shouldDebug;
        public static bool isFlying = false;

        public static void StartFlight() {
            isFlying = true;
            ModUtils.SetPrivateField("m_VerticalSpeed", Player.instance.fpcontroller, 0f);
            ModUtils.SetPrivateField("_stiltHeight", Player.instance.equipment.hoverPack, 0f);
            Player.instance.fpcontroller.m_Rigidbody.velocity = Vector3.zero;
            Player.instance.fpcontroller.maxWalkSpeed = 10f;
            Player.instance.fpcontroller.maxRunSpeed = 16f;
            if (shouldDebug) Debug.Log("Started Flying");
        }

        public static void StopFlight() {
            isFlying = false;
            Player.instance.fpcontroller.maxWalkSpeed = 5f;
            Player.instance.fpcontroller.maxRunSpeed = 8f;
            if (shouldDebug) Debug.Log("Stopped Flying");
        }
    }
}
