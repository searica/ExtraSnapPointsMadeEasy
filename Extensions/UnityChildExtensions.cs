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
        global::Utils.IterativeSearchType searchType = global::Utils.IterativeSearchType.BreadthFirst
    )
    {
        return gameObject.transform.FindDeepChild(childName, searchType);
    }
}


/// <summary>
///     Convenience methods for Transforms
/// </summary>
public static class UnityChildExtensions
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
        global::Utils.IterativeSearchType searchType = global::Utils.IterativeSearchType.BreadthFirst
    )
    {
        return global::Utils.FindChild(transform, childName, searchType);
    }
}
