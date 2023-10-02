
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtraSnapPointsMadeEasy.Patches
{
    public class ManualSnap
    {
        [HarmonyPatch(typeof(Player))]
        public class HookPieceRayTest
        {
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(Player), "PieceRayTest")]
            public static bool call_PieceRayTest(object instance, out Vector3 point, out Vector3 normal,
                out Piece piece, out Heightmap heightmap, out Collider waterSurface, bool water) =>
                throw new NotImplementedException();
        }

        [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
        private static class UpdatePlacementGhost_Patch
        {
            private static bool modifiedPlacementToggled = false;
            private static bool source_snap_changed = false;
            private static bool target_snap_changed = false;
            private static int currentSourceSnap = 0;
            private static int currentTargetSnap = 0;
            
            private static Transform currentTargetParent;
            private static Transform currentSourceParent;

            private static Dictionary<bool,string> snap_mode_msg = new Dictionary<bool,string>()
            {
                {true,  "Snap Mode: Manual"},
                {false, "Snap Mode: Auto" }
            };

            private static void Postfix(Player __instance,
                GameObject ___m_placementGhost)
            {
                if (Input.GetKeyDown(PluginConfig.SnapSettings.EnableManualSnap.Value))
                {
                    modifiedPlacementToggled = !modifiedPlacementToggled;
                    __instance.Message(MessageHud.MessageType.Center, snap_mode_msg[modifiedPlacementToggled]);
                }

                if (___m_placementGhost == null)
                {
                    return;
                }

                var sourcePiece = ___m_placementGhost.GetComponent<Piece>();
                if (sourcePiece == null)
                {
                    return;
                }

                Piece targetPiece = RayTest(__instance, ___m_placementGhost);
                if (!targetPiece)
                {
                    return;
                }

                if (modifiedPlacementToggled)
                {
                    if (currentTargetParent != targetPiece.transform)
                    {
                        if (PluginConfig.SnapSettings.ResetSnapsOnNewPiece.Value || currentTargetSnap < 0)
                        {
                            currentTargetSnap = 0;
                        }

                        currentTargetParent = targetPiece.transform;
                    }

                    if (currentSourceParent != sourcePiece.transform)
                    {
                        if (PluginConfig.SnapSettings.ResetSnapsOnNewPiece.Value || currentSourceSnap < 0)
                        {
                            currentSourceSnap = 0;
                        }

                        currentSourceParent = sourcePiece.transform;
                    }

                    
                    if (Input.GetKeyDown(PluginConfig.SnapSettings.IterateSourceSnapPoints.Value))
                    {
                        currentSourceSnap++;
                        source_snap_changed = true;
                    }

                    if (Input.GetKeyDown(PluginConfig.SnapSettings.IterateTargetSnapPoints.Value))
                    {
                        currentTargetSnap++;
                        target_snap_changed = true;
                    }

                    var sourceSnapPoints = GetSnapPoints(sourcePiece.transform);
                    var destSnapPoints = GetSnapPoints(currentTargetParent);

                    if (currentSourceSnap >= sourceSnapPoints.Count)
                    {
                        currentSourceSnap = 0;
                    }

                    if (currentTargetSnap >= destSnapPoints.Count)
                    {
                        currentTargetSnap = 0;
                    }

                    if (source_snap_changed)
                    {
                        __instance.Message(MessageHud.MessageType.Center, $"Source Snap Point: {currentSourceSnap}");
                        source_snap_changed = false;
                    }

                    if (target_snap_changed)
                    {
                        __instance.Message(MessageHud.MessageType.Center, $"Target Snap Point: {currentTargetSnap}");
                        target_snap_changed = false;
                    }

                    var a = sourceSnapPoints[currentSourceSnap];
                    var b = destSnapPoints[currentTargetSnap];

                    var p = b.position - (a.position - ___m_placementGhost.transform.position);
                    ___m_placementGhost.transform.position = p;
                }
            }

            private static Piece RayTest(Player player, GameObject placementGhost)
            {
                var component1 = placementGhost.GetComponent<Piece>();
                var water = component1.m_waterPiece || component1.m_noInWater;
                HookPieceRayTest.call_PieceRayTest(player, out Vector3 point, out Vector3 normal, out Piece piece, out Heightmap heightmap, out Collider waterSurface, water);
                return piece;
            }

            public static List<Transform> GetSnapPoints(Transform piece)
            {
                List<Transform> points = new List<Transform>();
                if (piece == null)
                {
                    return points;
                }

                bool skip_transform = false;
                for (var index = 0; index < piece.childCount; ++index)
                {
                    var child = piece.GetChild(index);
                    if (child.CompareTag("snappoint"))
                    {
                        if (child.localPosition == Vector3.zero)
                        {
                            skip_transform = true;
                        }
                        points.Add(child);
                    }
                }

                // avoid duplicating snap points or adding bad ones
                if (skip_transform || Plugin.SkipLocalCenterSnapPoint.Contains(piece.name)) {
                    return points;
                }

                points.Add(piece.transform);
                return points;
            }
        }
    }
}
