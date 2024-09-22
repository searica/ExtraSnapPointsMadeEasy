using System.Collections.Generic;
using ExtraSnapPointsMadeEasy.Extensions;
using UnityEngine;

/* In Unity
 * X = left/right
 * Y = up/down
 * Z = forward/back
 */

namespace ExtraSnapPointsMadeEasy.Helpers;

internal static class ShapeClassifier
{
    private const float Tolerance = 1e-6f;

    private static readonly HashSet<string> Torches = new()
    {
        "piece_groundtorch_mist",
        "dverger_demister",
        "dverger_demister_large",
    };

    /// <summary>
    ///     Check if piece has a single snap point.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    internal static bool IsPoint(GameObject gameObject)
    {
        if (!gameObject) { return false; }
        return gameObject.GetSnapPoints().Count == 1;
    }

    /// <summary>
    ///     Check if piece has 2 snap points that form a line.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    internal static bool IsLine(GameObject gameObject)
    {
        if (!gameObject) { return false; }
        List<Transform> snapPoints = gameObject.GetSnapPoints();
        return snapPoints.Count == 2 && EverySnapPointLiesOnExtrema(snapPoints);
    }

    /// <summary>
    ///     Check if piece has 3 snap points that form a triangle.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    internal static bool IsTriangle(GameObject gameObject)
    {
        if (!gameObject) { return false; }
        List<Transform> snapPoints = gameObject.GetSnapPoints();
        return snapPoints.Count == 3 && EverySnapPointLiesOnExtrema(snapPoints);
    }

    /// <summary>
    ///     Check if piece has 4 snap points that form a rectangle.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    internal static bool IsRect2D(GameObject gameObject)
    {
        if (!gameObject) { return false; }
        List<Transform> snapPoints = gameObject.GetSnapPoints();
        // must have 4 points that lie on extremes
        if (snapPoints.Count != 4 || !EverySnapPointLiesOnExtrema(snapPoints))
        {
            return false;
        }
        // check if all four points lie on the same plane
        Vector3 vec0_1 = snapPoints[1].localPosition - snapPoints[0].localPosition;
        Vector3 vec0_2 = snapPoints[2].localPosition - snapPoints[0].localPosition;
        Vector3 vec0_3 = snapPoints[3].localPosition - snapPoints[0].localPosition;
        return Equals(Vector3.Dot(vec0_3, Vector3.Cross(vec0_2, vec0_1)), 0.0f);
    }

    /// <summary>
    ///     Check if piece have 8 snap points that form a cube.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    internal static bool IsCube(GameObject gameObject)
    {
        if (!gameObject) { return false; }
        List<Transform> snapPoints = gameObject.GetSnapPoints();
        return snapPoints.Count == 8 && EverySnapPointLiesOnExtrema(snapPoints);
    }

    /// <summary>
    ///     Check if build piece is a cross shape with
    ///     4 corner snap points and one interior snap points.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    internal static bool IsCross(GameObject gameObject)
    {
        if (!gameObject) { return false; }
        List<Transform> snapPoints = gameObject.GetSnapPoints();

        if (snapPoints.Count != 5)
        {
            return false;
        }

        // Should have 4 extremus points and 1 center point
        Vector3 centerPoint = GetCenterOfSnapPoints(snapPoints);
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
    internal static bool IsRoofTop(GameObject gameObject)
    {
        if (gameObject == null) { return false; }

        bool hasRoofTag = false;
        foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>())
        {
            if (collider.CompareTag("roof"))
            {
                hasRoofTag = true;
                break;
            }
        }

        if (!hasRoofTag) { return false; }

        return IsWedge3D(gameObject);
    }

