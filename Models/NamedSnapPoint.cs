using UnityEngine;

namespace ExtraSnapsMadeEasy.Models;

internal sealed class NamedSnapPoint
{
    public Vector3 LocalPosition { get; }
    public string Name { get; }
    /// <summary>
    /// The RequestedIndex is currently ignored, but is kept as some calculations already want to provide this information
    /// </summary>
    public int? RequestedIndex { get; }

    public NamedSnapPoint(Vector3 localPosition, string name, int? requestedIndex = null)
    {
        LocalPosition = localPosition;
        Name = name;
        RequestedIndex = requestedIndex;
    }

    public NamedSnapPoint(float x, float y, float z, string name, int? requestedIndex = null)
    {
        LocalPosition = new Vector3(x, y, z);
        Name = name;
        RequestedIndex = requestedIndex;
    }

    public override string ToString()
    {
        return $"{LocalPosition} [{Name}]";
    }
}
