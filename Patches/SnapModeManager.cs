using System;
using System.Collections.Generic;
using System.Linq;
using ExtraSnapsMadeEasy.Models;
using HarmonyLib;
using UnityEngine;
using Logging;
using ExtraSnapsMadeEasy.Extensions;
using static ExtraSnapsMadeEasy.Models.GridSnapping;
using static ExtraSnapsMadeEasy.Models.SnapModes;

namespace ExtraSnapsMadeEasy.Patches;

[HarmonyPatch]
internal class SnapModeManager
{
    private static int CurrentSourceSnap = 0;
    private static int CurrentTargetSnap = 0;
    internal static SnapMode CurrentSnapMode;
    internal static GridPrecision CurrentGridPrecision;
    private static float CurrentGridPrecisionValue = GridPrecisionMap[GridPrecision.Low];
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


    [HarmonyPostfix]
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

        if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.CycleGridPrecision.Value))
        {
            if (CurrentGridPrecision == GridPrecision.Low) { CurrentGridPrecision = GridPrecision.High; }
            else { CurrentGridPrecision = GridPrecision.Low; }
            CurrentGridPrecisionValue = GridPrecisionMap[CurrentGridPrecision];
            player.Message(ExtraSnapsPlugin.Instance.NotificationType.Value, $"Grid Precision: {CurrentGridPrecisionValue}");
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

        if (currentSourceParent != sourcePiece.transform)
        {
            if (ExtraSnapsPlugin.Instance.ResetSnapsOnNewPiece.Value || CurrentSourceSnap < 0)
            {
                CurrentSourceSnap = 0;
            }
            currentSourceParent = sourcePiece.transform;
        }
        if (currentTargetParent != targetPiece.transform)
        {
            if (ExtraSnapsPlugin.Instance.ResetSnapsOnNewPiece.Value || CurrentTargetSnap < 0)
            {
                CurrentTargetSnap = 0;
            }
            currentTargetParent = targetPiece.transform;
        }

        int prevSourceSnap = CurrentSourceSnap;
        if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.IterateSourceSnapPoints.Value))
        {
            CurrentSourceSnap++;
        }

        int prevTargetSnap = CurrentTargetSnap;
        if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.IterateTargetSnapPoints.Value))
        {
            CurrentTargetSnap++;
        }

        TempSourceSnapPoints.Clear();
        sourcePiece.GetSnapPoints(TempSourceSnapPoints);
        TempTargetSnapPoints.Clear();
        targetPiece.GetSnapPoints(TempTargetSnapPoints);

        if (TempSourceSnapPoints.Count == 0 || TempTargetSnapPoints.Count == 0)
        {
            // Set this to LogLevel High
            Log.LogInfo("No snap points!", Log.InfoLevel.High);
            return;
        }

        if (CurrentSourceSnap >= TempSourceSnapPoints.Count) { CurrentSourceSnap = 0; }
        if (CurrentTargetSnap >= TempTargetSnapPoints.Count) { CurrentTargetSnap = 0; }

        Transform sourceSnap = TempSourceSnapPoints[CurrentSourceSnap];
        Transform targetSnap;
        switch (CurrentSnapMode)
        {
            case SnapMode.ManualPlus:
                targetSnap = TempTargetSnapPoints[CurrentTargetSnap];
                break;

            case SnapMode.Manual:
                if (!player.m_placementMarkerInstance) { return; }
                Vector3 markerPosition = player.m_placementMarkerInstance.transform.position;
                targetSnap = TempTargetSnapPoints.OrderBy(snapPoint => Vector3.Distance(markerPosition, snapPoint.position)).First();
                break;

            default:
                return;
        }

        if (prevSourceSnap != CurrentSourceSnap)
        {
            string name = HasFriendlySnapName(sourceSnap) ? sourceSnap.name : $"Point {CurrentSourceSnap + 1}";
            player.Message(ExtraSnapsPlugin.Instance.NotificationType.Value, $"Placing Snap Point: {name}");
        }

        if (IsManualPlusSnapMode && prevTargetSnap != CurrentTargetSnap)
        {
            string name = HasFriendlySnapName(targetSnap) ? targetSnap.name : $"Point {CurrentTargetSnap + 1}";
            player.Message(ExtraSnapsPlugin.Instance.NotificationType.Value, $"Target Snap Point: {name}");
        }

        // adjust placement ghost position based on the difference between sourceSnap and targetSnap
        player.m_placementGhost.transform.position += targetSnap.position - sourceSnap.position;
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
