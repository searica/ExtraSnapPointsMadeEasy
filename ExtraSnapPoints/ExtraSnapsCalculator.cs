using System;
using System.Collections.Generic;
using ExtraSnapsMadeEasy.Extensions;
using ExtraSnapsMadeEasy.Models;
using UnityEngine;
using Logging;
using static ExtraSnapsMadeEasy.Models.SnapPointNames;

namespace ExtraSnapsMadeEasy.ExtraSnapPoints;

/// <summary>
/// Calculates additional snap points based on existing snap points
/// </summary>
internal class ExtraSnapsCalculator
{
    /// <summary>
    /// The name of SnapPoints that the Valheim devs did not name yet
    /// (generally because they are supposed to be unavailable to the player)
    /// </summary>
    private const string DefaultSnapPointName = SnapPointNames.DEFAULT_NAME;

    /// <summary>
    /// Add snap points to a line.
    /// If the number of snap points to add is 1, this is equivalent to adding a center snap point.
    /// </summary>
    /// <param name="snapPoints">A list of 2 snap points</param>
    /// <param name="nrPointsToAdd">The number of Snap Points that should be added between the two points in <paramref name="snapPoints"/>.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">A line must consist out of 2 points.</exception>
    internal static NamedSnapPoint[] GetExtraPointsForLine(List<Transform> snapPoints, int nrPointsToAdd)
    {
        if (snapPoints.Count != 2)
        {
            throw new ArgumentException($"{nameof(GetExtraPointsForLine)} called with more than 2 snappoints.", nameof(snapPoints));
        }

        Vector3 start = snapPoints[0].transform.localPosition;
        Vector3 end = snapPoints[1].transform.localPosition;
        Vector3 difference = end - start;
        Vector3 delta = difference / (nrPointsToAdd + 1);
        NamedSnapPoint[] extraSnapPoints = new NamedSnapPoint[nrPointsToAdd];
        for (int i = 0; i < nrPointsToAdd; i++)
        {
            extraSnapPoints[i] = new NamedSnapPoint(start + delta * (i + 1), $"{CENTER} Line {i + 1}/{nrPointsToAdd + 1}");
        }

        return extraSnapPoints;
    }

    /// <summary>
    ///     Add snap points at the midpoint along each edge
    ///     of the triangle and in the center of the triangle.
    /// </summary>
    /// <param name="snapPoints">A list of 3 snap points</param>
    /// <exception cref="ArgumentException">A triangle must consist out of 3 snap points</exception>
    internal static NamedSnapPoint[] GetExtraSnapPointsForTriangle(List<Transform> snapPoints)
    {
        if (snapPoints.Count != 3)
        {
            throw new ArgumentException(
                $"{nameof(GetExtraSnapPointsForTriangle)} called on a list that does not have exactly 3 snap points.",
                nameof(snapPoints));
        }

        // compute center snap point
        Vector3 centerPoint = snapPoints.GetCenter();
        Vector3 mindpoint0_1 = (snapPoints[0].localPosition + snapPoints[1].localPosition) / 2;
        Vector3 mindpoint0_2 = (snapPoints[0].localPosition + snapPoints[2].localPosition) / 2;
        Vector3 mindpoint1_2 = (snapPoints[1].localPosition + snapPoints[2].localPosition) / 2;

        return new NamedSnapPoint[]
        {
            new(mindpoint0_1, $"{MID} {snapPoints[0].name} - {snapPoints[1].name}", 1),
            new(mindpoint1_2, $"{MID} {snapPoints[1].name} - {snapPoints[2].name}", 3),
            new(mindpoint0_2, $"{MID} {snapPoints[0].name} - {snapPoints[2].name}", 5),
            new(centerPoint, CENTER, 6)
        };
    }

