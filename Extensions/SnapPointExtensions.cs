
using System.Collections.Generic;
using UnityEngine;

namespace ExtraSnapPointsMadeEasy.Extensions
{
    internal static class SnapPointExtensions
    {
        /// <summary>
        ///     Name of all child snap point objects added by this mod.
        /// </summary>
        internal const string SnapPointName = "ExtraSnapPoint";

        internal static bool HasNoSnapPoints(this GameObject gameObject)
        {
            return gameObject.GetSnapPoints().Count == 0;
        }

        internal static List<Transform> GetSnapPoints(this Transform transform)
        {
            List<Transform> points = new();

            if (transform == null)
            {
                return points;
            }

            for (var index = 0; index < transform.childCount; ++index)
            {
                var child = transform.GetChild(index);
                if (child.CompareTag("snappoint"))
                {
                    points.Add(child);
                }
            }

            return points;
        }


        internal static List<Transform> GetSnapPoints(this GameObject gameObject)
        {
            List<Transform> points = new();

            if (!gameObject)
            {
                return points;
            }
            var transform = gameObject.transform;
            for (var index = 0; index < transform.childCount; ++index)
            {
                var child = transform.GetChild(index);
                if (child.CompareTag("snappoint"))
                {
                    points.Add(child);
                }
            }

            return points;
        }

        /// <summary>
        ///     Adds a snap point to the local center of
        ///     GameObject if there is not already one there.
        /// </summary>
        /// <param name="gameObject"></param>
        internal static void AddLocalCenterSnapPoint(this GameObject gameObject)
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

            AddSnapPoints(gameObject, new Vector3[] { Vector3.zero });
        }

        internal static void AddSnapPoints(
            this GameObject gameObject,
            IEnumerable<Vector3> points,
            bool fixPiece = false,
            bool fixZClipping = false
        )
        {
            if (!gameObject)
            {
                Log.LogWarning("GameObject is null. Cannot add snappoints");
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

                CreateSnapPoint(pos, gameObject.transform);
            }
        }

        private static void CreateSnapPoint(Vector3 pos, Transform parent)
        {
            GameObject snappoint = new(SnapPointName);
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
