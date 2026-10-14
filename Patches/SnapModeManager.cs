using ExtraSnapsMadeEasy.Extensions;
using ExtraSnapsMadeEasy.Models;
using HarmonyLib;
using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ExtraSnapsMadeEasy.Models.GridSnapping;
using static ExtraSnapsMadeEasy.Models.SnapModes;
using static Version;

namespace ExtraSnapsMadeEasy.Patches;

[HarmonyPatch]
internal class SnapModeManager
{
    private static int CurrentSourceSnapCounter = 0;
    private static int CurrentTargetSnapCounter = 0;
    private static int PrevSourceSnapCounter = 0;
    private static int PrevTargetSnapCounter = 0;

    
    internal static GridPrecision CurrentGridPrecision;
    internal static int CurrentGridPrecisionFromCustomList = 0;
    private static List<float> CustomGridSnappingList = [];
    private static float PrevGridPrecisionValue = GridPrecisionMap[GridPrecision.Low];
    private static float CurrentGridPrecisionValue = GridPrecisionMap[GridPrecision.Low];

    internal static SnapMode CurrentSnapMode;
    internal static string CurrentSnapModeName => SnapModeNames[CurrentSnapMode];

    private static Transform currentTargetParent;
    private static Transform currentSourceParent;

    /// <summary>
    /// Re-usable list for source snap points to avoid allocating a list every frame.
    /// Clear this list before reading snap points into it!
    /// </summary>
    private static readonly List<Transform> TempSourceSnapPoints = new();

    /// <summary>
    /// Re-usable list for target snap points to avoid allocating a list every frame.
    /// Clear this list before reading snap points into it!
    /// </summary>
    private static readonly List<Transform> TempTargetSnapPoints = new();

    internal static bool IsAutoSnapMode => CurrentSnapMode == SnapMode.Auto;
    internal static bool IsManualPlusSnapMode => CurrentSnapMode == SnapMode.ManualPlus;
    internal static bool IsManualSnapMode => CurrentSnapMode == SnapMode.Manual;
    internal static bool IsGridSnapMode => CurrentSnapMode == SnapMode.Grid;

    internal static float GetCurrentGridPrecision() => CurrentGridPrecisionValue;

    /// <summary>
    ///     Event triggered whenever the snap mode changes.
    /// </summary>
    internal static event Action OnSnapModeChanged;

    /// <summary>
    ///     Safely invoke the <see cref="OnSnapModeChanged"/> event
    /// </summary>
    private static void InvokeOnSnapModeChanged()
    {
        OnSnapModeChanged?.SafeInvoke();
    }

    /// <summary>
    ///     Event triggered whenever the current grid precision changes.
    /// </summary>
    internal static event Action OnGridPrecisionChanged;

    private static void InvokeOnGridPrecisionChanged()
    {
        OnGridPrecisionChanged?.SafeInvoke();
    }

    /// <summary>
    ///     Update custom precision grid snapping list if the setting has been changed and the cached list is 
    ///     no longer valid. Also resets the current position in the custom precision grid snapping list to 
    ///     prevent attempting to access values that are out of bounds when the list shrinks. Also sets the 
    ///     current grid precision to the first entry in the custom list if it is populated.
    /// </summary>
    private static void UpdateCustomGridSnapping()
    {
        if (!ExtraSnapsPlugin.Instance.ShouldUpdateGridSnappingList)
        {
            return;
        }
        
        CustomGridSnappingList.Clear();          // reset list before updating it
        CurrentGridPrecisionFromCustomList = 0;  // reset position in list whenever the list changes

        string customPrecisionListStringRaw = ExtraSnapsPlugin.Instance.GridPrecisionList.Value.Trim();

        if (customPrecisionListStringRaw.Length >= 1)
        {
            string[] customPrecisionListStrings = customPrecisionListStringRaw.Split(
                [',', ';'],
                System.StringSplitOptions.RemoveEmptyEntries
            );

            for (int i = 0; i < customPrecisionListStrings.Length; i++)
            {
                if (float.TryParse(customPrecisionListStrings[i].Trim(), out float p) && p > 0.0f)
                {
                    CustomGridSnappingList.Add(p);
                }
                else
                {
                    Log.LogWarning($"Invalid value for custom grid snap precision: '{customPrecisionListStrings[i]}'");
                }
            }
        }
        if (CustomGridSnappingList.Count > 0)
        {
            CurrentGridPrecisionValue = CustomGridSnappingList[CurrentGridPrecisionFromCustomList];
        }
        ExtraSnapsPlugin.Instance.ShouldUpdateGridSnappingList = false;
    }

