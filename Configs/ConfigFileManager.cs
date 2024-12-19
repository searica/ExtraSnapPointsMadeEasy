using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Rendering;

namespace ExtraSnapsMadeEasy.Configs;


/// <summary>
///     Extends ConfigFile with a convenience method to bind config entries with less boilerplate code 
///     and explicitly expose commonly used configuration manager attributes. Also adds an event when
///     config manager display window closes and can set up a file watcher.
/// </summary>
public static class ConfigFileManager
{
    internal static string ConfigFileName;
    internal static string ConfigFileFullPath;
    internal static DateTime lastRead = DateTime.MinValue;

    private static readonly List<string> ConfigManagerGUIDs = new()
    {
        "_shudnal.ConfigurationManager",
        "com.bepis.bepinex.configurationmanager"
    };

    private static BaseUnityPlugin _configManager = null;
    private static BaseUnityPlugin ConfigManager => _configManager ??= GetConfigManager();
    private static BaseUnityPlugin GetConfigManager()
    {
        foreach (string GUID in ConfigManagerGUIDs)
        {
            if (Chainloader.PluginInfos.TryGetValue(GUID, out PluginInfo configManagerInfo) && configManagerInfo.Instance)
            {
                return configManagerInfo.Instance;
            }
        }
        return null;
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

    /// <summary>
    ///     Event triggered after a the in-game configuration manager is closed.
    /// </summary>
    internal static event Action OnConfigWindowClosed;

    /// <summary>
    ///     Safely invoke the <see cref="OnConfigWindowClosed"/> event.
    /// </summary>
    private static void InvokeOnConfigWindowClosed()
    {
        OnConfigWindowClosed?.SafeInvoke();
    }

    /// <summary>
    ///     Initialize Config file with saveOnConfig setting and get filepath for config file watcher.
    /// </summary>
    /// <param name="configFile"></param>
    /// <param name="GUID"></param>
    /// <param name="saveOnConfigSet"></param>
    public static void Init(this ConfigFile configFile, string GUID, bool saveOnConfigSet = false)
    {
        configFile.SaveOnConfigSet = saveOnConfigSet;
        ConfigFileName = GUID + ".cfg";
        ConfigFileFullPath = Path.Combine(Paths.ConfigPath, ConfigFileName);
    }

    /// <summary>
    ///     Sets SaveOnConfigSet to false and returns
    ///     the Value prior to calling this method.
    /// </summary>
    /// <returns></returns>
    public static bool DisableSaveOnConfigSet(this ConfigFile configFile)
    {
        bool val = configFile.SaveOnConfigSet;
        configFile.SaveOnConfigSet = false;
        return val;
    }

    /// <summary>
    ///     Bind a new config entry to the config file and modify description to state whether the config entry is synced or not.
    /// </summary>
    /// <typeparam name="T">Type of the value the config entry holds.</typeparam>
    /// <param name="configFile">Configuration file to bind the config entry to.</param>
    /// <param name="section">Configuration file section to list the config entry in.</param>
    /// <param name="name">Display name of the config entry.</param>
    /// <param name="value">Default value of the config entry.</param>
    /// <param name="description">Plain text description of the config entry to display as hover text in configuration manager.</param>
    /// <param name="acceptVals">Acceptable values for config entry as an AcceptableValueRange, AcceptableValueList, or custom subclass.</param>
    /// <param name="synced">Whether the config entry IsAdminOnly and should be synced with server.</param>
    /// <param name="order">Order of the setting on the settings list relative to other settings in a category. 0 by default, higher number is higher on the list.</param>
    /// <param name="drawer">Custom setting editor (OnGUI code that replaces the default editor provided by ConfigurationManager).</param>
    /// <param name="configAttributes">Optional config manager attributes for additional user specified functionality. Any optional fields specified by the arguments of BindConfig will be overwritten by the parameters passed to BindConfig.</param>
    /// <returns>ConfigEntry bound to the config file.</returns>
    public static ConfigEntry<T> BindConfig<T>(
        this ConfigFile configFile,
        string section,
        string name,
        T value,
        string description,
        AcceptableValueBase acceptVals = null,
        int order = 0,
        int sectionOrder = 0,
        Action<ConfigEntryBase> drawer = null,
        ConfigurationManagerAttributes configAttributes = null
    )
    {
        string orderedSectionName = GetOrderedSectionName(section, sectionOrder);

        configAttributes ??= new ConfigurationManagerAttributes();
        configAttributes.Order = order;
        if (drawer != null)
        {
            configAttributes.CustomDrawer = drawer;
        }

        ConfigEntry<T> configEntry = configFile.Bind(
            orderedSectionName,
            name,
            value,
            new ConfigDescription(
                description,
                acceptVals,
                configAttributes
            )
        );
        return configEntry;
    }

    internal static string GetOrderedSectionName(string section, int sectionOrder)
    {
        if (sectionOrder > 0)
        {
            return $"{sectionOrder} - {section}";
        }
        return section;
    }

    internal static string GetExtendedDescription(string description, bool synchronizedSetting)
    {
        // these two hardcoded strings should probably be localized
        return description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]");
    }

    internal static void SetupWatcher(this ConfigFile configFile)
    {
        var watcher = new FileSystemWatcher(Paths.ConfigPath, ConfigFileName);
        watcher.Changed += configFile.ReloadConfigFile;
        watcher.Created += configFile.ReloadConfigFile;
        watcher.Renamed += configFile.ReloadConfigFile;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    internal static void ReloadConfigFile(this ConfigFile configFile, object sender, FileSystemEventArgs eventArgs)
    {
        if (!File.Exists(ConfigFileFullPath))
        {
            return;
        }

        try
        {
            DateTime lastWriteTime = File.GetLastWriteTime(eventArgs.FullPath);
            if (lastRead != lastWriteTime)
            {
                Log.LogInfo("Reloading config file");
                bool saveOnConfigSet = configFile.DisableSaveOnConfigSet(); // turn off saving on config entry set
                configFile.Reload();
                configFile.SaveOnConfigSet = saveOnConfigSet; // reset config saving state

                InvokeOnConfigFileReloaded(); // fire event
                lastRead = lastWriteTime;
            }
        }
        catch
        {
            Log.LogError($"There was an issue loading your {ConfigFileName}");
            Log.LogError("Please check your config entries for spelling and format!");
        }
    }

    /// <summary>
    ///     Checks for in-game configuration manager and
    ///     sets up OnConfigWindowClosed event if it is present.
    /// </summary>
    internal static void CheckForConfigManager()
    {
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
        {
            return;
        }

        if (ConfigManager != null)
        {
            Log.LogDebug("Configuration manager found, hooking DisplayingWindowChanged");

            EventInfo eventinfo = ConfigManager.GetType().GetEvent("DisplayingWindowChanged");

            if (eventinfo != null)
            {
                Action<object, object> local = new(OnConfigManagerDisplayingWindowChanged);
                Delegate converted = Delegate.CreateDelegate(
                    eventinfo.EventHandlerType,
                    local.Target,
                    local.Method
                );
                eventinfo.AddEventHandler(ConfigManager, converted);
            }
        }
    }

    private static void OnConfigManagerDisplayingWindowChanged(object sender, object e)
    {
        PropertyInfo pi = ConfigManager.GetType().GetProperty("DisplayingWindow");
        bool ConfigurationManagerWindowShown = (bool)pi.GetValue(ConfigManager, null);
        if (!ConfigurationManagerWindowShown)
        {
            InvokeOnConfigWindowClosed();
        }
    }
}



