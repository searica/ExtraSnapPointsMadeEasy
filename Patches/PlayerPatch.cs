using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ExtraSnapPointsMadeEasy.Helpers;
using ExtraSnapPointsMadeEasy.Configs;

namespace ExtraSnapPointsMadeEasy.Patches
{
    [HarmonyPatch(typeof(Player))]
    internal class PlayerPatch
    {
        private static readonly int TerrainRayMask = LayerMask.GetMask("terrain");
        private static int currentSourceSnap = 0;
        private static int currentTargetSnap = 0;

        private static Transform currentTargetParent;
        private static Transform currentSourceParent;

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

        [HarmonyReversePatch]
        [HarmonyPatch(nameof(Player.PieceRayTest))]
        public static bool Call_PieceRayTest(
            object instance,
            out Vector3 point,
            out Vector3 normal,
            out Piece piece,
            out Heightmap heightmap,
            out Collider waterSurface,
            bool water
        )
        => throw new NotImplementedException();

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.UpdatePlacementGhost))]
        private static void UpdatePlacementGhostPostfix(Player __instance)
        {
            SnapMode prevSnapMode = snapMode;

            if (Input.GetKeyDown(ConfigManager.EnableManualSnap.Value))
            {
                if (snapMode == SnapMode.Manual) { snapMode = SnapMode.Auto; }
                else { snapMode = SnapMode.Manual; }
            }
            else if (Input.GetKeyDown(ConfigManager.EnableManualClosestSnap.Value))
            {
                if (snapMode == SnapMode.ManualClosest) { snapMode = SnapMode.Auto; }
                else { snapMode = SnapMode.ManualClosest; }
            }
            else if (Input.GetKeyDown(ConfigManager.EnableGridSnap.Value))
            {
                if (snapMode == SnapMode.Grid) { snapMode = SnapMode.Auto; }
                else { snapMode = SnapMode.Grid; }
            }

            if (snapMode != prevSnapMode)
            {
                __instance.Message(ConfigManager.NotificationType.Value, SnapModeMsg[snapMode]);
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
            var sourcePiece = player.m_placementGhost?.GetComponent<Piece>();
            if (sourcePiece == null) { return; }

            if (Input.GetKeyDown(ConfigManager.CycleGridPrecision.Value))
            {
                if (gridPrecision == GridPrecision.Low) { gridPrecision = GridPrecision.High; }
                else { gridPrecision = GridPrecision.Low; }
                currentGridPrecision = GridPrecisionMap[gridPrecision];
                player.Message(ConfigManager.NotificationType.Value, $"Grid Precision: {currentGridPrecision}");
            }

            var position = player.m_placementGhost.transform.position;
            position.x = RoundToNearest(position.x, currentGridPrecision);
            position.z = RoundToNearest(position.z, currentGridPrecision);

            // Snaps center of piece to the ground, which is not what I want
            //Log.LogInfo($"Pre ground snap {position}");
            //var groundHeight = GetGroundHeight(position);
            //if (position.y != groundHeight) { position.y = groundHeight; }
            //Log.LogInfo($"Post ground snap {position}");
            //if (Mathf.Abs(position.y - groundHeight) < 0.25f) { position.y = groundHeight; }

            player.m_placementGhost.transform.position = position;
        }

        /// <summary>
        ///     Round to nearest multiple of precision (midpoint rounds away from zero)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        private static float RoundToNearest(float x, float precision)
        {
            if (precision <= 0) { return x; }
            var sign = Mathf.Sign(x);

            var val = (int)Mathf.Abs(x * 1000f);
            var whole = val / 1000;
            var fraction = val % 1000;

            int midPoint = (int)(precision * 1000f / 2f);

            if (fraction < midPoint)
            {
                return sign * whole;
            }
            return sign * (whole + precision);
        }

        private static float GetGroundHeight(Vector3 p)
        {
            Vector3 origin = p;
            origin.y = 6000f;
            if (Physics.Raycast(origin, Vector3.down, out var hitInfo, 10000f, TerrainRayMask))
            {
                return hitInfo.point.y;
            }
            return p.y;
        }

        private static void SnapManually(ref Player player)
        {
            var sourcePiece = player.m_placementGhost?.GetComponent<Piece>();
            if (sourcePiece == null) { return; }

            var targetPiece = RayTest(player, player.m_placementGhost);
            if (targetPiece == null) { return; }

            if (currentTargetParent != targetPiece.transform)
            {
                if (ConfigManager.ResetSnapsOnNewPiece.Value || currentTargetSnap < 0)
                {
                    currentTargetSnap = 0;
                }
                currentTargetParent = targetPiece.transform;
            }

            if (currentSourceParent != sourcePiece.transform)
            {
                if (ConfigManager.ResetSnapsOnNewPiece.Value || currentSourceSnap < 0)
                {
                    currentSourceSnap = 0;
                }

                currentSourceParent = sourcePiece.transform;
            }

            int prevSourceSnap = currentSourceSnap;
            if (Input.GetKeyDown(ConfigManager.IterateSourceSnapPoints.Value))
            {
                currentSourceSnap++;
            }

            int prevTargetSnap = currentTargetSnap;
            if (Input.GetKeyDown(ConfigManager.IterateTargetSnapPoints.Value))
            {
                currentTargetSnap++;
            }

            var sourceSnapPoints = SnapPointHelper.GetSnapPoints(sourcePiece.transform);
            var targetSnapPoints = SnapPointHelper.GetSnapPoints(currentTargetParent);

            if (sourceSnapPoints.Count == 0 || targetSnapPoints.Count == 0)
            {
                return;
            }

            if (currentSourceSnap >= sourceSnapPoints.Count) { currentSourceSnap = 0; }
            if (currentTargetSnap >= targetSnapPoints.Count) { currentTargetSnap = 0; }

            if (prevSourceSnap != currentSourceSnap)
            {
                player.Message(ConfigManager.NotificationType.Value, $"Source Snap Point: {currentSourceSnap}");
            }

            if (prevTargetSnap != currentTargetSnap && snapMode == SnapMode.Manual)
            {
                player.Message(ConfigManager.NotificationType.Value, $"Target Snap Point: {currentTargetSnap}");
            }

            Transform sourceSnap = sourceSnapPoints[currentSourceSnap];
            Transform targetSnap;
            switch (snapMode)
            {
                case SnapMode.Manual:
                    targetSnap = targetSnapPoints[currentTargetSnap];
                    break;

                case SnapMode.ManualClosest:
                    if (player.m_placementMarkerInstance == null) { return; }
                    var markerPosition = player.m_placementMarkerInstance.transform.position;
                    targetSnap = targetSnapPoints.OrderBy(snapPoint => Vector3.Distance(markerPosition, snapPoint.position)).First();
                    break;

                default:
                    return;
            }

            // adjust placement ghost position based on the difference between sourceSnap and targetSnap
            player.m_placementGhost.transform.position += targetSnap.position - sourceSnap.position;
        }

        private static Piece RayTest(Player player, GameObject placementGhost)
        {
            var component1 = placementGhost.GetComponent<Piece>();
            var water = component1.m_waterPiece || component1.m_noInWater;
            Call_PieceRayTest(player, out Vector3 point, out Vector3 normal, out Piece piece, out Heightmap heightmap, out Collider waterSurface, water);
            return piece;
        }
    }
}