using ExtraSnapsMadeEasy.ExtraSnapPoints;
using HarmonyLib;

namespace ExtraSnapsMadeEasy.Patches;

[HarmonyPatch(typeof(ZoneSystem))]
internal class ZoneSystemPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(nameof(ZoneSystem.Start))]
    public static void Start()
    {
        ExtraSnapsAdder.AddExtraSnapPoints("Adding extra snap points", true);
    }
}
