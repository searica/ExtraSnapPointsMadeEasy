using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx.Configuration;
using ExtraSnapsMadeEasy.Extensions;
using ExtraSnapsMadeEasy.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logging;
using static ExtraSnapsMadeEasy.Models.SnapPointNames;

namespace ExtraSnapsMadeEasy.ExtraSnapPoints;

internal class ExtraSnapsAdder
{
    private static readonly NamedSnapPoint[] Empty = Array.Empty<NamedSnapPoint>();
    private static readonly NamedSnapPoint[] OriginSnapPointArray = new[] { new NamedSnapPoint(Vector3.zero, ORIGIN) };

    private static readonly HashSet<string> DoNotAddSnapPoints = new()
    {
        "piece_dvergr_spiralstair",
        "piece_dvergr_spiralstair_right",
    };
    private static readonly string[] SkipIfStartsWithSubstrings = new[] {"_", "OLD_", "OLD", "vfx_", "sfx_", "fx_" };
    private static readonly string[] SkipIfHasComponents = new string[] 
    {
        nameof(Projectile),
        nameof(Humanoid),
        nameof(AnimalAI),
        nameof(Character),
        nameof(CreatureSpawner),
        nameof(SpawnArea),
        nameof(Fish),
        nameof(RandomFlyingBird),
        nameof(MusicLocation),
        nameof(Aoe),
        nameof(ItemDrop),
        nameof(DungeonGenerator),
        nameof(TerrainModifier),
        nameof(EventZone),
        nameof(LocationProxy),
        nameof(LootSpawner),
        nameof(Mister),
        nameof(Ragdoll),
        nameof(MineRock5),
        nameof(TombStone),
        nameof(LiquidVolume),
        nameof(Gibber),
        nameof(TimedDestruction),
        nameof(ShipConstructor),
        nameof(TriggerSpawner),
    };

    internal static void AddExtraSnapPoints(string msg, bool forceUpdate = false)
    {
        // Avoid updating before world and prefabs are loaded.
        if (!ZNetScene.instance || SceneManager.GetActiveScene().name != "main")
        {
            return;
        }

        // Only update if needed.
        if (!ExtraSnapsPlugin.Instance.ShouldUpdateExtraSnaps && !forceUpdate)
        {
            return;
        }

        Log.LogInfo(msg);
        List<GameObject> prefabPieces = FindPrefabPieces();

        SnapPointManager.Instance.ClearAddedSnapPoints();

        if (!ExtraSnapsPlugin.Instance.EnableExtraSnapPoints.Value)
        {
            return;
        }

        Stopwatch watch = Stopwatch.StartNew();

        foreach (GameObject prefab in prefabPieces)
        {
            try
            {
                NamedSnapPoint[] snapPointsToAdd = GetExtraSnapPointsFor(prefab);
                SnapPointManager.Instance.AddSnapPointsToPrefab(prefab, snapPointsToAdd);
            }
            catch (Exception e)
            {
                Log.LogWarning($"Failed to add snappoints to {prefab}: {e}");
            }
        }

        watch.Stop();
        Log.LogInfo($"Adding snap points complete. Time: {watch.ElapsedMilliseconds} ms.");

        ExtraSnapsPlugin.Instance.ShouldUpdateExtraSnaps = false;
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

    private static bool SkipPrefab(GameObject prefab)
    {
        if (!prefab)
        {
            return true;
        }
        if (DoNotAddSnapPoints.Contains(prefab.name))
        {
            return true;
        }

        // Customs filters
        if (prefab.name.StartsWithAny(SkipIfStartsWithSubstrings) ||
            prefab.HasAnyComponent(SkipIfHasComponents) ||
            prefab.GetComponentInChildren<Ship>() || // ignore ships
            prefab.GetComponentInChildren<Vagon>() || // ignore carts
            prefab.IsRepairPiece())
        {
            return true;
        }

        return false;
    }

    private static NamedSnapPoint[] GetExtraSnapPointsFor(GameObject prefab)
    {
        ConfigEntry<bool> prefabConfig = ExtraSnapsPlugin.Instance.LoadConfig(prefab);
        if (!prefabConfig.Value)
        {
            return Empty; // skip adding snap points if not enabled
        }

        switch (prefab.name)
        {
            /* Fences */
            case "wood_fence":
                return CreateNamedSnapPoints(new[]
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
                }.ApplyZIndexFix());

            case "piece_sharpstakes":
                return CreateNamedSnapPoints(new[]
                {
                    new Vector3(1.12f, -0.2f, 0f),
                    new Vector3(-1.12f, -0.2f, 0f),
                    new Vector3(1.12f, 0f, 0f),
                    new Vector3(-1.12f, 0f, 0f),
                    new Vector3(1.12f, 0.2f, 0f),
                    new Vector3(-1.12f, 0.2f, 0f),
                    });

            case "piece_dvergr_sharpstakes":
                return CreateNamedSnapPoints(new[]
                {
                    new Vector3(-0.5f, -0.5f, 2f),
                    new Vector3(-0.5f, -0.5f, -2f),
                    new Vector3(-0.5f, 0f, 2f),
                    new Vector3(-0.5f, 0f, -2f),
                    new Vector3(1f, 1f, 2f),
                    new Vector3(1f, 1f, -2f),
                });

            /* Item Stands */
            case "itemstandh": // itemstandh (horizontal)
                return new[]
                {
                    new NamedSnapPoint(0f, -0.0346f, 0f, $"{CENTER} 1"),
                    new NamedSnapPoint(0f, -0.1346f, 0f, $"{CENTER} 2"),
                    new NamedSnapPoint(0f, -0.2346f, 0f, $"{CENTER} 3"),
                };
                //return CreateNamedSnapPoints(new[]
                //{
                //    new Vector3 (0f, -0.0346f, 0f),
                //    new Vector3 (0.1f, -0.0346f, 0f),
                //    new Vector3 (-0.1f, -0.0346f, 0f),
                //    new Vector3 (0.0f, -0.0346f, 0.1f),
                //    new Vector3 (0.0f, -0.0346f, -0.1f),
                //});

            case "itemstand":  // itemstand (vertical)
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, -0.06f, $"{CENTER} 1"),
                    new NamedSnapPoint(0.0f, 0.0f, -0.16f, $"{CENTER} 2"),
                    new NamedSnapPoint(0.0f, 0.0f, -0.26f, $"{CENTER} 3"),
                };
                //return CreateNamedSnapPoints(new[]
                //{
                //    new Vector3 (0f, 0f, -0.06f),
                //    new Vector3 (0.22f, 0f, -0.06f),
                //    new Vector3 (-0.22f, 0f, -0.06f),
                //    new Vector3 (0.0f, 0.22f, -0.06f),
                //    new Vector3 (0.0f, -0.22f, -0.06f),
                // });

