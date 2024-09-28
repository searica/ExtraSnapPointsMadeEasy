using UnityEngine;

namespace ExtraSnapPointsMadeEasy.SnapPoints.Extensions;

internal static class Vector3ArrayExtensions
{
    public static Vector3[] ApplyZIndexFix(this Vector3[] positions)
    {
        Vector3[] result = new Vector3[positions.Length];
        float z = 0f;

        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 original = positions[i];
            result[i] = new Vector3(original.x, original.y, original.z + z);
            z += 0.0001f;
        }

        return result;
    }
}
