using BepInEx;
using BepInEx.Configuration;
using System.IO;
using UnityEngine;

namespace ExtraSnapPointsMadeEasy
{
    public class PluginConfig
    {
        private static ConfigFile configFile = null;
        private static readonly string ConfigFileName = Plugin.PluginGuid + ".cfg";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private const string MainSectionName = "\u200BGlobal";
        public static ConfigEntry<KeyCode> EnableManualSnap { get; private set; }
        public static ConfigEntry<KeyCode> EnableManualClosestSnap { get; private set; }
        public static ConfigEntry<KeyCode> IterateSourceSnapPoints { get; private set; }
        public static ConfigEntry<KeyCode> IterateTargetSnapPoints { get; private set; }
        public static ConfigEntry<bool> ResetSnapsOnNewPiece { get; private set; }

        internal static ConfigEntry<T> BindConfig<T>(string group, string name, T value, ConfigDescription description)
        {
            ConfigEntry<T> configEntry = configFile.Bind(group, name, value, description);
            return configEntry;
        }

        internal static ConfigEntry<T> BindConfig<T>(string group, string name, T value, string description) => BindConfig(group, name, value, new ConfigDescription(description));

        internal static readonly AcceptableValueList<bool> AcceptableToggleValuesList = new(new bool[] { true, false });

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
                "Controls if the selected snap point is reset for each placement, default to not reset. " +
                "This means your selections carry over between placements."
            );

            Save();

            Log.LogInfo(
                $"Loaded settings!\n" +
                $"EnableManualSnap: {EnableManualSnap.Value}\n" +
                $"EnableManualClosestSnap: {EnableManualClosestSnap.Value}\n" +
                $"IterateSourceSnapPoints:{IterateSourceSnapPoints.Value}\n" +
                $"IterateTargetSnapPoints:{IterateTargetSnapPoints.Value}"
            );
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
                Log.LogInfo("ReadConfigValues called");
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
        }
    }
}