using System.Collections.Generic;
using System.Linq;
using ExtraSnapsMadeEasy.Extensions;
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



    internal static SnapMode snapMode;


    internal static GridPrecision gridPrecision;
    private static float currentGridPrecision = GridPrecisionMap[GridPrecision.Low];

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
            if (snapMode == SnapMode.Precise) { snapMode = SnapMode.Auto; }
            else { snapMode = SnapMode.Precise; }
        }
        else if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.ToggleManualSnap.Value))
        {
            if (snapMode == SnapMode.Manual) { snapMode = SnapMode.Auto; }
            else { snapMode = SnapMode.Manual; }
        }
        else if (Input.GetKeyDown(ExtraSnapsPlugin.Instance.ToggleGridSnap.Value))
        {
            if (snapMode == SnapMode.Grid) { snapMode = SnapMode.Auto; }
            else { snapMode = SnapMode.Grid; }
        }

        if (snapMode != prevSnapMode)
        {
            __instance.Message(ExtraSnapsPlugin.Instance.NotificationType.Value, SnapModeMsg[snapMode]);
        }

        if (__instance.m_placementGhost == null || snapMode == SnapMode.Auto)
        {
            return;
        }

        if (snapMode == SnapMode.Precise || snapMode == SnapMode.Manual)
        {
            SnapManually(ref __instance);
        }

        if (snapMode == SnapMode.Grid)
        {
            SnapToGrid(ref __instance);
        }
    }

    private static void SnapToGrid(ref Player player)
    {
        Piece sourcePiece = player.m_placementGhost?.GetComponent<Piece>();
        if (sourcePiece == null) { return; }

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
        Piece sourcePiece = player.m_placementGhost?.GetComponent<Piece>();
        if (sourcePiece == null) { return; }

        Piece targetPiece = RayTest(player, player.m_placementGhost);
        if (targetPiece == null) { return; }

        if (currentTargetParent != targetPiece.transform)
        {
            if (ExtraSnapsPlugin.Instance.ResetSnapsOnNewPiece.Value || currentTargetSnap < 0)
            {
                currentTargetSnap = 0;
            }
            currentTargetParent = targetPiece.transform;
        }

        if (currentSourceParent != sourcePiece.transform)
        {
            if (ExtraSnapsPlugin.Instance.ResetSnapsOnNewPiece.Value || currentSourceSnap < 0)
            {
                currentSourceSnap = 0;
            }

            currentSourceParent = sourcePiece.transform;
        }

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

        TempSourceSnapPoints.Clear();
        sourcePiece.GetSnapPoints(TempSourceSnapPoints);
        TempTargetSnapPoints.Clear();
        targetPiece.GetSnapPoints(TempTargetSnapPoints);

        if (TempSourceSnapPoints.Count == 0 || TempTargetSnapPoints.Count == 0)
        {
            return;
        }

        if (currentSourceSnap >= TempSourceSnapPoints.Count) { currentSourceSnap = 0; }
        if (currentTargetSnap >= TempTargetSnapPoints.Count) { currentTargetSnap = 0; }

        Transform sourceSnap = TempSourceSnapPoints[currentSourceSnap];
        Transform targetSnap;
        switch (snapMode)
        {
            case SnapMode.Precise:
                targetSnap = TempTargetSnapPoints[currentTargetSnap];
                break;

            case SnapMode.Manual:
                if (player.m_placementMarkerInstance == null) { return; }
                Vector3 markerPosition = player.m_placementMarkerInstance.transform.position;
                targetSnap = TempTargetSnapPoints.OrderBy(snapPoint => Vector3.Distance(markerPosition, snapPoint.position)).First();
                break;

            default:
                return;
        }

        if (prevSourceSnap != currentSourceSnap)
        {
            string name = sourceSnap.name is not null and not "_snappoint" ? sourceSnap.name : $"Point {currentSourceSnap + 1}";
            player.Message(ExtraSnapsPlugin.Instance.NotificationType.Value, $"Source Snap Point: {name}");
        }

        if (prevTargetSnap != currentTargetSnap && snapMode == SnapMode.Precise)
        {
            string name = sourceSnap.name is not null and not "_snappoint" ? targetSnap.name : $"Point {currentTargetSnap + 1}";
            player.Message(ExtraSnapsPlugin.Instance.NotificationType.Value, $"Target Snap Point: {name}");
        }

        // adjust placement ghost position based on the difference between sourceSnap and targetSnap
        player.m_placementGhost.transform.position += targetSnap.position - sourceSnap.position;
    }

    private static Piece RayTest(Player player, GameObject placementGhost)
    {
        Piece placementGhostPiece = placementGhost.GetComponent<Piece>();
        bool water = placementGhostPiece.m_waterPiece || placementGhostPiece.m_noInWater;
        player.PieceRayTest(out _, out _, out Piece piece, out _, out _, water);
        return piece;
    }
}
