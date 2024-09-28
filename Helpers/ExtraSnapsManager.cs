using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx.Configuration;
using ExtraSnapPointsMadeEasy.Models;
using ExtraSnapPointsMadeEasy.SnapPoints;
using ExtraSnapPointsMadeEasy.SnapPoints.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ExtraSnapPointsMadeEasy.SnapPoints.SnapPointNames;

namespace ExtraSnapPointsMadeEasy.Helpers;

internal class ExtraSnapsManager
{
    private static readonly NamedSnapPoint[] Empty = Array.Empty<NamedSnapPoint>();
    private static readonly NamedSnapPoint[] OriginSnapPointArray = new[] { new NamedSnapPoint(Vector3.zero, ORIGIN) };

    private static readonly HashSet<string> DoNotAddSnapPoints = new()
    {
        "piece_dvergr_spiralstair",
        "piece_dvergr_spiralstair_right",
    };

    internal static void AddExtraSnapPoints(string msg, bool forceUpdate = false)
    {
        // Avoid updating before world and prefabs are loaded.
        if (!ZNetScene.instance || SceneManager.GetActiveScene().name != "main")
        {
            return;
        }

        // Only update if needed.
        if (!ExtraSnapPointsMadeEasy.UpdateExtraSnapPoints && !forceUpdate)
        {
            return;
        }        

        Log.LogInfo(msg);
        List<GameObject> prefabPieces = FindPrefabPieces();

        SnapPointManager.Instance.ClearAddedSnapPoints();

        if (!ExtraSnapPointsMadeEasy.EnableExtraSnapPoints.Value)
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

        ExtraSnapPointsMadeEasy.UpdateExtraSnapPoints = false;
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
        if(!prefab)
        {
            return true;
        }
        if (DoNotAddSnapPoints.Contains(prefab.name))
        {
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
            (prefab.TryGetComponent(out Piece piece) && piece.m_repairPiece))
        {
            return true;
        }

        return false;
    }

