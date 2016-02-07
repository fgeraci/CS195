using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Class to generalize rules that analyze the usage of participants within an arc.
/// </summary>
public abstract class UsageAnalysisRule : IRoleFillerRule 
{

    public abstract string Name { get; }

    /// <summary>
    /// Maps the participants to how often they are used.
    /// </summary>
    protected Dictionary<uint, int> usage;

    /// <summary>
    /// How often the least used participant is used.
    /// </summary>
    protected int minUsage;

    /// <summary>
    /// How often the most used participant is used.
    /// </summary>
    protected int maxUsage;

    public int[] Satisfies(IEnumerable<EventPopulation> populations, StoryArc arc, StoryEvent toFillIn)
    {
        CalculateUsage(arc);
        return SatisfiesWithUsage(populations, arc, toFillIn);
    }

    protected abstract int[] SatisfiesWithUsage(IEnumerable<EventPopulation> populations, StoryArc arc, StoryEvent toFillIn);
    
    /// <summary>
    /// Calculates the usage of all participants of an arc.
    /// </summary>
    private void CalculateUsage(StoryArc arc)
    {
        this.usage = new Dictionary<uint, int>();
        this.maxUsage = 0;
        this.minUsage = int.MaxValue;
        for (int i = 0; i < arc.Beats.Length; i++)
        {
            for (int j = 0; j < arc.Beats[i].Events.Length; j++)
            {
                StoryEvent evnt = arc.Beats[i].Events[j];
                for (int k = 0; k < evnt.Participants.Length; k++)
                {
                    usage[evnt.Participants[k]] = GetUsage(evnt.Participants[k]) + 1;
                    maxUsage = Mathf.Max(maxUsage, usage[evnt.Participants[k]]);
                    minUsage = Mathf.Min(minUsage, usage[evnt.Participants[k]]);
                }
            }
        }
    }

    /// <summary>
    /// Save getter for the usage. Also adds a default value
    /// to the mapping for the given id if it does not exist.
    /// </summary>
    protected int GetUsage(uint id)
    {
        if (!usage.ContainsKey(id))
        {
            usage[id] = 0;
        }
        return usage[id];
    }
}
