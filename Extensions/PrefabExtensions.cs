using System.Collections.Generic;
using UnityEngine;

namespace ExtraSnapsMadeEasy.Extensions;

/// <summary>
///     Extensions for managing Valheim prefab snap points and
///     checking the type of piece they are.
/// </summary>
internal static class PrefabExtensions
{
    private const string SnapPointTag = "snappoint";
    private const string RoofTag = "roof";
    private static readonly string[] TorchNameSubstrings = new[] { "torch", "demister" };
    private static readonly string[] TorchChildNames = new[]
    {
        "FireWarmth",
        "fx_Torch_Basic",
        "fx_Torch_Blue",
        "fx_Torch_Green",
        "demister_ball (1)"
    };
    private static readonly HashSet<string> TorchPrefabNames = new()
    {
        "piece_groundtorch_mist",
        "dverger_demister",
        "dverger_demister_large",
    };


    /// <summary>
    ///     Check if this is a demister
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    internal static bool IsDemister(this GameObject prefab)
    {
        return prefab.GetComponentInChildren<Demister>(true);
    }

    /// <summary>
    ///     Checks if this prefab modifies terrain.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    internal static bool IsTerrainOp(this GameObject prefab)
    {
        return prefab.GetComponent<TerrainOp>();
    }

    /// <summary>
    ///     Checks if this is a repair piece from a piece table.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    internal static bool IsRepairPiece(this GameObject prefab)
    {
        return prefab.TryGetComponent(out Piece piece) && piece.m_repairPiece;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    internal static List<Transform> GetSnapPoints(this GameObject prefab)
    {
        Transform transform = prefab.transform;
        int childCount = transform.childCount;

        List<Transform> result = new(childCount);
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.CompareTag(SnapPointTag))
            {
                result.Add(child);
            }
        }

        return result;
    }

    /// <summary>
    /// Check if the gameObject has a snap point at the local center (Vector3.zero)
    /// </summary>
    internal static bool HasOriginSnapPoint(this GameObject prefab)
    {
        Transform transform = prefab.transform;
        for (int index = 0; index < transform.childCount; ++index)
        {
            Transform child = transform.GetChild(index);
            if (child.CompareTag(SnapPointTag) && child.transform.localPosition == Vector3.zero)
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

    /// <summary>
    ///     Checks if game object is a torch.
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

        if (!prefab.IsDemister())
        {
            foreach (string name in TorchChildNames)
            {
                if (prefab.FindDeepChild(name))
                {
                    break;
                }
            }
            return false;
        }

        return prefab.name.ToLower().ContainsAny(TorchNameSubstrings);
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
            if (collider.CompareTag(RoofTag))
            {
                return true;
            }
        }
        return false;
    }
}
