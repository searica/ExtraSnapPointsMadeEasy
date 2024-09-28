using System.Collections.Generic;
using System.Linq;
using ExtraSnapPointsMadeEasy.Extensions;
using HarmonyLib;
using UnityEngine;

namespace ExtraSnapPointsMadeEasy.Patches;
[HarmonyPatch(typeof(Player))]
internal class PlayerPatch
{
    private static readonly int TerrainRayMask = LayerMask.GetMask("terrain");
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

    internal enum SnapMode
    {
        Auto,
        Manual,
        ManualClosest,
        Grid
    }

    private static readonly Dictionary<SnapMode, string> SnapModeMsg = new()
    {
        {SnapMode.Auto,  "Snap Mode: Auto"},
        {SnapMode.Manual, "Snap Mode: Manual"},
        {SnapMode.ManualClosest, "Snap Mode: Manual (Closest)"},
        {SnapMode.Grid, "SnapMode: Grid"}
    };

    internal static SnapMode snapMode;

    internal enum GridPrecision
    {
        Low,
        High,
    }

    internal static Dictionary<GridPrecision, float> GridPrecisionMap = new()
    {
        { GridPrecision.High, 0.5f },
        { GridPrecision.Low, 1f }
    };

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

        if (Input.GetKeyDown(ExtraSnapPointsMadeEasy.EnableManualSnap.Value))
        {
            if (snapMode == SnapMode.Manual) { snapMode = SnapMode.Auto; }
            else { snapMode = SnapMode.Manual; }
        }
        else if (Input.GetKeyDown(ExtraSnapPointsMadeEasy.EnableManualClosestSnap.Value))
        {
            if (snapMode == SnapMode.ManualClosest) { snapMode = SnapMode.Auto; }
            else { snapMode = SnapMode.ManualClosest; }
        }
        else if (Input.GetKeyDown(ExtraSnapPointsMadeEasy.EnableGridSnap.Value))
        {
            if (snapMode == SnapMode.Grid) { snapMode = SnapMode.Auto; }
            else { snapMode = SnapMode.Grid; }
        }

        if (snapMode != prevSnapMode)
        {
            __instance.Message(ExtraSnapPointsMadeEasy.NotificationType.Value, SnapModeMsg[snapMode]);
        }

        if (__instance.m_placementGhost == null || snapMode == SnapMode.Auto)
        {
            return;
        }

        if (snapMode == SnapMode.Manual || snapMode == SnapMode.ManualClosest)
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

        if (Input.GetKeyDown(ExtraSnapPointsMadeEasy.CycleGridPrecision.Value))
        {
            if (gridPrecision == GridPrecision.Low) { gridPrecision = GridPrecision.High; }
            else { gridPrecision = GridPrecision.Low; }
            currentGridPrecision = GridPrecisionMap[gridPrecision];
            player.Message(ExtraSnapPointsMadeEasy.NotificationType.Value, $"Grid Precision: {currentGridPrecision}");
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
            if (ExtraSnapPointsMadeEasy.ResetSnapsOnNewPiece.Value || currentTargetSnap < 0)
            {
                currentTargetSnap = 0;
            }
            currentTargetParent = targetPiece.transform;
        }

        if (currentSourceParent != sourcePiece.transform)
        {
            if (ExtraSnapPointsMadeEasy.ResetSnapsOnNewPiece.Value || currentSourceSnap < 0)
            {
                currentSourceSnap = 0;
            }

            currentSourceParent = sourcePiece.transform;
        }

        int prevSourceSnap = currentSourceSnap;
        if (Input.GetKeyDown(ExtraSnapPointsMadeEasy.IterateSourceSnapPoints.Value))
        {
            currentSourceSnap++;
        }

        int prevTargetSnap = currentTargetSnap;
        if (Input.GetKeyDown(ExtraSnapPointsMadeEasy.IterateTargetSnapPoints.Value))
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

        if (prevSourceSnap != currentSourceSnap)
        {
            player.Message(ExtraSnapPointsMadeEasy.NotificationType.Value, $"Source Snap Point: {currentSourceSnap}");
        }

        if (prevTargetSnap != currentTargetSnap && snapMode == SnapMode.Manual)
        {
            player.Message(ExtraSnapPointsMadeEasy.NotificationType.Value, $"Target Snap Point: {currentTargetSnap}");
        }

        Transform sourceSnap = TempSourceSnapPoints[currentSourceSnap];
        Transform targetSnap;
        switch (snapMode)
        {
            case SnapMode.Manual:
                targetSnap = TempTargetSnapPoints[currentTargetSnap];
                break;

            case SnapMode.ManualClosest:
                if (player.m_placementMarkerInstance == null) { return; }
                Vector3 markerPosition = player.m_placementMarkerInstance.transform.position;
                targetSnap = TempTargetSnapPoints.OrderBy(snapPoint => Vector3.Distance(markerPosition, snapPoint.position)).First();
                break;

            default:
                return;
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
