using HarmonyLib;

namespace ExtraSnapPointsMadeEasy.Patches
{
    [HarmonyPatch(typeof(ZoneSystem))]
    internal class ZoneSystemPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ZoneSystem.Start))]
        public static void Start()
        {
            if (!ZNetScene.instance)
            {
                return;
            }
            ExtraSnapPoints.AddExtraSnapPoints();
        }
    }
}
