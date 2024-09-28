using System;
using System.Collections.Generic;
using ExtraSnapPointsMadeEasy.Models;
using ExtraSnapPointsMadeEasy.SnapPoints.Extensions;
using UnityEngine;
using static ExtraSnapPointsMadeEasy.SnapPoints.SnapPointNames;

/* In Unity
 * X = left/right
 * Y = up/down
 * Z = forward/back
 */

namespace ExtraSnapPointsMadeEasy.Helpers;

internal static class ShapeClassifier
{
    private const float Tolerance = 1e-6f;

    /// <summary>
    ///     Check if piece has 2 snap points that form a line.
    /// </summary>
    /// <returns></returns>
    internal static bool FormLine(List<Transform> snapPoints)
    {
        // Any two points that are not in the exact same position form a line
        return snapPoints.Count == 2 && snapPoints[0].localPosition != snapPoints[1].localPosition;
    }

    /// <summary>
    ///     Check if piece has 3 snap points that form a triangle.
    /// </summary>
    /// <returns></returns>
    internal static bool FormTriangle(List<Transform> snapPoints)
    {
        // Any 3 points form a triangle, unless they are collinear
        return snapPoints.Count == 3 && !snapPoints.AreCollinear();
    }

    /// <summary>
    ///     Check if piece has 4 snap points that form a rectangle.
    /// </summary>
    /// <returns></returns>
    internal static bool FormRectangle(List<Transform> snapPoints)
    {
        // A rectangle should have 4 points on the same plane
        if (snapPoints.Count != 4 || !snapPoints.AreCoplanar())
        {
            return false;
        }

        // Check that this is actually a rectange,
        // meaning it has 2 sets of opposite sides with the same length and diagonals with the same length.

        Vector3 p0 = snapPoints[0].localPosition;
        Vector3 p1 = snapPoints[1].localPosition;
        Vector3 p2 = snapPoints[2].localPosition;
        Vector3 p3 = snapPoints[3].localPosition;

        // Calculate the distances between all pairs of points
        float distance0_1 = Vector3.Distance(p0, p1);
        float distance0_2 = Vector3.Distance(p0, p2);
        float distance0_3 = Vector3.Distance(p0, p3);
        float distance1_2 = Vector3.Distance(p1, p2);
        float distance1_3 = Vector3.Distance(p1, p3);
        float distance2_3 = Vector3.Distance(p2, p3);

        // There should be 3 sets of equal distances. 2 sets of (opposite) sides and 2 diagonals.
        // We only need to compare distance lines that do not share any vertex,
        // as any other line also intersecting the same vertex will belong to a different set by definition (where a set either contains opposite lines or diagonals).
        return distance0_1 == distance2_3
            && distance0_2 == distance1_3
            && distance0_3 == distance1_2;
    }

    /// <summary>
    ///     Check if piece have 8 snap points that form a cube.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    internal static bool IsCube(List<Transform> snapPoints)
    {
        return snapPoints.Count == 8 && EverySnapPointLiesOnExtrema(snapPoints);
    }

    /// <summary>
    ///     Check if build piece is a cross shape with
    ///     4 corner snap points and one interior snap points.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    internal static bool IsCross(List<Transform> snapPoints)
    {
        if (snapPoints.Count != 5)
        {
            return false;
        }

        // Should have 4 extremus points and 1 center point
        Vector3 centerPoint = snapPoints.GetCenter();
        Vector3 minimums = SolveMinimumsOf(snapPoints);
        Vector3 maximums = SolveMaximumsOf(snapPoints);
        int extremusPointCount = 0;
        int centerPointCount = 0;
        foreach (Transform snapNode in snapPoints)
        {
            if (!LiesOnExtrema(snapNode.localPosition, minimums, maximums))
            {
                if (snapNode.localPosition == centerPoint) { centerPointCount++; }
            }
            else
            {
                extremusPointCount++;
            }
        }
        return extremusPointCount == 4 && centerPointCount == 1;
    }

    /// <summary>
    ///     Checks if game object is top piece for roof.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    internal static bool IsWedge3D(List<Transform> snapPoints)
    {
        if (snapPoints.Count != 6)
        {
            return false;
        }

        // needs 6 points. 4 should lie on an extremus and 2 on edge mid-points
        Vector3 minimums = SolveMinimumsOf(snapPoints);
        Vector3 maximums = SolveMaximumsOf(snapPoints);
        int extremusPointCount = 0;
        int midEdgePointCount = 0;
        foreach (Transform snapNode in snapPoints)
        {
            if (LiesOnExtrema(snapNode.localPosition, minimums, maximums))
            {
                extremusPointCount++;
            }

            if (LiesOnEdgeMidPoint(snapNode.localPosition, minimums, maximums))
            {
                midEdgePointCount++;
            }
        }

        return extremusPointCount == 4 && midEdgePointCount == 2;
    }

