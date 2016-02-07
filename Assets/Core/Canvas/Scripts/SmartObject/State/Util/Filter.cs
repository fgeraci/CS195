// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Filter
{
    private static StateComparer compareByValue = null;
    public static StateComparer CompareByValue
    {
        get
        {
            if (compareByValue == null)
                compareByValue = new StateComparer();
            return compareByValue;
        }
    }

    /// <summary>
    /// Given a state list, returns only those of the desired types
    /// </summary>
    public static IList<StateName> StateList(
        IEnumerable<StateName> statesToFilter,
        IEnumerable<StateName> desiredStateTypes)
    {
        HashSet<StateName> filter = new HashSet<StateName>();
        desiredStateTypes.ForEach((StateName s) => filter.Add(s));

        return new List<StateName>(
            statesToFilter.Where(
            (StateName sn) => filter.Contains(sn) || filter.Contains(~sn)));
    }

    /// <summary>
    /// Returns all objects that have the given state flags
    /// </summary>
    public static IEnumerable<IHasState> ByState(
        IEnumerable<IHasState> candidates,
        IList<StateName> tags)
    {
        foreach (IHasState candidate in candidates)
            if (candidate.State.Require(tags) == true)
                yield return candidate;
        yield break;
    }

    /// <summary>
    /// Returns all objects that have the given relations
    /// relative to the subject
    /// </summary>
    public static IEnumerable<IHasState> ByRelationTo(
        IEnumerable<IHasState> candidates,
        IHasState subject,
        IList<RelationName> tags)
    {
        foreach (IHasState candidate in candidates)
            if (candidate.State.Require(subject.State.Id, tags) == true)
                yield return candidate;
        yield break;
    }

    /// <summary>
    /// Returns all SmartObjects such that the subject has the
    /// given relations towards them
    /// </summary>
    public static IEnumerable<IHasState> ByRelationFrom(
        IHasState subject,
        IEnumerable<IHasState> candidates,
        IList<RelationName> tags)
    {
        foreach (IHasState candidate in candidates)
            if (subject.State.Require(candidate.State.Id, tags) == true)
                yield return candidate;
        yield break;
    }
}
