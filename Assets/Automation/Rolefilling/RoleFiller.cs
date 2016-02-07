using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class for trying to fill in all the empty roles of a given event stub with a given set of SmartObjects to use.
/// Uses a given set of rules to determine which objects should be added to what role.
/// Does not add the same object to a given event stub twice.
/// </summary>
public class RoleFiller
{
    private IEnumerable<IRoleFillerRule> rules;

    private StoryArc arc;

    private int beatLevel;

    private int eventIndex;

    private StoryEvent evnt;

    private StateSpaceManager manager;

    public RoleFiller(
        StoryArc arc,
        int level,
        StoryEvent evnt,
        IEnumerable<IRoleFillerRule> rules,
        StateSpaceManager manager)
    {
        this.manager = manager;
        this.arc = arc;
        this.beatLevel = level;
        this.evnt = evnt;
        this.rules = rules;
        this.eventIndex = FindEventIndex();
    }

    /// <summary>
    /// Finds the index of the given StoryEvent in the specified beat
    /// of the arc.
    /// </summary>
    private int FindEventIndex()
    {
        for (int i = 0; i < arc.Beats[beatLevel].Events.Length; i++)
        {
            if (evnt.ID == arc.Beats[beatLevel].Events[i].ID)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Fills in the empty roles for the given event stub, using the given SmartObjects and rules.
    /// Returns whether the roles could successfully be filled in.
    /// Returns the EventPopulation corresponding to the best match if it exists, and null
    /// otherwise.
    /// The returned EventPopulation only has SmartObject params.
    /// </summary>
    public EventPopulation FillInRoles()
    {
        //TODO currently broken in some cases, when implicit objects and their state are required. Simulation
        //should automatically fill all these in before starting simulation, then it SHOULD work again.

        IEnumerable<EventPopulation> validPops = GetValidPopulations<EditablePrototype>(Simulate());

        //Get only those populations with valid types
        List<EventPopulation> validTypePops = GetValidTypePopulations(validPops);

        //first step: add the population to the necessary story event, and simulate all later beats
        //to check if it would cause any violations of later states
        //List<EventPopulation> nonViolatingPops = GetNonViolatingPopulations(validTypePops);
		//REMOVED BECAUSE FUTURE BEATS MIGHT NOT BE FULLY SPECIFIED
      
        //second step: all valid populations can now be checked against the rules for the best match
        Dictionary<EventPopulation, int> matchQuality = CalculateMatches(validTypePops);

        //find the best match
        return ToSmartObjectPopulation(FindBestMatch(matchQuality));
    }

    #region Helper functions

    /// <summary>
    /// Simulates running the story arc up to the beatLevel, to retrieve the states
    /// of all objects at that point.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<EditablePrototype> Simulate()
    {
        EventPlanner planner = new EventPlanner(manager);
        planner.SimulateUpTo(manager, beatLevel, arc);
        DefaultState stateBeforeBeat = manager.globalState;
		EditablePrototype[] statesBeforeBeat = new EditablePrototype[stateBeforeBeat.worldstate.Prototypes.Count];
		for(int i=0; i < stateBeforeBeat.worldstate.Prototypes.Count; i++)
        	statesBeforeBeat[i] = new EditablePrototype(stateBeforeBeat.worldstate.Prototypes[i]);
        manager.resetGlobalState();
        return statesBeforeBeat;
    }

    /// <summary>
    /// Get all the valid populations, depending on the given set of objects
    /// to choose from to build the populations. Will remove any objects with another
    /// event in the same beat, and fix any slots set by the author.
    /// </summary>
    private IEnumerable<EventPopulation> GetValidPopulations<T>(IEnumerable<T> allObjects)
        where T : IHasState
    {
        //remove all those SmartObjects with another event already in the same beat
        //RoleFillIn should never cause changes in the structure of the narrative
        HashSet<uint> blockedObjects = GetBeatParticipants(arc, beatLevel);
        var available = allObjects.Where((T p) => !blockedObjects.Contains(p.Id));
        //any arguments that are set by the author should not be changed by FillIn
        Dictionary<int, IHasState> fixedSlots = new Dictionary<int, IHasState>();
        for (int i = 0; i < evnt.Participants.Count(); i++)
        {
            if (evnt.Participants[i] != uint.MaxValue)
            {
                fixedSlots[i] =
                    allObjects.Where((T p) => p.Id == evnt.Participants[i]).First();
            }
        }

        IEnumerable<IHasState> castObjects = allObjects.Cast<IHasState>();
        return EventPopulator.GetValidPopulations(
                evnt.Signature,
                available.Cast<IHasState>(),
                castObjects,
                fixedSlots);
    }

    /// <summary>
    /// Get only the populations with matching valid types for the given event.
    /// </summary>
    private List<EventPopulation> GetValidTypePopulations(IEnumerable<EventPopulation> candidates)
    {
        List<EventPopulation> result = new List<EventPopulation>();
        foreach (EventPopulation pop in candidates)
        {
            if (evnt.Signature.CheckTypes(ToSmartObjectPopulation(pop).AsParams()))
            {
                result.Add(pop);
            }
        }
        return result;
    }


    /// <summary>
    /// Finds the population giving the best match for the rules. Returns null if no match
    /// is found.
    /// </summary>
    private EventPopulation FindBestMatch(IDictionary<EventPopulation, int> matchQuality)
    {
        Tuple<EventPopulation, int> bestMatch = new Tuple<EventPopulation, int>(null, -1);
        foreach (EventPopulation pop in matchQuality.Keys)
        {
            if (matchQuality[pop] > bestMatch.Item2)
            {
                bestMatch.Item1 = pop;
                bestMatch.Item2 = matchQuality[pop];
            }
        }
        return bestMatch.Item1;
    }

    /// <summary>
    /// Calculate the quality of the matches for all candidate populations based on the defined
    /// set of rules.
    /// </summary>
    private Dictionary<EventPopulation, int> CalculateMatches(IList<EventPopulation> candidates)
    {
        Dictionary<EventPopulation, int> matchQuality = new Dictionary<EventPopulation, int>();
        candidates.ForEach((EventPopulation pop) => matchQuality[pop] = 0);
        foreach (IRoleFillerRule rule in rules)
        {
            int[] match = rule.Satisfies(candidates, arc, evnt);
            for (int i = 0; i < match.Length; i++)
            {
                if (match[i] < 0 || matchQuality[candidates[i]] < 0)
                {
                    matchQuality[candidates[i]] = -1;
                    continue;
                }
                matchQuality[candidates[i]] = matchQuality[candidates[i]] + match[i];
            }
        }
        return matchQuality;
    }

    /// <summary>
    /// Get all the populations that, if inserted to the story event, would not cause any
    /// other event in a later beat to violate its precondition.
    /// </summary>
    private List<EventPopulation> GetNonViolatingPopulations(IEnumerable<EventPopulation> candidates)
    {
        List<EventPopulation> result = new List<EventPopulation>();
        EventPlanner planner = new EventPlanner(manager);
        StoryEvent copy = new StoryEvent(evnt);
        foreach (EventPopulation pop in candidates)
        {
            for (int i = 0; i < pop.Count; i++)
            {
                arc.Beats[beatLevel].Events[eventIndex].Participants[i] = pop[i].Id;
            }

            planner.SimulateUpTo(manager, arc.Beats.Length - 1, arc);

            //check if all the preconditions still hold
            bool allRequirementsOk = true;
            for (int i = beatLevel + 1; i < arc.Beats.Length; i++)
            {
                for (int j = 0; j < arc.Beats[i].Events.Length; j++)
                {
                    EventSignature sig = arc.Beats[i].Events[j].Signature;

                    ReadOnlyPrototype[] allStateObjs = 
                        manager.globalState.worldstate.Prototypes.ToArray();
                    IHasState[] filtered =
                        FilterParticipants<ReadOnlyPrototype>(
                            allStateObjs,
                            arc.Beats[i].Events[j]);

                    if (sig.CheckRequirements(filtered, allStateObjs) == false)
                    {
                        allRequirementsOk = false;
                    }
                }
            }
            manager.resetGlobalState();
            if (allRequirementsOk)
            {
                result.Add(pop);
            }
        }
        arc.Beats[beatLevel].Events[eventIndex] = copy;
        return result;
    }

    /// <summary>
    /// From the choice set, get the participants of the given StoryEvent based on
    /// the IHasState.Id of the event's participants.
    /// </summary>
    private IHasState[] FilterParticipants<T>(IEnumerable<T> choice, StoryEvent evnt)
        where T : IHasState
    {
        IHasState[] result = new IHasState[evnt.Participants.Length];
        for (int i = 0; i < evnt.Participants.Length; i++)
        {
            result[i] = choice.Where((T t) => t.Id == evnt.Participants[i]).First();
        }
        return result;
    }

    /// <summary>
    /// Get all the participants in the given beat.
    /// </summary>
    private HashSet<uint> GetBeatParticipants(StoryArc arc, int level)
    {
        HashSet<uint> participants = new HashSet<uint>();
        for (int i = 0; i < arc.Beats[level].Events.Length; i++)
        {
            participants.UnionWith(arc.Beats[level].Events[i].Participants);
        }
        return participants;
    }

    /// <summary>
    /// Return a new EventPopulation, containing the SmartObjects corresponding
    /// to the IHasState.Ids in the original population.
    /// </summary>
    private EventPopulation ToSmartObjectPopulation(EventPopulation original)
    {
        if (original == null)
        {
            return original;
        }
        EventPopulation result = new EventPopulation(0);
        for (int i = 0; i < original.Count; i++)
        {
            result.Add(ObjectManager.Instance.GetObjectById(original[i].Id));
        }
        return result;
    }
    #endregion
}