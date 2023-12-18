using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using ExtraSnapPointsMadeEasy.Extensions;

namespace ExtraSnapPointsMadeEasy.Helpers {
    internal class ExtraSnapsManager {
        private static readonly HashSet<string> DoNotAddSnapPoints = new()
        {
            "piece_dvergr_spiralstair",
            "piece_dvergr_spiralstair_right",
        };

        /// <summary>
        ///     List of prefabs that extra snap points have been added to.
        /// </summary>
        private static readonly List<GameObject> AlteredPrefabs = new();

        internal static void AddExtraSnapPoints(string msg, bool forceUpdate = false) {
            // Avoid updating before world and prefabs are loaded.
            if (!ZNetScene.instance || SceneManager.GetActiveScene().name != "main") {
                return;
            }

            // Only update if needed.
            if (!ExtraSnapPointsMadeEasy.UpdateExtraSnapPoints && !forceUpdate) {
                return;
            }

            var watch = new System.Diagnostics.Stopwatch();
            if (Log.IsVerbosityMedium) {
                watch.Start();
            }

            Log.LogInfo(msg);
            var prefabPieces = FindPrefabPieces();

            if (AlteredPrefabs.Count > 0) {
                RemoveExtraSnapPoints(AlteredPrefabs);
                AlteredPrefabs.Clear();
            }

            foreach (var prefab in prefabPieces) {
                try {
                    if (AddSnapPoints(prefab)) {
                        AlteredPrefabs.Add(prefab);
                    }
                }
                catch (Exception e) {
                    Log.LogWarning($"Failed to add snappoints to {prefab}: {e}");
                }
            }

            if (Log.IsVerbosityMedium) {
                watch.Stop();
                Log.LogInfo($"Time to add snap points: {watch.ElapsedMilliseconds} ms");
            }
            else {
                Log.LogInfo("Adding snap points complete");
            }

            ExtraSnapPointsMadeEasy.UpdateExtraSnapPoints = false;
        }

        /// <summary>
        ///      Iterate over piece tables to get all existing prefabPieces
        /// </summary>
        /// <returns></returns>
        internal static List<GameObject> FindPrefabPieces() {
            return Resources.FindObjectsOfTypeAll<PieceTable>()
                .SelectMany(pieceTable => pieceTable.m_pieces)
                .Where(piecePrefab => !SkipPrefab(piecePrefab))
                .ToList();
        }

        internal static void RemoveExtraSnapPoints(List<GameObject> prefabs) {
            foreach (var prefab in prefabs) {
                // destroy all extra snap points on prefab
                var snapPoints = prefab.GetSnapPoints();

                for (int i = 0; i < snapPoints.Count; i++) {
                    if (snapPoints[i].name == SnapPointExtensions.SnapPointName) {
                        GameObject.DestroyImmediate(snapPoints[i].gameObject);
                    }
                }
            }
        }

        private static bool SkipPrefab(GameObject prefab) {
            if (DoNotAddSnapPoints.Contains(prefab.name)) {
                return true;
            }

            // Customs filters
            if (prefab.name.StartsWith("_") ||
                prefab.name.StartsWith("OLD_") ||
                prefab.name.EndsWith("OLD") ||
                prefab.name.StartsWith("vfx_") ||
                prefab.name.StartsWith("sfx_") ||
                prefab.name.StartsWith("fx_") ||
                prefab.GetComponent<Projectile>() ||
                prefab.GetComponent<Humanoid>() ||
                prefab.GetComponent<AnimalAI>() ||
                prefab.GetComponent<Character>() ||
                prefab.GetComponent<CreatureSpawner>() ||
                prefab.GetComponent<SpawnArea>() ||
                prefab.GetComponent<Fish>() ||
                prefab.GetComponent<RandomFlyingBird>() ||
                prefab.GetComponent<MusicLocation>() ||
                prefab.GetComponent<Aoe>() ||
                prefab.GetComponent<ItemDrop>() ||
                prefab.GetComponent<DungeonGenerator>() ||
                prefab.GetComponent<TerrainModifier>() ||
                prefab.GetComponent<EventZone>() ||
                prefab.GetComponent<LocationProxy>() ||
                prefab.GetComponent<LootSpawner>() ||
                prefab.GetComponent<Mister>() ||
                prefab.GetComponent<Ragdoll>() ||
                prefab.GetComponent<MineRock5>() ||
                prefab.GetComponent<TombStone>() ||
                prefab.GetComponent<LiquidVolume>() ||
                prefab.GetComponent<Gibber>() ||
                prefab.GetComponent<TimedDestruction>() ||
                prefab.GetComponent<ShipConstructor>() ||
                prefab.GetComponent<TriggerSpawner>() ||
                prefab.GetComponentInChildren<Ship>() || // ignore ships
                prefab.GetComponentInChildren<Vagon>() || // ignore carts
                (prefab.TryGetComponent(out Piece piece) && piece.m_repairPiece)) {
                return true;
            }

            return false;
        }

