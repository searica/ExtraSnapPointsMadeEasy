using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

// TODO: Look into checking collider values and just using those to dictate snap points for furniture
namespace ExtraSnapPointsMadeEasy
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginName = "ExtraSnapPointsMadeEasy";
        internal const string Author = "Searica";
        public const string PluginGuid = $"{Author}.Valheim.{PluginName}";
        public const string PluginVersion = "1.0.6";

        public static HashSet<string> SkipLocalCenterSnapPoint = new()
        {
            "dvergrprops_hooknchain",
        };

        Harmony _harmony;

        private void Awake()
        {
            Log.Init(Logger);
            PluginConfig.Init(Config);
            PluginConfig.SetUp();

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);

            PluginConfig.SetupWatcher();
        }

        public void OnDestroy()
        {
            PluginConfig.Save();
            _harmony?.UnpatchSelf();
        }
    }
}
