using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Configs;
using Logging;
using ExtraSnapsMadeEasy.ExtraSnapPoints;


// TODO: Look into checking collider values and just using those to dictate snap points for furniture
namespace ExtraSnapsMadeEasy;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
internal sealed class ExtraSnapsPlugin : BaseUnityPlugin
{
    public const string PluginName = "ExtraSnapPointsMadeEasy";
    public const string Author = "Searica";
    public const string PluginGUID = $"{Author}.Valheim.{PluginName}";
    public const string PluginVersion = "2.0.2";

    internal static ExtraSnapsPlugin Instance;

    private const string MainSection = "Global";
    private const string SnapModeSection = "Manual Snapping";
    private const string ExtraSnapsSection = "​Extra Snap Points";
    private const string PrefabSnapSettings = "Individual Snap Point Settings";

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
        Config.CheckForConfigManager();
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
        Log.Verbosity = Config.BindConfigInOrder(
            MainSection,
            "Verbosity",
            Log.InfoLevel.Low,
            "Low will log basic information about the mod. Medium will log information that " +
            "is useful for troubleshooting. High will log a lot of information, do not set " +
            "it to this without good reason as it will slow down your game."
        );

        VanillaManualSnapEnabled = Config.BindConfigInOrder(
            MainSection,
            "Vanilla Manual Snapping",
            false,
            "Whether vanilla manual snapping is enabled. If disabled then the vanilla keybinds for manual snapping will have no effect."
        );

        TogglePreciseSnap = Config.BindConfigInOrder(
            SnapModeSection,
            "Toggle Manual+ Snap Mode",
            KeyCode.LeftAlt,
            "This key will enable or disable manual snapping mode."
        );

        ToggleManualSnap = Config.BindConfigInOrder(
            SnapModeSection,
            "Toggle Manual Snap Mode",
            KeyCode.CapsLock,
            "This key will enable or disable manual closest snapping mode."
        );

        ToggleGridSnap = Config.BindConfigInOrder(
            SnapModeSection,
            "Toggle Grid Snap Mode",
            KeyCode.F3,
            "This key will enable or disable snap to grid mode."
        );

        CycleGridPrecision = Config.BindConfigInOrder(
            SnapModeSection,
            "Cycle Grid Snap Precision",
            KeyCode.F4,
            "This key will change the precision of the grid in when in grid mode."
        );

        IterateSourceSnapPoints = Config.BindConfigInOrder(
            SnapModeSection,
            "Iterate Placing Piece Snap Points",
            KeyCode.Q,
            "This key will cycle through the snap points on the piece you are placing."
        );

        IterateTargetSnapPoints = Config.BindConfigInOrder(
            SnapModeSection,
            "Iterate Targeted Piece Points",
            KeyCode.E,
            "This key will cycle through the snap points on the piece you are attaching to."
        );

        ResetSnapsOnNewPiece = Config.BindConfigInOrder(
            SnapModeSection,
            "Reset Snaps On New Piece",
            false,
            "Controls if the selected snap point is reset for each placement, defaults to not reset." +
            "This means your selections carry over between placements."
        );

        NotificationType = Config.BindConfigInOrder(
            SnapModeSection,
            "Notification Type",
            MessageHud.MessageType.Center,
            "Set the type of notification for when manual snapping mode is changed or selected snap points are changed. \"Center\" will display in the center of the screen in large yellow text. \"TopLeft\" will display under the hotkey bar in small white text."
        );

        EnableExtraSnapPoints = Config.BindConfigInOrder(
            ExtraSnapsSection,
            "Extra Snap Points",
            true,
            "Globally enable/disable all extra snap points."
        );
        EnableExtraSnapPoints.SettingChanged += SnapSettingChanged;

        EnableLineSnapPoints = Config.BindConfigInOrder(
            ExtraSnapsSection,
            "Extra Snap Points: Line",
            true,
            "Enabled adds extra snap points for all \"Line\" pieces. " +
            "Disabled will prevent extra snap points being added to any \"Line\" pieces."
        );
        EnableLineSnapPoints.SettingChanged += SnapSettingChanged;

        EnableTriangleSnapPoints = Config.BindConfigInOrder(
            ExtraSnapsSection,
            "Extra Snap Points: Triangle",
            true,
            "Enabled adds extra snap points for all \"Triangle\" pieces. " +
            "Disabled will prevent extra snap points being added to any \"Triangle\" pieces."
        );
        EnableTriangleSnapPoints.SettingChanged += SnapSettingChanged;

        EnableRect2DSnapPoints = Config.BindConfigInOrder(
            ExtraSnapsSection,
            "Extra Snap Points: 2D-Rectangle",
            true,
            "Enabled adds extra snap points for all \"Rect2D\" pieces. " +
            "Disabled will prevent extra snap points being added to any \"Rect2D\" pieces."
        );
        EnableRect2DSnapPoints.SettingChanged += SnapSettingChanged;

        EnableRoofTopSnapPoints = Config.BindConfigInOrder(
           ExtraSnapsSection,
           "Extra Snap Points: Roof Top",
           true,
           "Enabled adds extra snap points for all \"RoofTop\" pieces. " +
           "Disabled will prevent extra snap points being added to any \"RoofTop\" pieces."
        );
        EnableRoofTopSnapPoints.SettingChanged += SnapSettingChanged;


        EnableTerrainOpSnapPoints = Config.BindConfigInOrder(
           ExtraSnapsSection,
           "Extra Snap Points: Terrain",
           false,
           "Enabled adds extra snap points for all \"TerrainOp\" pieces like the level ground tool in the Hoe. Disabled will prevent extra snap points being added to any \"TerrainOp\" pieces."
        );
        EnableTerrainOpSnapPoints.SettingChanged += SnapSettingChanged;
    }

    internal ConfigEntry<bool> LoadConfig(GameObject gameObject)
    {
        ConfigEntry<bool> prefabConfig = Config.BindConfigInOrder(
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
