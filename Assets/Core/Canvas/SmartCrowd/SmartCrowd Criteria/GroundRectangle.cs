using UnityEngine;
using System.Collections;

/// <summary>
/// A rectangle on the ground. It is constructed by taking two positions within the world as edges, then a rectangle on ground is created
/// by spanning a rectangle between those position's x and z coordinates. An object is in that rectangle, if its x and z coordinates are
/// contained within the rectangle, no matter its y (j.e. height) coordinate.
/// </summary>
public class GroundRectangle : IArea
{
    public readonly Vector3 FirstEdge;

    public readonly Vector3 SecondEdge;

    private Interval<float> xPositionInterval;

    private Interval<float> zPositionInterval;

    public GroundRectangle(Vector3 worldPosition1, Vector3 worldPosition2)
    {
        float minX, maxX, minZ, maxZ;
        minX = Mathf.Min(worldPosition1.x, worldPosition2.x);
        maxX = Mathf.Max(worldPosition1.x, worldPosition2.x);
        minZ = Mathf.Min(worldPosition1.z, worldPosition2.z);
        maxZ = Mathf.Max(worldPosition1.z, worldPosition2.z);
        xPositionInterval = new Interval<float>(minX, maxX);
        zPositionInterval = new Interval<float>(minZ, maxZ);
        this.FirstEdge = worldPosition1;
        this.SecondEdge = worldPosition2;
    }

    /// <summary>
    /// Gets a random point within the rectangle at the
    /// specified height.
    /// </summary>
    public Vector3 RandomPoint(float height)
    {
        return new Vector3(
            Random.Range(xPositionInterval.min, xPositionInterval.max),
            height,
            Random.Range(zPositionInterval.min, zPositionInterval.max)
        );
    }

    /// <summary>
    /// Returns the ground area covered by this rectangle.
    /// </summary>
    /// <returns></returns>
    public float Area()
    {
        return (xPositionInterval.max - xPositionInterval.min) *
            (zPositionInterval.max - zPositionInterval.min);
    }

    /// <summary>
    /// Returns whether the rectangle contains the given position.
    /// </summary>
    public bool Contains(Vector3 position)
    {
        return (xPositionInterval.IsInRange(position.x) == 0 
            && zPositionInterval.IsInRange(position.z) == 0);
    }
}
