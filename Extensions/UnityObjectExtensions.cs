using System;
using UnityEngine;

namespace ExtraSnapsMadeEasy.Extensions;


/// <summary>
///     Convenience methods for GameObjects
/// </summary>
internal static class GameObjectExtensions
{
    /// <summary>
    ///     Extension method to find nested children by name using either
    ///     a breadth-first or depth-first search. Default is breadth-first.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="childName">Name of the child object to search for.</param>
    /// <param name="searchType">Whether to preform a breadth first or depth first search. Default is breadth first.</param>
    public static Transform FindDeepChild(
        this GameObject gameObject,
        string childName,
        Utils.IterativeSearchType searchType = global::Utils.IterativeSearchType.BreadthFirst
    )
    {
        return gameObject.transform.FindDeepChild(childName, searchType);
    }

    /// <summary>
    ///     Check if GameObject has any of the specified components.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="components"></param>
    /// <returns></returns>
    public static bool HasAnyComponent(this GameObject gameObject, params Type[] components)
    {
        foreach (Type compo in components)
        {
            if (gameObject.GetComponent(compo))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    ///     Check if GameObject has any of the specified components.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="componentNames"></param>
    /// <returns></returns>
    public static bool HasAnyComponent(this GameObject gameObject, params string[] componentNames)
    {
        foreach (string name in componentNames)
        {
            if (gameObject.GetComponent(name))
            {
                return true;
            }
        }

        return false;
    }


}


/// <summary>
///     Convenience methods for Transforms
/// </summary>
public static class TransformExtensions
{
    /// <summary>
    ///     Extension method to find nested children by name using either
    ///     a breadth-first or depth-first search. Default is breadth-first.
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="childName">Name of the child object to search for.</param>
    /// <param name="searchType">Whether to preform a breadth first or depth first search. Default is breadth first.</param>
    /// <returns></returns>
    public static Transform FindDeepChild(
        this Transform transform,
        string childName,
        Utils.IterativeSearchType searchType = Utils.IterativeSearchType.BreadthFirst
    )
    {
        return Utils.FindChild(transform, childName, searchType);
    }
}
