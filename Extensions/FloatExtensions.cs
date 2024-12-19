using UnityEngine;

namespace ExtraSnapsMadeEasy.Extensions;
internal static class FloatExtensions
{
    /// <summary>
    ///     Round to nearest multiple of precision (midpoint rounds away from zero)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="precision"></param>
    /// <returns></returns>
    internal static float RoundToNearest(this float x, float precision)
    {
        if (precision <= 0) { return x; }
        float sign = Mathf.Sign(x);

        int val = (int)Mathf.Abs(x * 1000f);
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
