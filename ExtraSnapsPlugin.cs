﻿using System;
using System.Collections.Generic;
using System.Reflection;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using ExtraSnapsMadeEasy.Configs;
using ExtraSnapsMadeEasy.ExtraSnapPoints;

// TODO: Look into checking collider values and just using those to dictate snap points for furniture
namespace ExtraSnapsMadeEasy;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
internal sealed class ExtraSnapsPlugin : BaseUnityPlugin
{
    public const string PluginName = "ExtraSnapPointsMadeEasy";
    public const string Author = "Searica";
    public const string PluginGUID = $"{Author}.Valheim.{PluginName}";
    public const string PluginVersion = "2.0.0";

    internal static ExtraSnapsPlugin Instance;

    private const string MainSection = "1 - Global";
    private const string SnapModeSection = "2 - Manual Snapping";
    private const string ExtraSnapsSection = "3 - ​Extra Snap Points";
    private const string PrefabSnapSettings = "4 - Individual Snap Point Settings";

    public ConfigEntry<bool> VanillaManualSnapEnabled { get; private set; }
    public ConfigEntry<KeyCode> TogglePreciseSnap { get; private set; }
    public ConfigEntry<KeyCode> ToggleManualSnap { get; private set; }
    public ConfigEntry<KeyCode> ToggleGridSnap { get; private set; }
    public ConfigEntry<KeyCode> CycleGridPrecision { get; private set; }
    public ConfigEntry<KeyCode> IterateSourceSnapPoints { get; private set; }
    public ConfigEntry<KeyCode> IterateTargetSnapPoints { get; private set; }
    public ConfigEntry<bool> ResetSnapsOnNewPiece { get; private set; }
    public ConfigEntry<bool> EnableExtraSnapPoints { get; private set; }
    public ConfigEntry<bool> EnableLineSnapPoints { get; private set; }
    public ConfigEntry<bool> EnableTriangleSnapPoints { get; private set; }
    public ConfigEntry<bool> EnableRect2DSnapPoints { get; private set; }
    public ConfigEntry<bool> EnableRoofTopSnapPoints { get; private set; }
    public ConfigEntry<bool> EnableTerrainOpSnapPoints { get; private set; }
    internal ConfigEntry<MessageHud.MessageType> NotificationType { get; private set; }

    internal readonly static Dictionary<string, ConfigEntry<bool>> SnapPointSettings = new();
    internal bool ShouldUpdateExtraSnaps { get; set; } = false;

    private void Awake()
    {
        Instance = this;
        Log.Init(Logger);
        Config.Init(PluginGUID, false);
        SetUpConfigEntries();
        Config.Save();

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
        Game.isModded = true;

        Config.SetupWatcher();
        ConfigFileManager.CheckForConfigManager();
        ConfigFileManager.OnConfigWindowClosed += () =>
        {
            ExtraSnapsAdder.AddExtraSnapPoints("Config settings changed, re-initializing");
        };
        ConfigFileManager.OnConfigFileReloaded += () =>
        {
            ExtraSnapsAdder.AddExtraSnapPoints("Config settings changed after reloading config file, re-initializing");
        };
    }

    private void OnDestroy()
    {
        Config.Save();
    }

