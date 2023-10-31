using System.Collections.Generic;
using System.Linq;
using ExtraSnapPointsMadeEasy.Configs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExtraSnapPointsMadeEasy.Helpers
{
    internal class SnapPointAdder
    {
        private static readonly HashSet<string> DoNotAddSnapPoints = new()
        {
            "piece_dvergr_spiralstair",
            "piece_dvergr_spiralstair_right",
        };

        /// <summary>
        ///     List of prefabs that extra snap points have been added to.
        /// </summary>
        private static readonly List<GameObject> AlteredPrefabs = new();

        /// <summary>
        ///     Default number of snap points for each prefab before adding extras.
        /// </summary>
        private static readonly Dictionary<string, int> DefaultNumSnapPoints = new();

        internal static void AddExtraSnapPoints(string msg, bool forceUpdate = false)
        {
            // Avoid updating before world and prefabs are loaded.
            if (!ZNetScene.instance || SceneManager.GetActiveScene() == null
                || SceneManager.GetActiveScene().name != "main")
            {
                return;
            }

            // Only update if needed.
            if (!Config.UpdateExtraSnapPoints && !forceUpdate)
            {
                return;
            }

            var watch = new System.Diagnostics.Stopwatch();
            if (Config.IsVerbosityMedium)
            {
                watch.Start();
            }

            Log.LogInfo(msg);
            var prefabPieces = FindPrefabPieces();
            InitSnapDefaults(prefabPieces);

            if (AlteredPrefabs.Count > 0)
            {
                RemoveExtraSnapPoints(AlteredPrefabs);
                AlteredPrefabs.Clear();
            }

            foreach (var prefab in prefabPieces)
            {
                if (AddSnapPoints(prefab))
                {
                    AlteredPrefabs.Add(prefab);
                }
            }

            if (Config.IsVerbosityMedium)
            {
                watch.Stop();
                Log.LogInfo($"Time to add snap points: {watch.ElapsedMilliseconds} ms");
            }
            else
            {
                Log.LogInfo("Adding snap points complete");
            }

            Config.UpdateExtraSnapPoints = false;
        }

        /// <summary>
        ///      Iterate over piece tables to get all existing prefabPieces
        /// </summary>
        /// <returns></returns>
        internal static List<GameObject> FindPrefabPieces()
        {
            return Resources.FindObjectsOfTypeAll<PieceTable>()
                .SelectMany(pieceTable => pieceTable.m_pieces)
                .Where(piecePrefab => !SkipPrefab(piecePrefab))
                .ToList();
        }

        internal static void InitSnapDefaults(List<GameObject> prefabs)
        {
            foreach (var prefab in prefabs)
            {
                if (!DefaultNumSnapPoints.ContainsKey(prefab.name))
                {
                    var snapPoints = SnapPointHelper.GetSnapPoints(prefab.transform);
                    DefaultNumSnapPoints.Add(prefab.name, snapPoints.Count);
                }
            }
        }

        internal static void RemoveExtraSnapPoints(List<GameObject> prefabs)
        {
            foreach (var prefab in prefabs)
            {
                if (DefaultNumSnapPoints.ContainsKey(prefab.name))
                {
                    // destroy all extra snap points on prefab
                    var defaultNum = DefaultNumSnapPoints[prefab.name];
                    var snapPoints = SnapPointHelper.GetSnapPoints(prefab.transform);

                    // iterate in reverse to avoid NRE
                    for (int i = snapPoints.Count - 1; i > defaultNum - 1; i--)
                    {
                        Object.DestroyImmediate(snapPoints[i].gameObject);
                    }
                }
            }
        }

        private static bool SkipPrefab(GameObject prefab)
        {
            if (DoNotAddSnapPoints.Contains(prefab.name))
            {
                return true;
            }
            // Customs filters
            if (prefab.GetComponent("Projectile") != null ||
                prefab.GetComponent("Humanoid") != null ||
                prefab.GetComponent("AnimalAI") != null ||
                prefab.GetComponent("Character") != null ||
                prefab.GetComponent("CreatureSpawner") != null ||
                prefab.GetComponent("SpawnArea") != null ||
                prefab.GetComponent("Fish") != null ||
                prefab.GetComponent("RandomFlyingBird") != null ||
                prefab.GetComponent("MusicLocation") != null ||
                prefab.GetComponent("Aoe") != null ||
                prefab.GetComponent("ItemDrop") != null ||
                prefab.GetComponent("DungeonGenerator") != null ||
                prefab.GetComponent("TerrainModifier") != null ||
                prefab.GetComponent("EventZone") != null ||
                prefab.GetComponent("LocationProxy") != null ||
                prefab.GetComponent("LootSpawner") != null ||
                prefab.GetComponent("Mister") != null ||
                prefab.GetComponent("Ragdoll") != null ||
                prefab.GetComponent("MineRock5") != null ||
                prefab.GetComponent("TombStone") != null ||
                prefab.GetComponent("LiquidVolume") != null ||
                prefab.GetComponent("Gibber") != null ||
                prefab.GetComponent("TimedDestruction") != null ||
                prefab.GetComponent("ShipConstructor") != null ||
                prefab.GetComponent("TriggerSpawner") != null ||
                prefab.GetComponent("TerrainOp") != null ||

                prefab.GetComponentInChildren<Ship>() != null || // ignore ships
                prefab.GetComponentInChildren<Vagon>() != null || // ignore carts
                prefab.GetComponent<Piece>().m_repairPiece == true ||

                prefab.name.StartsWith("_") ||
                prefab.name.StartsWith("OLD_") ||
                prefab.name.EndsWith("OLD") ||
                prefab.name.StartsWith("vfx_") ||
                prefab.name.StartsWith("sfx_") ||
                prefab.name.StartsWith("fx_")
            )
            {
                return true;
            }
            return false;
        }

        private static bool AddSnapPoints(GameObject prefab)
        {
            var prefabConfig = Config.LoadConfig(prefab);
            if (!prefabConfig.Value || Config.DisableExtraSnapPoints.Value)
            {
                return false; // skip adding snap points if not enabled
            }

            switch (prefab.name)
            {
                /* Fences */
                case "wood_fence":
                    SnapPointHelper.AddSnapPoints(
                       prefab,
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
                    SnapPointHelper.AddSnapPoints(
                        prefab,
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
                        false
                    );
                    break;

                case "piece_dvergr_sharpstakes":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
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
                // TODO: Change these snap points to reduce clipping
                case "itemstandh":
                    SnapPointHelper.AddSnapPoints(
                        prefab, // itemstandh (horizontal)
                        new[] {
                            new Vector3 (0f, 0f, 0f),
                            new Vector3 (0.1f, 0f, 0f),
                            new Vector3 (-0.1f, 0f, 0f),
                            new Vector3 (0.0f, 0f, 0.1f),
                            new Vector3 (0.0f, 0f, -0.1f),
                        }
                    );
                    break;

                case "itemstand":
                    SnapPointHelper.AddSnapPoints(
                        prefab, // itemstand (vertical)
                        new[] {
                            new Vector3 (0f, 0f, 0f),
                            new Vector3 (0.22f, 0f, 0f),
                            new Vector3 (-0.22f, 0f, 0f),
                            new Vector3 (0.0f, 0.22f, 0f),
                            new Vector3 (0.0f, -0.22f, 0f),
                        }
                     );
                    break;

                /* Chests */
                case "piece_chest_wood":
                    SnapPointHelper.AddSnapPoints(
                       prefab,
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

                case "piece_chest":
                    SnapPointHelper.AddSnapPoints(
                        prefab, // (Reinforced Chest)
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
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[]
                        {
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
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
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
                case "piece_walltorch":
                    SnapPointHelper.AddSnapPoints(
                        prefab, // (sconce)
                        new[] {
                            new Vector3(-0.2f, 0.0f, 0.0f), // black marble snap
                            new Vector3(-0.25f, 0.0f, 0.0f), // stone snap
                            new Vector3(-0.35f, 0.0f, 0.0f),  // wood snap
                            // new Vector3(0.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                case "piece_dvergr_lantern_pole":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            Vector3.zero,
                        }
                    );
                    break;

                /* Furniture */
                case "sign":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(0.0f, 0.0f, -0.05f), // marble & stone
                            new Vector3(0.0f, 0.0f, -0.20f), // wood
                        }
                    );
                    break;

                case "ArmorStand":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
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
                case "jute_carpet":
                    SnapPointHelper.AddSnapPoints(
                        prefab, // (red jute carpet)
                        new[] {
                            new Vector3(0.0f, -0.01f, 0.0f),
                            new Vector3(2.0f, -0.01f, -1.25f),
                            new Vector3(2.0f, -0.01f, 1.25f),
                            new Vector3(-2.0f, -0.01f, -1.25f),
                            new Vector3(-2.0f, -0.01f, 1.25f),
                        }
                    );
                    break;

                case "rug_fur":
                    SnapPointHelper.AddSnapPoints(
                        prefab, // (lox rug)
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
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, -0.01f, 0.0f),
                        }
                    );
                    break;

                /* Thrones, Chairs, Benches */
                case "piece_throne01": // (Raven Throne)
                case "piece_throne02": // (Stone Throne)
                case "piece_blackmarble_throne":
                case "piece_chair": // (stool)
                case "piece_chair02": // (finewood chair)
                case "piece_chair03": // (darkwood chair)
                    SnapPointHelper.AddLocalCenterSnapPoint(prefab);
                    break;

                case "piece_bench01":
                case "piece_blackmarble_bench":
                case "piece_logbench01": // sitting log
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
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
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(0.136f, 0.0f, 0.0f), // stone walls
                            new Vector3(-0.136f, 0.0f, 0.0f), // stone walls
                            new Vector3(0.236f, 0.0f, 0.0f), // wood walls
                            new Vector3(-0.236f, 0.0f, 0.0f), // wood walls
                        }
                    );
                    break;

                /* Blue Jute Hangings */
                case "piece_cloth_hanging_door_blue2":
                    SnapPointHelper.AddSnapPoints(
                        prefab, // (Blue Jute Curtain) TODO
                        new[] {
                            new Vector3(0.0f, 1.5f, 0.0f),
                            new Vector3(0.0f, 1.5f, 2.0f),
                            new Vector3(0.0f, 1.5f, -2.0f),
                            new Vector3(0.14f*2, 1.5f, 2.0f), // stone walls
                            new Vector3(0.14f*2, 1.5f, -2.0f), // stone walls
                            new Vector3(-0.14f*2, 1.5f, -2.0f), // stone walls
                            new Vector3(-0.14f*2, 1.5f, 2.0f), // stone walls
                            new Vector3(0.236f*2, 1.5f, 2.0f), // wood walls
                            new Vector3(0.236f*2, 1.5f, -2.0f), // wood walls
                            new Vector3(-0.236f*2, 1.5f, -2.0f), // wood walls
                            new Vector3(-0.236f*2, 1.5f, 2.0f), // wood walls
                        }
                    );
                    break;

                case "piece_cloth_hanging_door_blue":
                    SnapPointHelper.AddSnapPoints(
                        prefab, // (Blue Jute Drape)
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
                    SnapPointHelper.AddLocalCenterSnapPoint(prefab);
                    break;

                case "piece_workbench_ext2": // (Tanning rack)
                    SnapPointHelper.AddSnapPoints(
                       prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, -1.0f),
                            new Vector3(-1.0f, 0.0f, -1.0f),
                        }
                    );
                    break;

                case "piece_workbench_ext3": // (Adze)
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                            new Vector3(-1.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                case "piece_workbench_ext4":  // (Tool shelf)
                    SnapPointHelper.AddSnapPoints(
                        prefab,
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
                    SnapPointHelper.AddLocalCenterSnapPoint(prefab);
                    break;

                /* Black Forge */
                case "blackforge": // galdr table
                case "blackforge_ext1": // cooler
                case "blackforge_ext2_vise": // vice
                    SnapPointHelper.AddLocalCenterSnapPoint(prefab);
                    break;

                /* Galdr Table */
                case "piece_magetable": // galdr table
                case "piece_magetable_ext": // rune table
                case "piece_magetable_ext2": // candles
                    SnapPointHelper.AddLocalCenterSnapPoint(prefab);
                    break;

                /* Cooking Pieces */
                case "piece_cauldron":
                case "cauldron_ext5_mortarandpestle":
                case "fermenter":
                    SnapPointHelper.AddLocalCenterSnapPoint(prefab);
                    break;

                case "cauldron_ext1_spice":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            // new Vector3(0.0f, 0.75f, 0.0f),
                            new Vector3(0.0f, 1.25f, 0.0f),
                        }
                    );
                    break;

                case "cauldron_ext3_butchertable":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(0.5f, 0.0f, -0.5f),
                            new Vector3(0.5f, 0.0f, 0.5f),
                            new Vector3(-0.5f, 0.0f, -0.5f),
                            new Vector3(-0.5f, 0.0f, 0.5f),
                        }
                    );
                    break;

                case "cauldron_ext4_pots":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 1.0f, 0.0f),
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(-1.0f, 1.0f, 0.0f),
                        }
                    );
                    break;

                case "piece_cookingstation":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                case "piece_cookingstation_iron":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(-2.0f, 0.0f, 0.0f),
                            new Vector3(2.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                /* Fires */
                case "hearth": // already has snappoints but not a center one
                    SnapPointHelper.AddLocalCenterSnapPoint(prefab);
                    break;

                /* Beams & Poles */
                // Core wood log walls have 4 snap points by default
                case "wood_wall_log": // core wood beam 2m
                case "wood_wall_log_4x0.5": // core wood beam 4m
                    SnapPointHelper.AddLocalCenterSnapPoint(prefab);
                    break;

                /* Beds */
                case "bed":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(0.5f, 0.0f, -1.5f),
                            new Vector3(0.5f, 0.0f, 1.5f),
                            new Vector3(-0.5f, 0.0f, -1.5f),
                            new Vector3(-0.5f, 0.0f, 1.5f),
                        }
                    );
                    break;

                case "piece_bed02":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, -1.5f),
                            new Vector3(1.0f, 0.0f, 1.5f),
                            new Vector3(-1.0f, 0.0f, -1.5f),
                            new Vector3(-1.0f, 0.0f, 1.5f),
                        }
                    );
                    break;

                /* Tables */
                case "piece_table":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                case "piece_blackmarble_table":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                case "piece_table_round": // round table
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                case "piece_table_oak": // long heavy table
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(-2.0f, 0.0f, 0.0f),
                            new Vector3(2.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                /* Misc */
                case "piece_bathtub": // has snap points but adding center
                    SnapPointHelper.AddLocalCenterSnapPoint(prefab);
                    break;

                // TODO: add snaps to these
                case "piece_cartographytable":
                case "piece_spinningwheel":
                case "piece_stonecutter":
                case "piece_artisanstation":
                    SnapPointHelper.AddLocalCenterSnapPoint(prefab);
                    break;

                case "piece_barber":
                    SnapPointHelper.AddSnapPoints(
                        prefab, // wisp fountain
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, -0.75f),
                            new Vector3(1.0f, 0.0f, 0.75f),
                            new Vector3(-1.0f, 0.0f, -0.75f),
                            new Vector3(-1.0f, 0.0f, 0.75f),
                        }
                    );
                    break;

                case "piece_wisplure":
                    SnapPointHelper.AddSnapPoints(
                        prefab, // wisp fountain
                        new[] {
                            new Vector3(0.0f, -0.05f, 0.0f),
                        }
                    );
                    break;

                case "eitrrefinery":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(2.75f, 0.0f, -1.0f),
                            new Vector3(2.75f, 0.0f, 1.0f),
                            new Vector3(-2.75f, 0.0f, -1.0f),
                            new Vector3(-2.75f, 0.0f, 1.0f),
                        }
                    );
                    break;

                case "windmill":
                    SnapPointHelper.AddSnapPoints(
                       prefab,
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
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, -1.0f),
                            new Vector3(1.0f, 0.0f, 1.0f),
                            new Vector3(-1.0f, 0.0f, -1.0f),
                            new Vector3(-1.0f, 0.0f, 1.0f),
                        }
                    );
                    break;

                case "blastfurnace":
                    SnapPointHelper.AddSnapPoints(
                        prefab,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(2.0f, 0.0f, -1.25f),
                            new Vector3(2.0f, 0.0f, 1.25f),
                            new Vector3(-1.75f, 0.0f, -1.25f),
                            new Vector3(-1.75f, 0.0f, 1.25f),
                        }
                    );
                    break;

                default:
                    if (SnapPointHelper.IsPoint(prefab) || SnapPointHelper.IsCross(prefab))
                    {
                        return false;
                    }
                    if (SnapPointHelper.IsCeilingBrazier(prefab)
                        && SnapPointHelper.HasNoSnapPoints(prefab))
                    {
                        SnapPointHelper.AddSnapPoints( // (Hanging Brazier)
                            prefab,
                            new[] {
                                new Vector3(0.0f, 2.0f, 0.0f),
                                new Vector3(0.0f, 1.5f, 0.0f),
                            }
                        );
                    }
                    else if (SnapPointHelper.IsFloorBrazier(prefab)
                        && SnapPointHelper.HasNoSnapPoints(prefab))
                    {
                        // standing brazier, blue standing brazier, mountainkit, etc.
                        SnapPointHelper.AddSnapPoints(
                           prefab,
                           new[] {
                                new Vector3(0.0f, -1.0f, 0.0f),
                                new Vector3(0.0f, 0.0f, 0.0f),
                           }
                        );
                    }
                    else if (SnapPointHelper.IsTorch(prefab)
                        && SnapPointHelper.HasNoSnapPoints(prefab))
                    {
                        // piece_groundtorch_wood, piece_groundtorch, piece_groundtorch_green,
                        // piece_groundtorch_blue, piece_groundtorch_mist, etc.
                        SnapPointHelper.AddSnapPoints(
                            prefab,
                            new[] {
                                new Vector3(0.0f, 0.0f, 0.0f),
                                new Vector3(0.0f, -0.7f, 0.0f),
                            }
                        );
                    }
                    else if (SnapPointHelper.IsLine(prefab) && !Config.DisableLineSnapPoints.Value)
                    {
                        SnapPointHelper.AddSnapPointToLine(prefab);
                    }
                    else if (SnapPointHelper.IsTriangle(prefab) && !Config.DisableTriangleSnapPoints.Value)
                    {
                        SnapPointHelper.AddSnapPointsToTriangle(prefab);
                    }
                    else if (SnapPointHelper.IsRect2D(prefab) && !Config.DisableRect2DSnapPoints.Value)
                    {
                        SnapPointHelper.AddSnapPointsToRect2D(prefab);
                    }
                    else if (SnapPointHelper.IsRoofTop(prefab) && !Config.DisableRoofTopSnapPoints.Value)
                    {
                        SnapPointHelper.AddSnapPointsToRoofTop(prefab);
                    }
                    else
                    {
                        SnapPointHelper.AddLocalCenterSnapPoint(prefab);
                    }

                    break;
            }
            return true;
        }
    }
}