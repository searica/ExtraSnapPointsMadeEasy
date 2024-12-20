using System.Collections.Generic;
using System.Linq;


namespace ExtraSnapsMadeEasy.Models;
internal static class SnapModes
{
    internal enum SnapMode
    {
        Auto,
        Manual,
        Precise,
        Grid
    }

    internal static readonly Dictionary<SnapMode, string> SnapModeNames = new()
    {
        {SnapMode.Auto,  "Auto"},
        {SnapMode.Manual, "Manual"},
        {SnapMode.Precise, "Manual+"},
        {SnapMode.Grid, "Grid"}
    };
    private static string AutoModeName => SnapModeNames[SnapMode.Auto];

    internal static string GetSnapModeNames(string separator = "/")
    {
        return string.Join(separator, SnapModeNames.Values.Where(x => x != AutoModeName).ToArray());
    }
}
