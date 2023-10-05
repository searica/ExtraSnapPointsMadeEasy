using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

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
            SnapMode prevSnapMode = PlayerPatch.snapMode;
            if (Input.GetKeyDown(PluginConfig.EnableManualSnap.Value))
            {
                if (PlayerPatch.snapMode == SnapMode.Manual)
                {
                    PlayerPatch.snapMode = SnapMode.Auto;
                }
                else
                {
                    PlayerPatch.snapMode = SnapMode.Manual;
                }
            }
            else if (Input.GetKeyDown(PluginConfig.EnableManualClosestSnap.Value))
            {
                if (PlayerPatch.snapMode == SnapMode.ManualClosest)
                {
                    PlayerPatch.snapMode = SnapMode.Auto;
                }
                else
                {
                    PlayerPatch.snapMode = SnapMode.ManualClosest;
                }
            }

            if (prevSnapMode != PlayerPatch.snapMode)
            {
                __instance.Message(MessageHud.MessageType.Center, SnapModeMsg[PlayerPatch.snapMode]);
            }

            if (__instance.m_placementGhost == null || PlayerPatch.snapMode == SnapMode.Auto)
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
                if (PluginConfig.ResetSnapsOnNewPiece.Value || currentTargetSnap < 0)
                {
                    currentTargetSnap = 0;
                }

                currentTargetParent = targetPiece.transform;
            }

            if (currentSourceParent != sourcePiece.transform)
            {
                if (PluginConfig.ResetSnapsOnNewPiece.Value || currentSourceSnap < 0)
                {
                    currentSourceSnap = 0;
                }

                currentSourceParent = sourcePiece.transform;
            }

            int prevSourceSnap = currentSourceSnap;
            if (Input.GetKeyDown(PluginConfig.IterateSourceSnapPoints.Value))
            {
                currentSourceSnap++;
            }

            int prevTargetSnap = currentTargetSnap;
            if (Input.GetKeyDown(PluginConfig.IterateTargetSnapPoints.Value))
            {
                currentTargetSnap++;
            }

            var sourceSnapPoints = GetSnapPoints(sourcePiece.transform);
            var targetSnapPoints = GetSnapPoints(currentTargetParent);

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
                __instance.Message(MessageHud.MessageType.Center, $"Source Snap Point: {currentSourceSnap}");
            }

            if (prevTargetSnap != currentTargetSnap && snapMode == SnapMode.Manual)
            {
                __instance.Message(MessageHud.MessageType.Center, $"Target Snap Point: {currentTargetSnap}");
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
            
            // adjust placement ghost position based on the differrence between a and b
            __instance.m_placementGhost.transform.position += b.position - a.position;
        }

        private static Piece RayTest(Player player, GameObject placementGhost)
        {
            var component1 = placementGhost.GetComponent<Piece>();
            var water = component1.m_waterPiece || component1.m_noInWater;
            Call_PieceRayTest(player, out Vector3 point, out Vector3 normal, out Piece piece, out Heightmap heightmap, out Collider waterSurface, water);
            return piece;
        }

        public static List<Transform> GetSnapPoints(Transform piece)
        {
            List<Transform> points = new();

            if (piece == null)
            {
                return points;
            }

            for (var index = 0; index < piece.childCount; ++index)
            {
                var child = piece.GetChild(index);
                if (child.CompareTag("snappoint"))
                {
                    points.Add(child);
                }
            }
            return points;
        }
    }
} 
