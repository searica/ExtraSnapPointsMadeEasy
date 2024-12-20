using HarmonyLib;
using System.Collections.Generic;
using ExtraSnapsMadeEasy.Patches;
using UnityEngine;
using ExtraSnapsMadeEasy.Extensions;
using UnityEngine.UI;

namespace ExtraSnapsMadeEasy.SnapKeyHints;

[HarmonyPatch]
internal static class KeyHintPatches
{
    private const string ToggleSnapModeHintInternalName = "ToggleSnapMode";
    private const string CurrentSnapModeHintInternalName = "CurrentSnapMode";

    internal static readonly KeyHintInfo CycleSnapsHintInfo = new()
    {
        BoundObject = null,
        TransformPath = "BuildHints/Keyboard/Snap",
        HintText = new Dictionary<string, string>()
        {
            {"Text", "Next snap point<br>Placing / Target"}
        },
        PreferredTextWidth = 130,
        KeyHintConfigs = new List<KeyHintInfo.KeyHintConfig>()
        {
            new ("key_bkg/Key", ExtraSnapsPlugin.Instance.IterateSourceSnapPoints),
            new ("key_bkg (1)/Key", ExtraSnapsPlugin.Instance.IterateTargetSnapPoints),
        }
    };

    internal static readonly KeyHintInfo AltPlaceHintInfo = new()
    {
        BoundObject = null,
        TransformPath = "BuildHints/Keyboard/AltPlace",
        HintText = new Dictionary<string, string>() 
        {
            {"Text", "Disable<br>snapping"}
        },
        PreferredTextWidth = 90,
        KeyHintConfigs = new List<KeyHintInfo.KeyHintConfig>(),
    };


    internal static readonly KeyHintInfo ToggleSnapModeHintInfo = new()
    {
        BoundObject = null,
        TransformPath = $"BuildHints/Keyboard/{ToggleSnapModeHintInternalName}",
        HintText = new Dictionary<string, string>()
        {
            {"Text", $"Toggle snap mode<br>Manual/Manual+/Grid"}
        },
        PreferredTextWidth = 170,
        KeyHintConfigs = new List<KeyHintInfo.KeyHintConfig>()
        {
            new ("key_bkg/Key", ExtraSnapsPlugin.Instance.ToggleManualSnap),
            new ("key_bkg (1)/Key", ExtraSnapsPlugin.Instance.TogglePreciseSnap),
            new ("key_bkg (2)/Key", ExtraSnapsPlugin.Instance.ToggleGridSnap),
        }
    };

    internal static readonly KeyHintInfo CurrentSnapModeHintInfo = new()
    {
        BoundObject = null,
        TransformPath = $"BuildHints/Keyboard/{CurrentSnapModeHintInternalName}",
        HintText = new Dictionary<string, string>(){
            {"Text", "Snap<br>Mode"}
        },
        PreferredTextWidth = 50,
        KeyHintConfigs = new List<KeyHintInfo.KeyHintConfig>()
        {
            new ("key_bkg/Key", keyConfig: null, rawKeyText: SnapModeManager.CurrentSnapModeName),
        }
    };


    [HarmonyPostfix]
    [HarmonyPatch(typeof(KeyHints), nameof(KeyHints.Awake))]
    internal static void KeyHints_Awake_Postfix(KeyHints __instance)
    {
        if (!__instance)
        {
            return;
        }

        // Set up cycling snaps hint
        CycleSnapsHintInfo.SetParentObject(__instance);
        CycleSnapsHintInfo.SubscribeToKeyConfigUpdates(performInitialUpdate: true);

        // toggle the alternative snapping key hint when not in "Auto" snap mode.
        AltPlaceHintInfo.SetParentObject(__instance);
        AltPlaceHintInfo.UpdateKeyHintUI();
        AltPlaceHintInfo.SetVisibility(SnapModeManager.IsAutoSnapMode); // perform initial update manually.
        SnapModeManager.OnSnapModeChanged += () =>
        {
            // Update visibility on snap mode change.
            AltPlaceHintInfo.SetVisibility(SnapModeManager.IsAutoSnapMode);
        };

        // Create toggle Snap Mode Hint Info
        ToggleSnapModeHintInfo.BoundObject = Object.Instantiate(AltPlaceHintInfo.BoundObject, AltPlaceHintInfo.BoundObject.transform.parent);
        ToggleSnapModeHintInfo.BoundObject.name = ToggleSnapModeHintInternalName;

        ToggleSnapModeHintInfo.CreateKeysIfNeeded();
        ToggleSnapModeHintInfo.SubscribeToKeyConfigUpdates(performInitialUpdate: true);

        // Create current Snap Mode Hint Info
        CurrentSnapModeHintInfo.BoundObject = Object.Instantiate(AltPlaceHintInfo.BoundObject, AltPlaceHintInfo.BoundObject.transform.parent);
        CurrentSnapModeHintInfo.BoundObject.name = CurrentSnapModeHintInternalName;

        CurrentSnapModeHintInfo.UpdateKeyHintUI(); // perform initial update manually.
        SnapModeManager.OnSnapModeChanged += () =>
        {
            // Update current snap mode UI hint on change.
            CurrentSnapModeHintInfo.KeyHintConfigs[0].RawKeyText = SnapModeManager.CurrentSnapModeName;
            CurrentSnapModeHintInfo.UpdateKeyHintUI();
        };
        
    }
}