        private static bool AddSnapPoints(GameObject prefab) {
            if (!prefab) {
                return false;
            }

            var prefabConfig = ExtraSnapPointsMadeEasy.LoadConfig(prefab);
            if (!prefabConfig.Value || !ExtraSnapPointsMadeEasy.EnableExtraSnapPoints.Value) {
                return false; // skip adding snap points if not enabled
            }

            switch (prefab.name) {
                /* Fences */
                case "wood_fence":
                    prefab.AddSnapPoints(
                       new[]
                       {
                            new Vector3(+1f, -0.2f, 0f),
                            new Vector3(-1f, -0.2f, 0f),
                            new Vector3(1f, 0f, 0f),
                            new Vector3(-1f, 0f, 0f),
                            new Vector3(1f, 0.2f, 0f),
                            new Vector3(-1f, 0.2f, 0f),
                            new Vector3(1f, 0.4f, 0f),
                            new Vector3(-1f, 0.4f, 0f),
                            new Vector3(1f, 0.6f, 0f),
                            new Vector3(-1f, 0.6f, 0f),
                            new Vector3(1f, 0.8f, 0f),
                            new Vector3(-1f, 0.8f, 0f),
                            new Vector3(1f, 1f, 0f),
                            new Vector3(-1f, 1f, 0f),
                       },
                       false,
                       true
                    );
                    break;

                case "piece_sharpstakes":
                    prefab.AddSnapPoints(
                        new[]
                        {
                            new Vector3(1.12f, -0.2f, 0f),
                            new Vector3(-1.12f, -0.2f, 0f),
                            new Vector3(1.12f, 0f, 0f),
                            new Vector3(-1.12f, 0f, 0f),
                            new Vector3(1.12f, 0.2f, 0f),
                            new Vector3(-1.12f, 0.2f, 0f),
                        },
                        false,
                        false);
                    break;

                case "piece_dvergr_sharpstakes":
                    prefab.AddSnapPoints(
                        new[] {
                            new Vector3(-0.5f, -0.5f, 2f),
                            new Vector3(-0.5f, -0.5f, -2f),
                            new Vector3(-0.5f, 0f, 2f),
                            new Vector3(-0.5f, 0f, -2f),
                            new Vector3(1f, 1f, 2f),
                            new Vector3(1f, 1f, -2f),
                        },
                        false,
                        false
                    );
                    break;

                /* Item Stands */
                case "itemstandh": // itemstandh (horizontal)
                    prefab.AddSnapPoints(
                        new[] {
                            new Vector3 (0f, -0.0346f, 0f),
                            new Vector3 (0.1f, -0.0346f, 0f),
                            new Vector3 (-0.1f, -0.0346f, 0f),
                            new Vector3 (0.0f, -0.0346f, 0.1f),
                            new Vector3 (0.0f, -0.0346f, -0.1f),
                        }
                    );
                    break;

                case "itemstand":  // itemstand (vertical)
                    prefab.AddSnapPoints(
                        new[] {
                            new Vector3 (0f, 0f, -0.06f),
                            new Vector3 (0.22f, 0f, -0.06f),
                            new Vector3 (-0.22f, 0f, -0.06f),
                            new Vector3 (0.0f, 0.22f, -0.06f),
                            new Vector3 (0.0f, -0.22f, -0.06f),
                        }
                     );
                    break;

                /* Chests */
                case "piece_chest_wood":
                    prefab.AddSnapPoints(
                       new[] {
                            new Vector3(0.0f, -0.01f, 0.0f),
                            new Vector3(0.8f, -0.01f, 0.37f),
                            new Vector3(0.8f, -0.01f, -0.37f),
                            new Vector3(-0.8f, -0.01f, 0.37f),
                            new Vector3(-0.8f, -0.01f, -0.37f),
                            new Vector3(0.65f, 0.8f, 0.35f),
                            new Vector3(0.65f, 0.8f, -0.35f),
                            new Vector3(-0.65f, 0.8f, 0.35f),
                            new Vector3(-0.65f, 0.8f, -0.35f)
                       }
                    );
                    break;

                case "piece_chest": // (Reinforced Chest)
                    prefab.AddSnapPoints(
                        new[] {
                            new Vector3(0.0f, -0.01f, 0.0f),
                            new Vector3(0.9f, -0.01f, 0.47f),
                            new Vector3(0.9f, -0.01f, -0.47f),
                            new Vector3(-0.9f, -0.01f, 0.47f),
                            new Vector3(-0.9f, -0.01f, -0.47f),
                            new Vector3(0.7f, 0.99f, 0.47f),
                            new Vector3(0.7f, 0.99f, -0.47f),
                            new Vector3(-0.7f, 0.99f, 0.47f),
                            new Vector3(-0.7f, 0.99f, -0.47f)
                        }
                    );
                    break;

                case "piece_chest_private":
                    prefab.AddSnapPoints(
                        new[] {
                            new Vector3(0.0f, -0.01f, 0.0f),
                            new Vector3(0.45f, -0.01f, 0.25f),
                            new Vector3(0.45f, -0.01f, -0.25f),
                            new Vector3(-0.45f, -0.01f, 0.25f),
                            new Vector3(-0.45f, -0.01f, -0.25f),
                            new Vector3(0.36f, 0.55f, 0.23f),
                            new Vector3(0.36f, 0.55f, -0.23f),
                            new Vector3(-0.36f, 0.55f, 0.23f),
                            new Vector3(-0.36f, 0.55f, -0.23f)
                        }
                    );
                    break;

                case "piece_chest_blackmetal":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(1.0f, 0.0f, 0.5f),
                            new Vector3(1.0f, 0.0f, -0.5f),
                            new Vector3(-1.0f, 0.0f, 0.5f),
                            new Vector3(-1.0f, 0.0f, -0.5f),
                            new Vector3(0.85f, 1.0f, 0.5f),
                            new Vector3(0.85f, 1.0f, -0.5f),
                            new Vector3(-0.85f, 1.0f, 0.5f),
                            new Vector3(-0.85f, 1.0f, -0.5f)
                        }
                    );
                    break;

