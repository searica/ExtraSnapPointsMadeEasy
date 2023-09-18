using BepInEx.Configuration;
using UnityEngine;


namespace ExtraSnapPointsMadeEasy
{
    public class PluginConfig
    {
        public static void Init(ConfigFile config)
        {
            SnapSettings.Init(config);
        }

        public class SnapSettings
        {
            public static ConfigEntry<KeyCode> EnableManualSnap { get; private set; }
            public static ConfigEntry<KeyCode> IterateSourceSnapPoints { get; private set; }
            public static ConfigEntry<KeyCode> IterateTargetSnapPoints { get; private set; }
            public static ConfigEntry<bool> ResetSnapsOnNewPiece { get; private set; }

            public static void Init(ConfigFile config)
            {
                string name = "SnapSettings";
                EnableManualSnap = config.Bind(name, "EnableManualSnap", KeyCode.LeftAlt,
                    "This key will enable or disable manual snapping mode.");

                IterateSourceSnapPoints = config.Bind(name, "IterateSourceSnapPoints", KeyCode.LeftControl,
                    "This key will cycle through the snap points on the piece you are placing.");

                IterateTargetSnapPoints = config.Bind(name, "IterateTargetSnapPoints", KeyCode.LeftShift,
                    "This key will cycle through the snap points on the piece you are attaching to.");

                ResetSnapsOnNewPiece = config.Bind(name, "ResetSnapsOnNewPiece", false,
                    "Controls if the selected snap point is reset for each placement, default to not reset. This means your selections carry over between placements.");

                Debug.Log($"Loaded settings!\n" +
                    $"EnableManualSnap: {EnableManualSnap.Value}\n" +
                    $"IterateSourceSnapPoints:{IterateSourceSnapPoints.Value}\n" +
                    $"IterateTargetSnapPoints:{IterateTargetSnapPoints.Value}");
            }
        }
    }
}