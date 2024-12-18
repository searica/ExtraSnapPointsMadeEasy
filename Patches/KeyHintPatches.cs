using System;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ExtraSnapPointsMadeEasy.Patches;

[HarmonyPatch]
internal static class KeyHintPatches
{

    internal const string BuildHintsSnapPath = "BuildHints/Keyboard/Snap";
    internal static readonly Dictionary<string, string> CycleSnapHintsText = new()
    {
        {"Text", "Next snap point<br>(Source/Target)"}
    };
    internal static readonly Dictionary<string, ConfigEntry<KeyCode>> CycleSnapHintsKeys = new()
    {
        {"key_bkg/Key", ExtraSnapPointsMadeEasy.IterateSourceSnapPoints},
        {"key_bkg (1)/Key", ExtraSnapPointsMadeEasy.IterateTargetSnapPoints }
    };
    internal static GameObject CycleSnapsUI;


    internal const string AltPlaceHintPath = "BuildHints/Keyboard/AltPlace";
    internal static readonly Dictionary<string, string> ToggleManualSnapHintText = new()
    {
        {"Text", "Toggle manual<br>snapping" },
    };
    internal static readonly Dictionary<string, ConfigEntry<KeyCode>> ToggleManualSnapHintKeys = new()
    {
        {"key_bkg/Key", ExtraSnapPointsMadeEasy.EnableManualSnap }
    };
    internal static GameObject ToggleManualSnapUI;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(KeyHints), nameof(KeyHints.Awake))]
    internal static void KeyHints_Awake_Postfix(KeyHints __instance)
    {
        if (!__instance)
        {
            return;
        }
        try
        {
            SetUpCycleSnapsUI(__instance);
        }
        catch (Exception e)
        {
            Log.LogWarning($"Failed to set up Cycle Snap Points Key Hints: {e}");
        }
        try
        {
            SetUpToggleManualSnapUI(__instance);
        }
        catch (Exception e)
        {
            Log.LogWarning($"Failed to set up Toggle Manual Snapping Key Hints: {e}");
        }
    }

    /// <summary>
    ///     Crete gameObject for key hints UI if needed and hook events
    /// </summary>
    /// <param name="keyHints"></param>
    internal static void SetUpCycleSnapsUI(KeyHints keyHints)
    {
        // Set up snap point cycling hint
        if (CycleSnapsUI == null)
        {
            CycleSnapsUI = CreateCycleSnapsUI(keyHints);
        }
        UpdateKeyHintUI(CycleSnapsUI, CycleSnapHintsText, CycleSnapHintsKeys);
        ExtraSnapPointsMadeEasy.IterateSourceSnapPoints.SettingChanged += (obj, attr) =>
        {
            UpdateKeyHintUI(CycleSnapsUI, CycleSnapHintsText, CycleSnapHintsKeys);
        };
        ExtraSnapPointsMadeEasy.IterateTargetSnapPoints.SettingChanged += (obj, attr) =>
        {
            UpdateKeyHintUI(CycleSnapsUI, CycleSnapHintsText, CycleSnapHintsKeys);
        };
    }

    /// <summary>
    ///     Crete gameObject for key hints UI if needed and hook events
    /// </summary>
    /// <param name="keyHints"></param>
    internal static void SetUpToggleManualSnapUI(KeyHints keyHints)
    {
        if (ToggleManualSnapUI == null)
        {
            ToggleManualSnapUI = CreateToggleManualSnapHintUI(keyHints);
        }
        UpdateKeyHintUI(ToggleManualSnapUI, ToggleManualSnapHintText, ToggleManualSnapHintKeys);
        ExtraSnapPointsMadeEasy.IterateSourceSnapPoints.SettingChanged += (obj, attr) =>
        {
            UpdateKeyHintUI(ToggleManualSnapUI, ToggleManualSnapHintText, ToggleManualSnapHintKeys);
        };
    }

    internal static GameObject CreateCycleSnapsUI(KeyHints keyHints)
    {
        Transform snapHint = keyHints.transform.Find(BuildHintsSnapPath);
        if (snapHint == null)
        {
            Log.LogWarning($"{BuildHintsSnapPath} is no longer valid, could not find Snap key hint");
            return null;
        }
        return snapHint.gameObject;
    }

    internal static GameObject CreateToggleManualSnapHintUI(KeyHints keyHints)
    {
        Transform vanillaToggleHint = keyHints.transform.Find(AltPlaceHintPath);
        if (vanillaToggleHint == null)
        {
            Log.LogWarning($"{AltPlaceHintPath} is no longer valid, could not find Alt Place key hint");
            return null;
        }

        ToggleManualSnapUI = GameObject.Instantiate(vanillaToggleHint.gameObject, vanillaToggleHint.transform.parent);
        ToggleManualSnapUI.name = "Toggle Manual Snap";

        return vanillaToggleHint.gameObject;
    }

    internal static void UpdateKeyHintUI(GameObject keyHintUI, Dictionary<string, string> textHints, Dictionary<string, ConfigEntry<KeyCode>> keyCodeHints)
    {
        if (!keyHintUI)
        {
            return;
        }

        foreach (KeyValuePair<string, string> pair in textHints)
        {
            Transform child = keyHintUI.transform.Find(pair.Key);
            if (child == null || !child.TryGetComponent(out TextMeshProUGUI tmpText))
            {
                continue;
            }
            tmpText.text = pair.Value;
        }

        foreach (KeyValuePair<string, ConfigEntry<KeyCode>> pair in keyCodeHints)
        {
            Transform child = keyHintUI.transform.Find(pair.Key);
            if (child == null || !child.TryGetComponent(out TextMeshProUGUI tmpText))
            {
                continue;
            }
            tmpText.text = pair.Value.Value.ToString();
        }

        Log.LogInfo($"Updated {keyHintUI.name} key hint");
    }
}