    private static NamedSnapPoint[] GetExtraSnapPointsFor(GameObject prefab)
    {
        ConfigEntry<bool> prefabConfig = ExtraSnapPointsMadeEasy.LoadConfig(prefab);
        if (!prefabConfig.Value)
        {
            return Empty; // skip adding snap points if not enabled
        }

        switch (prefab.name)
        {
            /* Fences */
            case "wood_fence":
                return NameSnapPoints(new[]
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
                return NameSnapPoints(new[]
                {
                    new Vector3(1.12f, -0.2f, 0f),
                    new Vector3(-1.12f, -0.2f, 0f),
                    new Vector3(1.12f, 0f, 0f),
                    new Vector3(-1.12f, 0f, 0f),
                    new Vector3(1.12f, 0.2f, 0f),
                    new Vector3(-1.12f, 0.2f, 0f),
                    });

            case "piece_dvergr_sharpstakes":
                return NameSnapPoints(new[]
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
                return NameSnapPoints(new[]
                {
                    new Vector3 (0f, -0.0346f, 0f),
                    new Vector3 (0.1f, -0.0346f, 0f),
                    new Vector3 (-0.1f, -0.0346f, 0f),
                    new Vector3 (0.0f, -0.0346f, 0.1f),
                    new Vector3 (0.0f, -0.0346f, -0.1f),
                });

            case "itemstand":  // itemstand (vertical)
                return NameSnapPoints(new[]
                {
                    new Vector3 (0f, 0f, -0.06f),
                    new Vector3 (0.22f, 0f, -0.06f),
                    new Vector3 (-0.22f, 0f, -0.06f),
                    new Vector3 (0.0f, 0.22f, -0.06f),
                    new Vector3 (0.0f, -0.22f, -0.06f),
                 });

            /* Chests */
            case "piece_chest_wood":
                return NameSnapPoints(new[]
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
                });

            case "piece_chest": // (Reinforced Chest)
                return NameSnapPoints(new[]
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
                });

            case "piece_chest_private":
                return NameSnapPoints(new[]
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
                });

            case "piece_chest_blackmetal":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(1.0f, 0.0f, 0.5f),
                    new Vector3(1.0f, 0.0f, -0.5f),
                    new Vector3(-1.0f, 0.0f, 0.5f),
                    new Vector3(-1.0f, 0.0f, -0.5f),
                    new Vector3(0.85f, 1.0f, 0.5f),
                    new Vector3(0.85f, 1.0f, -0.5f),
                    new Vector3(-0.85f, 1.0f, 0.5f),
                    new Vector3(-0.85f, 1.0f, -0.5f)
                });

            /* Torches */
            case "piece_walltorch":  // (sconce)
                return NameSnapPoints(new[]
                {
                    new Vector3(-0.2f, 0.0f, 0.0f), // black marble snap
                    new Vector3(-0.25f, 0.0f, 0.0f), // stone snap
                    new Vector3(-0.35f, 0.0f, 0.0f),  // wood snap
                    // Vector3.zero,
                });

            case "piece_dvergr_lantern_pole":
                return NameSnapPoints(new[] { Vector3.zero });

            /* Furniture */
            case "sign":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(0.0f, 0.0f, -0.05f), // marble & stone
                    new Vector3(0.0f, 0.0f, -0.20f), // wood
                });

            case "ArmorStand":
                return NameSnapPoints(new[]
                {
                    new Vector3(0.0f, -0.1f, 0.0f),
                    new Vector3(0.5f, -0.1f, 0.0f),
                    new Vector3(-0.5f, -0.1f, 0.0f),
                    new Vector3(0.0f, -0.1f, 0.5f),
                    new Vector3(0.0f, -0.1f, -0.5f),
                });

            /* Rugs & Carpets */
            case "jute_carpet": // (red jute carpet)
                return NameSnapPoints(new[]
                {
                    new Vector3(0.0f, -0.01f, 0.0f),
                    new Vector3(2.0f, -0.01f, -1.25f),
                    new Vector3(2.0f, -0.01f, 1.25f),
                    new Vector3(-2.0f, -0.01f, -1.25f),
                    new Vector3(-2.0f, -0.01f, 1.25f),
                });

            case "rug_fur": // (lox rug)
                return NameSnapPoints(new[]
                {
                    new Vector3(0.0f, -0.01f, 0.0f),
                    new Vector3(1.25f, -0.01f, -2.0f),
                    new Vector3(1.25f, -0.01f, 2.0f),
                    new Vector3(-1.25f, -0.01f, -2.0f),
                    new Vector3(-1.25f, -0.01f, 2.0f),
                });

            case "rug_deer":
            case "rug_wolf":
            case "rug_hare": // (scale rug)
            case "jute_carpet_blue": // (round blue jute carpet)
                return NameSnapPoints(new[] { new Vector3(0.0f, -0.01f, 0.0f) });

            /* Thrones, Chairs, Benches */
            case "piece_throne01": // (Raven Throne)
            case "piece_throne02": // (Stone Throne)
            case "piece_blackmarble_throne":
            case "piece_chair": // (stool)
            case "piece_chair02": // (finewood chair)
            case "piece_chair03": // (darkwood chair)
                return prefab.HasOriginSnapPoint() ? Empty : OriginSnapPointArray;

            case "piece_bench01":
            case "piece_blackmarble_bench":
            case "piece_logbench01": // sitting log
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                });

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
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(0.136f, 0.0f, 0.0f), // stone walls
                    new Vector3(-0.136f, 0.0f, 0.0f), // stone walls
                    new Vector3(0.236f, 0.0f, 0.0f), // wood walls
                    new Vector3(-0.236f, 0.0f, 0.0f), // wood walls
                });

            /* Blue Jute Hangings */
            case "piece_cloth_hanging_door_blue2":  // (Blue Jute Curtain)
                return NameSnapPoints(new[]
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
                return NameSnapPoints(new[]
                {
                    new Vector3(0.0f, 4.0f, 0.0f), // center
                    new Vector3(0.16f, 4.0f, 0.0f),
                    new Vector3(-0.16f, 4.0f, 0.0f),
                    new Vector3(0.36f, 4.0f, 0.0f),
                    new Vector3(-0.36f, 4.0f, 0.0f),
                });

            /* Workbench */
            case "piece_workbench":
            case "piece_workbench_ext1": // (Chopping block)
                return prefab.HasOriginSnapPoint() ? Empty : OriginSnapPointArray;

            case "piece_workbench_ext2": // (Tanning rack)
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(1.0f, 0.0f, 0.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, -1.0f),
                    new Vector3(-1.0f, 0.0f, -1.0f),
                });

            case "piece_workbench_ext3": // (Adze)
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(1.0f, 0.0f, 0.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f),
                });

            case "piece_workbench_ext4":  // (Tool shelf)
                return NameSnapPoints(new[]
                {
                    new Vector3(0.0f, 0.0f, -0.1f),
                    new Vector3(1.0f, 0.0f, -0.1f),
                    new Vector3(-1.0f, 0.0f, -0.1f),
                    new Vector3(1.0f, 1.0f, -0.1f),
                    new Vector3(-1.0f, 1.0f, -0.1f),
                });

            /* Forge */
            case "forge": // (Forge)
            case "forge_ext1": // (Forge bellows)
            case "forge_ext2": // (Anvils)
            case "forge_ext3": // (Grinding wheel)
            case "forge_ext4": // (Smith's anvil)
            case "forge_ext5": // (Forge cooler)
            case "forge_ext6": // (Forge toolrack)
                return prefab.HasOriginSnapPoint() ? Empty : OriginSnapPointArray;

            /* Black Forge */
            case "blackforge": // galdr table
            case "blackforge_ext1": // cooler
            case "blackforge_ext2_vise": // vice
                return prefab.HasOriginSnapPoint() ? Empty : OriginSnapPointArray;

            /* Galdr Table */
            case "piece_magetable": // galdr table
            case "piece_magetable_ext": // rune table
            case "piece_magetable_ext2": // candles
                return prefab.HasOriginSnapPoint() ? Empty : OriginSnapPointArray;

            /* Cooking Pieces */
            case "piece_cauldron":
            case "cauldron_ext5_mortarandpestle":
            case "fermenter":
                return prefab.HasOriginSnapPoint() ? Empty : OriginSnapPointArray;

            case "cauldron_ext1_spice":
                return NameSnapPoints(new[]
                {
                    new Vector3(0.0f, 1.25f, 0.0f)
                });

            case "cauldron_ext3_butchertable":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(0.5f, 0.0f, -0.5f),
                    new Vector3(0.5f, 0.0f, 0.5f),
                    new Vector3(-0.5f, 0.0f, -0.5f),
                    new Vector3(-0.5f, 0.0f, 0.5f),
                });

            case "cauldron_ext4_pots":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 1.0f, 0.0f),
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(-1.0f, 1.0f, 0.0f),
                });

            case "piece_cookingstation":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                });

            case "piece_cookingstation_iron":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(-2.0f, 0.0f, 0.0f),
                    new Vector3(2.0f, 0.0f, 0.0f),
                });

            /* Fires */
            case "hearth": // already has snappoints but not a center one
                return prefab.HasOriginSnapPoint() ? Empty : OriginSnapPointArray;

            /* Beams & Poles */
            // Core wood log walls have 4 snap points by default
            case "wood_wall_log": // core wood beam 2m
            case "wood_wall_log_4x0.5": // core wood beam 4m
                return new[]
                {
                    new NamedSnapPoint(0.0f, -0.25f, 0.0f, $"{BOTTOM} {CENTER}"),
                    new NamedSnapPoint(Vector3.zero, CENTER),
                    new NamedSnapPoint(0.0f, 0.25f,  0.0f, $"{TOP} {CENTER}"),
                };

            /* Beds */
            case "bed":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(0.5f, 0.0f, -1.5f),
                    new Vector3(0.5f, 0.0f, 1.5f),
                    new Vector3(-0.5f, 0.0f, -1.5f),
                    new Vector3(-0.5f, 0.0f, 1.5f),
                });

            case "piece_bed02":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(1.0f, 0.0f, -1.5f),
                    new Vector3(1.0f, 0.0f, 1.5f),
                    new Vector3(-1.0f, 0.0f, -1.5f),
                    new Vector3(-1.0f, 0.0f, 1.5f),
                });

            /* Tables */
            case "piece_table":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                });

            case "piece_blackmarble_table":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(-1.0f, 0.0f, 0.0f),
                    new Vector3(1.0f, 0.0f, 0.0f),
                });

            case "piece_table_round": // round table
                return prefab.HasOriginSnapPoint() ? Empty : OriginSnapPointArray;

            case "piece_table_oak": // long heavy table
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(-2.0f, 0.0f, 0.0f),
                    new Vector3(2.0f, 0.0f, 0.0f),
                });

            /* Misc */
            case "piece_bathtub": // has snap points but adding center
                return prefab.HasOriginSnapPoint() ? Empty : OriginSnapPointArray;

            // TODO: add snaps to these
            case "piece_cartographytable":
            case "piece_spinningwheel":
            case "piece_stonecutter":
            case "piece_artisanstation":
                return prefab.HasOriginSnapPoint() ? Empty : OriginSnapPointArray;

            case "piece_barber":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(1.0f, 0.0f, -0.75f),
                    new Vector3(1.0f, 0.0f, 0.75f),
                    new Vector3(-1.0f, 0.0f, -0.75f),
                    new Vector3(-1.0f, 0.0f, 0.75f),
                });

            case "piece_wisplure": // wisp fountain
                return NameSnapPoints(new[]
                {
                    new Vector3(0.0f, -0.05f, 0.0f)
                });

            case "eitrrefinery":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(2.75f, 0.0f, -1.0f),
                    new Vector3(2.75f, 0.0f, 1.0f),
                    new Vector3(-2.75f, 0.0f, -1.0f),
                    new Vector3(-2.75f, 0.0f, 1.0f),
                });

            case "windmill":
                return NameSnapPoints(new[]
                {
                    new Vector3(0.0f, -0.005f, 0.0f),
                    new Vector3(2.0f, -0.005f, -2.0f),
                    new Vector3(2.0f, -0.005f, 2.0f),
                    new Vector3(-2.0f, -0.005f, -2.0f),
                    new Vector3(-2.0f, -0.005f, 2.0f),
                });

            case "smelter":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(1.0f, 0.0f, -1.0f),
                    new Vector3(1.0f, 0.0f, 1.0f),
                    new Vector3(-1.0f, 0.0f, -1.0f),
                    new Vector3(-1.0f, 0.0f, 1.0f),
                });

            case "blastfurnace":
                return NameSnapPoints(new[]
                {
                    Vector3.zero,
                    new Vector3(2.0f, 0.0f, -1.25f),
                    new Vector3(2.0f, 0.0f, 1.25f),
                    new Vector3(-1.75f, 0.0f, -1.25f),
                    new Vector3(-1.75f, 0.0f, 1.25f),
                });

            default:
                return GetCalculatedSnapPointsOrEmpty(prefab);
        }
    }

    private static NamedSnapPoint[] GetCalculatedSnapPointsOrEmpty(GameObject prefab)
    {
        List<Transform> existingSnapPoints = prefab.GetSnapPoints();

        if (existingSnapPoints.Count == 1 || ShapeClassifier.IsCross(existingSnapPoints))
        {
            return Empty;
        }

        if (!ExtraSnapPointsMadeEasy.EnableTerrainOpSnapPoints.Value && prefab.GetComponent<TerrainOp>())
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
                    new NamedSnapPoint(Vector3.zero, ORIGIN) // This worked badly during testing (item could not be placed)
                };
            }
            if (prefab.IsTorch())
            {
                return new[]
                {
                    new NamedSnapPoint(Vector3.zero, ORIGIN), // This worked badly during testing (item could not be placed)
                    new NamedSnapPoint(0.0f, -0.7f, 0.0f, EXTRA),
                };
            }
        }
        else if (ShapeClassifier.FormLine(existingSnapPoints) && ExtraSnapPointsMadeEasy.EnableLineSnapPoints.Value)
        {
            return ExtraSnapPointsCalculator.GetExtraPointsForLine(existingSnapPoints, 1);
        }
        else if (ShapeClassifier.FormTriangle(existingSnapPoints) && ExtraSnapPointsMadeEasy.EnableTriangleSnapPoints.Value)
        {
            return ExtraSnapPointsCalculator.GetExtraSnapPointsForTriangle(existingSnapPoints);
        }
        else if (ShapeClassifier.FormRectangle(existingSnapPoints) && ExtraSnapPointsMadeEasy.EnableRect2DSnapPoints.Value)
        {
            return ExtraSnapPointsCalculator.GetSnapPointsForRectangle(existingSnapPoints);
        }
        else if (prefab.IsRoof() && ShapeClassifier.IsWedge3D(existingSnapPoints) && ExtraSnapPointsMadeEasy.EnableRoofTopSnapPoints.Value)
        {
            ShapeClassifier.GetExtraSnapPointsForRoofTop(existingSnapPoints, prefab.name);
        }

        if (!existingSnapPoints.ContainsOriginSnapPoint())
        {
            return OriginSnapPointArray;
        }

        return Empty;
    }

    private static NamedSnapPoint[] NameSnapPoints(Vector3[] positions, string prefix = EXTRA, int startNumber = 1)
    {
        NamedSnapPoint[] result = new NamedSnapPoint[positions.Length];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new NamedSnapPoint(positions[i], $"{prefix} {startNumber + i}");
        }
        return result;
    }
}
