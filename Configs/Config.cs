using BepInEx;
using BepInEx.Configuration;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using BepInEx.Bootstrap;
using System.Reflection;
using UnityEngine.Rendering;
using ExtraSnapPointsMadeEasy.Helpers;

namespace ExtraSnapPointsMadeEasy.Configs
{
    public class Config
    {
        private static BaseUnityPlugin configurationManager;

        private static ConfigFile configFile = null;
        private static readonly string ConfigFileName = ExtraSnapPointsMadeEasy.PluginGuid + ".cfg";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private const string MainSectionName = "\u200BGlobal";
        public static ConfigEntry<KeyCode> EnableManualSnap { get; private set; }
        public static ConfigEntry<KeyCode> EnableManualClosestSnap { get; private set; }
        public static ConfigEntry<KeyCode> IterateSourceSnapPoints { get; private set; }
        public static ConfigEntry<KeyCode> IterateTargetSnapPoints { get; private set; }
        public static ConfigEntry<bool> ResetSnapsOnNewPiece { get; private set; }
        public static ConfigEntry<bool> DisableExtraSnapPoints { get; private set; }

        internal static Dictionary<string, ConfigEntry<bool>> SnapPointSettings = new();

        internal static ConfigEntry<LoggerLevel> Verbosity { get; private set; }

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

        internal static readonly AcceptableValueList<bool> AcceptableToggleValuesList = new(new bool[] { true, false });

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
                new ConfigDescription(
                    description,
                    acceptVals
                )
            );
            return configEntry;
        }

        internal static ConfigEntry<T> BindConfig<T>(string group, string name, T value, ConfigDescription description)
        {
            ConfigEntry<T> configEntry = configFile.Bind(group, name, value, description);
            return configEntry;
        }

        public static void Init(ConfigFile config)
        {
            configFile = config;
            configFile.SaveOnConfigSet = false;
        }

        public static void SetUp()
        {
            EnableManualSnap = BindConfig(
                MainSectionName,
                "EnableManualSnap",
                KeyCode.LeftAlt,
                "This key will enable or disable manual snapping mode."
            );

            EnableManualClosestSnap = BindConfig(
                MainSectionName,
                "EnableManualClosestSnap",
                KeyCode.CapsLock,
                "This key will enable or disable manual closest snapping mode."
            );

            IterateSourceSnapPoints = BindConfig(
                MainSectionName,
                "IterateSourceSnapPoints",
                KeyCode.LeftControl,
                "This key will cycle through the snap points on the piece you are placing."
            );

            IterateTargetSnapPoints = BindConfig(
                MainSectionName,
                "IterateTargetSnapPoints",
                KeyCode.LeftShift,
                "This key will cycle through the snap points on the piece you are attaching to."
            );

            ResetSnapsOnNewPiece = BindConfig(
                MainSectionName,
                "ResetSnapsOnNewPiece",
                false,
                "Controls if the selected snap point is reset for each placement, " +
                "defaults to not reset. This means your selections carry over between placements.",
                AcceptableToggleValuesList
            );

            DisableExtraSnapPoints = BindConfig(
                MainSectionName,
                "DisableExtraSnapPoints",
                false,
                "Globally disable all extra snap points.",
                AcceptableToggleValuesList
            );
            DisableExtraSnapPoints.SettingChanged += SnapSettingChanged;

            Verbosity = BindConfig(
                MainSectionName,
                "Verbosity",
                LoggerLevel.Low,
                "Low will log basic information about the mod. Medium will log information that " +
                "is useful for troubleshooting. High will log a lot of information, do not set " +
                "it to this without good reason as it will slow down your game."
            );

            Save();
        }

        internal static ConfigEntry<bool> LoadConfig(GameObject gameObject)
        {
            ConfigEntry<bool> prefabConfig = BindConfig(
                "SnapPoints",
                gameObject.name,
                true,
                new ConfigDescription(
                    "Set to True to enable snap points for this prefab (Requires Restart).",
                    AcceptableToggleValuesList
                )
            );
            prefabConfig.SettingChanged += SnapSettingChanged;
            SnapPointSettings[gameObject.name] = prefabConfig;
            return prefabConfig;
        }

        internal static void SnapSettingChanged(object o, EventArgs e)
        {
            if (!UpdateExtraSnapPoints)
            {
                UpdateExtraSnapPoints = true;
            }
        }

        public static void Save()
        {
            configFile.Save();
        }

        internal static void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private static void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Log.LogInfo("Reloading config file");
                var saveOnConfig = configFile.SaveOnConfigSet;
                configFile.SaveOnConfigSet = false;
                configFile.Reload();
                configFile.SaveOnConfigSet = saveOnConfig;
            }
            catch
            {
                Log.LogError($"There was an issue loading your {ConfigFileName}");
                Log.LogError("Please check your config entries for spelling and format!");
            }
            var msg = "Config settings changed after reloading config file, re-intializing";
            SnapPointAdder.AddExtraSnapPoints(msg);
        }

        internal static void CheckForConfigManager()
        {
            // Is headless server
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                return;
            }

            if (
                Chainloader.PluginInfos.TryGetValue(
                    "com.bepis.bepinex.configurationmanager",
                    out PluginInfo configManagerInfo
                )
                && configManagerInfo.Instance
            )
            {
                configurationManager = configManagerInfo.Instance;
                Log.LogDebug("Configuration manager found, hooking DisplayingWindowChanged");

                EventInfo eventinfo = configurationManager.GetType()
                    .GetEvent("DisplayingWindowChanged");

                if (eventinfo != null)
                {
                    Action<object, object> local = new(OnConfigManagerDisplayingWindowChanged);
                    Delegate converted = Delegate.CreateDelegate(
                        eventinfo.EventHandlerType,
                        local.Target,
                        local.Method
                    );
                    eventinfo.AddEventHandler(configurationManager, converted);
                }
            }
        }

        private static void OnConfigManagerDisplayingWindowChanged(object sender, object e)
        {
            PropertyInfo pi = configurationManager.GetType().GetProperty("DisplayingWindow");
            bool cmActive = (bool)pi.GetValue(configurationManager, null);

            if (!cmActive)
            {
                var msg = "Config settings changed via in-game manager, re-intializing";
                SnapPointAdder.AddExtraSnapPoints(msg);
            }
        }
    }
}