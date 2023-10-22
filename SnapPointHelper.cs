using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* In Unity
 * X = left/right
 * Y = up/down
 * Z = forward/back
 */

namespace ExtraSnapPointsMadeEasy
{
    internal class SnapPointHelper
    {
        internal static bool IsPoint(GameObject gameObject)
        {
            if (gameObject == null) { return false; }
            var snapPoints = GetSnapPoints(gameObject.transform);
            return snapPoints.Count == 1;
        }

        internal static bool IsLine(GameObject gameObject)
        {
            if (gameObject == null) { return false; }
            var snapPoints = GetSnapPoints(gameObject.transform);
            return snapPoints.Count == 2 && EverySnapPointLiesOnExtremums(snapPoints);
        }

        internal static bool IsTriangle(GameObject gameObject)
        {
            if (gameObject == null) { return false; }
            var snapPoints = GetSnapPoints(gameObject.transform);
            return snapPoints.Count == 3 && EverySnapPointLiesOnExtremums(snapPoints);
        }

        internal static bool IsSquare(GameObject gameObject)
        {
            if (gameObject == null) { return false; }
            var snapPoints = GetSnapPoints(gameObject.transform);
            return snapPoints.Count == 4 && EverySnapPointLiesOnExtremums(snapPoints);
        }

        internal static bool IsCube(GameObject gameObject)
        {
            if (gameObject == null) { return false; }
            var snapPoints = GetSnapPoints(gameObject.transform);
            return snapPoints.Count == 8 && EverySnapPointLiesOnExtremums(snapPoints);
        }

        internal static List<Transform> GetSnapPoints(Transform pieceTransform)
        {
            List<Transform> points = new();

            if (pieceTransform == null)
            {
                return points;
            }

            for (var index = 0; index < pieceTransform.childCount; ++index)
            {
                var child = pieceTransform.GetChild(index);
                if (child.CompareTag("snappoint"))
                {
                    points.Add(child);
                }
            }
            return points;
        }

