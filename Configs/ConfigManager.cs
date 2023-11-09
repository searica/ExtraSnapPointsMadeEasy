using BepInEx;
using BepInEx.Configuration;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using BepInEx.Bootstrap;
using System.Reflection;
using UnityEngine.Rendering;
using ExtraSnapPointsMadeEasy.Extensions;

namespace ExtraSnapPointsMadeEasy.Configs
{
    internal class ConfigManager
    {
        private static BaseUnityPlugin ConfigurationManager;
        private const string ConfigManagerGUID = "com.bepis.bepinex.configurationmanager";

        private static ConfigFile configFile = null;
        private static readonly string ConfigFileName = ExtraSnapPointsMadeEasy.PluginGuid + ".cfg";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

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

        internal static ConfigEntry<LoggerLevel> Verbosity { get; private set; }

        internal static ConfigEntry<MessageHud.MessageType> NotificationType { get; private set; }

        internal enum LoggerLevel
        {
            Low = 0,
            Medium = 1,
            High = 2,
        }

        internal static LoggerLevel VerbosityLevel => Verbosity.Value;

        internal static bool IsVerbosityLow => Verbosity.Value >= LoggerLevel.Low;
        internal static bool IsVerbosityMedium => Verbosity.Value >= LoggerLevel.Medium;
        internal static bool IsVerbosityHigh => Verbosity.Value >= LoggerLevel.High;

        internal static bool UpdateExtraSnapPoints { get; set; } = false;

        #region Events

        /// <summary>
        ///     Event triggered after a the in-game configuration manager is closed.
        /// </summary>
        internal static event Action OnConfigWindowClosed;

        /// <summary>
        ///     Safely invoke the <see cref="OnConfigWindowClosed"/> event
        /// </summary>
        private static void InvokeOnConfigWindowClosed()
        {
            OnConfigWindowClosed?.SafeInvoke();
        }

        /// <summary>
        ///     Event triggered after the file watcher reloads the configuration file.
        /// </summary>
        internal static event Action OnConfigFileReloaded;

        /// <summary>
        ///     Safely invoke the <see cref="OnConfigFileReloaded"/> event
        /// </summary>
        private static void InvokeOnConfigFileReloaded()
        {
            OnConfigFileReloaded?.SafeInvoke();
        }

        #endregion Events

        internal static ConfigEntry<T> BindConfig<T>(
            string section,
            string name,
            T value,
            string description,
            AcceptableValueBase acceptVals = null
        )
        {
            ConfigEntry<T> configEntry = configFile.Bind(
                section,
                name,
                value,
                new ConfigDescription(description, acceptVals)
            );
            return configEntry;
        }

        private const char ZWS = '\u200B';

        /// <summary>
        ///     Prepends Zero-Width-Space to set ordering of configuration sections
        /// </summary>
        /// <param name="sectionName">Section name</param>
        /// <param name="priority">Number of ZWS chars to prepend</param>
        /// <returns></returns>
        private static string SetStringPriority(string sectionName, int priority)
        {
            if (priority == 0) { return sectionName; }
            return new string(ZWS, priority) + sectionName;
        }

        private static readonly string MainSection = SetStringPriority("Global", 3);
        private static readonly string SnapModeSection = SetStringPriority("ManualSnapping", 2);
        private static readonly string ExtraSnapsSection = SetStringPriority("​ExtraSnapPoints (Requires Restart)", 1);
        private static readonly string PrefabSnapSettings = "Individual Snap Point Settings (Requires Restart)";

        public static void Init(ConfigFile config)
        {
            configFile = config;
            configFile.SaveOnConfigSet = false;
        }