                /* Torches */
                case "piece_walltorch":  // (sconce)
                    prefab.AddSnapPoints(
                        new[] {
                            new Vector3(-0.2f, 0.0f, 0.0f), // black marble snap
                            new Vector3(-0.25f, 0.0f, 0.0f), // stone snap
                            new Vector3(-0.35f, 0.0f, 0.0f),  // wood snap
                            // Vector3.zero,
                        }
                    );
                    break;

                case "piece_dvergr_lantern_pole":
                    prefab.AddSnapPoints(new[] { Vector3.zero });
                    break;

                /* Furniture */
                case "sign":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(0.0f, 0.0f, -0.05f), // marble & stone
                            new Vector3(0.0f, 0.0f, -0.20f), // wood
                        }
                    );
                    break;

                case "ArmorStand":
                    prefab.AddSnapPoints(
                        new[] {
                            new Vector3(0.0f, -0.1f, 0.0f),
                            new Vector3(0.5f, -0.1f, 0.0f),
                            new Vector3(-0.5f, -0.1f, 0.0f),
                            new Vector3(0.0f, -0.1f, 0.5f),
                            new Vector3(0.0f, -0.1f, -0.5f),
                        }
                    );
                    break;

                /* Rugs & Carpets */
                case "jute_carpet": // (red jute carpet)
                    prefab.AddSnapPoints(
                        new[] {
                            new Vector3(0.0f, -0.01f, 0.0f),
                            new Vector3(2.0f, -0.01f, -1.25f),
                            new Vector3(2.0f, -0.01f, 1.25f),
                            new Vector3(-2.0f, -0.01f, -1.25f),
                            new Vector3(-2.0f, -0.01f, 1.25f),
                        }
                    );
                    break;

                case "rug_fur": // (lox rug)
                    prefab.AddSnapPoints(
                        new[] {
                            new Vector3(0.0f, -0.01f, 0.0f),
                            new Vector3(1.25f, -0.01f, -2.0f),
                            new Vector3(1.25f, -0.01f, 2.0f),
                            new Vector3(-1.25f, -0.01f, -2.0f),
                            new Vector3(-1.25f, -0.01f, 2.0f),
                        }
                    );
                    break;

                case "rug_deer":
                case "rug_wolf":
                case "rug_hare": // (scale rug)
                case "jute_carpet_blue": // (round blue jute carpet)
                    prefab.AddSnapPoints(new[] { new Vector3(0.0f, -0.01f, 0.0f) });
                    break;

                /* Thrones, Chairs, Benches */
                case "piece_throne01": // (Raven Throne)
                case "piece_throne02": // (Stone Throne)
                case "piece_blackmarble_throne":
                case "piece_chair": // (stool)
                case "piece_chair02": // (finewood chair)
                case "piece_chair03": // (darkwood chair)
                    prefab.AddLocalCenterSnapPoint();
                    break;

                case "piece_bench01":
                case "piece_blackmarble_bench":
                case "piece_logbench01": // sitting log
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                /* Banners */
                // Banners are about 1.25m wide up top
                case "piece_banner01": // (black)
                case "piece_banner02": // (blue)
                case "piece_banner03": // (white & red)
                case "piece_banner04": // (red)
                case "piece_banner05": // (green)
                case "piece_banner06": // (white, blue, red)
                case "piece_banner07": // (white & blue)
                case "piece_banner08": // (yellow)
                case "piece_banner09": // (purple)
                case "piece_banner10": // (orange)
                case "piece_banner11": // (white)
                case "piece_cloth_hanging_door": // (red jute curtain)
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(0.136f, 0.0f, 0.0f), // stone walls
                            new Vector3(-0.136f, 0.0f, 0.0f), // stone walls
                            new Vector3(0.236f, 0.0f, 0.0f), // wood walls
                            new Vector3(-0.236f, 0.0f, 0.0f), // wood walls
                        }
                    );
                    break;

                /* Blue Jute Hangings */
                case "piece_cloth_hanging_door_blue2":  // (Blue Jute Curtain)
                    prefab.AddSnapPoints(
                        new[] {
                            new Vector3(0.0f, 1.5f, 0.0f),
                            new Vector3(0.0f, 1.5f, 2.0f),
                            new Vector3(0.0f, 1.5f, -2.0f),
                            new Vector3(0.14f * 2, 1.5f, 2.0f), // stone walls
                            new Vector3(0.14f * 2, 1.5f, -2.0f), // stone walls
                            new Vector3(-0.14f * 2, 1.5f, -2.0f), // stone walls
                            new Vector3(-0.14f * 2, 1.5f, 2.0f), // stone walls
                            new Vector3(0.236f * 2, 1.5f, 2.0f), // wood walls
                            new Vector3(0.236f * 2, 1.5f, -2.0f), // wood walls
                            new Vector3(-0.236f * 2, 1.5f, -2.0f), // wood walls
                            new Vector3(-0.236f * 2, 1.5f, 2.0f), // wood walls
                        }
                    );
                    break;

                case "piece_cloth_hanging_door_blue": // (Blue Jute Drape)
                    prefab.AddSnapPoints(
                        new[] {
                            new Vector3(0.0f, 4.0f, 0.0f), // center
                            new Vector3(0.16f, 4.0f, 0.0f),
                            new Vector3(-0.16f, 4.0f, 0.0f),
                            new Vector3(0.36f, 4.0f, 0.0f),
                            new Vector3(-0.36f, 4.0f, 0.0f),
                        }
                    );
                    break;

                /* Workbench */
                case "piece_workbench":
                case "piece_workbench_ext1": // (Chopping block)
                    prefab.AddLocalCenterSnapPoint();
                    break;

                case "piece_workbench_ext2": // (Tanning rack)
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(1.0f, 0.0f, 0.0f),
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, -1.0f),
                            new Vector3(-1.0f, 0.0f, -1.0f),
                        }
                    );
                    break;

                case "piece_workbench_ext3": // (Adze)
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(1.0f, 0.0f, 0.0f),
                            new Vector3(-1.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                case "piece_workbench_ext4":  // (Tool shelf)
                    prefab.AddSnapPoints(
                        new[] {
                            new Vector3(0.0f, 0.0f, -0.1f),
                            new Vector3(1.0f, 0.0f, -0.1f),
                            new Vector3(-1.0f, 0.0f, -0.1f),
                            new Vector3(1.0f, 1.0f, -0.1f),
                            new Vector3(-1.0f, 1.0f, -0.1f),
                        }
                    );
                    break;

                /* Forge */
                case "forge": // (Forge)
                case "forge_ext1": // (Forge bellows)
                case "forge_ext2": // (Anvils)
                case "forge_ext3": // (Grinding wheel)
                case "forge_ext4": // (Smith's anvil)
                case "forge_ext5": // (Forge cooler)
                case "forge_ext6": // (Forge toolrack)
                    prefab.AddLocalCenterSnapPoint();
                    break;

                /* Black Forge */
                case "blackforge": // galdr table
                case "blackforge_ext1": // cooler
                case "blackforge_ext2_vise": // vice
                    prefab.AddLocalCenterSnapPoint();
                    break;

                /* Galdr Table */
                case "piece_magetable": // galdr table
                case "piece_magetable_ext": // rune table
                case "piece_magetable_ext2": // candles
                    prefab.AddLocalCenterSnapPoint();
                    break;

                /* Cooking Pieces */
                case "piece_cauldron":
                case "cauldron_ext5_mortarandpestle":
                case "fermenter":
                    prefab.AddLocalCenterSnapPoint();
                    break;

                case "cauldron_ext1_spice":
                    prefab.AddSnapPoints(new[] { new Vector3(0.0f, 1.25f, 0.0f) });
                    break;

                case "cauldron_ext3_butchertable":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(0.5f, 0.0f, -0.5f),
                            new Vector3(0.5f, 0.0f, 0.5f),
                            new Vector3(-0.5f, 0.0f, -0.5f),
                            new Vector3(-0.5f, 0.0f, 0.5f),
                        }
                    );
                    break;

                case "cauldron_ext4_pots":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 1.0f, 0.0f),
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(-1.0f, 1.0f, 0.0f),
                        }
                    );
                    break;

                case "piece_cookingstation":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                case "piece_cookingstation_iron":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(-2.0f, 0.0f, 0.0f),
                            new Vector3(2.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                /* Fires */
                case "hearth": // already has snappoints but not a center one
                    prefab.AddLocalCenterSnapPoint();
                    break;

                /* Beams & Poles */
                // Core wood log walls have 4 snap points by default
                case "wood_wall_log": // core wood beam 2m
                case "wood_wall_log_4x0.5": // core wood beam 4m
                    prefab.AddLocalCenterSnapPoint();
                    break;

                /* Beds */
                case "bed":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(0.5f, 0.0f, -1.5f),
                            new Vector3(0.5f, 0.0f, 1.5f),
                            new Vector3(-0.5f, 0.0f, -1.5f),
                            new Vector3(-0.5f, 0.0f, 1.5f),
                        }
                    );
                    break;

                case "piece_bed02":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(1.0f, 0.0f, -1.5f),
                            new Vector3(1.0f, 0.0f, 1.5f),
                            new Vector3(-1.0f, 0.0f, -1.5f),
                            new Vector3(-1.0f, 0.0f, 1.5f),
                        }
                    );
                    break;

                /* Tables */
                case "piece_table":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                case "piece_blackmarble_table":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                case "piece_table_round": // round table
                    prefab.AddLocalCenterSnapPoint();
                    break;

                case "piece_table_oak": // long heavy table
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(-2.0f, 0.0f, 0.0f),
                            new Vector3(2.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                /* Misc */
                case "piece_bathtub": // has snap points but adding center
                    prefab.AddLocalCenterSnapPoint();
                    break;

                // TODO: add snaps to these
                case "piece_cartographytable":
                case "piece_spinningwheel":
                case "piece_stonecutter":
                case "piece_artisanstation":
                    prefab.AddLocalCenterSnapPoint();
                    break;

                case "piece_barber":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(1.0f, 0.0f, -0.75f),
                            new Vector3(1.0f, 0.0f, 0.75f),
                            new Vector3(-1.0f, 0.0f, -0.75f),
                            new Vector3(-1.0f, 0.0f, 0.75f),
                        }
                    );
                    break;

                case "piece_wisplure": // wisp fountain
                    prefab.AddSnapPoints(new[] { new Vector3(0.0f, -0.05f, 0.0f) });
                    break;

                case "eitrrefinery":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(2.75f, 0.0f, -1.0f),
                            new Vector3(2.75f, 0.0f, 1.0f),
                            new Vector3(-2.75f, 0.0f, -1.0f),
                            new Vector3(-2.75f, 0.0f, 1.0f),
                        }
                    );
                    break;

                case "windmill":
                    prefab.AddSnapPoints(
                        new[] {
                            new Vector3(0.0f, -0.005f, 0.0f),
                            new Vector3(2.0f, -0.005f, -2.0f),
                            new Vector3(2.0f, -0.005f, 2.0f),
                            new Vector3(-2.0f, -0.005f, -2.0f),
                            new Vector3(-2.0f, -0.005f, 2.0f),
                        }
                    );
                    break;

                case "smelter":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(1.0f, 0.0f, -1.0f),
                            new Vector3(1.0f, 0.0f, 1.0f),
                            new Vector3(-1.0f, 0.0f, -1.0f),
                            new Vector3(-1.0f, 0.0f, 1.0f),
                        }
                    );
                    break;

                case "blastfurnace":
                    prefab.AddSnapPoints(
                        new[] {
                            Vector3.zero,
                            new Vector3(2.0f, 0.0f, -1.25f),
                            new Vector3(2.0f, 0.0f, 1.25f),
                            new Vector3(-1.75f, 0.0f, -1.25f),
                            new Vector3(-1.75f, 0.0f, 1.25f),
                        }
                    );
                    break;

                default:
                    if (ShapeClassifier.IsPoint(prefab) || ShapeClassifier.IsCross(prefab)) {
                        return false;
                    }

                    if (!ExtraSnapPointsMadeEasy.EnableTerrainOpSnapPoints.Value && prefab.GetComponent<TerrainOp>()) {
                        return false;
                    }

                    if (ShapeClassifier.IsCeilingBrazier(prefab) && prefab.HasNoSnapPoints()) {
                        prefab.AddSnapPoints( // (Hanging Brazier)
                            new[] {
                                new Vector3(0.0f, 2.0f, 0.0f),
                                new Vector3(0.0f, 1.5f, 0.0f),
                            }
                        );
                    }
                    else if (ShapeClassifier.IsFloorBrazier(prefab) && prefab.HasNoSnapPoints()) {
                        // standing brazier, blue standing brazier, mountainkit, etc.
                        prefab.AddSnapPoints(
                            new[] {
                                new Vector3(0.0f, -1.0f, 0.0f),
                                Vector3.zero,
                           }
                        );
                    }
                    else if (ShapeClassifier.IsTorch(prefab) && prefab.HasNoSnapPoints()) {
                        // piece_groundtorch_wood, piece_groundtorch, piece_groundtorch_green,
                        // piece_groundtorch_blue, piece_groundtorch_mist, etc.
                        prefab.AddSnapPoints(
                            new[] {
                                Vector3.zero,
                                new Vector3(0.0f, -0.7f, 0.0f),
                            }
                        );
                    }
                    else if (ShapeClassifier.IsLine(prefab) && ExtraSnapPointsMadeEasy.EnableLineSnapPoints.Value) {
                        ShapeClassifier.AddSnapPointToLine(prefab);
                    }
                    else if (ShapeClassifier.IsTriangle(prefab) && ExtraSnapPointsMadeEasy.EnableTriangleSnapPoints.Value) {
                        ShapeClassifier.AddSnapPointsToTriangle(prefab);
                    }
                    else if (ShapeClassifier.IsRect2D(prefab) && ExtraSnapPointsMadeEasy.EnableRect2DSnapPoints.Value) {
                        ShapeClassifier.AddSnapPointsToRect2D(prefab);
                    }
                    else if (ShapeClassifier.IsRoofTop(prefab) && ExtraSnapPointsMadeEasy.EnableRoofTopSnapPoints.Value) {
                        ShapeClassifier.AddSnapPointsToRoofTop(prefab);
                    }
                    else {
                        prefab.AddLocalCenterSnapPoint();
                    }

                    break;
            }

            return true;
        }
    }
}
