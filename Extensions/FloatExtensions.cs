using UnityEngine;

namespace ExtraSnapsMadeEasy.Extensions;
internal static class FloatExtensions
{

    private const float Tolerance = 1e-6f;

    /// <summary>
    ///     Checks equality using both relative and absolute tolerance.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    internal static bool Equals(this float x, float y, float eps = Tolerance)
    {
        float diff = Mathf.Abs(x - y);
        return diff <= eps || diff <= Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) * eps;
    }

    /// <summary>
    ///     Round to nearest multiple of precision (midpoint rounds away from zero)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="precision"></param>
    /// <returns></returns>
    internal static float RoundToNearest(this float x, float precision)
    {
        return (precision != 0.0)
            ? Mathf.Sign(x) * Mathf.Floor((Mathf.Abs(x) / precision) + 0.5f) * precision
            : x
        ;
    }
}
