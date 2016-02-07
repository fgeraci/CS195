using UnityEngine;
using TreeSharpPlus;

/// <summary>
/// An area consisting of multiple waypoints, which can be used for multiple
/// simaltaneous Approach events.
/// </summary>
public class SmartWaypointArea : SmartObject 
{
    public override string Archetype
    {
        get { return this.GetType().Name; }
    }

    private int nextIndex;

    /// <summary>
    /// All available waypoints.
    /// </summary>
    public Transform[] Waypoints;

    /// <summary>
    /// Get a waypoint from the area.
    /// </summary>
    public Transform GetWaypoint()
    {
        nextIndex = (nextIndex + 1) % Waypoints.Length;
        return Waypoints[nextIndex];
    }

}
