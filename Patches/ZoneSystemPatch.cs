using HarmonyLib;

namespace ExtraSnapPointsMadeEasy.Patches
{
    [HarmonyPatch(typeof(ZoneSystem))]
    internal class ZoneSystemPatch
    {
        [HarmonyPostfix]
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
