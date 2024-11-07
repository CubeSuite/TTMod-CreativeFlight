using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeFlight.Patches
{
    internal class StiltsPatch
    {
        [HarmonyPatch(typeof(Stilts), nameof(Stilts.ActivateStilts))]
        [HarmonyPrefix]
        static bool BlockActivateStilts() {
            Jetpack.StartFlight();
            return false;
        }
    }
}
