using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A rule which does more or less the contrary of BalancedUsageRule. It prefers
/// using characters that participate in many events under the assumption that
/// these are more significant to the story and as such likelier to be used by the
/// author himself.
/// </summary>
public class PreferOftenUsedObjectsRule : UsageAnalysisRule
{
    public override string Name
    {
        get { return "Prefer Often Used Objects"; }
    }

    protected override int[] SatisfiesWithUsage(IEnumerable<EventPopulation> populations, StoryArc arc, StoryEvent toFillIn)
    {
        int[] result = new int[populations.Count()];
        int index = 0;
        foreach (EventPopulation population in populations)
        {
            for (int i = 0; i < population.Count; i++)
            {
                result[index] += GetUsage(population[i].Id);
            }
            index++;
        }
        return result;
    }
}
