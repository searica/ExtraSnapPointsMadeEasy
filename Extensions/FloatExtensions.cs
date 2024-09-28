using UnityEngine;

namespace ExtraSnapPointsMadeEasy.Extensions;

internal static class FloatExtensions
{
    /// <summary>
    /// Round to nearest multiple of precision (midpoint rounds away from zero)
    /// </summary>
    internal static float RoundToNearest(this float value, float precision)
    {
        if (precision <= 0)
        {
            return value;
        }
        float sign = Mathf.Sign(value);

        int val = (int)Mathf.Abs(value * 1000f);
        int whole = val / 1000;
        int fraction = val % 1000;

        int midPoint = (int)(precision * 1000f / 2f);

        if (fraction < midPoint)
        {
            return sign * whole;
        }
        return sign * (whole + precision);
    }
}
