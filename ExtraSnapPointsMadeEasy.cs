using BepInEx;
using BepInEx.Logging;
using ExtraSnapPointsMadeEasy.Helpers;
using ExtraSnapPointsMadeEasy.Configs;
using HarmonyLib;
using System.Reflection;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;
using System;

// TODO: Look into checking collider values and just using those to dictate snap points for furniture
namespace ExtraSnapPointsMadeEasy
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class ExtraSnapPointsMadeEasy : BaseUnityPlugin
    {
        internal const string PluginName = "ExtraSnapPointsMadeEasy";
        internal const string Author = "Searica";
        public const string PluginGUID = $"{Author}.Valheim.{PluginName}";
        public const string PluginVersion = "1.2.2";

        private static readonly string MainSection = ConfigManager.SetStringPriority("Global", 3);
        private static readonly string SnapModeSection = ConfigManager.SetStringPriority("ManualSnapping", 2);
        private static readonly string ExtraSnapsSection = ConfigManager.SetStringPriority("​ExtraSnapPoints", 1);
        private static readonly string PrefabSnapSettings = "Individual Snap Point Settings";

        public static ConfigEntry<KeyCode> EnableManualSnap { get; private set; }
        public static ConfigEntry<KeyCode> EnableManualClosestSnap { get; private set; }
        public static ConfigEntry<KeyCode> EnableGridSnap { get; private set; }
        public static ConfigEntry<KeyCode> CycleGridPrecision { get; private set; }
        public static ConfigEntry<KeyCode> IterateSourceSnapPoints { get; private set; }
        public static ConfigEntry<KeyCode> IterateTargetSnapPoints { get; private set; }
        public static ConfigEntry<bool> ResetSnapsOnNewPiece { get; private set; }
        public static ConfigEntry<bool> EnableExtraSnapPoints { get; private set; }
        public static ConfigEntry<bool> EnableLineSnapPoints { get; private set; }
        public static ConfigEntry<bool> EnableTriangleSnapPoints { get; private set; }
        public static ConfigEntry<bool> EnableRect2DSnapPoints { get; private set; }
        public static ConfigEntry<bool> EnableRoofTopSnapPoints { get; private set; }

        internal static Dictionary<string, ConfigEntry<bool>> SnapPointSettings = new();

        internal static ConfigEntry<MessageHud.MessageType> NotificationType { get; private set; }

        internal static bool UpdateExtraSnapPoints { get; set; } = false;

        internal static ExtraSnapPointsMadeEasy Instance;

        private void Awake()
        {
            Instance = this;
            Log.Init(Logger);
            ConfigManager.Init(PluginGUID, Config, false);
            Initialize();
            ConfigManager.Save();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);

            Game.isModded = true;

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

        public static void Initialize()
        {
            Log.Verbosity = ConfigManager.BindConfig(
                MainSection,
                "Verbosity",
                LogLevel.Low,
                "Low will log basic information about the mod. Medium will log information that " +
                "is useful for troubleshooting. High will log a lot of information, do not set " +
                "it to this without good reason as it will slow down your game."
            );

            EnableManualSnap = ConfigManager.BindConfig(
                SnapModeSection,
                "ToggleManualSnapMode",
                KeyCode.LeftAlt,
                "This key will enable or disable manual snapping mode."
            );

            EnableManualClosestSnap = ConfigManager.BindConfig(
                SnapModeSection,
                "ToggleManualClosestMode",
                KeyCode.CapsLock,
                "This key will enable or disable manual closest snapping mode."
            );

            EnableGridSnap = ConfigManager.BindConfig(
                SnapModeSection,
                "ToggleSnapToGridMode",
                KeyCode.F3,
                "This key will enable or disable snap to grid mode."
            );

            CycleGridPrecision = ConfigManager.BindConfig(
                SnapModeSection,
                "CycleGridPrecision",
                KeyCode.F4,
                "This key will change the precision of the grid in when in grid mode."
            );

            IterateSourceSnapPoints = ConfigManager.BindConfig(
                SnapModeSection,
                "IterateSourceSnapPoints",
                KeyCode.LeftControl,
                "This key will cycle through the snap points on the piece you are placing."
            );

            IterateTargetSnapPoints = ConfigManager.BindConfig(
                SnapModeSection,
                "IterateTargetSnapPoints",
                KeyCode.LeftShift,
                "This key will cycle through the snap points on the piece you are attaching to."
            );

            ResetSnapsOnNewPiece = ConfigManager.BindConfig(
                SnapModeSection,
                "ResetSnapsOnNewPiece",
                false,
                "Controls if the selected snap point is reset for each placement, defaults to not reset." +
                "This means your selections carry over between placements."
            );

            NotificationType = ConfigManager.BindConfig(
                SnapModeSection,
                "NotificationType",
                MessageHud.MessageType.Center,
                "Set the type of notification for when manual snapping mode is changed or selected snap points are changed. \"Center\" will display in the center of the screen in large yellow text. \"TopLeft\" will display under the hotkey bar in small white text."
            );

            EnableExtraSnapPoints = ConfigManager.BindConfig(
                ExtraSnapsSection,
                "ExtraSnapPoints",
                true,
                "Globally enable/disable all extra snap points."
            );
            EnableExtraSnapPoints.SettingChanged += SnapSettingChanged;

            EnableLineSnapPoints = ConfigManager.BindConfig(
                ExtraSnapsSection,
                "LineSnapPoints",
                true,
                "Enabled adds extra snap points for all \"Line\" pieces. " +
                "Disabled will prevent extra snap points being added to any \"Line\" pieces."
            );
            EnableLineSnapPoints.SettingChanged += SnapSettingChanged;

            EnableTriangleSnapPoints = ConfigManager.BindConfig(
                ExtraSnapsSection,
                "TriangleSnapPoints",
                true,
                "Enabled adds extra snap points for all \"Triangle\" pieces. " +
                "Disabled will prevent extra snap points being added to any \"Triangle\" pieces."
            );
            EnableTriangleSnapPoints.SettingChanged += SnapSettingChanged;

            EnableRect2DSnapPoints = ConfigManager.BindConfig(
                ExtraSnapsSection,
                "Rect2DSnapPoints",
                true,
                "Enabled adds extra snap points for all \"Rect2D\" pieces. " +
                "Disabled will prevent extra snap points being added to any \"Rect2D\" pieces."
            );
            EnableRect2DSnapPoints.SettingChanged += SnapSettingChanged;

            EnableRoofTopSnapPoints = ConfigManager.BindConfig(
               ExtraSnapsSection,
               "RoofTopSnapPoints",
               true,
               "Enabled adds extra snap points for all \"RoofTop\" pieces. " +
               "Disabled will prevent extra snap points being added to any \"RoofTop\" pieces."
           );
            EnableRoofTopSnapPoints.SettingChanged += SnapSettingChanged;
        }

        internal static ConfigEntry<bool> LoadConfig(GameObject gameObject)
        {
            ConfigEntry<bool> prefabConfig = ConfigManager.BindConfig(
                PrefabSnapSettings,
                gameObject.name,
                true,
                "Set to true/enabled to enable snap points for this prefab and false/disabled to disable them."
            );
            prefabConfig.SettingChanged += SnapSettingChanged;
            SnapPointSettings[gameObject.name] = prefabConfig;
            return prefabConfig;
        }

        internal static void SnapSettingChanged(object o, EventArgs e)
        {
            if (!UpdateExtraSnapPoints) { UpdateExtraSnapPoints = true; }
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
    ///     Log level to control output to BepInEx log
    /// </summary>
    internal enum LogLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
    }

    /// <summary>
    ///     Helper class for properly logging from static contexts.
    /// </summary>
    internal static class Log
    {
        #region Verbosity

        internal static ConfigEntry<LogLevel> Verbosity { get; set; }
        internal static LogLevel VerbosityLevel => Verbosity.Value;

        internal static bool IsVerbosityLow => Verbosity.Value >= LogLevel.Low;
        internal static bool IsVerbosityMedium => Verbosity.Value >= LogLevel.Medium;
        internal static bool IsVerbosityHigh => Verbosity.Value >= LogLevel.High;

        #endregion Verbosity

        internal static ManualLogSource _logSource;

        private const BindingFlags AllBindings =
            BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.GetField
            | BindingFlags.SetField
            | BindingFlags.GetProperty
            | BindingFlags.SetProperty;

        internal static void Init(ManualLogSource logSource)
        {
            _logSource = logSource;
        }

        internal static void LogDebug(object data) => _logSource.LogDebug(data);

        internal static void LogError(object data) => _logSource.LogError(data);

        internal static void LogFatal(object data) => _logSource.LogFatal(data);

        internal static void LogInfo(object data, LogLevel level = LogLevel.Low)
        {
            if (Verbosity is null || VerbosityLevel >= level)
            {
                _logSource.LogInfo(data);
            }
        }

        internal static void LogMessage(object data) => _logSource.LogMessage(data);

        internal static void LogWarning(object data) => _logSource.LogWarning(data);

        #region Logging Unity Objects

        internal static void LogGameObject(GameObject prefab, bool includeChildren = false)
        {
            LogInfo("***** " + prefab.name + " *****");
            foreach (Component compo in prefab.GetComponents<Component>())
            {
                LogComponent(compo);
            }

            if (!includeChildren) { return; }

            LogInfo("***** " + prefab.name + " (children) *****");
            foreach (Transform child in prefab.transform)
            {
                LogInfo($" - {child.gameObject.name}");
                foreach (Component compo in child.gameObject.GetComponents<Component>())
                {
                    LogComponent(compo);
                }
            }
        }

        internal static void LogComponent(Component compo)
        {
            LogInfo($"--- {compo.GetType().Name}: {compo.name} ---");

            PropertyInfo[] properties = compo.GetType().GetProperties(AllBindings);
            foreach (var property in properties)
            {
                LogInfo($" - {property.Name} = {property.GetValue(compo)}");
            }

            FieldInfo[] fields = compo.GetType().GetFields(AllBindings);
            foreach (var field in fields)
            {
                LogInfo($" - {field.Name} = {field.GetValue(compo)}");
            }
        }

        #endregion Logging Unity Objects
    }
}