            /* Chests */
            case "piece_chest_wood":
                return new[]
                {
                    new NamedSnapPoint(0.0f, -0.025f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.8f, -0.025f, 0.37f, $"{BOTTOM} {CORNER} 1"),
                    new NamedSnapPoint(0.8f, -0.025f, -0.37f, $"{BOTTOM} {CORNER} 2"),
                    new NamedSnapPoint(-0.8f, -0.025f, 0.37f, $"{BOTTOM} {CORNER} 3"),
                    new NamedSnapPoint(-0.8f, -0.025f, -0.37f, $"{BOTTOM} {CORNER} 4"),
                    new NamedSnapPoint(0.65f, 0.8f, 0.35f, $"{TOP} {CORNER} 1"),
                    new NamedSnapPoint(0.65f, 0.8f, -0.35f, $"{TOP} {CORNER} 2"),
                    new NamedSnapPoint(-0.65f, 0.8f, 0.35f, $"{TOP} {CORNER} 3"),
                    new NamedSnapPoint(-0.65f, 0.8f, -0.35f, $"{TOP} {CORNER} 4"),
                };


            case "piece_chest": // (Reinforced Chest)
                return new[]
                {
                    new NamedSnapPoint(0.0f, -0.01f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.9f, -0.01f, 0.47f, $"{BOTTOM} {CORNER} 1"),
                    new NamedSnapPoint(0.9f, -0.01f, -0.47f, $"{BOTTOM} {CORNER} 2"),
                    new NamedSnapPoint(-0.9f, -0.01f, 0.47f, $"{BOTTOM} {CORNER} 3"),
                    new NamedSnapPoint(-0.9f, -0.01f, -0.47f, $"{BOTTOM} {CORNER} 4"),
                    new NamedSnapPoint(0.7f, 0.99f, 0.47f, $"{TOP} {CORNER} 1"),
                    new NamedSnapPoint(0.7f, 0.99f, -0.47f, $"{TOP} {CORNER} 2"),
                    new NamedSnapPoint(-0.7f, 0.99f, 0.47f, $"{TOP} {CORNER} 3"),
                    new NamedSnapPoint(-0.7f, 0.99f, -0.47f, $"{TOP} {CORNER} 4"),
                };

 
            case "piece_chest_private":
                return new[]
                {
                    new NamedSnapPoint(0.0f, -0.01f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.45f, -0.01f, 0.25f, $"{BOTTOM} {CORNER} 1"),
                    new NamedSnapPoint(0.45f, -0.01f, -0.25f, $"{BOTTOM} {CORNER} 2"),
                    new NamedSnapPoint(-0.45f, -0.01f, 0.25f, $"{BOTTOM} {CORNER} 3"),
                    new NamedSnapPoint(-0.45f, -0.01f, -0.25f, $"{BOTTOM} {CORNER} 4"),
                    new NamedSnapPoint(0.36f, 0.55f, 0.23f, $"{TOP} {CORNER} 1"),
                    new NamedSnapPoint(0.36f, 0.55f, -0.23f, $"{TOP} {CORNER} 2"),
                    new NamedSnapPoint(-0.36f, 0.55f, 0.23f, $"{TOP} {CORNER} 3"),
                    new NamedSnapPoint(-0.36f, 0.55f, -0.23f, $"{TOP} {CORNER} 4"),
                };

