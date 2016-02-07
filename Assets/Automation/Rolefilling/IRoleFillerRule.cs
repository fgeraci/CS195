using System.Collections.Generic;

/// <summary>
/// Interface for implementing rules when filling in roles automatically.
/// The implementing class must have an empty constructor.
/// </summary>
public interface IRoleFillerRule
{
    /// <summary>
    /// Name of the rule to be displayed in the GUI.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// For all the populations, returns the quality of the given match.
    /// A negative result for a population means there is no match at all, while a positive
    /// result indicates a match, with the quality of the match increasing with a higher
    /// result.
    /// Returns one int for each population.
    /// The arc does not need to have the matching events filled in.
    /// </summary>
    int[] Satisfies(IEnumerable<EventPopulation> populations, StoryArc arc, StoryEvent toFillIn);
}