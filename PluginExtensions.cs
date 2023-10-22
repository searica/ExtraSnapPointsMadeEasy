using System.Collections.Generic;
using UnityEngine;

namespace ExtraSnapPointsMadeEasy
{
    internal static class TransformExtensions
    {
        /// <summary>
        ///     Breadth-first search for transform child by name.
        /// </summary>
        /// <param name="aParent"></param>
        /// <param name="aName"></param>
        /// <returns></returns>
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            Queue<Transform> queue = new();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == aName)
                {
                    return c;
                }

                foreach (Transform t in c)
                {
                    queue.Enqueue(t);
                }

            }
            return null;
        }

        /*
        //Depth-first search
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            foreach(Transform child in aParent)
            {
                if(child.name == aName )
                    return child;
                var result = child.FindDeepChild(aName);
                if (result != null)
                    return result;
            }
            return null;
        }
        */
    }

}