            case "piece_chest_blackmetal":
                return new[]
                {
                    new NamedSnapPoint(0.0f, -0.01f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.0f, 0.0f, 0.5f, $"{BOTTOM} {CORNER} 1"),
                    new NamedSnapPoint(1.0f, 0.0f, -0.5f, $"{BOTTOM} {CORNER} 2"),
                    new NamedSnapPoint(-1.0f, 0.0f, 0.5f, $"{BOTTOM} {CORNER} 3"),
                    new NamedSnapPoint(1.0f, 0.0f, -0.5f, $"{BOTTOM} {CORNER} 4"),
                    new NamedSnapPoint(0.85f, 1.0f, 0.5f, $"{TOP} {CORNER} 1"),
                    new NamedSnapPoint(0.85f, 1.0f, -0.5f, $"{TOP} {CORNER} 2"),
                    new NamedSnapPoint(-0.85f, 1.0f, 0.5f, $"{TOP} {CORNER} 3"),
                    new NamedSnapPoint(-0.85f, 1.0f, -0.5f, $"{TOP} {CORNER} 4"),
                };

            /* Torches and lights */
            case "piece_walltorch":  // (sconce)
                return new[]
                {
                    new NamedSnapPoint(-0.2f, 0.0f, 0.0f, $"{CENTER} 1"),
                    new NamedSnapPoint(-0.25f, 0.0f, 0.0f, $"{CENTER} 2"),
                    new NamedSnapPoint(-0.35f, 0.0f, 0.0f, $"{CENTER} 3"),
                };