    /// <summary>
    ///     Check that all snap points like on extremus
    ///     as determined from the maximums and minimums
    ///     of the snap points.
    /// </summary>
    /// <param name="snapPoints"></param>
    /// <returns></returns>
    private static bool EverySnapPointLiesOnExtrema(List<Transform> snapPoints)
    {
        Vector3 minimums = SolveMinimumsOf(snapPoints);
        Vector3 maximums = SolveMaximumsOf(snapPoints);
        foreach (Transform snapNode in snapPoints)
        {
            if (!LiesOnExtrema(snapNode.localPosition, minimums, maximums))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Find the minimums of all snap points in x, y, z
    /// </summary>
    /// <param name="snapPoints"></param>
    /// <returns></returns>
    private static Vector3 SolveMinimumsOf(List<Transform> snapPoints)
    {
        float xMin = float.PositiveInfinity;
        float yMin = float.PositiveInfinity;
        float zMin = float.PositiveInfinity;
        foreach (Transform node in snapPoints)
        {
            Vector3 sn = node.localPosition;
            xMin = xMin > sn.x ? sn.x : xMin;
            yMin = yMin > sn.y ? sn.y : yMin;
            zMin = zMin > sn.z ? sn.z : zMin;
        }

        return new Vector3(xMin, yMin, zMin);
    }

    /// <summary>
    ///     Find the maximums of all snap points in x, y, z
    /// </summary>
    /// <param name="snapPoints"></param>
    /// <returns></returns>
    private static Vector3 SolveMaximumsOf(List<Transform> snapPoints)
    {
        float xMax = float.NegativeInfinity;
        float yMax = float.NegativeInfinity;
        float zMax = float.NegativeInfinity;
        foreach (Transform node in snapPoints)
        {
            Vector3 sn = node.localPosition;
            xMax = xMax < sn.x ? sn.x : xMax;
            yMax = yMax < sn.y ? sn.y : yMax;
            zMax = zMax < sn.z ? sn.z : zMax;
        }

        return new Vector3(xMax, yMax, zMax);
    }

    /// <summary>
    ///     Check if a snap point lies on an extremus as
    ///     defined by minimums and maximums.
    /// </summary>
    /// <param name="snapPoint"></param>
    /// <param name="minimums"></param>
    /// <param name="maximums"></param>
    /// <returns></returns>
    private static bool LiesOnExtrema(
        Vector3 snapPoint,
        Vector3 minimums,
        Vector3 maximums
    )
    {
        if (!Equals(snapPoint.x, minimums.x) && !Equals(snapPoint.x, maximums.x))
        {
            return false;
        }

        if (!Equals(snapPoint.y, minimums.y) && !Equals(snapPoint.y, maximums.y))
        {
            return false;
        }

        if (!Equals(snapPoint.z, minimums.z) && !Equals(snapPoint.z, maximums.z))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Check if a snap point lies on the mid point
    ///     of an edge of the bounding box defined by
    ///     minumums and maximums.
    /// </summary>
    /// <param name="snapPoint"></param>
    /// <param name="minimums"></param>
    /// <param name="maximums"></param>
    /// <returns></returns>
    private static bool LiesOnEdgeMidPoint(
        Vector3 snapPoint,
        Vector3 minimums,
        Vector3 maximums
    )
    {
        // two of the coordinates should be on an extreme
        int extremusCoordinates = 0;
        if (Equals(snapPoint.x, minimums.x) || Equals(snapPoint.x, maximums.x))
        {
            extremusCoordinates++;
        }

        if (Equals(snapPoint.y, minimums.y) || Equals(snapPoint.y, maximums.y))
        {
            extremusCoordinates++;
        }

        if (Equals(snapPoint.z, minimums.z) || Equals(snapPoint.z, maximums.z))
        {
            extremusCoordinates++;
        }

        if (extremusCoordinates != 2) { return false; }

        // one should be in the center of two extremes
        Vector3 middles = (minimums + maximums) / 2;
        int middleCoordinates = 0;
        if (Equals(snapPoint.x, middles.x)) { middleCoordinates++; }
        if (Equals(snapPoint.y, middles.y)) { middleCoordinates++; }
        if (Equals(snapPoint.z, middles.z)) { middleCoordinates++; }
        if (middleCoordinates != 1) { return false; }

        return true;
    }

    //TODO: Consider refactoring this to SnapPointCalculator
    /// <summary>
    ///     Add snap points at the midpoint along each edge of the roof top.
    /// </summary>
    internal static NamedSnapPoint[] GetExtraSnapPointsForRoofTop(List<Transform> snapPoints, string name)
    {
        Vector3 minimums = SolveMinimumsOf(snapPoints);
        Vector3 maximums = SolveMaximumsOf(snapPoints);
        Vector3 middles = (minimums + maximums) / 2;

        // Get which points are top points and ID of the axis across the front of the V shape.
        List<Vector3> topPoints = new();
        int frontAxis = -1;
        foreach (Transform snapPoint in snapPoints)
        {
            for (int i = 0; i < 3; i++) // loop through vector
            {
                float coordinate = snapPoint.localPosition[i];
                if (!Equals(coordinate, minimums[i]) && !Equals(coordinate, maximums[i]))
                {
                    if (!Equals(coordinate, middles[i]))
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
            if (!Equals(ridgeVec[i], 0.0f))
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
        if (Equals(topPoints[0][vertAxis], maximums[vertAxis]))
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

    /// <summary>
    ///     Checks equality using both relative and absolute tolerance.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static bool Equals(float x, float y)
    {
        float diff = Mathf.Abs(x - y);
        return diff <= Tolerance || diff <= Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) * Tolerance;
    }
}
