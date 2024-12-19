using System.Collections.Generic;


namespace ExtraSnapsMadeEasy.Models;
internal static class SnapModes
{
    internal enum SnapMode
    {
        Auto,
        Precise,
        Manual,
        Grid
    }

    internal static readonly Dictionary<SnapMode, string> SnapModeMsg = new()
    {
        {SnapMode.Auto,  "Snap Mode: Auto"},
        {SnapMode.Precise, "Snap Mode: Precise"},
        {SnapMode.Manual, "Snap Mode: Manual"},
        {SnapMode.Grid, "SnapMode: Grid"}
    };
}