            case "piece_dvergr_lantern":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER} 1"),
                    new NamedSnapPoint(0.05f, 0.0f, 0.0f, $"{CENTER} 2"),
                    new NamedSnapPoint(0.1f, 0.0f, 0.0f, $"{CENTER} 3"),
                };

            case "piece_dvergr_lantern_pole":
                return new[]
                {
                    new NamedSnapPoint(-0.15f, -3.0f, 0.0f, $"{BOTTOM} {CENTER}"),
                    new NamedSnapPoint(0.1f, -3.0f, -0.25f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.1f, -3.0f, 0.25f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.35f, -3.0f, -0.25f, $"{CORNER} 3"),
                    new NamedSnapPoint(-0.35f, -3.0f, 0.25f, $"{CORNER} 4"),
                    new NamedSnapPoint(-0.15f, 0.5f, 0.0f, $"{TOP}"),
                };

            /* Furniture */
            case "sign":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER} 1"),
                    new NamedSnapPoint(0.0f, 0.0f, -0.05f, $"{CENTER} 2"),
                    new NamedSnapPoint(0.0f, 0.0f, -0.1f, $"{CENTER} 3"),
                    new NamedSnapPoint(0.0f, 0.0f, -0.2f, $"{CENTER} 4"),
                    new NamedSnapPoint(0.0f, 0.0f, -0.25f, $"{CENTER} 5"),
                    //new NamedSnapPoint(0.0f, 0.0f, -0.35f, $"{CENTER} 4"),
                };

            case "ArmorStand":
                return CreateNamedSnapPoints(new[]
                {
                    new Vector3(0.0f, -0.1f, 0.0f),
                    new Vector3(0.5f, -0.1f, 0.0f),
                    new Vector3(-0.5f, -0.1f, 0.0f),
                    new Vector3(0.0f, -0.1f, 0.5f),
                    new Vector3(0.0f, -0.1f, -0.5f),
                });

            /* Rugs & Carpets */
            case "jute_carpet": // (red jute carpet)
                return CreateNamedSnapPoints(new[]
                {
                    new Vector3(0.0f, -0.01f, 0.0f),
                    new Vector3(2.0f, -0.01f, -1.25f),
                    new Vector3(2.0f, -0.01f, 1.25f),
                    new Vector3(-2.0f, -0.01f, -1.25f),
                    new Vector3(-2.0f, -0.01f, 1.25f),

                    new Vector3(0.0f, -0.05f, 0.0f),
                    new Vector3(2.0f, -0.05f, -1.25f),
                    new Vector3(2.0f, -0.05f, 1.25f),
                    new Vector3(-2.0f, -0.05f, -1.25f),
                    new Vector3(-2.0f, -0.05f, 1.25f),
                });

            case "rug_fur": // (lox rug)
                return CreateNamedSnapPoints(new[]
                {
                    new Vector3(0.0f, -0.01f, 0.0f),
                    new Vector3(1.25f, -0.01f, -2.0f),
                    new Vector3(1.25f, -0.01f, 2.0f),
                    new Vector3(-1.25f, -0.01f, -2.0f),
                    new Vector3(-1.25f, -0.01f, 2.0f),

                    new Vector3(0.0f, -0.05f, 0.0f),
                    new Vector3(1.25f, -0.05f, -2.0f),
                    new Vector3(1.25f, -0.05f, 2.0f),
                    new Vector3(-1.25f, -0.05f, -2.0f),
                    new Vector3(-1.25f, -0.05f, 2.0f),
                });

            case "rug_deer":
            case "rug_wolf":
            case "rug_hare": // (scale rug)
            case "jute_carpet_blue": // (round blue jute carpet)
                return CreateNamedSnapPoints(
                    new[] {
                        new Vector3(0.0f, -0.01f, 0.0f),
                        new Vector3(0.0f, -0.05f, 0.0f),
                        new Vector3(0.0f, -0.1f, 0.0f)
                    }
                );

            /* Thrones, Chairs, Benches */
            case "piece_throne01": // (Raven Throne)
            case "piece_throne02": // (Stone Throne)
            case "piece_blackmarble_throne":
            case "piece_chair": // (stool)
            case "piece_chair02": // (finewood chair)
            case "piece_chair03": // (darkwood chair)
                return GetOriginSnapPointIfNeeded(prefab);

            case "piece_bench01":
            case "piece_blackmarble_bench":
            case "piece_logbench01": // sitting log
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(-1.0f, 0.0f, 0.0f, $"{EDGE} 1"),
                    new NamedSnapPoint(1.0f, 0.0f, 0.0f, $"{EDGE} 1"),
          
                };

            /* Banners */
            // Banners are 1.4m wide up top
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
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.0f, 0.1f, 0.0f, $"{TOP} {CENTER}"),

                    new NamedSnapPoint(0.136f, 0.0f, 0.0f, $"{FRONT} {CENTER} 1"), // stone wall
                    new NamedSnapPoint(-0.136f, 0.0f, 0.0f, $"{BACK} {CENTER} 1"),
                    new NamedSnapPoint(0.136f, 0.1f, 0.0f, $"{TOP} {FRONT} {CENTER} 1"),
                    new NamedSnapPoint(-0.136f, 0.1f, 0.0f, $"{TOP} {BACK} {CENTER} 1"),

                    new NamedSnapPoint(0.236f, 0.0f, 0.0f, $"{FRONT} {CENTER} 2"), // wood wall
                    new NamedSnapPoint(-0.236f, 0.0f, 0.0f, $"{BACK} {CENTER} 2"),
                    new NamedSnapPoint(0.236f, 0.1f, 0.0f, $"{TOP} {FRONT} {CENTER} 2"),
                    new NamedSnapPoint(-0.236f, 0.1f, 0.0f, $"{TOP} {BACK} {CENTER} 2"),
                };

            /* Blue Jute Hangings */
            case "piece_cloth_hanging_door_blue2":  // (Blue Jute Curtain)
                return CreateNamedSnapPoints(new[]
                {
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
                });

            case "piece_cloth_hanging_door_blue": // (Blue Jute Drape)
                return CreateNamedSnapPoints(new[]
                {
                    new Vector3(0.0f, 4.0f, 0.0f), // center
                    new Vector3(0.16f, 4.0f, 0.0f),
                    new Vector3(-0.16f, 4.0f, 0.0f),
                    new Vector3(0.36f, 4.0f, 0.0f),
                    new Vector3(-0.36f, 4.0f, 0.0f),
                });

            /* Workbench */
            case "piece_workbench":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.25f, 0.0f, -0.5f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.25f, 0.0f, 0.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.25f, 0.0f, -0.5f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.25f, 0.0f, 0.5f, $"{CORNER} 4"),
                };

            case "piece_workbench_ext1": // (Chopping block)
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.25f, 0.0f, 0.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.0f, 0.0f, -0.25f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.25f, 0.0f, 0.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(0.0f, 0.0f, 0.25f, $"{CORNER} 4"),
                };

            case "piece_workbench_ext2": // (Tanning rack)
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.0f, 0.0f, 0.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.0f, 0.0f, -1.0f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.0f, 0.0f, 0.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.0f, 0.0f, -1.0f, $"{CORNER} 4"),
                };

            case "piece_workbench_ext3": // (Adze)
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.0f, 0.0f, 0.5f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.0f, 0.0f, -0.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.0f, 0.0f, 0.5f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.0f, 0.0f, -0.5f, $"{CORNER} 4"),
                    new NamedSnapPoint(1.0f, 0.0f, 0.0f, $"{EDGE} 1"),
                    new NamedSnapPoint(-1.0f, 0.0f, 0.0f, $"{EDGE} 2"),
                };

            case "piece_workbench_ext4":  // (Tool shelf)
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.25f, -0.125f, $"{CENTER}"),
                    new NamedSnapPoint(0.75f, 0.25f, -0.125f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.75f, 1.25f, -0.125f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.75f, 0.25f, -0.125f, $"{CORNER} 3"),
                    new NamedSnapPoint(-0.75f, 1.25f, -0.125f, $"{CORNER} 4"),
                };


            /* Forge */
            case "forge": // (Forge)
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.0f, 0.0f, -0.5f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.0f, 0.0f, 0.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.0f, 0.0f, -0.5f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.0f, 0.0f, 0.5f, $"{CORNER} 4"),
                };

            case "forge_ext1": // (Forge bellows)
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.5f, 0.0f, -0.75f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.5f, 0.0f, 0.75f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.5f, 0.0f, -0.75f, $"{CORNER} 3"),
                    new NamedSnapPoint(-0.5f, 0.0f, 0.75f, $"{CORNER} 4"),
                };

            case "forge_ext2": // (Anvils)
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.25f, 0.0f, 0.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.0f, 0.0f, -0.25f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.25f, 0.0f, 0.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(0.0f, 0.0f, 0.25f, $"{CORNER} 4"),
                };

            case "forge_ext3": // (Grinding wheel)
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.25f, 0.0f, -0.5f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.25f, 0.0f, 0.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.25f, 0.0f, -0.5f, $"{CORNER} 3"),
                    new NamedSnapPoint(-0.25f, 0.0f, 0.5f, $"{CORNER} 4"),
                };

            case "forge_ext4": // (Smith's anvil)
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.5f, 0.0f, 0.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.0f, 0.0f, -0.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.5f, 0.0f, 0.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(0.0f, 0.0f, 0.5f, $"{CORNER} 4"),
                };

            case "forge_ext5": // (Forge cooler)
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.5f, 0.0f, 0.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.0f, 0.0f, -0.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.5f, 0.0f, 0.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(0.0f, 0.0f, 0.5f, $"{CORNER} 4"),
                };

            case "forge_ext6": // (Forge toolrack)
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, -0.125f, $"{CENTER}"),
                    new NamedSnapPoint(1.25f, 0.0f, -0.125f, $"{EDGE} 1"),
                    new NamedSnapPoint(-1.25f, 0.0f, -0.125f, $"{EDGE} 2"),
                };

            /* Black Forge */
            case "blackforge": // galdr table
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.5f, 0.0f, -0.25f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.5f, 0.0f, 0.75f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.5f, 0.0f, -0.25f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.5f, 0.0f, 0.75f, $"{CORNER} 4"),
                    new NamedSnapPoint(0.0f, 0.0f, -0.75f, $"{BACK}"),
                };

            case "blackforge_ext1": // cooler
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.75f, 0.0f, -0.25f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.75f, 0.0f, 0.25f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.75f, 0.0f, -0.25f, $"{CORNER} 3"),
                    new NamedSnapPoint(-0.75f, 0.0f, 0.25f, $"{CORNER} 4"),
                };

            case "blackforge_ext2_vise": // vice
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.25f, 0.0f, -0.25f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.25f, 0.0f, 0.25f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.25f, 0.0f, -0.25f, $"{CORNER} 3"),
                    new NamedSnapPoint(-0.25f, 0.0f, 0.25f, $"{CORNER} 4"),
                };

            /* Galdr Table */
            case "piece_magetable": // galdr table
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.0f, 0.0f, -1.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.0f, 0.0f, 1.0f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.0f, 0.0f, -1.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.0f, 0.0f, 1.0f, $"{CORNER} 4"),
                    new NamedSnapPoint(0.0f, 0.0f, -1.25f, $"{BACK}"),
                };

            case "piece_magetable_ext": // rune table
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.5f, 0.0f, -0.5f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.5f, 0.0f, 0.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.5f, 0.0f, -0.5f, $"{CORNER} 3"),
                    new NamedSnapPoint(-0.5f, 0.0f, 0.5f, $"{CORNER} 4"),
                };

            case "piece_magetable_ext2": // candles
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.25f, 0.0f, -0.25f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.25f, 0.0f, 0.25f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.25f, 0.0f, -0.25f, $"{CORNER} 3"),
                    new NamedSnapPoint(-0.25f, 0.0f, 0.25f, $"{CORNER} 4"),
                };

            /* Cooking Pieces */
            case "piece_cauldron":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.5f, 0.0f, -0.5f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.5f, 0.0f, 0.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.5f, 0.0f, -0.5f, $"{CORNER} 3"),
                    new NamedSnapPoint(-0.5f, 0.0f, 0.5f, $"{CORNER} 4"),
                };
            case "cauldron_ext1_spice":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 1.25f, 0.0f, $"{TOP} {CENTER}"),
                    new NamedSnapPoint(0.0f, 0.75f, -0.05f, $"{BACK} {CENTER}"),
                    new NamedSnapPoint(1.0f, 0.75f, -0.05f, $"{BACK} {EDGE} 1"),
                    new NamedSnapPoint(-1.0f, 0.75f, -0.05f, $"{BACK} {EDGE} 2"),
                };