        public static void SetUp()
        {
            Verbosity = BindConfig(
                MainSection,
                "Verbosity",
                LoggerLevel.Low,
                "Low will log basic information about the mod. Medium will log information that " +
                "is useful for troubleshooting. High will log a lot of information, do not set " +
                "it to this without good reason as it will slow down your game."
            );

            EnableManualSnap = BindConfig(
                SnapModeSection,
                "ToggleManualSnapMode",
                KeyCode.LeftAlt,
                "This key will enable or disable manual snapping mode."
            );

            EnableManualClosestSnap = BindConfig(
                SnapModeSection,
                "ToggleManualClosestMode",
                KeyCode.CapsLock,
                "This key will enable or disable manual closest snapping mode."
            );

            EnableGridSnap = BindConfig(
                SnapModeSection,
                "ToggleSnapToGridMode",
                KeyCode.F3,
                "This key will enable or disable snap to grid mode."
            );

            CycleGridPrecision = BindConfig(
                SnapModeSection,
                "CycleGridPrecision",
                KeyCode.F4,
                "This key will change the precision of the grid in when in grid mode."
            );

            IterateSourceSnapPoints = BindConfig(
                SnapModeSection,
                "IterateSourceSnapPoints",
                KeyCode.LeftControl,
                "This key will cycle through the snap points on the piece you are placing."
            );

            IterateTargetSnapPoints = BindConfig(
                SnapModeSection,
                "IterateTargetSnapPoints",
                KeyCode.LeftShift,
                "This key will cycle through the snap points on the piece you are attaching to."
            );

            ResetSnapsOnNewPiece = BindConfig(
                SnapModeSection,
                "ResetSnapsOnNewPiece",
                false,
                "Controls if the selected snap point is reset for each placement, defaults to not reset." +
                "This means your selections carry over between placements."
            );

            NotificationType = BindConfig(
                SnapModeSection,
                "NotificationType",
                MessageHud.MessageType.Center,
                "Set the type of notification for when manual snapping mode is changed or selected snap points are changed. \"Center\" will display in the center of the screen in large yellow text. \"TopLeft\" will display under the hotkey bar in small white text."
            );

            EnableExtraSnapPoints = BindConfig(
                ExtraSnapsSection,
                "ExtraSnapPoints",
                true,
                "Globally enable/disable all extra snap points."
            );
            EnableExtraSnapPoints.SettingChanged += SnapSettingChanged;

            EnableLineSnapPoints = BindConfig(
                ExtraSnapsSection,
                "LineSnapPoints",
                true,
                "Enabled adds extra snap points for all \"Line\" pieces. " +
                "Disabled will prevent extra snap points being added to any \"Line\" pieces."
            );
            EnableLineSnapPoints.SettingChanged += SnapSettingChanged;

            EnableTriangleSnapPoints = BindConfig(
                ExtraSnapsSection,
                "TriangleSnapPoints",
                true,
                "Enabled adds extra snap points for all \"Triangle\" pieces. " +
                "Disabled will prevent extra snap points being added to any \"Triangle\" pieces."
            );
            EnableTriangleSnapPoints.SettingChanged += SnapSettingChanged;

            EnableRect2DSnapPoints = BindConfig(
                ExtraSnapsSection,
                "Rect2DSnapPoints",
                true,
                "Enabled adds extra snap points for all \"Rect2D\" pieces. " +
                "Disabled will prevent extra snap points being added to any \"Rect2D\" pieces."
            );
            EnableRect2DSnapPoints.SettingChanged += SnapSettingChanged;

            EnableRoofTopSnapPoints = BindConfig(
               ExtraSnapsSection,
               "RoofTopSnapPoints",
               true,
               "Enabled adds extra snap points for all \"RoofTop\" pieces. " +
               "Disabled will prevent extra snap points being added to any \"RoofTop\" pieces."
           );
            EnableRoofTopSnapPoints.SettingChanged += SnapSettingChanged;

            Save();
        }

        internal static ConfigEntry<bool> LoadConfig(GameObject gameObject)
        {
            ConfigEntry<bool> prefabConfig = BindConfig(
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
        ///     Sets SaveOnConfigSet to false and returns
        ///     the value prior to calling this method.
        /// </summary>
        /// <returns></returns>
        private static bool DisableSaveOnConfigSet()
        {
            var val = configFile.SaveOnConfigSet;
            configFile.SaveOnConfigSet = false;
            return val;
        }

        /// <summary>
        ///     Set the value for the SaveOnConfigSet field.
        /// </summary>
        /// <param name="value"></param>
        internal static void SaveOnConfigSet(bool value)
        {
            configFile.SaveOnConfigSet = value;
        }

        /// <summary>
        ///     Save config file to disk.
        /// </summary>
        internal static void Save()
        {
            configFile.Save();
        }

        #region FileWatcher

        internal static void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReloadConfigFile;
            watcher.Created += ReloadConfigFile;
            watcher.Renamed += ReloadConfigFile;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private static void ReloadConfigFile(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) { return; }
            try
            {
                Log.LogInfo("Reloading config file");

                // turn off saving on config entry set
                var saveOnConfigSet = DisableSaveOnConfigSet();
                configFile.Reload();
                SaveOnConfigSet(saveOnConfigSet); // reset config saving state
                InvokeOnConfigFileReloaded(); // fire event
            }
            catch
            {
                Log.LogError($"There was an issue loading your {ConfigFileName}");
                Log.LogError("Please check your config entries for spelling and format!");
            }
        }

        #endregion FileWatcher

        #region ConfigManager

        /// <summary>
        ///     Checks for in-game configuration manager and
        ///     sets up OnConfigWindowClosed event if it is present
        /// </summary>
        internal static void CheckForConfigManager()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                return;
            }

            if (Chainloader.PluginInfos.TryGetValue(ConfigManagerGUID, out PluginInfo configManagerInfo) && configManagerInfo.Instance)
            {
                ConfigurationManager = configManagerInfo.Instance;
                Log.LogDebug("Configuration manager found, hooking DisplayingWindowChanged");

                EventInfo eventinfo = ConfigurationManager.GetType().GetEvent("DisplayingWindowChanged");

                if (eventinfo != null)
                {
                    Action<object, object> local = new(OnConfigManagerDisplayingWindowChanged);
                    Delegate converted = Delegate.CreateDelegate(
                        eventinfo.EventHandlerType,
                        local.Target,
                        local.Method
                    );
                    eventinfo.AddEventHandler(ConfigurationManager, converted);
                }
            }
        }

        private static void OnConfigManagerDisplayingWindowChanged(object sender, object e)
        {
            PropertyInfo pi = ConfigurationManager.GetType().GetProperty("DisplayingWindow");
            bool ConfigurationManagerWindowShown = (bool)pi.GetValue(ConfigurationManager, null);

            if (!ConfigurationManagerWindowShown)
            {
                InvokeOnConfigWindowClosed();
            }
        }

        #endregion ConfigManager
    }
}