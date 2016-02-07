using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A spatial area criteria uses a given area to identify whether a SmartObject satisfies the criteria.
/// </summary>
public class SpatialAreaCriteria : ISmartCrowdCriteria
{
    public readonly IArea Area;

    public SpatialAreaCriteria(IArea area)
    {
        this.Area = area;
    }

    public bool SatisfiesCriteria(SmartObject smartObject)
    {
        return Area.Contains(smartObject.transform.position);
    }
}
