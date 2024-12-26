using System.Collections.Generic;
using UnityEngine;
using ExtraSnapsMadeEasy.Models;
using Logging;

namespace ExtraSnapsMadeEasy.ExtraSnapPoints;

/// <summary>
/// Manage and track all snap points added by this mod, so that they can be deleted/recreated
/// </summary>
internal class SnapPointManager
{
    private const string SnapPointTag = SnapPointNames.TAG;  //This must match the value in the Valheim assemblies

    internal static SnapPointManager Instance = new();

    private readonly List<GameObject> AddedSnapPoints = new();

    public void AddSnapPointsToPrefab(GameObject prefab, NamedSnapPoint[] snapPoints)
    {
        if (!prefab)
        {
            Log.LogWarning("GameObject prefab is null. Cannot add snap points.");
            return;
        }

        foreach (NamedSnapPoint snapPoint in snapPoints)
        {
            AddSnapPointInternal(prefab, snapPoint.LocalPosition, snapPoint.Name);
        }
    }

    public void ClearAddedSnapPoints()
    {
        Log.LogInfo($"Clearing {AddedSnapPoints.Count} snap points", Log.InfoLevel.Medium);
        foreach (GameObject snapPoint in AddedSnapPoints)
        {
            Object.DestroyImmediate(snapPoint);
        }
        AddedSnapPoints.Clear();
    }

    private void AddSnapPointInternal(GameObject prefab, Vector3 snapPointLocalPosition, string snapPointName)
    {
        GameObject newSnapPoint = new(snapPointName);
        AddedSnapPoints.Add(newSnapPoint);
        newSnapPoint.transform.parent = prefab.transform;
        newSnapPoint.transform.localPosition = snapPointLocalPosition;
        newSnapPoint.tag = SnapPointNames.TAG;
        newSnapPoint.SetActive(false);
    }
}
