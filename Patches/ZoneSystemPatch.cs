using ExtraSnapPointsMadeEasy.Helpers;
using HarmonyLib;

namespace ExtraSnapPointsMadeEasy.Patches
{
    [HarmonyPatch(typeof(ZoneSystem))]
    internal class ZoneSystemPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Low)]
        [HarmonyPatch(nameof(ZoneSystem.Start))]
        public static void Start()
        {
            SnapPointAdder.AddExtraSnapPoints("Adding extra snap points", true);
        }
    }
}