    /// <summary>
    ///     Get snap points at the midpoint along each edge
    ///     of the square and in the center of the square.
    /// </summary>
    /// <param name="snapPoints">A list of 4 snap points</param>
    /// <exception cref="ArgumentException">A rectangle must consist out of 4 snap points</exception>
    internal static NamedSnapPoint[] GetSnapPointsForRectangle(List<Transform> snapPoints)
    {
        if (snapPoints.Count != 4)
        {
            throw new InvalidOperationException($"{nameof(GetSnapPointsForRectangle)} called with {snapPoints.Count} points instead of 4");
        }

        List<NamedSnapPoint> result = new(5);
        Vector3 center = snapPoints.GetCenter();

        // compute mid points of each edge
        for (int i = 0; i < snapPoints.Count - 1; i++)
        {
            Transform pointA = snapPoints[i];
            for (int j = i + 1; j < snapPoints.Count; j++)
            {
                Transform pointB = snapPoints[j];
                Vector3 newPt = (pointA.transform.localPosition + pointB.transform.localPosition) / 2;
                if (newPt != center) // Avoid adding a snap point to the middle of the diagonal, which is already the center snap point
                {
                    string name = EXTRA;
                    if (pointA.name.StartsWith(TOP) && pointB.name.StartsWith(TOP))
                    {
                        name = $"{TOP} {CENTER}";
                    }
                    else if (pointA.name.StartsWith(BOTTOM) && pointB.name.StartsWith(BOTTOM))
                    {
                        name = $"{BOTTOM} {CENTER}";
                    }
                    else if (pointA.name.StartsWith(TOP) && pointB.name.StartsWith(BOTTOM)
                        || pointA.name.StartsWith(BOTTOM) && pointB.name.StartsWith(TOP))
                    {
                        if (int.TryParse(pointA.name.Substring(pointA.name.IndexOf(' ')), out int number1)
                        && int.TryParse(pointB.name.Substring(pointB.name.IndexOf(' ')), out int number2)
                            && number1 == number2)
                        {
                            name = $"{EDGE} {number1} {CENTER}";
                        }
                    }
                    else if (pointA.name != DefaultSnapPointName && pointB.name != DefaultSnapPointName)
                    {
                        name = $"{MID} {pointA.name} - {pointB.name}";
                    }
                    result.Add(new NamedSnapPoint(newPt, name));
                }
            }
        }

        result.Add(new NamedSnapPoint(center, CENTER));

        return result.ToArray();
    }

    /// <summary>
    ///     Add snap points at the midpoint along each edge of the roof top.
    /// </summary>
    internal static NamedSnapPoint[] GetExtraSnapPointsForRoofTop(List<Transform> snapPoints, string name)
    {
        Vector3 minimums = ShapeClassifier.SolveMinimumsOf(snapPoints);
        Vector3 maximums = ShapeClassifier.SolveMaximumsOf(snapPoints);
        Vector3 middles = (minimums + maximums) / 2;

        // Get which points are top points and ID of the axis across the front of the V shape.
        List<Vector3> topPoints = new();
        int frontAxis = -1;
        foreach (Transform snapPoint in snapPoints)
        {
            for (int i = 0; i < 3; i++) // loop through vector
            {
                float coordinate = snapPoint.localPosition[i];
                if (!coordinate.Equals(minimums[i]) && !coordinate.Equals(maximums[i]))
                {
                    if (!coordinate.Equals(middles[i]))
                    {
                        Log.LogError($"{name} is not a RoofTop piece");
                    }
                    if (frontAxis == -1)
                    {
                        frontAxis = i;
                    }
                    else if (frontAxis != i)
                    {
                        Log.LogWarning($"Invalid front axis for RoofTop piece: {name}, will not add extra snap points.");
                        return Array.Empty<NamedSnapPoint>();
                    }
                    topPoints.Add(snapPoint.localPosition);
                }
            }
        }
        if (topPoints.Count != 2)
        {
            Log.LogError($"{name} is not a RoofTop piece");
        }

        // Get ID of axis along the roof ridge
        Vector3 ridgeVec = (topPoints[1].normalized - topPoints[0].normalized).normalized;
        int ridgeAxis = -1;
        for (int i = 0; i < 3; i++)
        {
            if (!ridgeVec[i].Equals(0.0f))
            {
                if (ridgeAxis == -1)
                {
                    ridgeAxis = i;
                }
                else if (ridgeAxis != i)
                {
                    Log.LogWarning($"Invalid ridge axis for RoofTop piece: {name}, will not add extra snap points.");
                    return Array.Empty<NamedSnapPoint>();
                }
            }
        }

        // Get ID of the axis up/down the V shape
        int vertAxis = 3 - ridgeAxis - frontAxis;

        // Compute top mid-point
        Vector3 topCenter = (topPoints[0] + topPoints[1]) / 2;

        // Compute side mid-point
        Vector3 mid1 = new();
        mid1[frontAxis] = minimums[frontAxis];
        mid1[ridgeAxis] = middles[ridgeAxis];
        if (topPoints[0][vertAxis].Equals(maximums[vertAxis]))
        {
            mid1[vertAxis] = minimums[vertAxis];
        }
        else
        {
            mid1[vertAxis] = maximums[vertAxis];
        }

        // Compute side midpoint
        Vector3 mid2 = mid1;
        mid2[frontAxis] = maximums[frontAxis];

        // TODO: Test if names make sense in game
        return new NamedSnapPoint[]
        {
            new(topCenter, $"{TOP} {CENTER}"),
            new(mid1, $"{MID} 1"),
            new(mid2, $"{MID} 2")
        };
    }
}
