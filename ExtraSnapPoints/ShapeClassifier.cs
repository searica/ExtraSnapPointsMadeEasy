using System;
using System.Collections.Generic;
using ExtraSnapsMadeEasy.Extensions;
using ExtraSnapsMadeEasy.Models;
using UnityEngine;
using static ExtraSnapsMadeEasy.Models.SnapPointNames;

/* In Unity
 * X = left/right
 * Y = up/down
 * Z = forward/back
 */

namespace ExtraSnapsMadeEasy.ExtraSnapPoints;

internal static class ShapeClassifier
{

    /// <summary>
    ///     Check if piece has 2 snap points that form a line.
    /// </summary>
    /// <returns></returns>
    internal static bool FormsLine(List<Transform> snapPoints)
    {
        // Any two points that are not in the exact same position form a line
        return snapPoints.Count == 2 && snapPoints[0].localPosition != snapPoints[1].localPosition;
    }

    /// <summary>
    ///     Check if piece has 3 snap points that form a triangle.
    /// </summary>
    /// <returns></returns>
    internal static bool FormsTriangle(List<Transform> snapPoints)
    {
        // Any 3 points form a triangle, unless they are collinear
        return snapPoints.Count == 3 && !snapPoints.AreCollinear();
    }

    /// <summary>
    ///     Check if piece has 4 snap points that form a rectangle.
    /// </summary>
    /// <returns></returns>
    internal static bool FormsRectangle(List<Transform> snapPoints)
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
    internal static Vector3 SolveMinimumsOf(List<Transform> snapPoints)
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
    internal static Vector3 SolveMaximumsOf(List<Transform> snapPoints)
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
        if (!snapPoint.x.Equals(minimums.x) && !snapPoint.x.Equals(maximums.x))
        {
            return false;
        }

        if (!snapPoint.y.Equals(minimums.y) && !snapPoint.y.Equals(maximums.y))
        {
            return false;
        }

        if (!snapPoint.z.Equals(minimums.z) && !snapPoint.z.Equals(maximums.z))
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
        if (snapPoint.x.Equals( minimums.x) || snapPoint.x.Equals( maximums.x))
        {
            extremusCoordinates++;
        }

        if (snapPoint.y.Equals( minimums.y) || snapPoint.y.Equals( maximums.y))
        {
            extremusCoordinates++;
        }

        if (snapPoint.z.Equals( minimums.z) || snapPoint.z.Equals( maximums.z))
        {
            extremusCoordinates++;
        }

        if (extremusCoordinates != 2) { return false; }

        // one should be in the center of two extremes
        Vector3 middles = (minimums + maximums) / 2;
        int middleCoordinates = 0;
        if (snapPoint.x.Equals( middles.x)) { middleCoordinates++; }
        if (snapPoint.y.Equals( middles.y)) { middleCoordinates++; }
        if (snapPoint.z.Equals( middles.z)) { middleCoordinates++; }
        if (middleCoordinates != 1) { return false; }

        return true;
    }
}