    [HarmonyPostfix]
    [HarmonyAfter("Searica.Valheim.MoreVanillaBuildPrefabs")]
    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
    private static void UpdatePlacementGhostPostfix(Player __instance)
    {
        if (!__instance || !__instance.InPlaceMode() || __instance.IsDead())
        {
            return;
        }

        SnapMode prevSnapMode = CurrentSnapMode;
        if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.TogglePreciseSnap.Value))
        {
            CurrentSnapMode = !IsManualPlusSnapMode ? SnapMode.ManualPlus : SnapMode.Auto;
        }
        else if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.ToggleManualSnap.Value))
        {
            CurrentSnapMode = !IsManualSnapMode ? SnapMode.Manual : SnapMode.Auto;
        }
        else if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.ToggleGridSnap.Value))
        {
            CurrentSnapMode = !IsGridSnapMode ? SnapMode.Grid : SnapMode.Auto;
        }

        if (CurrentSnapMode != prevSnapMode)
        {
            InvokeOnSnapModeChanged();
            __instance.Message(ExtraSnapsPlugin.Instance.NotificationType.Value, $"Snap Mode: {CurrentSnapModeName}");
        }

        if (!__instance.m_placementGhost || CurrentSnapMode == SnapMode.Auto)
        {
            return;
        }

        if (IsManualPlusSnapMode || IsManualSnapMode)
        {
            SnapManually(ref __instance);
        }

        if (IsGridSnapMode)
        {
            SnapToGrid(ref __instance);
        }
    }

    private static void SnapToGrid(ref Player player)
    {
        if (!player || !player.m_placementGhost || !player.m_placementGhost.TryGetComponent(out Piece sourcePiece))
        {
            return;
        }

        PrevGridPrecisionValue = CurrentGridPrecisionValue;
        UpdateCustomGridSnapping(); // Lazy update as needed

        if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.CycleGridPrecision.Value))
        {

            if (CustomGridSnappingList.Count() > 0)
            {
                if (Input.GetKey(ExtraSnapsPlugin.Instance.ReverseCyclingDirection.Value))  // check if held down
                {
                    if (--CurrentGridPrecisionFromCustomList < 0)
                    {
                        CurrentGridPrecisionFromCustomList = CustomGridSnappingList.Count() - 1;
                    }
                }
                else
                {
                    if (++CurrentGridPrecisionFromCustomList >= CustomGridSnappingList.Count())
                    {
                        CurrentGridPrecisionFromCustomList = 0;
                    }
                }
                CurrentGridPrecisionValue = CustomGridSnappingList[CurrentGridPrecisionFromCustomList];
            }
            else
            {
                switch (CurrentGridPrecision)
                {
                    case GridPrecision.Low:
                        CurrentGridPrecision = GridPrecision.High;
                        break;

                    case GridPrecision.High:
                        CurrentGridPrecision = GridPrecision.Low;
                        break;

                    default:
                        CurrentGridPrecision = GridPrecision.Low;
                        break;
                }

                CurrentGridPrecisionValue = GridPrecisionMap[CurrentGridPrecision];
            }
        }

        // value can change in UpdateCustomGridSnapping or due to key presses so check here
        if (PrevGridPrecisionValue != CurrentGridPrecisionValue)  
        {
            player.Message(ExtraSnapsPlugin.Instance.NotificationType.Value, $"Grid Precision: {CurrentGridPrecisionValue}");
            InvokeOnGridPrecisionChanged();
        }

        Vector3 position = player.m_placementGhost.transform.position;
        position.x = position.x.RoundToNearest(CurrentGridPrecisionValue);
        position.z = position.z.RoundToNearest(CurrentGridPrecisionValue);
        player.m_placementGhost.transform.position = position;
    }

    private static void SnapManually(ref Player player)
    {

        if (!player || !player.m_placementGhost || !player.m_placementGhost.TryGetComponent(out Piece sourcePiece) && sourcePiece)
        {
            return;
        }

        if (!TryGetTargetPiece(player, sourcePiece, out Piece targetPiece))
        {
            return;
        }

   
        PrevTargetSnapCounter = CurrentTargetSnapCounter;
        TryUpdateSourceSnapPoint(player, sourcePiece, out Transform sourceSnap);
        TryUpdateTargetSnapPoint(player, targetPiece, out Transform targetSnap);

        // adjust placement ghost position based on the difference between sourceSnap and targetSnap
        player.m_placementGhost.transform.position += targetSnap.position - sourceSnap.position;
    }

    private static bool TryUpdateSourceSnapPoint(Player player, Piece sourcePiece, out Transform sourceSnap)
    {
        PrevSourceSnapCounter = CurrentSourceSnapCounter;

        if (currentSourceParent != sourcePiece.transform)
        {
            if (ExtraSnapsPlugin.Instance.ResetSnapsOnNewPiece.Value || CurrentSourceSnapCounter < 0)
            {
                CurrentSourceSnapCounter = 0;
            }
            currentSourceParent = sourcePiece.transform;
        }
        
        if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.IterateSourceSnapPoints.Value))
        {
            if (Input.GetKey(ExtraSnapsPlugin.Instance.ReverseCyclingDirection.Value)) // check if held
            {
                CurrentSourceSnapCounter--;
            }
            else
            {
                CurrentSourceSnapCounter++;
            }
        }

        TempSourceSnapPoints.Clear();
        sourcePiece.GetSnapPoints(TempSourceSnapPoints);

        if (TempSourceSnapPoints.Count == 0 )
        {
            Log.LogInfo("No snap points on source piece!", Log.InfoLevel.High);
            sourceSnap = null;
            return false;
        }
        
        if (CurrentSourceSnapCounter >= TempSourceSnapPoints.Count) { CurrentSourceSnapCounter = 0; }
        else if (CurrentSourceSnapCounter < 0) { CurrentSourceSnapCounter = TempSourceSnapPoints.Count - 1; }
           
        sourceSnap = TempSourceSnapPoints[CurrentSourceSnapCounter];

        if (PrevSourceSnapCounter != CurrentSourceSnapCounter)
        {
            NotifyPlayerSnapPointChanged(player, sourceSnap, CurrentTargetSnapCounter, "Placing");
        }

        return true;
    }

    private static bool TryUpdateTargetSnapPoint(Player player, Piece targetPiece, out Transform targetSnap)
    {
        PrevTargetSnapCounter = CurrentTargetSnapCounter;

        if (currentTargetParent != targetPiece.transform)
        {
            if (ExtraSnapsPlugin.Instance.ResetSnapsOnNewPiece.Value || CurrentTargetSnapCounter < 0)
            {
                CurrentTargetSnapCounter = 0;
            }
            currentTargetParent = targetPiece.transform;
        }
        
        if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.IterateTargetSnapPoints.Value))
        {
            if (Input.GetKey(ExtraSnapsPlugin.Instance.ReverseCyclingDirection.Value)) // check if held
            {
                CurrentTargetSnapCounter--;
            }
            else
            {
                CurrentTargetSnapCounter++;
            }
        }

        TempTargetSnapPoints.Clear();
        targetPiece.GetSnapPoints(TempTargetSnapPoints);

        if (TempTargetSnapPoints.Count == 0)
        {
            Log.LogInfo("No snap points on target piece!", Log.InfoLevel.High);
            targetSnap = null;
            return false;
        }

        if (CurrentTargetSnapCounter >= TempTargetSnapPoints.Count) { CurrentTargetSnapCounter = 0; }
        else if (CurrentTargetSnapCounter < 0) { CurrentTargetSnapCounter = TempTargetSnapPoints.Count - 1; }

        switch (CurrentSnapMode)
        {
            case SnapMode.ManualPlus:
                targetSnap = TempTargetSnapPoints[CurrentTargetSnapCounter];
                if (PrevTargetSnapCounter != CurrentTargetSnapCounter)
                {
                    NotifyPlayerSnapPointChanged(player, targetSnap, CurrentTargetSnapCounter, "Target");
                }
                return true;

            case SnapMode.Manual:
                if (!player.m_placementMarkerInstance) {
                    break; 
                }
                Vector3 markerPosition = player.m_placementMarkerInstance.transform.position;
                targetSnap = TempTargetSnapPoints.OrderBy(snapPoint => Vector3.Distance(markerPosition, snapPoint.position)).First();
                return true;
            
            default:
                break;  
        }

        targetSnap = null;
        return false;
    }

    private static void NotifyPlayerSnapPointChanged(Player player, Transform snapPoint, int counter, string snapPieceType)
    {
        string name = HasFriendlySnapName(snapPoint) ? snapPoint.name : $"Point {counter + 1}";
        player.Message(ExtraSnapsPlugin.Instance.NotificationType.Value, $"{snapPieceType} Snap Point: {name}");
    }

    private static bool TryGetTargetPiece(Player player, Piece placementGhostPiece, out Piece targetPiece)
    {
        bool water = placementGhostPiece.m_waterPiece || placementGhostPiece.m_noInWater;
        player.PieceRayTest(out _, out _, out targetPiece, out _, out _, water);
        return targetPiece;
    }

    private static bool HasFriendlySnapName(Transform snapPoint)
    {
        return snapPoint.name is not null and not SnapPointNames.DEFAULT_NAME;
    }
}