;
            case "cauldron_ext3_butchertable":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.5f, 0.0f, -0.5f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.5f, 0.0f, 0.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.5f, 0.0f, -0.5f, $"{CORNER} 3"),
                    new NamedSnapPoint(-0.5f, 0.0f, 0.5f, $"{CORNER} 4"),
                };

            case "cauldron_ext4_pots":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.5f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.0f, 0.0f, 0.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.0f, 0.75f, 0.0f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.0f, 0.0f, 0.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.0f, 0.75f, 0.0f, $"{CORNER} 4"),
                };

            case "cauldron_ext5_mortarandpestle":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.6f, 0.0f, -0.3f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.6f, 0.0f, 0.3f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.6f, 0.0f, -0.3f, $"{CORNER} 3"),
                    new NamedSnapPoint(-0.6f, 0.0f, 0.3f, $"{CORNER} 4"),
                };
            case "fermenter":
                return GetOriginSnapPointIfNeeded(prefab);


            case "piece_cookingstation":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.0f, 0.0f, -0.25f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.0f, 0.0f, 0.25f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.0f, 0.0f, -0.25f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.0f, 0.0f, 0.25f, $"{CORNER} 4"),
                    new NamedSnapPoint(1.0f, 0.0f, 0.0f, $"{MID} {EDGE} 1"),
                    new NamedSnapPoint(-1.0f, 0.0f, 0.0f, $"{MID} {EDGE} 2"),
                };

            case "piece_cookingstation_iron":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(2.0f, 0.0f, -0.5f, $"{CORNER} 1"),
                    new NamedSnapPoint(2.0f, 0.0f, 0.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-2.0f, 0.0f, -0.5f, $"{CORNER} 3"),
                    new NamedSnapPoint(-2.0f, 0.0f, 0.5f, $"{CORNER} 4"),
                    new NamedSnapPoint(2.0f, 0.0f, 0.0f, $"{MID} {EDGE} 1"),
                    new NamedSnapPoint(-2.0f, 0.0f, 0.0f, $"{MID} {EDGE} 2"),
                };

            /* Fires */
            case "hearth": // already has snappoints but not a center one
                return GetOriginSnapPointIfNeeded(prefab);

            /* Beams & Poles */
            // Core wood log walls have 4 snap points by default
            case "wood_wall_log": // core wood beam 2m
                return new[]
                {
                    new NamedSnapPoint(0.0f, -0.25f, 0.0f, $"{BOTTOM} {CENTER}"),
                    new NamedSnapPoint(Vector3.zero, CENTER),
                    new NamedSnapPoint(0.0f, 0.25f,  0.0f, $"{TOP} {CENTER}"),
                    new NamedSnapPoint(1.0f, 0.25f,  0.0f, $"{EDGE} {CENTER} 1"),
                    new NamedSnapPoint(-1.0f, 0.25f,  0.0f, $"{EDGE} {CENTER} 2"),
                };
            case "wood_wall_log_4x0.5": // core wood beam 4m
                return new[]
                {
                    new NamedSnapPoint(0.0f, -0.25f, 0.0f, $"{BOTTOM} {CENTER}"),
                    new NamedSnapPoint(Vector3.zero, CENTER),
                    new NamedSnapPoint(0.0f, 0.25f,  0.0f, $"{TOP} {CENTER}"),
                    new NamedSnapPoint(2.0f, 0.25f,  0.0f, $"{EDGE} {CENTER} 1"),
                    new NamedSnapPoint(-2.0f, 0.25f,  0.0f, $"{EDGE} {CENTER} 2"),
                };

            /* Beds */
            case "bed":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.5f, 0.0f, -1.5f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.5f, 0.0f, 1.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-0.5f, 0.0f, -1.5f, $"{CORNER} 3"),
                    new NamedSnapPoint(-0.5f, 0.0f, 1.5f, $"{CORNER} 4"),
                };

            case "piece_bed02":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.0f, 0.0f, -1.5f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.0f, 0.0f, 1.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.0f, 0.0f, -1.5f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.0f, 0.0f, 1.5f, $"{CORNER} 4"),
                };

            /* Tables */
            case "piece_table":
                return CreateNamedSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                });

            case "piece_blackmarble_table":
                return CreateNamedSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                });

            case "piece_table_round": // round table
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.25f, 0.0f, 0.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.0f, 0.0f, -1.25f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.25f, 0.0f, 0.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(0.0f, 0.0f, 1.25f, $"{CORNER} 4"),
                };

            case "piece_table_oak": // long heavy table
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(3.0f, 0.0f, 0.5f, $"{CORNER} 1"),
                    new NamedSnapPoint(3.0f, 0.0f, -0.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-3.0f, 0.0f, 0.5f, $"{CORNER} 3"),
                    new NamedSnapPoint(-3.0f, 0.0f, -0.5f, $"{CORNER} 4"),
                };

            /* Misc */
            case "piece_bathtub": // has snap points but adding center
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.5f, 0.0f, 0.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.0f, 0.0f, -1.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.5f, 0.0f, 0.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(0.0f, 0.0f, 1.5f, $"{CORNER} 4"),
                };

            // TODO: add snaps to these
            case "piece_cartographytable":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.5f, 0.0f, -1.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.5f, 0.0f, 1.0f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.5f, 0.0f, -1.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.5f, 0.0f, 1.0f, $"{CORNER} 4"),
                };
            case "piece_spinningwheel":
                return new[] 
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.0f, 0.0f, -1.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.0f, 0.0f, 1.0f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.0f, 0.0f, -1.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.0f, 0.0f, 1.0f, $"{CORNER} 4"),
                };

            case "piece_stonecutter":
                return new[] 
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.7f, 0.0f, -0.4f, $"{CORNER} 1"),
                    new NamedSnapPoint(0.7f, 0.0f, 0.4f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.5f, 0.0f, -0.6f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.5f, 0.0f, 0.6f, $"{CORNER} 4"),
                };
            case "piece_artisanstation":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.25f, 0.0f, -0.75f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.25f, 0.0f, 0.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.25f, 0.0f, -0.75f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.25f, 0.0f, 0.5f, $"{CORNER} 4"),
                };

            case "piece_barber":
                return new[] 
                {
                    new NamedSnapPoint(0.0f, 0.0f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.0f, 0.0f, -0.75f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.0f, 0.0f, 0.75f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.0f, 0.0f, -0.75f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.0f, 0.0f, 0.75f, $"{CORNER} 4"),
                };

            case "piece_wisplure": // wisp fountain
                return new[] 
                {
                    new NamedSnapPoint(0.0f, -0.05f, 0.0f, $"{ORIGIN}"),
                };

            case "eitrrefinery":
                return new[] // may want to go back to y = 0
                {
                    new NamedSnapPoint(0.0f, -0.005f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(2.75f, -0.005f, -1.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(2.75f, -0.005f, 1.0f, $"{CORNER} 2"),
                    new NamedSnapPoint(-2.75f, -0.005f, -1.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(-2.75f, -0.005f, 1.0f, $"{CORNER} 4"),
                };

            case "windmill":
                return new[]
                {
                    new NamedSnapPoint(0.0f, -0.005f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(2.0f, -0.005f, -2.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(2.0f, -0.005f, 2.0f, $"{CORNER} 2"),
                    new NamedSnapPoint(-2.0f, -0.005f, -2.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(-2.0f, -0.005f, 2.0f, $"{CORNER} 4"),
                };

            case "smelter":
                return new[]
                {
                    new NamedSnapPoint(0.0f, -0.05f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.0f, -0.05f, -1.0f, $"{CORNER} 1"),
                    new NamedSnapPoint(1.0f, -0.05f, 1.0f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.0f, -0.05f, -1.0f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.0f, -0.05f, 1.0f, $"{CORNER} 4"),
                };

            case "blastfurnace":
                return new[]
                {
                    new NamedSnapPoint(0.0f, -0.05f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(2.0f, -0.05f, -1.25f, $"{CORNER} 1"),
                    new NamedSnapPoint(2.0f, -0.05f, 1.25f, $"{CORNER} 2"),
                    new NamedSnapPoint(-1.75f, -0.05f, -1.25f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.75f, -0.05f, 1.25f, $"{CORNER} 4"),
                }; 

            case "Piece_grausten_pillarbase_tapered":
                return new[]
                {
                    new NamedSnapPoint(0.0f, 0.75f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(0.0f, 0.5f, 0.0f, $"Floor Height {CENTER}")
                };

            /* Portals */
            case "portal_wood":
                return new[]
                {
                    new NamedSnapPoint(0.0f, -0.05f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.5f, -0.05f, 0.0f, $"{EDGE} 1"),
                    new NamedSnapPoint(-1.5f, -0.05f, 0.0f, $"{EDGE} 2")
                };

            case "portal_stone":
                return new[]
                {
                    new NamedSnapPoint(0.0f, -0.25f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(3.5f, -0.25f, 0.0f, $"{EDGE} 1"),
                    new NamedSnapPoint(-3.5f, -0.25f, 0.0f, $"{EDGE} 2"),
                    new NamedSnapPoint(1.5f, -0.25f, 1.5f, $"{INNER} {CORNER} 1"),
                    new NamedSnapPoint(-1.5f, -0.25f, 1.5f, $"{INNER} {CORNER} 2"),
                    new NamedSnapPoint(1.5f, -0.25f, -1.5f, $"{INNER} {CORNER} 3"),
                    new NamedSnapPoint(-1.5f, -0.25f, -1.5f, $"{INNER} {CORNER} 4"),
                };

            case "portal":  // enabled by MVBP
                return new[]
                {
                    new NamedSnapPoint(0.0f, -0.25f, 0.0f, $"{CENTER}"),
                    new NamedSnapPoint(1.5f, -0.25f, 0.0f, $"{EDGE} 1"),
                    new NamedSnapPoint(-1.5f, -0.25f, 0.0f, $"{EDGE} 2"),
                    new NamedSnapPoint(1.5f, -0.25f, 1.5f, $"{CORNER} 1"),
                    new NamedSnapPoint(-1.5f, -0.25f, 1.5f, $"{CORNER} 2"),
                    new NamedSnapPoint(1.5f, -0.25f, -1.5f, $"{CORNER} 3"),
                    new NamedSnapPoint(-1.5f, -0.25f, -1.5f, $"{CORNER} 4"),
                };

            /* MVBP */
            case "Grausten_Floor_4x4":
                List<Vector3> pts = new();
                for (float y = -0.5f; y <= 0.5f; y += 1)
                {
                    for (int x = -2; x <= 2; x += 1)
                    {
                        for (int z = -2; z <= 2; z += 1)
                        {   
                            pts.Add(new Vector3(x, y, z));
                        }
                    }
                }
                return CreateNamedSnapPoints(pts);
      

            default:
                return GetCalculatedSnapPointsOrEmpty(prefab);
        }
    }

    private static NamedSnapPoint[] GetOriginSnapPointIfNeeded(GameObject prefab)
    {
        return prefab.HasOriginSnapPoint() ? Empty : OriginSnapPointArray;
    }

    /// <summary>
    ///     Calculate if prfab should have extra snap points added and return them if it should, otherwise return Empty.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    private static NamedSnapPoint[] GetCalculatedSnapPointsOrEmpty(GameObject prefab)
    {
        List<Transform> existingSnapPoints = prefab.GetSnapPoints();

        if (existingSnapPoints.Count == 1 || ShapeClassifier.IsCross(existingSnapPoints))
        {
            return Empty;
        }

        if (!ExtraSnapsPlugin.Instance.EnableTerrainOpSnapPoints.Value && prefab.IsTerrainOp())
        {
            return Empty;
        }

        if (existingSnapPoints.Count == 0)
        {
            if (prefab.IsCeilingBrazier())
            {
                return new[]
                {
                    new NamedSnapPoint(0.0f, 2.0f, 0.0f, $"{TOP} of Chain"),
                    new NamedSnapPoint(0.0f, 1.5f, 0.0f, EXTRA) // This worked badly during testing (item could not be placed)
                };
            }
            if (prefab.IsFloorBrazier())
            {
                // standing brazier, blue standing brazier, mountainkit, etc.
                return new[]
                {
                    new NamedSnapPoint(0.0f, -1.0f, 0.0f, $"Base {CENTER}"),
                    new NamedSnapPoint(0.0f, -0.25f, 0.0f, TOP) // This worked badly during testing (item could not be placed)
                };
            }

            if (prefab.IsTorch(out bool castleKit))
            {
                if (castleKit)
                {
                    return
                    [
                        new NamedSnapPoint(Vector3.zero, BOTTOM), 
                        new NamedSnapPoint(0.0f, 0.7f, 0.0f, ORIGIN),
                    ];
                }
                return 
                [
                    new NamedSnapPoint(Vector3.zero, ORIGIN), 
                    new NamedSnapPoint(0.0f, -0.7f, 0.0f, BOTTOM),
                ];
            }            
        }
        else if (ExtraSnapsPlugin.Instance.EnableLineSnapPoints.Value && ShapeClassifier.FormsLine(existingSnapPoints))
        {
            return ExtraSnapsCalculator.GetExtraPointsForLine(existingSnapPoints, 1);
        }
        else if (ExtraSnapsPlugin.Instance.EnableTriangleSnapPoints.Value && ShapeClassifier.FormsTriangle(existingSnapPoints))
        {
            return ExtraSnapsCalculator.GetExtraSnapPointsForTriangle(existingSnapPoints);
        }
        else if (ExtraSnapsPlugin.Instance.EnableRect2DSnapPoints.Value && ShapeClassifier.FormsRectangle(existingSnapPoints))
        {
            return ExtraSnapsCalculator.GetSnapPointsForRectangle(existingSnapPoints);
        }
        else if (ExtraSnapsPlugin.Instance.EnableRoofTopSnapPoints.Value && prefab.IsRoof() && ShapeClassifier.IsWedge3D(existingSnapPoints))
        {
            return ExtraSnapsCalculator.GetExtraSnapPointsForRoofTop(existingSnapPoints, prefab.name);
        }
        
        if (!existingSnapPoints.ContainsOriginSnapPoint())
        {
            // Only return an origin snap point for prefabs that have not already had extra snap points added.
            // This is because sometimes snapping to the origin results in the piece being unplace-able or
            // breaking the moment it is placed.
            // So if a piece has already had custom extra snap points added to it, that means that it may need
            // to not add a snap point at the origin.
            return OriginSnapPointArray;
        }

        return Empty;
    }

    private static NamedSnapPoint[] CreateNamedSnapPoints(Vector3[] positions, string prefix = EXTRA, int startNumber = 1)
    {
        NamedSnapPoint[] result = new NamedSnapPoint[positions.Length];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new NamedSnapPoint(positions[i], $"{prefix} {startNumber + i}");
        }
        return result;
    }

    private static NamedSnapPoint[] CreateNamedSnapPoints(IEnumerable<Vector3> positions, string prefix = EXTRA, int startNumber = 1)
    {
        NamedSnapPoint[] result = new NamedSnapPoint[positions.Count()];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new NamedSnapPoint(positions.ElementAt(i), $"{prefix} {startNumber + i}");
        }
        return result;
    }
}