        /// <summary>
        ///     Check that all snap points like on extremus
        ///     as determined from the maximums and minimums
        ///     of the snap points.
        /// </summary>
        /// <param name="snapPoints"></param>
        /// <returns></returns>
        private static bool EverySnapPointLiesOnExtremums(List<Transform> snapPoints)
        {
            var minimums = SolveMinimumsOf(snapPoints);
            var maximums = SolveMaximumsOf(snapPoints);
            foreach (var snapNode in snapPoints)
            {
                if (!LiesOnExtremums(snapNode.localPosition, minimums, maximums))
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
            var xMin = float.PositiveInfinity;
            var yMin = float.PositiveInfinity;
            var zMin = float.PositiveInfinity;
            foreach (var node in snapPoints)
            {
                var sn = node.localPosition;
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
            var xMax = float.NegativeInfinity;
            var yMax = float.NegativeInfinity;
            var zMax = float.NegativeInfinity;
            foreach (var node in snapPoints)
            {
                var sn = node.localPosition;
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
        private static bool LiesOnExtremums(
            Vector3 snapPoint,
            Vector3 minimums,
            Vector3 maximums
        )
        {
            if (snapPoint.x != minimums.x && snapPoint.x != maximums.x)
            {
                Log.LogInfo("x");
                return false;
            }
            if (snapPoint.y != minimums.y && snapPoint.y != maximums.y)
            {
                Log.LogInfo("y");
                return false;
            }
            if (snapPoint.z != minimums.z && snapPoint.z != maximums.z)
            {
                Log.LogInfo("z");
                return false;
            }
            return true;
        }

        internal static Vector3 GetCenterOfSnapPoints(List<Transform> snapPoints)
        {
            // compute center snap point
            var centerPoint = Vector3.zero;
            foreach (var snapPoint in snapPoints)
            {
                centerPoint += snapPoint.transform.localPosition;
            }
            centerPoint /= snapPoints.Count;
            return centerPoint;
        }

        /// <summary>
        ///     Add a snap point at the midpoint between
        ///     the two existing snap points.
        /// </summary>
        /// <param name="gameObject"></param>
        internal static void AddSnapPointToLine(GameObject gameObject)
        {
            var snapPoints = GetSnapPoints(gameObject.transform);
            var centerPoint = GetCenterOfSnapPoints(snapPoints);
            AddSnapPoints(
                gameObject,
                new Vector3[]
                {
                    centerPoint,
                }
            );
        }

        /// <summary>
        ///     Add a snap points at the midpoint along
        ///     each edge of the triangle and in the center of the triangle.
        /// </summary>
        /// <param name="gameObject"></param>
        internal static void AddSnapPointsToTriangle(GameObject gameObject)
        {
            var snapPoints = GetSnapPoints(gameObject.transform);
            var pts = new HashSet<Vector3>();

            // compute center snap point
            var centerPoint = GetCenterOfSnapPoints(snapPoints);
            pts.Add(centerPoint);

            // compute mid points of each edge
            for (int i = 0; i < snapPoints.Count - 1; i++)
            {
                var snap1 = snapPoints[i];
                for (int j = i + 1; j < snapPoints.Count; j++)
                {
                    var snap2 = snapPoints[j];
                    var newPt = (snap1.transform.localPosition
                        + snap2.transform.localPosition) / 2;
                    if (newPt != centerPoint)
                    {
                        pts.Add(newPt);
                    }
                }
            }
            AddSnapPoints(gameObject, pts);
        }

        /// <summary>
        ///     Add a snap points at the midpoint along
        ///     each edge of the triangle and in the center of the triangle.
        /// </summary>
        /// <param name="gameObject"></param>
        internal static void AddSnapPointsToSquare(GameObject gameObject)
        {
            var snapPoints = GetSnapPoints(gameObject.transform);
            var pts = new HashSet<Vector3>();

            // compute center snap point
            var centerPoint = GetCenterOfSnapPoints(snapPoints);
            pts.Add(centerPoint);

            // compute mid points of each edge
            for (int i = 0; i < snapPoints.Count - 1; i++)
            {
                var snap1 = snapPoints[i];
                for (int j = i + 1; j < snapPoints.Count; j++)
                {
                    var snap2 = snapPoints[j];
                    var newPt = (snap1.transform.localPosition
                        + snap2.transform.localPosition) / 2;
                    if (newPt != centerPoint)
                    {
                        pts.Add(newPt);
                    }
                }
            }
            AddSnapPoints(gameObject, pts);
        }

        /// <summary>
        ///     Adds a snap point to the local center of
        ///     GameObject if there is not already one there.
        /// </summary>
        /// <param name="gameObject"></param>
        internal static void AddLocalCenterSnapPoint(GameObject gameObject)
        {
            // Only add snap point if it doesn't have one there already
            var snapPts = GetSnapPoints(gameObject.transform);
            foreach (var snapPoint in snapPts)
            {
                if (snapPoint.transform.localPosition == Vector3.zero)
                {
                    return;
                }
            }
            AddSnapPoints(
                gameObject,
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                }
            );
        }

        internal static void AddSnapPoints(
            GameObject gameObject,
            IEnumerable<Vector3> points,
            bool fixPiece = false,
            bool fixZClipping = false
        )
        {
            GameObject target = ZNetScene.instance.GetPrefab(gameObject.name);

            if (!target)
            {
                Log.LogWarning(
                    $"Prefab {gameObject.name} not found. Cannot add snappoints"
                );
                return;
            }

            if (fixPiece)
            {
                FixPiece(gameObject);
            }

            float z = 0f;

            foreach (Vector3 point in points)
            {
                Vector3 pos = point;

                if (fixZClipping)
                {
                    pos.z = z;
                    z += 0.0001f;
                }

                CreateSnapPoint(pos, target.transform);
            }
        }

        private static void CreateSnapPoint(Vector3 pos, Transform parent)
        {
            GameObject snappoint = new("_snappoint");
            snappoint.transform.parent = parent;
            snappoint.transform.localPosition = pos;
            snappoint.tag = "snappoint";
            snappoint.SetActive(false);
        }

        internal static void FixPiece(GameObject gameObject)
        {
            if (!gameObject)
            {
                Log.LogWarning($"Prefab is null. Cannot fix pieceTransform");
                return;
            }

            foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>())
            {
                collider.gameObject.layer = LayerMask.NameToLayer("pieceTransform");
            }
        }
    }
}