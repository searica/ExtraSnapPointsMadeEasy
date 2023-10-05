using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExtraSnapPointsMadeEasy
{
    internal class ExtraSnapPoints
    {
        // iterate over piece tables to get all existing pieces
        public static void AddExtraSnapPoints()
        {
            Log.LogInfo("AddExtraSnapPoints()");
            Resources.FindObjectsOfTypeAll<PieceTable>()
                .SelectMany(pieceTable => pieceTable.m_pieces)
                .ToList()
                .ForEach(AddSnapPoints);
        }

        private static bool SkipPrefab(GameObject prefab)
        {
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

                prefab.GetComponent<Piece>().m_repairPiece == true ||

                prefab.name.StartsWith("_") ||
                prefab.name.StartsWith("OLD_") ||
                prefab.name.EndsWith("_OLD") ||
                prefab.name.StartsWith("vfx_") ||
                prefab.name.StartsWith("sfx_") ||
                prefab.name.StartsWith("fx_")
            )
            {
                return true;
            }
            return false;
        }

        private static void AddSnapPoints(GameObject prefab)
        {
            // skip prefabs that are not build pieces
            if (SkipPrefab(prefab))
            {
                Log.LogInfo($"Skipping: {prefab.name}");
                return;
            }

            // Check config for this prefab
            ConfigEntry<bool> prefab_config = PluginConfig.BindConfig(
                "SnapPoints",
                prefab.name,
                true,
                new ConfigDescription(
                    "Set to True to enable snap points for this prefab.",
                    PluginConfig.AcceptableToggleValuesList
                )
            );

            if (!prefab_config.Value)
            {
                return; // skip adding snap points if not enabled
            }

            switch (prefab.name)
            {
                /* Fences */
                case "wood_fence":
                    SnapPointHelper.AddSnapPoints(
                       prefab.name,
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
                        prefab.name,
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
                        prefab.name,
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
                        prefab.name, // itemstandh (horizontal)
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
                        prefab.name, // itemstand (vertical)
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
                       "piece_chest_wood",
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
                        "piece_chest", // (Reinforced Chest)
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
                        prefab.name,
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
                        prefab.name,
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
                case "piece_groundtorch_wood":
                case "piece_groundtorch":
                case "piece_groundtorch_green":
                case "piece_groundtorch_blue":
                case "piece_groundtorch_mist":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(0.0f, -0.7f, 0.0f),
                        }
                    );
                    break;
                case "piece_walltorch":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name, // (sconce)
                        new[] {
                            new Vector3(-0.2f, 0.0f, 0.0f), // black marble snap
                            new Vector3(-0.25f, 0.0f, 0.0f), // stone snap
                            new Vector3(-0.35f, 0.0f, 0.0f),  // wood snap
                            // new Vector3(0.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                /* Furniture */
                case "sign":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(0.0f, 0.0f, -0.05f), // marble & stone
                            new Vector3(0.0f, 0.0f, -0.20f), // wood
                        }
                    );
                    break;
                case "ArmorStand":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
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
                        prefab.name, // (red jute carpet)
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
                        prefab.name, // (lox rug)
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
                        prefab.name,
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
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                        }
                    );
                    break;
                case "piece_bench01":
                case "piece_blackmarble_bench":
                case "piece_logbench01": // sitting log
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
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
                        prefab.name,
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
                        prefab.name, // (Blue Jute Curtain) TODO
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
                        prefab.name, // (Blue Jute Drape)
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
                    SnapPointHelper.AddCenterSnapPoint(prefab.name);
                    break;
                case "piece_workbench_ext2": // (Tanning rack)
                    SnapPointHelper.AddSnapPoints(
                       prefab.name,
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
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                            new Vector3(-1.0f, 0.0f, 0.0f),
                        }
                    );
                    break;
                case "piece_workbench_ext4":  // (Tool shelf)
                    // TODO: may need to switch the sign of the z component
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
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
                    SnapPointHelper.AddCenterSnapPoint(prefab.name);
                    break;

                /* Black Forge */
                case "blackforge": // galdr table
                case "blackforge_ext1": // cooler
                case "blackforge_ext2_vise": // vice
                    SnapPointHelper.AddCenterSnapPoint(prefab.name);
                    break;

                /* Galdr Table */
                case "piece_magetable": // galdr table
                case "piece_magetable_ext": // rune table
                case "piece_magetable_ext2": // candles
                    SnapPointHelper.AddCenterSnapPoint(prefab.name);
                    break;

                /* Cooking Pieces */
                case "piece_cauldron":
                case "cauldron_ext5_mortarandpestle":
                case "fermenter":
                    SnapPointHelper.AddCenterSnapPoint(prefab.name);
                    break;
                case "cauldron_ext1_spice":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            // new Vector3(0.0f, 0.75f, 0.0f),
                            new Vector3(0.0f, 1.25f, 0.0f),
                        }
                    );
                    break;
                case "cauldron_ext3_butchertable":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
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
                        prefab.name,
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
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                        }
                    );
                    break;
                case "piece_cookingstation_iron":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(-2.0f, 0.0f, 0.0f),
                            new Vector3(2.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                /* Fires */
                case "hearth": // already has snappoints but not a center one
                case "fire_pit": // (campfire)
                case "fire_pit_iron": // hildir campfire
                case "bonfire":
                    SnapPointHelper.AddCenterSnapPoint(prefab.name);
                    break;
                case "piece_brazierceiling01": // (Hanging Brazier)
                    SnapPointHelper.AddSnapPoints(
                       prefab.name,
                       new[] {
                            new Vector3(0.0f, 2.0f, 0.0f),
                            new Vector3(0.0f, 1.5f, 0.0f),
                       }
                    );
                    break;
                case "piece_brazierfloor01": // standing brazier
                    SnapPointHelper.AddSnapPoints(
                       prefab.name,
                       new[] {
                            new Vector3(0.0f, -1.0f, 0.0f),
                            new Vector3(0.0f, 0.0f, 0.0f),
                       }
                    );
                    break;

                /* Beams & Poles */
                case "wood_beam": // 2m
                case "wood_beam_1": // 1m
                case "wood_beam_45":
                case "wood_pole": // 1m
                case "wood_pole2": // 2m
                case "woodiron_beam":
                case "woodiron_pole":
                case "wood_pole_log_4": // core wood pole 4m
                case "wood_pole_log": // core wood pole 2m
                case "wood_log_45": // core wood 45
                case "wood_wall_log": // core wood beam 2m
                case "wood_wall_log_4x0.5": // core wood beam 4m
                case "darkwood_beam": // 2m
                case "darkwood_pole": // 2m
                case "darkwood_pole4": // 4m
                case "darkwood_beam_45":
                    SnapPointHelper.AddCenterSnapPoint(prefab.name);
                    break;
                // these pieces have offset transforms
                case "wood_beam_26":
                case "wood_log_26":  // core wood 26
                case "darkwood_beam_26":
                case "darkwood_beam4x4": // 4m horizontal beam
                    SnapPointHelper.AddSnapPoints(
                            prefab.name,
                            new[] {
                                new Vector3(0.0f, 0.5f, 0.0f), // add "center" point
                            }
                        );
                    break;

                /* Roof Tiles */
                case "wood_roof":
                case "darkwood_roof":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.5f, 0.0f), // center
                            new Vector3(0.0f, 0.0f, 1.0f), // Bottom mid edge
                            new Vector3(0.0f, 1.0f, -1.0f), // top mid edge
                            new Vector3(1.0f, 0.5f, 0.0f), // left mid edge
                            new Vector3(-1.0f, 0.5f, 0.0f), // right mid edge
                        }
                    );
                    break;
                case "wood_roof_45":
                case "darkwood_roof_45":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f), // center
                            new Vector3(0.0f, -1.0f, 1.0f), // bottom mid edge
                            new Vector3(0.0f, 1.0f, -1.0f), // top mid edge
                            new Vector3(1.0f, 0.0f, 0.0f), // left mid edge
                            new Vector3(-1.0f, 0.0f, 0.0f), // right mid edge
                        }
                    );
                    break;
                case "wood_roof_top":
                case "darkwood_roof_top":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.0f, 1.0f),  // side mid edge
                            new Vector3(0.0f, 0.0f, -1.0f),  // side mid edge
                            new Vector3(0.0f, 0.5f, 0.0f),  // top mid edge
                        }
                    );
                    break;
                case "wood_roof_top_45":
                case "darkwood_roof_top_45":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.0f, 1.0f),  // side mid edge
                            new Vector3(0.0f, 0.0f, -1.0f),  // side mid edge
                            new Vector3(0.0f, 1.0f, 0.0f),  // top mid edge
                        }
                    );
                    break;


                /* Walls */
                case "wood_wall_roof": // 26 degrees
                case "wood_wall_roof_upsidedown": // 26 degrees
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.5f, 0.0f),
                        }
                    );
                    break;
                case "wood_wall_roof_45":
                case "wood_wall_roof_45_upsidedown":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 1.0f, 0.0f),
                        }
                    );
                    break;

                /* Beds */
                case "bed":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
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
                        prefab.name,
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
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                        }
                    );
                    break;
                case "piece_blackmarble_table":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(-1.0f, 0.0f, 0.0f),
                            new Vector3(1.0f, 0.0f, 0.0f),
                        }
                    );
                    break;
                case "piece_table_round": // round table
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                        }
                    );
                    break;
                case "piece_table_oak": // long heavy table
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
                        new[] {
                            new Vector3(0.0f, 0.0f, 0.0f),
                            new Vector3(-2.0f, 0.0f, 0.0f),
                            new Vector3(2.0f, 0.0f, 0.0f),
                        }
                    );
                    break;

                /* Misc */
                case "charcoal_kiln":
                case "incinerator": // (obliterator)
                case "piece_maypole":
                case "piece_xmastree":
                case "piece_cartographytable":
                case "piece_spinningwheel":
                case "piece_stonecutter":
                case "piece_artisanstation":
                case "piece_bathtub": // has snap points but adding center
                case "guard_stone": // ward
                case "piece_turret": // ballista
                case "piece_beehive":
                    SnapPointHelper.AddCenterSnapPoint(prefab.name);
                    break;
                case "piece_barber":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name, // wisp fountain
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
                        prefab.name, // wisp fountain
                        new[] {
                            new Vector3(0.0f, -0.05f, 0.0f),
                        }
                    );
                    break;
                case "eitrrefinery":
                    SnapPointHelper.AddSnapPoints(
                        prefab.name,
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
                       prefab.name,
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
                        prefab.name,
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
                        prefab.name,
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
                    // Add SnapPoint to Local Center if there is not already one present there
                    Transform transform = prefab.GetComponent<Piece>().transform;
                    for (var index = 0; index < transform.childCount; ++index)
                    {
                        var child = transform.GetChild(index);
                        if (child.CompareTag("snappoint"))
                        {
                            if (child.localPosition.Equals(Vector3.zero))
                            {
                                return;
                            }
                        }
                    }
                    SnapPointHelper.AddCenterSnapPoint(prefab.name);
                    break;
            }
        }
    }
}
