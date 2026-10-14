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
    private const string ReverseCyclingDirectionInternalName = "ReverseCyclingDirection";
    private const string CycleGridPrecisionInternalName = "CycleGridPrecision";
    private const string GridPrecisionInternalName = "GridPrecisionHint";

    internal static readonly KeyHintInfo CycleSnapsHintInfo = new()
    {
        BoundObject = null,
        TransformPath = "BuildHints/Keyboard/Snap",
        HintText = new Dictionary<string, string>()
        {
            {"Text", "Cycle snap point<br>Placing / Target"}
        },
        PreferredTextWidth = 140,
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
        PreferredTextWidth = 70,
        KeyHintConfigs = new List<KeyHintInfo.KeyHintConfig>(),
    };

    internal static readonly KeyHintInfo ReverseCyclingDirectionHintInfo = new()
    {
        BoundObject = null,
        TransformPath = $"BuildHints/Keyboard/{CurrentSnapModeHintInternalName}",
        HintText = new Dictionary<string, string>(){
            {"Text", "Reverse<br>Cycling"}
        },
        PreferredTextWidth = 70,
        KeyHintConfigs = new List<KeyHintInfo.KeyHintConfig>()
        {
            new ("key_bkg/Key", keyConfig: ExtraSnapsPlugin.Instance.ReverseCyclingDirection),
        }
    };


    internal static readonly KeyHintInfo CycleGridPrecisionHintInfo = new()
    {
        BoundObject = null,
        TransformPath = $"BuildHints/Keyboard/{CycleGridPrecisionInternalName}",
        HintText = new Dictionary<string, string>(){
            {"Text", "Cycle<br>Precision"}
        },
        PreferredTextWidth = 70,
        KeyHintConfigs = new List<KeyHintInfo.KeyHintConfig>()
        {
            new ("key_bkg/Key", keyConfig: ExtraSnapsPlugin.Instance.CycleGridPrecision),
        }
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

    internal static readonly KeyHintInfo GridPrecisionHintInfo = new()
    {
        BoundObject = null,
        TransformPath = $"BuildHints/Keyboard/{GridPrecisionHintInfo}",
        HintText = new Dictionary<string, string>(){
            {"Text", "Grid<br>Precision"}
        },
        PreferredTextWidth = 75,
        KeyHintConfigs = new List<KeyHintInfo.KeyHintConfig>()
        {
            new ("key_bkg/Key", keyConfig: null, rawKeyText: SnapModeManager.GetCurrentGridPrecision().ToString()),
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
        
        // Set up new alternative snapping key hint and make it toggle on/off based on whether player is in "Auto" snap mode.
        AltPlaceHintInfo.SetParentObject(__instance);
        AltPlaceHintInfo.UpdateKeyHintUI();
        AltPlaceHintInfo.SetVisibility(SnapModeManager.IsAutoSnapMode); // perform initial update manually.
        SnapModeManager.OnSnapModeChanged += () => // Update visibility on snap mode change.
        {
            AltPlaceHintInfo.SetVisibility(SnapModeManager.IsAutoSnapMode);
        };

        // Create Reverse Cycling key hint and set it up to toggle on/off based on the active snap mode
        ReverseCyclingDirectionHintInfo.BoundObject = Object.Instantiate(AltPlaceHintInfo.BoundObject, AltPlaceHintInfo.BoundObject.transform.parent);
        ReverseCyclingDirectionHintInfo.BoundObject.name = ReverseCyclingDirectionInternalName;
        ReverseCyclingDirectionHintInfo.BoundObject.transform.SetSiblingIndex(AltPlaceHintInfo.BoundObject.transform.GetSiblingIndex() + 1);
        ReverseCyclingDirectionHintInfo.SubscribeToKeyConfigUpdates(performInitialUpdate: true);
        ReverseCyclingDirectionHintInfo.SetVisibility(!SnapModeManager.IsAutoSnapMode);
        SnapModeManager.OnSnapModeChanged += () => // Update visibility on snap mode change.
        {
            ReverseCyclingDirectionHintInfo.SetVisibility(!SnapModeManager.IsAutoSnapMode);
        };

        // Set up cycling snaps hint for manual and manual+ snapping modes
        CycleSnapsHintInfo.SetParentObject(__instance);
        CycleSnapsHintInfo.SubscribeToKeyConfigUpdates(performInitialUpdate: true);
        CycleSnapsHintInfo.SetVisibility(!SnapModeManager.IsGridSnapMode); // perform initial update manually.
        SnapModeManager.OnSnapModeChanged += () => // Update visibility on snap mode change.
        {
            CycleSnapsHintInfo.SetVisibility(!SnapModeManager.IsGridSnapMode);
        };

        // Set up cycling grid precision hint to replace snapping hint with in grid snap mode
        CycleGridPrecisionHintInfo.BoundObject = Object.Instantiate(AltPlaceHintInfo.BoundObject, AltPlaceHintInfo.BoundObject.transform.parent);
        CycleGridPrecisionHintInfo.BoundObject.name = CycleGridPrecisionInternalName;
        CycleGridPrecisionHintInfo.BoundObject.transform.SetSiblingIndex(CycleSnapsHintInfo.BoundObject.transform.GetSiblingIndex() + 1);
        CycleGridPrecisionHintInfo.SubscribeToKeyConfigUpdates(performInitialUpdate: true);
        CycleGridPrecisionHintInfo.SetVisibility(SnapModeManager.IsGridSnapMode); // perform initial update manually.
        SnapModeManager.OnSnapModeChanged += () => // Update visibility on snap mode change.
        {
            CycleGridPrecisionHintInfo.SetVisibility(SnapModeManager.IsGridSnapMode);
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
        SnapModeManager.OnSnapModeChanged += () => // Update visibility on snap mode change.
        {
            CurrentSnapModeHintInfo.KeyHintConfigs[0].RawKeyText = SnapModeManager.CurrentSnapModeName;
            CurrentSnapModeHintInfo.UpdateKeyHintUI();
        };

        // Create curent grid precision Hint Info
        GridPrecisionHintInfo.BoundObject = Object.Instantiate(AltPlaceHintInfo.BoundObject, AltPlaceHintInfo.BoundObject.transform.parent);
        GridPrecisionHintInfo.BoundObject.name = GridPrecisionInternalName;
        GridPrecisionHintInfo.UpdateKeyHintUI(); // perform initial update manually.
        GridPrecisionHintInfo.SetVisibility(SnapModeManager.IsGridSnapMode);
        SnapModeManager.OnSnapModeChanged += () => // Hide key hint if not in grid snap mode.
        {
            GridPrecisionHintInfo.SetVisibility(SnapModeManager.IsGridSnapMode);
        };
        SnapModeManager.OnGridPrecisionChanged += () => // Update grid precision when it changes.
        {
            GridPrecisionHintInfo.KeyHintConfigs[0].RawKeyText = SnapModeManager.GetCurrentGridPrecision().ToString();
            GridPrecisionHintInfo.UpdateKeyHintUI();
        };

    }
}
