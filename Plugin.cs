using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

// TODO: Look into checking collider values and just using those to dictate snap points for furniture
namespace ExtraSnapPointsMadeEasy
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginName = "ExtraSnapPointsMadeEasy";
        internal const string Author = "Searica";
        public const string PluginGuid = $"{Author}.Valheim.{PluginName}";
        public const string PluginVersion = "1.0.1";

        //public static HashSet<string> SkipLocalCenterSnapPoint = new()
        //{
        //    "piece_chest_private",
        //    "piece_chest",
        //    "piece_chest_wood",
        //    "rug_deer",
        //    "rug_wolf",
        //    "rug_hare", // (scale rug)
        //    "jute_carpet_blue", // (round blue jute carpet)
        //    "rug_fur", // (lox rug)
        //    "jute_carpet", // (red jute carpet)
        //    "windmill",
        //    "piece_wisplure", // wisp fountain
        //    "wood_wall_roof", // 26 degrees
        //    "wood_wall_roof_upsidedown", // 26 degrees
        //    "wood_wall_roof_45", 
        //    "wood_wall_roof_45_upsidedown",
        //    "piece_walltorch", // sconce
        //    "piece_cloth_hanging_door_blue", // (Blue Jute Drape)
        //    "piece_cloth_hanging_door_blue2", // (Blue Jute Curtain)
        //    "cauldron_ext1_spice",
        //    "piece_workbench_ext4", // (Tool shelf)
        //    "darkwood_beam4x4", 
        //};

        Harmony _harmony;

        private void Awake()
        {
            Log.Init(Logger);
            PluginConfig.Init(Config);
            PluginConfig.SetUp();

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
        }

        public void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
