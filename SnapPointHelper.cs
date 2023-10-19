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
        public static void AddCenterSnapPoint(GameObject gameObject)
        {
            AddSnapPoints(
                gameObject,
                new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                }
            );
        }

        public static void AddSnapPoints(
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


        public static void FixPiece(GameObject gameObject)
        {
            if (!gameObject)
            {
                Log.LogWarning($"Prefab is null. Cannot fix piece");
                return;
            }

            foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>())
            {
                collider.gameObject.layer = LayerMask.NameToLayer("piece");
            }
        }
    }
}
