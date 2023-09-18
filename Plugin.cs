using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;


// TODO: Look into checking collider values and just using those to dictate snap points for furniture
namespace ExtraSnapPointsMadeEasy
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModName = "ExtraSnapPointsMadeEasy";
        public const string ModGuid = "searica.valheim.ExtraSnapPointsMadeEasy";
        public const string ModVersion = "0.0.2";

        public static HashSet<string> skip_transform_pieces = new HashSet<string>()
        {
            "piece_chest_private",
            "piece_chest",
            "piece_chest_wood",
            "rug_deer",
            "rug_wolf",
            "rug_hare", // (scale rug)
            "jute_carpet_blue", // (round blue jute carpet)
            "rug_fur", // (lox rug)
            "jute_carpet", // (red jute carpet)
            "windmill",
            "piece_wisplure", // wisp fountain
            "wood_wall_roof", // 26 degrees
            "wood_wall_roof_upsidedown", // 26 degrees
            "wood_wall_roof_45", 
            "wood_wall_roof_45_upsidedown",
            "piece_walltorch", // sconce
            "piece_cloth_hanging_door_blue", // (Blue Jute Drape)
            "piece_cloth_hanging_door_blue2", // (Blue Jute Curtain)
            "cauldron_ext1_spice",
            "piece_workbench_ext4", // (Tool shelf)
            "darkwood_beam4x4", 
        };

        Harmony _harmony;

        private void Awake()
        {
            Log.Init(Logger);
            PluginConfig.Init(Config);

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: ModGuid);
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        public static void AddExtraSnapPoints()
        {
            /* Fences */
            SnapPointHelper.AddSnapPoints(
                "wood_fence",
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

            SnapPointHelper.AddSnapPoints(
                "piece_sharpstakes",
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

            SnapPointHelper.AddSnapPoints(
                "piece_dvergr_sharpstakes",
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


            /* Item Stands */
            // TODO: Change these snap points to reduce clipping
            SnapPointHelper.AddSnapPoints(
                "itemstandh", // itemstandh (horizontal)
                new Vector3[]
                {
                    new Vector3 (0f, 0f, 0f),
                    new Vector3 (0.1f, 0f, 0f),
                    new Vector3 (-0.1f, 0f, 0f),
                    new Vector3 (0.0f, 0f, 0.1f),
                    new Vector3 (0.0f, 0f, -0.1f),

                }
            );

            SnapPointHelper.AddSnapPoints(
                "itemstand", // itemstand (vertical)
                new Vector3[]
                {
                    new Vector3 (0f, 0f, 0f),
                    new Vector3 (0.22f, 0f, 0f),
                    new Vector3 (-0.22f, 0f, 0f),
                    new Vector3 (0.0f, 0.22f, 0f),
                    new Vector3 (0.0f, -0.22f, 0f),

                }
            );


            /* Chests */
            SnapPointHelper.AddSnapPoints(
                "piece_chest_wood",
                new Vector3[]
                {
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

            SnapPointHelper.AddSnapPoints(
                "piece_chest", // (Reinforced Chest)
                new Vector3[]
                {
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

            SnapPointHelper.AddSnapPoints(
                "piece_chest_private",
                new Vector3[]
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

            SnapPointHelper.AddSnapPoints(
                "piece_chest_blackmetal",
                new Vector3[]
                {
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


            /* Torches */
            List<string> torches = new List<string>() { 
                "piece_groundtorch_wood",
                "piece_groundtorch",
                "piece_groundtorch_green",
                "piece_groundtorch_blue",
                "piece_groundtorch_mist"
            };
            foreach ( var tor in torches)
            {
                SnapPointHelper.AddSnapPoints(
                    tor,
                    new Vector3[]
                    {
                        new Vector3(0.0f, 0.0f, 0.0f),
                        new Vector3(0.0f, -0.7f, 0.0f),
                    }
                );
            }

            SnapPointHelper.AddSnapPoints(
                "piece_walltorch", // (sconce)
                new Vector3[]
                {
                    new Vector3(-0.2f, 0.0f, 0.0f), // black marble snap
                    new Vector3(-0.25f, 0.0f, 0.0f), // stone snap
                    new Vector3(-0.35f, 0.0f, 0.0f),  // wood snap
                    // new Vector3(0.0f, 0.0f, 0.0f),
                }
            );


            /* Furniture */
            SnapPointHelper.AddSnapPoints(
                "sign", 
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(0.0f, 0.0f, -0.05f), // marble & stone
                    new Vector3(0.0f, 0.0f, -0.20f), // wood
                }
            );

            SnapPointHelper.AddSnapPoints(
                "ArmorStand",
                new Vector3[]
                {
                    new Vector3(0.0f, -0.1f, 0.0f),
                    new Vector3(0.5f, -0.1f, 0.0f),
                    new Vector3(-0.5f, -0.1f, 0.0f),
                    new Vector3(0.0f, -0.1f, 0.5f),
                    new Vector3(0.0f, -0.1f, -0.5f),
                }
            );


            /* Rugs & Carpets */       
            SnapPointHelper.AddSnapPoints(
                "jute_carpet", // (red jute carpet)
                new Vector3[]
                {
                    new Vector3(0.0f, -0.01f, 0.0f),
                    new Vector3(2.0f, -0.01f, -1.25f),
                    new Vector3(2.0f, -0.01f, 1.25f),
                    new Vector3(-2.0f, -0.01f, -1.25f),
                    new Vector3(-2.0f, -0.01f, 1.25f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "rug_fur", // (lox rug)
                new Vector3[]
                {
                    new Vector3(0.0f, -0.01f, 0.0f),
                    new Vector3(1.25f, -0.01f, -2.0f),
                    new Vector3(1.25f, -0.01f, 2.0f),
                    new Vector3(-1.25f, -0.01f, -2.0f),
                    new Vector3(-1.25f, -0.01f, 2.0f),
                }
            );

            List<string> rugs = new List<string>()
            {
                "rug_deer",
                "rug_wolf",
                "rug_hare", // (scale rug)
                "jute_carpet_blue", // (round blue jute carpet)
            };
            foreach (var rug in rugs)
            {
                SnapPointHelper.AddSnapPoints(
                    rug,
                    new Vector3[]
                    {
                        new Vector3(0.0f, -0.01f, 0.0f),
                    }
                );
            }


            /* Thrones, Chairs, Benches */
            List<string> chairs = new List<string>() {
                "piece_throne01", // (Raven Throne)
                "piece_throne02", // (Stone Throne)
                "piece_blackmarble_throne",
                "piece_chair", // (stool)
                "piece_chair02", // (finewood chair)
                "piece_chair03", // (darkwood chair)
            };
            foreach ( var chair in chairs )
            {
                SnapPointHelper.AddSnapPoints(
                   chair,
                   new Vector3[]
                   {
                        new Vector3(0.0f, 0.0f, 0.0f),
                   }
               );
            }

            List<string> benches = new List<string>() {
                "piece_bench01",
                "piece_blackmarble_bench",
                "piece_logbench01", // sitting log
            };
            foreach (var bench in benches)
            {
                SnapPointHelper.AddSnapPoints(
                   bench,
                   new Vector3[]
                   {
                        new Vector3(0.0f, 0.0f, 0.0f),
                        new Vector3(-1.0f, 0.0f, 0.0f),
                        new Vector3(1.0f, 0.0f, 0.0f),
                   }
               );
            }


            /* Banners */
            // Banners are about 1.25m wide up top
            List<string> banners = new List<string>()
            {
                "piece_banner01", // (black)
                "piece_banner02", // (blue)
                "piece_banner03", // (white & red)
                "piece_banner04", // (red)
                "piece_banner05", // (green)
                "piece_banner06", // (white, blue, red)
                "piece_banner07", // (white & blue)
                "piece_banner08", // (yellow)
                "piece_banner09", // (purple)
                "piece_banner10", // (orange)
                "piece_banner11", // (white)
                "piece_cloth_hanging_door", // (red jute curtain)
            };
            foreach( var banner in banners)
            {
                SnapPointHelper.AddSnapPoints(
                   banner,
                   new Vector3[]
                   {
                        new Vector3(0.0f, 0.0f, 0.0f),
                        new Vector3(0.136f, 0.0f, 0.0f), // stone walls
                        new Vector3(-0.136f, 0.0f, 0.0f), // stone walls
                        new Vector3(0.236f, 0.0f, 0.0f), // wood walls
                        new Vector3(-0.236f, 0.0f, 0.0f), // wood walls
                   }
               );
            }

            /* Blue Jute Hangings */
            SnapPointHelper.AddSnapPoints(
                "piece_cloth_hanging_door_blue2", // (Blue Jute Curtain) TODO
                new Vector3[]
                {
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

            SnapPointHelper.AddSnapPoints(
                "piece_cloth_hanging_door_blue", // (Blue Jute Drape)
                new Vector3[]
                {
                    new Vector3(0.0f, 4.0f, 0.0f), // center
                    new Vector3(0.16f, 4.0f, 0.0f),
                    new Vector3(-0.16f, 4.0f, 0.0f),
                    new Vector3(0.36f, 4.0f, 0.0f),
                    new Vector3(-0.36f, 4.0f, 0.0f),
                }
            );


            /* Workbench */
            List<string> workbench_pieces = new List<string>()
            {
                "piece_workbench",
                "piece_workbench_ext1", // (Chopping block)
            };
            foreach (var workbench_piece in workbench_pieces )
            {
                SnapPointHelper.AddCenterSnapPoint( workbench_piece );
            }

            SnapPointHelper.AddSnapPoints(
                "piece_workbench_ext2", // (Tanning rack)
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, -1.0f),
                    new Vector3(-1.0f, 0.0f, -1.0f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "piece_workbench_ext3", // (Adze)
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f),
                }
            );

            // TODO: may need to switch the sign of the z component
            SnapPointHelper.AddSnapPoints(
                "piece_workbench_ext4", // (Tool shelf)
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, -0.1f),
                    new Vector3(1.0f, 0.0f, -0.1f),
                    new Vector3(-1.0f, 0.0f, -0.1f),
                    new Vector3(1.0f, 1.0f, -0.1f),
                    new Vector3(-1.0f, 1.0f, -0.1f),
                }
            );


            /* Forge */
            List<string> forge_pieces = new List<string>()
            {
                "forge", // (Forge)	
                "forge_ext1", // (Forge bellows)
                "forge_ext2", // (Anvils)
                "forge_ext3", // (Grinding wheel)
                "forge_ext4", // (Smith's anvil)
                "forge_ext5", // (Forge cooler) 
                "forge_ext6", // (Forge toolrack)
            };
            foreach (var forge_piece in forge_pieces )
            {
                SnapPointHelper.AddCenterSnapPoint(forge_piece);
            }


            /* Black Forge */
            List<string> blackforge_pieces = new List<string>()
            {
                "blackforge", // galdr table
                "blackforge_ext1", // cooler
                "blackforge_ext2_vise", // vice
            };
            foreach ( var blackforge_piece in blackforge_pieces )
            {
                SnapPointHelper.AddCenterSnapPoint(blackforge_piece);
            }


            /* Galdr Table */
            List<string> galdr_pieces = new List<string>()
            {
                "piece_magetable", // galdr table
                "piece_magetable_ext", // rune table
                "piece_magetable_ext2", // candles
            };
            foreach( var galdr_piece in galdr_pieces )
            {
                SnapPointHelper.AddCenterSnapPoint(galdr_piece);
            }

            /* Cooking Pieces */
            List<string> cooking_pieces = new List<string>()
            {
                "piece_cauldron",
                "cauldron_ext5_mortarandpestle",
                "fermenter",
            };
            foreach (var cooking_piece in cooking_pieces)
            {
                SnapPointHelper.AddCenterSnapPoint(cooking_piece);
            }

                
            SnapPointHelper.AddSnapPoints(
                "cauldron_ext1_spice",
                new Vector3[]
                {
                    // new Vector3(0.0f, 0.75f, 0.0f),
                    new Vector3(0.0f, 1.25f, 0.0f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "cauldron_ext3_butchertable",
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(0.5f, 0.0f, -0.5f),
                    new Vector3(0.5f, 0.0f, 0.5f),
                    new Vector3(-0.5f, 0.0f, -0.5f),
                    new Vector3(-0.5f, 0.0f, 0.5f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "cauldron_ext4_pots",
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(-1.0f, 1.0f, 0.0f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "piece_cookingstation",
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "piece_cookingstation_iron",
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(-2.0f, 0.0f, 0.0f),
                    new Vector3(2.0f, 0.0f, 0.0f),
                }
            );


            /* Fires */
            List<string> fires = new List<string>()
            {
                "hearth", // already has snappoints but not a center one
                "fire_pit", // (campfire)
                "fire_pit_iron", // hildir campfire
                "bonfire",
            };
            foreach (var fire in fires)
            {
                SnapPointHelper.AddCenterSnapPoint(fire);
            }
            
            SnapPointHelper.AddSnapPoints(
               "piece_brazierceiling01", // (Hanging Brazier)
               new Vector3[]
               {
                    new Vector3(0.0f, 2.0f, 0.0f),
                    new Vector3(0.0f, 1.5f, 0.0f),
               }
            );

            SnapPointHelper.AddSnapPoints(
                "piece_brazierfloor01", // standing brazier
                new Vector3[]
                {
                    new Vector3(0.0f, -1.0f, 0.0f),
                    new Vector3(0.0f, 0.0f, 0.0f),
                }
            );


            /* Beams & Poles */
            List<string> beams_n_poles = new List<string>()
            {
                "wood_beam", // 2m
                "wood_beam_1", // 1m
                "wood_beam_45",
                "wood_pole", // 1m
                "wood_pole2", // 2m
                "woodiron_beam",
                "woodiron_pole",
                "wood_pole_log_4", // core wood pole 4m
                "wood_pole_log", // core wood pole 2m
                "wood_log_45", // core wood 45
                "wood_wall_log", // core wood beam 2m
                "wood_wall_log_4x0.5", // core wood beam 4m
                "darkwood_beam", // 2m
                "darkwood_pole", // 2m
                "darkwood_pole4", // 4m
                "darkwood_beam_45"
            };
            foreach (var beam_or_pole in beams_n_poles)
            {
                SnapPointHelper.AddCenterSnapPoint(beam_or_pole);
            }

            List<string> degree26 = new List<string>()  // these pieces have offset transforms
            {
                "wood_beam_26",
                "wood_log_26",  // core wood 26
                "darkwood_beam_26",
                "darkwood_beam4x4", // 4m horizontal beam
            };
            foreach (var deg26 in degree26)
            {
                SnapPointHelper.AddSnapPoints(
                    deg26,
                    new Vector3[]
                    {
                        new Vector3(0.0f, 0.5f, 0.0f), // add "center" point
                    }
                );
            }

            /* Roof Tiles */
            List<string> roof26_tiles = new List<string>()
            {
                "wood_roof",
                "darkwood_roof",
            };
            foreach (var tile in roof26_tiles)
            {
                SnapPointHelper.AddSnapPoints(
                    tile,
                    new Vector3[]
                    {
                        new Vector3(0.0f, 0.5f, 0.0f), // center
                        new Vector3(0.0f, 0.0f, 1.0f), // Bottom mid edge
                        new Vector3(0.0f, 1.0f, -1.0f), // top mid edge
                        new Vector3(1.0f, 0.5f, 0.0f), // left mid edge
                        new Vector3(-1.0f, 0.5f, 0.0f), // right mid edge
                    }
                );
            }

            List<string> roof45_tiles = new List<string>()
            {
                "wood_roof_45",
                "darkwood_roof_45",
            };
            foreach(var tile in roof45_tiles)
            {
                SnapPointHelper.AddSnapPoints(
                    tile,
                    new Vector3[]
                    {
                        new Vector3(0.0f, 0.0f, 0.0f), // center
                        new Vector3(0.0f, -1.0f, 1.0f), // bottom mid edge
                        new Vector3(0.0f, 1.0f, -1.0f), // top mid edge
                        new Vector3(1.0f, 0.0f, 0.0f), // left mid edge
                        new Vector3(-1.0f, 0.0f, 0.0f), // right mid edge
                    }
                );
            }

            List<string> roof26_ridge = new List<string>()
            {
                "wood_roof_top",
                "darkwood_roof_top",
            };
            foreach (var ridge in roof26_ridge)
            {
                SnapPointHelper.AddSnapPoints(
                    ridge,
                    new Vector3[]
                    {
                        new Vector3(0.0f, 0.0f, 1.0f),  // side mid edge
                        new Vector3(0.0f, 0.0f, -1.0f),  // side mid edge
                        new Vector3(0.0f, 0.5f, 0.0f),  // top mid edge
                    }
                );
            }

            List<string> roof45_ridge = new List<string>()
            {
                "wood_roof_top_45",
                "darkwood_roof_top_45",
            };
            foreach (var ridge in roof45_ridge)
            {
                SnapPointHelper.AddSnapPoints(
                    ridge,
                    new Vector3[]
                    {
                        new Vector3(0.0f, 0.0f, 1.0f),  // side mid edge
                        new Vector3(0.0f, 0.0f, -1.0f),  // side mid edge
                        new Vector3(0.0f, 1.0f, 0.0f),  // top mid edge
                    }
                );
            }


            /* Walls */
            // Fix this section so they snap to the center of the diagonal beam piece
            List<string> wall_roofs_26 = new List<string>()
            {
                "wood_wall_roof", // 26 degrees
                "wood_wall_roof_upsidedown", // 26 degrees
            };
            foreach (var wall_roof in wall_roofs_26)
            {
                SnapPointHelper.AddSnapPoints(
                    wall_roof,
                    new Vector3[]
                    {
                        new Vector3(0.0f, 0.5f, 0.0f),
                    }
                );
            }

            List<string> wall_roofs_45 = new List<string>()
            {
                "wood_wall_roof_45", 
                "wood_wall_roof_45_upsidedown", 
            };
            foreach (var wall_roof in wall_roofs_45)
            {
                SnapPointHelper.AddSnapPoints(
                    wall_roof,
                    new Vector3[]
                    {
                        new Vector3(0.0f, 1.0f, 0.0f),
                    }
                );
            }


            /* Beds */
            SnapPointHelper.AddSnapPoints(
                "bed",
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(0.5f, 0.0f, -1.5f),
                    new Vector3(0.5f, 0.0f, 1.5f),
                    new Vector3(-0.5f, 0.0f, -1.5f),
                    new Vector3(-0.5f, 0.0f, 1.5f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "piece_bed02", // dragon bed
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, -1.5f),
                    new Vector3(1.0f, 0.0f, 1.5f),
                    new Vector3(-1.0f, 0.0f, -1.5f),
                    new Vector3(-1.0f, 0.0f, 1.5f),
                }
            );

            /* Tables */
            SnapPointHelper.AddSnapPoints(
                "piece_table",
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "piece_blackmarble_table",
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                }
            );
            
            SnapPointHelper.AddSnapPoints(
                "piece_table_round", // round table
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "piece_table_oak", // long heavy table
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(-2.0f, 0.0f, 0.0f),
                    new Vector3(2.0f, 0.0f, 0.0f),
                }
            );

            /* Misc */
            List<string> misc_pieces = new List<string>()
            {
                "charcoal_kiln",
                "incinerator", // (obliterator)
                "piece_maypole",
                "piece_xmastree",
                "piece_cartographytable",
                "piece_spinningwheel",
                "piece_stonecutter",
                "piece_artisanstation",
                "piece_bathtub", // has snap points but adding center
                "guard_stone", // ward
                "piece_turret", // ballista
                "piece_beehive",
            };
            foreach (var misc_piece in misc_pieces)
            {
                SnapPointHelper.AddCenterSnapPoint(misc_piece);
            }

            SnapPointHelper.AddSnapPoints(
                "piece_barber", // wisp fountain
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, -0.75f),
                    new Vector3(1.0f, 0.0f, 0.75f),
                    new Vector3(-1.0f, 0.0f, -0.75f),
                    new Vector3(-1.0f, 0.0f, 0.75f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "piece_wisplure", // wisp fountain
                new Vector3[]
                {
                    new Vector3(0.0f, -0.05f, 0.0f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "eitrrefinery",
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(2.75f, 0.0f, -1.0f),
                    new Vector3(2.75f, 0.0f, 1.0f),
                    new Vector3(-2.75f, 0.0f, -1.0f),
                    new Vector3(-2.75f, 0.0f, 1.0f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "windmill",
                new Vector3[]
                {
                    new Vector3(0.0f, -0.005f, 0.0f),
                    new Vector3(2.0f, -0.005f, -2.0f),
                    new Vector3(2.0f, -0.005f, 2.0f),
                    new Vector3(-2.0f, -0.005f, -2.0f),
                    new Vector3(-2.0f, -0.005f, 2.0f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "smelter",
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, -1.0f),
                    new Vector3(1.0f, 0.0f, 1.0f),
                    new Vector3(-1.0f, 0.0f, -1.0f),
                    new Vector3(-1.0f, 0.0f, 1.0f),
                }
            );

            SnapPointHelper.AddSnapPoints(
                "blastfurnace",
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(2.0f, 0.0f, -1.25f),
                    new Vector3(2.0f, 0.0f, 1.25f),
                    new Vector3(-1.75f, 0.0f, -1.25f),
                    new Vector3(-1.75f, 0.0f, 1.25f),
                }
            );

        }
    }
}
