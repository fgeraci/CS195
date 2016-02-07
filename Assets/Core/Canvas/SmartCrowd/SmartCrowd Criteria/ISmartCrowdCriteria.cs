using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A criteria to check whether a certain SmartObject fulfills it.
/// </summary>
public interface ISmartCrowdCriteria  
{
    /// <summary>
    /// Does the given SmartObject fulfill the criteria?
    /// </summary>
    bool SatisfiesCriteria(SmartObject smartObject);
}

public static class SmartCrowdCriteria
{
    private static List<SmartObject> toRemove = new List<SmartObject>();

    /// <summary>
    /// Gets all SmartObjects satisfying the given criteria.
    /// </summary>
    public static HashSet<SmartObject> AllSatisfyingCriteria(ISmartCrowdCriteria criteria)
    {
        return AllSatisfyingCriteria(criteria, new HashSet<SmartObject>());
    }

    /// <summary>
    /// Gets all the SmartObjects satisfying the given criteria, using the known satisfiers given in existingSatisfiers as base. Modifies this set
    /// and then returns it.
    /// </summary>
    public static HashSet<SmartObject> AllSatisfyingCriteria(ISmartCrowdCriteria criteria, HashSet<SmartObject> existingSatisfiers)
    {
        foreach (SmartObject obj in ObjectManager.Instance.GetObjects())
        {
            if (!(obj is SmartCrowd) && criteria.SatisfiesCriteria(obj))
            {
                existingSatisfiers.Add(obj);
            }
        }
        toRemove.Clear();
        foreach (SmartObject obj in existingSatisfiers)
        {
            if (!criteria.SatisfiesCriteria(obj))
            {
                toRemove.Add(obj);
            }
        }
        foreach (SmartObject obj in toRemove)
        {
            existingSatisfiers.Remove(obj);
        }
        return existingSatisfiers;
    }
}
