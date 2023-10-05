using System.Collections.Generic;
using UnityEngine;

/* In Unity
 * X = left/right
 * Y = up/down
 * Z = forward/back
 */

namespace ExtraSnapPointsMadeEasy
{
    public class SnapPointHelper
    {
        public static void AddCenterSnapPoint(string name)
        {
            SnapPointHelper.AddSnapPoints(
                name,
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                }
            );
        }

        public static void AddSnapPoints(
            string name,
            IEnumerable<Vector3> points,
            bool fixPiece = false,
            bool fixZClipping = false
        )
        {
            GameObject target = ZNetScene.instance.GetPrefab(name);

            if (!target)
            {
                Log.LogWarning(
                    $"Prefab {name} not found. Cannot add snappoints"
                );
                return;
            }

            if (fixPiece)
            {
                FixPiece(name);
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
            GameObject snappoint = new GameObject("_snappoint");
            snappoint.transform.parent = parent;
            snappoint.transform.localPosition = pos;
            snappoint.tag = "snappoint";
            snappoint.SetActive(false);
        }


        public static void FixPiece(string name)
        {
            GameObject target = ZNetScene.instance.GetPrefab(name);

            if (!target)
            {
                Log.LogWarning($"Prefab {name} not found. Cannot fix piece");
                return;
            }

            foreach (Collider collider in target.GetComponentsInChildren<Collider>())
            {
                collider.gameObject.layer = LayerMask.NameToLayer("piece");
            }
        }
    }
}