    /// <summary>
    ///     Checks if game object is top piece for roof.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    internal static bool IsWedge3D(GameObject gameObject)
    {
        if (!gameObject)
        {
            return false;
        }

        List<Transform> snapPoints = gameObject.GetSnapPoints();
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
    ///     Checks if game object is a floor braizer.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    internal static bool IsFloorBrazier(GameObject prefab)
    {
        return prefab.name.Contains("brazier") && !prefab.name.Contains("ceiling");
    }

    /// <summary>
    ///     Checks if game object is a ceiling brazier.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    internal static bool IsCeilingBrazier(GameObject prefab)
    {
        return prefab.name.Contains("brazier") && prefab.name.Contains("ceiling");
    }

    /// <summary>
    ///     Checks if game object is a torch.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    internal static bool IsTorch(GameObject prefab)
    {
        if (Torches.Contains(prefab.name))
        {
            return true;
        }

        if (prefab.FindDeepChild("FireWarmth") ||
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

    /// <summary>
    ///     Add a snap point at the midpoint between
    ///     the two existing snap points.
    /// </summary>
    /// <param name="gameObject"></param>
    internal static void AddSnapPointToLine(GameObject gameObject)
    {
        List<Transform> snapPoints = gameObject.GetSnapPoints();
        Vector3 centerPoint = GetCenterOfSnapPoints(snapPoints);
        gameObject.AddSnapPoints(new Vector3[] { centerPoint });
    }

    /// <summary>
    ///     Add snap points at the midpoint along each edge
    ///     of the triangle and in the center of the triangle.
    /// </summary>
    /// <param name="gameObject"></param>
    internal static void AddSnapPointsToTriangle(GameObject gameObject)
    {
        List<Transform> snapPoints = gameObject.GetSnapPoints();
        HashSet<Vector3> pts = new();

        // compute center snap point
        Vector3 centerPoint = GetCenterOfSnapPoints(snapPoints);
        pts.Add(centerPoint);

        // compute mid points of each edge
        for (int i = 0; i < snapPoints.Count - 1; i++)
        {
            Transform snap1 = snapPoints[i];
            for (int j = i + 1; j < snapPoints.Count; j++)
            {
                Transform snap2 = snapPoints[j];
                Vector3 newPt = (snap1.transform.localPosition + snap2.transform.localPosition) / 2;
                if (newPt != centerPoint)
                {
                    pts.Add(newPt);
                }
            }
        }

        gameObject.AddSnapPoints(pts);
    }

    /// <summary>
    ///     Add snap points at the midpoint along each edge
    ///     of the square and in the center of the square.
    /// </summary>
    /// <param name="gameObject"></param>
    internal static void AddSnapPointsToRect2D(GameObject gameObject)
    {
        List<Transform> snapPoints = gameObject.GetSnapPoints();
        HashSet<Vector3> pts = new();

        // compute center snap point
        Vector3 centerPoint = GetCenterOfSnapPoints(snapPoints);
        pts.Add(centerPoint);

        // compute mid points of each edge
        for (int i = 0; i < snapPoints.Count - 1; i++)
        {
            Transform snap1 = snapPoints[i];
            for (int j = i + 1; j < snapPoints.Count; j++)
            {
                Transform snap2 = snapPoints[j];
                Vector3 newPt = (snap1.transform.localPosition + snap2.transform.localPosition) / 2;
                if (newPt != centerPoint)
                {
                    pts.Add(newPt);
                }
            }
        }
        gameObject.AddSnapPoints(pts);
    }

    /// <summary>
    ///     Add snap points at the midpoint along each edge of the roof top.
    /// </summary>
    /// <param name="gameObject"></param>
    internal static void AddSnapPointsToRoofTop(GameObject gameObject)
    {
        List<Transform> snapPoints = gameObject.GetSnapPoints();
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
                        Log.LogError($"{gameObject.name} is not a RoofTop piece");
                    }
                    if (frontAxis == -1)
                    {
                        frontAxis = i;
                    }
                    else if (frontAxis != i)
                    {
                        Log.LogWarning($"Invalid front axis for RoofTop piece: {gameObject.name}, will not add extra snap points.");
                        return;
                    }
                    topPoints.Add(snapPoint.localPosition);
                }
            }
        }
        if (topPoints.Count != 2)
        {
            Log.LogError($"{gameObject.name} is not a RoofTop piece");
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
                    Log.LogWarning($"Invalid ridge axis for RoofTop piece: {gameObject.name}, will not add extra snap points.");
                    return;
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

        gameObject.AddSnapPoints(new[] { topCenter, mid1, mid2 });
    }


    private static Vector3 GetCenterOfSnapPoints(List<Transform> snapPoints)
    {
        // compute center snap point
        Vector3 centerPoint = Vector3.zero;
        foreach (Transform snapPoint in snapPoints)
        {
            centerPoint += snapPoint.transform.localPosition;
        }
        centerPoint /= snapPoints.Count;
        return centerPoint;
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
