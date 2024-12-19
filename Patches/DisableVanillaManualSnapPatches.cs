using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;


namespace ExtraSnapsMadeEasy.Patches;

[HarmonyPatch]
internal static class DisableVanillaManualSnapPatches
{

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
    internal static void UpdatePlacementGhost_Postfix(Player __instance)
    {
        ResetManualSnapPointToAuto();
    }

    /// <summary>
    ///     Set Vanilla snapping mode to "Auto" if Vanilla-Manual-Snap is not enabled.
    /// </summary>
    internal static void ResetManualSnapPointToAuto()
    {
        if (ExtraSnapsPlugin.Instance.VanillaManualSnapEnabled.Value || !Player.m_localPlayer)
        {
            return;
        }
        Player.m_localPlayer.m_manualSnapPoint = -1;
    }

    /// <summary>
    ///     Reset manual snap after increment/decrement is applied
    /// </summary>
    /// <param name="instructions"></param>
    /// <returns></returns>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
    internal static IEnumerable<CodeInstruction> UpdatePlacementGhost_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Targeting code
        // m_manualSnapPoint++;
        //IL_05e5: ldarg.0
        //IL_05e6: ldarg.0
        //IL_05e7: ldfld int32 Player::m_manualSnapPoint
        //IL_05ec: ldc.i4.1
        //IL_05ed: add
        //IL_05ee: stfld int32 Player::m_manualSnapPoint

        // m_tempSnapPoints1.Clear();
        //IL_05f3: ldarg.0
        //IL_05f4: ldfld class [mscorlib] System.Collections.Generic.List`1<class [UnityEngine.CoreModule] UnityEngine.Transform> Player::m_tempSnapPoints1
        //IL_05f9: callvirt instance void class [mscorlib] System.Collections.Generic.List`1<class [UnityEngine.CoreModule] UnityEngine.Transform>::Clear()
        var codeMatches = new CodeMatch[]
        {
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Player), nameof(Player.m_tempSnapPoints1))),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(List<Transform>), nameof(List<Transform>.Clear)))
        };
        return new CodeMatcher(instructions)
            .MatchStartForward(codeMatches)
            .InsertAndAdvance(Transpilers.EmitDelegate(ResetManualSnapPointToAuto))
            .InstructionEnumeration();
    }
}
