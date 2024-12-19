using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtraSnapsMadeEasy.Extensions;
internal static class ListOfTransformExtensions
{
    private const float Epsilon = 0.0001f;

    internal static bool AreCoplanar(this List<Transform> transforms)
    {
        // By definition, 3 points or less are always on the same plane
        if (transforms.Count < 4)
        {
            return true;
        }

        Vector3 p0 = transforms[0].localPosition;
        Vector3 p1 = transforms[1].localPosition;
        Vector3 p2 = transforms[2].localPosition;

        Vector3 normal = Vector3.Cross(p1 - p0, p2 - p0);

        for (int i = 3; i < transforms.Count; i++)
        {
            float dot = Vector3.Dot(normal, transforms[i].localPosition - p0);
            // Allow for some epsilon to handle floating point precision issues
            if (Mathf.Abs(dot) > Epsilon)
            {
                return false;
            }
        }

        return true;
    }

    internal static bool AreCollinear(this List<Transform> transforms)
    {
        int pointsCount = transforms.Count;
        if (pointsCount < 3)
        {
            // One or two points are Collinear by definition
            return true;
        }

        Vector3 origin = transforms[0].localPosition;
        Vector3 baseVector = transforms[1].localPosition - origin;

        for (int i = 2; i < pointsCount; i++)
        {
            Vector3 currentVector = transforms[i].localPosition - origin;
            Vector3 crossProduct = Vector3.Cross(baseVector, currentVector);

            // Allow for some epsilon to handle floating point precision issues
            if (crossProduct.magnitude >= Epsilon)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Checks for snap point at [0, 0, 0]
    /// </summary>
    /// <param name="snapPoints"></param>
    /// <returns></returns>
    internal static bool ContainsOriginSnapPoint(this List<Transform> snapPoints)
    {
        for (int i = 0; i < snapPoints.Count; ++i)
        {
            if (snapPoints[i].localPosition == Vector3.zero)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Compute the position of the center of the provided snap points.
    /// </summary>
    /// <param name="snapPoints">List of at least one snap point</param>
    /// <returns>The average of the points provided.</returns>
    /// <exception cref="ArgumentException">Cannot get the center of 0 SnapPoints</exception>
    internal static Vector3 GetCenter(this List<Transform> snapPoints)
    {
        if (snapPoints.Count == 0)
        {
            throw new ArgumentException("Cannot get the center of 0 SnapPoints", nameof(snapPoints));
        }
        Vector3 sum = Vector3.zero;
        foreach (Transform snapPoint in snapPoints)
        {
            sum += snapPoint.transform.localPosition;
        }
        return sum / snapPoints.Count;
    }
}
