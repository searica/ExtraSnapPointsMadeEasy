using BepInEx;
using BepInEx.Logging;
using ExtraSnapPointsMadeEasy.Helpers;
using ExtraSnapPointsMadeEasy.Configs;
using HarmonyLib;
using System.Reflection;

// TODO: Look into checking collider values and just using those to dictate snap points for furniture
namespace ExtraSnapPointsMadeEasy
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ExtraSnapPointsMadeEasy : BaseUnityPlugin
    {
        internal const string PluginName = "ExtraSnapPointsMadeEasy";
        internal const string Author = "Searica";
        public const string PluginGuid = $"{Author}.Valheim.{PluginName}";
        public const string PluginVersion = "1.2.1";

        internal static ExtraSnapPointsMadeEasy Instance;

        private void Awake()
        {
            Instance = this;
            Log.Init(Logger);
            ConfigManager.Init(Config);
            ConfigManager.SetUp();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);

            ConfigManager.SetupWatcher();
            ConfigManager.CheckForConfigManager();
            ConfigManager.OnConfigWindowClosed += () =>
            {
                SnapPointAdder.AddExtraSnapPoints("Config settings changed, re-initializing");
            };

            ConfigManager.OnConfigFileReloaded += () =>
            {
                SnapPointAdder.AddExtraSnapPoints("Config settings changed after reloading config file, re-initializing");
            };
        }

        private void OnDestroy()
        {
            ConfigManager.Save();
        }

        /// <summary>
        ///     Public API so other mods can rescan piece tables and add
        ///     extra snap points after dynamically adding/removing pieces
        ///     from piece tables.
        /// </summary>
        public static void ReInitExtraSnapPoints()
        {
            //var pluginInfo = BepInExUtils.GetSourceModMetadata();
            //var msg = $"{pluginInfo.Name} triggered a re-initialization, adding extra snap points";
            var msg = $"External mod triggered a re-initialization, adding extra snap points";
            SnapPointAdder.AddExtraSnapPoints(msg, true);
        }
    }

    /// <summary>
    /// Helper class for properly logging from static contexts.
    /// </summary>
    internal static class Log
    {
        internal static ManualLogSource _logSource;

        internal static void Init(ManualLogSource logSource)
        {
            _logSource = logSource;
        }

        internal static void LogDebug(object data) => _logSource.LogDebug(data);

        internal static void LogError(object data) => _logSource.LogError(data);

        internal static void LogFatal(object data) => _logSource.LogFatal(data);

        internal static void LogInfo(object data) => _logSource.LogInfo(data);

        internal static void LogMessage(object data) => _logSource.LogMessage(data);

        internal static void LogWarning(object data) => _logSource.LogWarning(data);
    }
}