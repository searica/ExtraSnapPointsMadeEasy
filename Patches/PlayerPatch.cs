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
        private static int currentSourceSnap = 0;
        private static int currentTargetSnap = 0;

        private static Transform currentTargetParent;
        private static Transform currentSourceParent;

        internal enum SnapMode
        {
            Auto,
            Manual,
            ManualClosest
        }

        private static readonly Dictionary<SnapMode, string> SnapModeMsg = new()
            {
                {SnapMode.Auto,  "Snap Mode: Auto"},
                {SnapMode.Manual, "Snap Mode: Manual"},
                {SnapMode.ManualClosest, "Snap Mode: Manual (Closest)"}
            };

        internal static SnapMode snapMode;

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
            if (Input.GetKeyDown(Config.EnableManualSnap.Value))
            {
                if (snapMode == SnapMode.Manual)
                {
                    snapMode = SnapMode.Auto;
                }
                else
                {
                    snapMode = SnapMode.Manual;
                }
            }
            else if (Input.GetKeyDown(Config.EnableManualClosestSnap.Value))
            {
                if (snapMode == SnapMode.ManualClosest)
                {
                    snapMode = SnapMode.Auto;
                }
                else
                {
                    snapMode = SnapMode.ManualClosest;
                }
            }

            if (prevSnapMode != snapMode)
            {
                __instance.Message(Config.NotificationType.Value, SnapModeMsg[snapMode]);
            }

            if (__instance.m_placementGhost == null || snapMode == SnapMode.Auto)
            {
                return;
            }

            var sourcePiece = __instance.m_placementGhost.GetComponent<Piece>();
            if (sourcePiece == null)
            {
                return;
            }

            Piece targetPiece = RayTest(__instance, __instance.m_placementGhost);
            if (!targetPiece)
            {
                return;
            }

            if (currentTargetParent != targetPiece.transform)
            {
                if (Config.ResetSnapsOnNewPiece.Value || currentTargetSnap < 0)
                {
                    currentTargetSnap = 0;
                }

                currentTargetParent = targetPiece.transform;
            }

            if (currentSourceParent != sourcePiece.transform)
            {
                if (Config.ResetSnapsOnNewPiece.Value || currentSourceSnap < 0)
                {
                    currentSourceSnap = 0;
                }

                currentSourceParent = sourcePiece.transform;
            }

            int prevSourceSnap = currentSourceSnap;
            if (Input.GetKeyDown(Config.IterateSourceSnapPoints.Value))
            {
                currentSourceSnap++;
            }

            int prevTargetSnap = currentTargetSnap;
            if (Input.GetKeyDown(Config.IterateTargetSnapPoints.Value))
            {
                currentTargetSnap++;
            }

            var sourceSnapPoints = SnapPointHelper.GetSnapPoints(sourcePiece.transform);
            var targetSnapPoints = SnapPointHelper.GetSnapPoints(currentTargetParent);

            if (sourceSnapPoints.Count == 0 || targetSnapPoints.Count == 0)
            {
                return;
            }

            if (currentSourceSnap >= sourceSnapPoints.Count)
            {
                currentSourceSnap = 0;
            }

            if (currentTargetSnap >= targetSnapPoints.Count)
            {
                currentTargetSnap = 0;
            }

            if (prevSourceSnap != currentSourceSnap)
            {
                __instance.Message(Config.NotificationType.Value, $"Source Snap Point: {currentSourceSnap}");
            }

            if (prevTargetSnap != currentTargetSnap && snapMode == SnapMode.Manual)
            {
                __instance.Message(Config.NotificationType.Value, $"Target Snap Point: {currentTargetSnap}");
            }

            Transform a = sourceSnapPoints[currentSourceSnap];
            Transform b;
            switch (snapMode)
            {
                case SnapMode.Manual:
                    b = targetSnapPoints[currentTargetSnap];
                    break;

                case SnapMode.ManualClosest:
                    if (Player.m_localPlayer.m_placementMarkerInstance == null)
                    {
                        return;
                    }
                    b = targetSnapPoints.OrderBy(
                            (Transform snapPoint) => Vector3.Distance(Player.m_localPlayer.m_placementMarkerInstance.transform.position, snapPoint.position)
                        ).First();
                    break;

                default:
                    return;
            }

            // adjust placement ghost position based on the difference between a and b
            __instance.m_placementGhost.transform.position += b.position - a.position;
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