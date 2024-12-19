﻿using System.Collections.Generic;
using ExtraSnapPointsMadeEasy.Models;
using UnityEngine;

namespace ExtraSnapPointsMadeEasy.SnapPoints;

/// <summary>
/// Manage and track all snap points added by this mod, so that they can be deleted/recreated
/// </summary>
internal class SnapPointManager
{
    private const string SnapPointTag = "snappoint"; //This must match the value in the Valheim assemblies

    internal static SnapPointManager Instance = new();

    private readonly List<GameObject> AddedSnapPoints = new();

    public void AddSnapPointsToPrefab(GameObject prefab, NamedSnapPoint[] snapPoints)
    {
        if (!prefab)
        {
            Log.LogWarning("GameObject prefab is null. Cannot add snappoints.");
            return;
        }

        foreach (NamedSnapPoint snapPoint in snapPoints)
        {
            AddSnapPointInternal(prefab, snapPoint.LocalPosition, snapPoint.Name);
        }
    }

    public void ClearAddedSnapPoints()
    {
        Log.LogInfo($"Clearing {AddedSnapPoints.Count} snap points", LogLevel.Medium);
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
        newSnapPoint.tag = SnapPointTag;
        newSnapPoint.SetActive(false);
    }
}