    public void SetUpConfigEntries()
    {
        Log.Verbosity = Config.BindConfig(
            MainSection,
            "Verbosity",
            LogLevel.Low,
            "Low will log basic information about the mod. Medium will log information that " +
            "is useful for troubleshooting. High will log a lot of information, do not set " +
            "it to this without good reason as it will slow down your game."
        );

        VanillaManualSnapEnabled = Config.BindConfig(
            MainSection,
            "Vanilla Manual Snapping",
            false,
            "Whether vanilla manual snapping is enabled. If disabled then the vanilla keybinds for manual snapping will have no effect."
        );

        TogglePreciseSnap = Config.BindConfig(
            SnapModeSection,
            "Toggle Precise Snap Mode",
            KeyCode.LeftAlt,
            "This key will enable or disable manual snapping mode."
        );

        ToggleManualSnap = Config.BindConfig(
            SnapModeSection,
            "Toggle Manual Snap Mode",
            KeyCode.CapsLock,
            "This key will enable or disable manual closest snapping mode."
        );

        ToggleGridSnap = Config.BindConfig(
            SnapModeSection,
            "Toggle Grid Snap Mode",
            KeyCode.F3,
            "This key will enable or disable snap to grid mode."
        );

        CycleGridPrecision = Config.BindConfig(
            SnapModeSection,
            "Cycle Grid Snap Precision",
            KeyCode.F4,
            "This key will change the precision of the grid in when in grid mode."
        );

        IterateSourceSnapPoints = Config.BindConfig(
            SnapModeSection,
            "Iterate Placing Piece Snap Points",
            KeyCode.LeftControl,
            "This key will cycle through the snap points on the piece you are placing."
        );

        IterateTargetSnapPoints = Config.BindConfig(
            SnapModeSection,
            "Iterate Targeted Piece Points",
            KeyCode.LeftShift,
            "This key will cycle through the snap points on the piece you are attaching to."
        );

        ResetSnapsOnNewPiece = Config.BindConfig(
            SnapModeSection,
            "Reset Snaps On New Piece",
            false,
            "Controls if the selected snap point is reset for each placement, defaults to not reset." +
            "This means your selections carry over between placements."
        );

        NotificationType = Config.BindConfig(
            SnapModeSection,
            "Notification Type",
            MessageHud.MessageType.Center,
            "Set the type of notification for when manual snapping mode is changed or selected snap points are changed. \"Center\" will display in the center of the screen in large yellow text. \"TopLeft\" will display under the hotkey bar in small white text."
        );

        EnableExtraSnapPoints = Config.BindConfig(
            ExtraSnapsSection,
            "Extra Snap Points",
            true,
            "Globally enable/disable all extra snap points."
        );
        EnableExtraSnapPoints.SettingChanged += SnapSettingChanged;

        EnableLineSnapPoints = Config.BindConfig(
            ExtraSnapsSection,
            "Extra Snap Points: Line",
            true,
            "Enabled adds extra snap points for all \"Line\" pieces. " +
            "Disabled will prevent extra snap points being added to any \"Line\" pieces."
        );
        EnableLineSnapPoints.SettingChanged += SnapSettingChanged;

        EnableTriangleSnapPoints = Config.BindConfig(
            ExtraSnapsSection,
            "Extra Snap Points: Triangle",
            true,
            "Enabled adds extra snap points for all \"Triangle\" pieces. " +
            "Disabled will prevent extra snap points being added to any \"Triangle\" pieces."
        );
        EnableTriangleSnapPoints.SettingChanged += SnapSettingChanged;

        EnableRect2DSnapPoints = Config.BindConfig(
            ExtraSnapsSection,
            "Extra Snap Points: 2D-Rectangle",
            true,
            "Enabled adds extra snap points for all \"Rect2D\" pieces. " +
            "Disabled will prevent extra snap points being added to any \"Rect2D\" pieces."
        );
        EnableRect2DSnapPoints.SettingChanged += SnapSettingChanged;

        EnableRoofTopSnapPoints = Config.BindConfig(
           ExtraSnapsSection,
           "Extra Snap Points: Roof Top",
           true,
           "Enabled adds extra snap points for all \"RoofTop\" pieces. " +
           "Disabled will prevent extra snap points being added to any \"RoofTop\" pieces."
        );
        EnableRoofTopSnapPoints.SettingChanged += SnapSettingChanged;


        EnableTerrainOpSnapPoints = Config.BindConfig(
           ExtraSnapsSection,
           "Extra Snap Points: Terrain",
           false,
           "Enabled adds extra snap points for all \"TerrainOp\" pieces like the level ground tool in the Hoe. Disabled will prevent extra snap points being added to any \"TerrainOp\" pieces."
        );
        EnableTerrainOpSnapPoints.SettingChanged += SnapSettingChanged;
    }

    internal ConfigEntry<bool> LoadConfig(GameObject gameObject)
    {
        ConfigEntry<bool> prefabConfig = Config.BindConfig(
            PrefabSnapSettings,
            gameObject.name,
            true,
            "Set to true/enabled to enable snap points for this prefab and false/disabled to disable them."
        );
        prefabConfig.SettingChanged += SnapSettingChanged;
        SnapPointSettings[gameObject.name] = prefabConfig;
        return prefabConfig;
    }

    private void SnapSettingChanged(object o, EventArgs e)
    {
        if (!ShouldUpdateExtraSnaps) { ShouldUpdateExtraSnaps = true; }
    }

    /// <summary>
    ///     Public API so other mods can rescan piece tables and add
    ///     extra snap points after dynamically adding/removing pieces
    ///     from piece tables.
    /// </summary>
    public void ReInitExtraSnapPoints()
    {
        string msg = $"External mod triggered a re-initialization, adding extra snap points";
        ExtraSnapsAdder.AddExtraSnapPoints(msg, true);
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
        foreach (PropertyInfo property in properties)
        {
            LogInfo($" - {property.Name} = {property.GetValue(compo)}");
        }

        FieldInfo[] fields = compo.GetType().GetFields(AllBindings);
        foreach (FieldInfo field in fields)
        {
            LogInfo($" - {field.Name} = {field.GetValue(compo)}");
        }
    }

    #endregion Logging Unity Objects
}
