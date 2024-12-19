using System.Collections.Generic;


namespace ExtraSnapsMadeEasy.Models;
internal static class GridSnapping
{
    internal enum GridPrecision
    {
        Low,
        High,
    }

    internal static Dictionary<GridPrecision, float> GridPrecisionMap = new()
    {
        { GridPrecision.High, 0.5f },
        { GridPrecision.Low, 1f }
    };

}
