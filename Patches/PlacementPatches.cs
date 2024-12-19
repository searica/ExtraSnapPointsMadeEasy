using System.Collections.Generic;
using System.Linq;
using ExtraSnapsMadeEasy.Extensions;
using ExtraSnapsMadeEasy.Models;
using HarmonyLib;
using UnityEngine;
namespace ExtraSnapsMadeEasy.Patches;
using static ExtraSnapsMadeEasy.Models.GridSnapping;
using static ExtraSnapsMadeEasy.Models.SnapModes;

[HarmonyPatch(typeof(Player))]
internal class PlacementPatches
{
    private static int currentSourceSnap = 0;
    private static int currentTargetSnap = 0;
    internal static SnapMode snapMode;
    internal static GridPrecision gridPrecision;
    private static float currentGridPrecision = GridPrecisionMap[GridPrecision.Low];

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

    internal static bool IsAutoSnapMode => snapMode == SnapMode.Auto;
    internal static bool IsPreciseSnapMode => snapMode == SnapMode.Precise;
    internal static bool IsManualSnapMode => snapMode == SnapMode.Manual;
    internal static bool IsGridSnapMode => snapMode == SnapMode.Grid;
    

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.UpdatePlacementGhost))]
    private static void UpdatePlacementGhostPostfix(Player __instance)
    {
        if (!__instance || !__instance.InPlaceMode() || __instance.IsDead())
        {
            return;
        }

        SnapMode prevSnapMode = snapMode;
        if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.TogglePreciseSnap.Value))
        {
            snapMode = !IsPreciseSnapMode ? SnapMode.Precise : SnapMode.Auto;
        }
        else if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.ToggleManualSnap.Value))
        {
            snapMode = !IsManualSnapMode ? SnapMode.Manual : SnapMode.Auto;
        }
        else if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.ToggleGridSnap.Value))
        {
            snapMode = !IsGridSnapMode ? SnapMode.Grid : SnapMode.Auto;
        }

        if (snapMode != prevSnapMode)
        {
            __instance.Message(ExtraSnapsPlugin.Instance.NotificationType.Value, SnapModeMsg[snapMode]);
        }

        if (!__instance.m_placementGhost || snapMode == SnapMode.Auto)
        {
            return;
        }

        if (IsPreciseSnapMode || IsManualSnapMode)
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
            if (gridPrecision == GridPrecision.Low) { gridPrecision = GridPrecision.High; }
            else { gridPrecision = GridPrecision.Low; }
            currentGridPrecision = GridPrecisionMap[gridPrecision];
            player.Message(ExtraSnapsPlugin.Instance.NotificationType.Value, $"Grid Precision: {currentGridPrecision}");
        }

        Vector3 position = player.m_placementGhost.transform.position;
        position.x = position.x.RoundToNearest(currentGridPrecision);
        position.z = position.z.RoundToNearest(currentGridPrecision);
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

        Log.LogInfo("Snapping Manually!");

        if (currentSourceParent != sourcePiece.transform)
        {
            if (ExtraSnapsPlugin.Instance.ResetSnapsOnNewPiece.Value || currentSourceSnap < 0)
            {
                currentSourceSnap = 0;
            }
            currentSourceParent = sourcePiece.transform;
        }


        if (currentTargetParent != targetPiece.transform)
        {
            if (ExtraSnapsPlugin.Instance.ResetSnapsOnNewPiece.Value || currentTargetSnap < 0)
            {
                currentTargetSnap = 0;
            }
            currentTargetParent = targetPiece.transform;
        }

        

        Log.LogInfo("Point A");
        int prevSourceSnap = currentSourceSnap;
        if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.IterateSourceSnapPoints.Value))
        {
            currentSourceSnap++;
        }

        int prevTargetSnap = currentTargetSnap;
        if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.IterateTargetSnapPoints.Value))
        {
            currentTargetSnap++;
        }
        Log.LogInfo("Point B");

        TempSourceSnapPoints.Clear();
        sourcePiece.GetSnapPoints(TempSourceSnapPoints);
        TempTargetSnapPoints.Clear();
        targetPiece.GetSnapPoints(TempTargetSnapPoints);

        Log.LogInfo("Point C");
        if (TempSourceSnapPoints.Count == 0 || TempTargetSnapPoints.Count == 0)
        {
            Log.LogInfo("No snap points!");
            return;
        }

        if (currentSourceSnap >= TempSourceSnapPoints.Count) { currentSourceSnap = 0; }
        if (currentTargetSnap >= TempTargetSnapPoints.Count) { currentTargetSnap = 0; }

        Log.LogInfo("Point D");

        Transform sourceSnap = TempSourceSnapPoints[currentSourceSnap];
        Transform targetSnap;
        switch (snapMode)
        {
            case SnapMode.Precise:
                targetSnap = TempTargetSnapPoints[currentTargetSnap];
                break;

            case SnapMode.Manual:
                if (!player.m_placementMarkerInstance) { return; }
                Vector3 markerPosition = player.m_placementMarkerInstance.transform.position;
                targetSnap = TempTargetSnapPoints.OrderBy(snapPoint => Vector3.Distance(markerPosition, snapPoint.position)).First();
                break;

            default:
                return;
        }
        Log.LogInfo("Point E");

        if (prevSourceSnap != currentSourceSnap)
        {
            string name = HasFriendlySnapName(sourceSnap) ? sourceSnap.name : $"Point {currentSourceSnap + 1}";
            player.Message(ExtraSnapsPlugin.Instance.NotificationType.Value, $"Source Snap Point: {name}");
        }

        Log.LogInfo("Point F");
        if (IsPreciseSnapMode && prevTargetSnap != currentTargetSnap)
        {
            string name = HasFriendlySnapName(targetSnap) ? targetSnap.name : $"Point {currentTargetSnap + 1}";
            player.Message(ExtraSnapsPlugin.Instance.NotificationType.Value, $"Target Snap Point: {name}");
        }

        Log.LogInfo("Point G");
        // adjust placement ghost position based on the difference between sourceSnap and targetSnap
        player.m_placementGhost.transform.position += targetSnap.position - sourceSnap.position;
    }

    private static bool TryGetTargetPiece(Player player, Piece placementGhostPiece, out Piece targetPiece)
    {
        bool water = placementGhostPiece.m_waterPiece || placementGhostPiece.m_noInWater;
        player.PieceRayTest(out _, out _, out targetPiece, out _, out _, water);
        return targetPiece;
    }

    private static Piece RayTest(Player player, GameObject placementGhost)
    {
        Piece placementGhostPiece = placementGhost.GetComponent<Piece>();
        bool water = placementGhostPiece.m_waterPiece || placementGhostPiece.m_noInWater;
        player.PieceRayTest(out _, out _, out Piece piece, out _, out _, water);
        return piece;
    }

    private static bool HasFriendlySnapName(Transform snapPoint)
    {
        return snapPoint.name is not null and not SnapPointNames.DEFAULT_NAME;
    }
}
