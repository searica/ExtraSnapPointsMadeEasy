using HarmonyLib;

namespace ExtraSnapPointsMadeEasy.Patches
{
    // hook right when the game first starts
    // to avoid touching pieces added by mods
    [HarmonyPatch(typeof(ObjectDB))]
    internal class ObjectDBPatch
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Low)]
        [HarmonyPatch(nameof(ObjectDB.Awake))]
        public static void Awake()
        {
            ExtraSnapPoints.AddExtraSnapPoints();
        }
    }
}
