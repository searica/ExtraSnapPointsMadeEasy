using System.Collections.Generic;
using ExtraSnapPointsMadeEasy.Extensions;
using UnityEngine;

namespace ExtraSnapPointsMadeEasy.SnapPoints.Extensions;

internal static class GameObjectExtensions
{
    internal static List<Transform> GetSnapPoints(this GameObject gameObject)
    {
        Transform transform = gameObject.transform;
        int childCount = transform.childCount;
        List<Transform> result = new(childCount);
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.CompareTag("snappoint"))
            {
                result.Add(child);
            }
        }

        return result;
    }

    /// <summary>
    /// Check if the gameObject has a snap point at the local center (Vector3.zero)
    /// </summary>
    internal static bool HasOriginSnapPoint(this GameObject gameObject)
    {
        Transform transform = gameObject.transform;
        for (int index = 0; index < transform.childCount; ++index)
        {
            Transform child = transform.GetChild(index);
            if (child.CompareTag("snappoint") && child.transform.localPosition == Vector3.zero)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if game object is a floor braizer.
    /// </summary>
    /// <remarks>
    /// Matches: standing brazier, blue standing brazier, mountainkit, etc.
    /// </remarks>
    internal static bool IsFloorBrazier(this GameObject prefab)
    {
        return prefab.name.Contains("brazier")
            && !prefab.name.Contains("ceiling");
    }

    /// <summary>
    /// Checks if game object is a ceiling brazier. Mainly 'Hanging Brazier'.
    /// </summary>
    internal static bool IsCeilingBrazier(this GameObject prefab)
    {
        return prefab.name.Contains("brazier")
            && prefab.name.Contains("ceiling");
    }

    private static readonly HashSet<string> TorchPrefabNames = new()
    {
        "piece_groundtorch_mist",
        "dverger_demister",
        "dverger_demister_large",
    };

    /// <summary>
    /// Checks if game object is a torch.
    /// </summary>
    /// <remarks>
    /// Matches: piece_groundtorch_wood, piece_groundtorch, piece_groundtorch_green,
    ///          piece_groundtorch_blue, piece_groundtorch_mist, etc.
    /// </remarks>
    internal static bool IsTorch(this GameObject prefab)
    {
        if (TorchPrefabNames.Contains(prefab.name))
        {
            return true;
        }

        if (prefab.transform.FindDeepChild("FireWarmth") ||
            prefab.GetComponentInChildren<Demister>(true) ||
            prefab.transform.FindDeepChild("fx_Torch_Basic") ||
            prefab.transform.FindDeepChild("fx_Torch_Blue") ||
            prefab.transform.FindDeepChild("fx_Torch_Green") ||
            prefab.transform.FindDeepChild("demister_ball (1)"))
        {
            string prefabName = prefab.name.ToLower();
            if (prefabName.Contains("torch") || prefabName.Contains("demister"))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the GameObject has a collider tagged with "roof".
    /// </summary>
    /// <remarks>
    /// The "roof" tag seems mostly unused by the game.
    /// Having the tag "roof" does not mean the piece will protect from rain for instance.
    /// Whether a piece acts as a roof is mostly dependant on the absence of a "leaky" collider.
    /// </remarks>
    internal static bool IsRoof(this GameObject prefab)
    {
        foreach (Collider collider in prefab.GetComponentsInChildren<Collider>())
        {
            if (collider.CompareTag("roof"))
            {
                return true;
            }
        }
        return false;
    }
}
