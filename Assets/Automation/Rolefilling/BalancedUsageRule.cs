using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Rule that tries to balance usage of smart objects. Does not reject anything, simply indicates
/// which objects are better matches for the rule.
/// </summary>
public class BalancedUsageRule : UsageAnalysisRule
{
    public override string Name
    {
        get { return "Balanced Usage"; }
    }

    /// <summary>
    /// Returns satisfaction based on trying to balance usage across all objects. Will prefer
    /// rarely used over often used objects, however will not reject any population.
    /// </summary>
    protected override int[] SatisfiesWithUsage(IEnumerable<EventPopulation> populations, StoryArc arc, StoryEvent toFillIn)
    {
        int[] result = new int[populations.Count()];
        int index = 0;
        foreach (EventPopulation population in populations)
        {
            for (int i = 0; i < population.Count; i++)
            {
                result[index] += maxUsage - GetUsage(population[i].Id);
            }
            index++;
        }
        return result;
    }
}
