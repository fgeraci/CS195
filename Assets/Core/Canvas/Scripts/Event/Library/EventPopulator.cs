// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System.Collections;
using System.Collections.Generic;

public static class EventPopulator
{
    /// <summary>
    /// A typed list of lists for holding "bins" of potential participants
    /// for each participant slot of an EventDescriptor
    /// </summary>
    private class BinHolder
    {
        private readonly List<List<IHasState>> bins;

        public int Count { get { return this.bins.Count; } }
        public IEnumerable<IHasState> this[int index]
        {
            get { return this.bins[index].AsReadOnly(); }
        }

        public BinHolder(int binCount)
        {
            this.bins = new List<List<IHasState>>(binCount);
        }

        public void AddBin(IEnumerable<IHasState> bin)
        {
            this.bins.Add(new List<IHasState>(bin));
        }
    }

    /// <summary>
    /// Given an enumeration of objects, assembles them into valid
    /// participant populations for this event. Requires that the event
    /// takes only SmartObject participants (or equivalent).
    /// </summary>
    public static IEnumerable<EventPopulation> GetValidPopulations(
        EventDescriptor evtDesc,
        IEnumerable<IHasState> objsToConsider,
        IEnumerable<IHasState> allWorldObjs)
    {
        BinHolder bins = BinIntoParameterSlots(evtDesc, objsToConsider);
        return GetApplicable(
            evtDesc, 
            GetPopulations(evtDesc, bins), 
            allWorldObjs);
    }

    public static IEnumerable<EventPopulation> GetValidPopulations(
        EventDescriptor evtDesc,
        IEnumerable<IHasState> objsToConsider,
        IEnumerable<IHasState> allWorldObjs,
        IDictionary<int, IHasState> fixedSlots)
    {
        BinHolder bins = 
            BinIntoParameterSlots(evtDesc, objsToConsider, fixedSlots);
        return GetApplicable(
            evtDesc, 
            GetPopulations(evtDesc, bins), 
            allWorldObjs);
    }

    /// <summary>
    /// Returns all populations that are valid for this signature
    /// </summary>
    private static IEnumerable<EventPopulation> GetApplicable(
        EventDescriptor evtDesc,
        IEnumerable<EventPopulation> candidates,
        IEnumerable<IHasState> allWorldObjs)
    {
        foreach (EventPopulation candidate in candidates)
            if (evtDesc.CheckRequirements(candidate.AsParams(), allWorldObjs) == true)
                yield return candidate;
    }

    /// <summary>
    /// Takes a list of world SmartObjects and an EventDescriptor and creates
    /// one bin for each of the event's participant slot containing the
    /// objects that satisfy that slot's State (not Relation) requirements.
    /// Makes sure that any index in fixedSlots only considers the accompanied
    /// SmartObject. Does however not check state for those.
    /// </summary>
    private static BinHolder BinIntoParameterSlots(
        EventDescriptor evtDesc,
        IEnumerable<IHasState> objs,
        IDictionary<int, IHasState> fixedSlots = null)
    {
        StateName[][] stateReqs = evtDesc.StateRequirements;
        BinHolder bins = new BinHolder(stateReqs.Length);

        for (int i = 0; i < stateReqs.Length; i++)
        {
            if (fixedSlots != null && fixedSlots.ContainsKey(i) == true)
                bins.AddBin(new IHasState[] { fixedSlots[i] });
            else
                bins.AddBin(Filter.ByState(objs, stateReqs[i]));
        }

        return bins;
    }

    /// <summary>
    /// Given a bin holder, creates all variations of event populations
    /// without using a single object in more than one slot
    /// </summary>
    private static List<EventPopulation> GetPopulations(
        EventDescriptor evtDesc,
        BinHolder bins)
    {
        int slots = bins.Count;
        if (slots == 0)
            return new List<EventPopulation>();

        // Convert the first bin to a list of EventPopulations
        List<EventPopulation> result =
            bins[0].Convert(s => new EventPopulation(slots, s));

        for (int i = 1; i < slots; i++)
            result = AddBin(evtDesc, result, bins[i]);

        return result;
    }

    /// <summary>
    /// Takes a bin and permutes it through a list of event populations
    /// </summary>
    private static List<EventPopulation> AddBin(
        EventDescriptor evtDesc,
        IEnumerable<EventPopulation> lhs,
        IEnumerable<IHasState> binMembers)
    {
        List<EventPopulation> result = new List<EventPopulation>();

        foreach (EventPopulation lPopulation in lhs)
            foreach (IHasState rMember in binMembers)
                if (CheckContains(evtDesc, lPopulation, rMember) == false)
                    result.Add(new EventPopulation(lPopulation, rMember));
        return result;
    }

    private static bool CheckContains(
        EventDescriptor evtDesc,
        EventPopulation currentPop,
        IHasState newMember)
    {
        IList<IHasState> currentMembers = currentPop.Members;
        int newIndex = currentMembers.Count;

        for (int i = 0; i < currentMembers.Count; i++)
        {
            IHasState currentMember = currentMembers[i];
            if (evtDesc.CanEqual(i, newIndex) == false)
                if (object.ReferenceEquals(currentMember, newMember) == true)
                    return true;
        }

        return false;
    }
}
