using System;
using System.Collections.Generic;
using ExtraSnapsMadeEasy.Extensions;
using ExtraSnapsMadeEasy.Models;
using UnityEngine;
using static ExtraSnapsMadeEasy.SnapPoints.SnapPointNames;

namespace ExtraSnapsMadeEasy.SnapPoints;

/// <summary>
/// Calculates additional snap points based on existing snap points
/// </summary>
internal class ExtraSnapPointsCalculator
{
    /// <summary>
    /// The name of SnapPoints that the Valheim devs did not name yet
    /// (generally because they are supposed to be unavailable to the player)
    /// </summary>
    private const string DefaultSnapPointName = "_snappoint";

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
            extraSnapPoints[i] = new NamedSnapPoint(start + (delta * (i + 1)), $"{CENTER} Line {i + 1}/{nrPointsToAdd + 1}");
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
                    else if ((pointA.name.StartsWith(TOP) && pointB.name.StartsWith(BOTTOM))
                        || (pointA.name.StartsWith(BOTTOM) && pointB.name.StartsWith(TOP)))
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
}
