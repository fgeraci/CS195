// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EventPopulation
{
    private bool isDirty;
    private readonly List<IHasState> parameters;

    private readonly List<IHasState> members;
    private readonly HashSet<IHasState> membersHashed;

    public int Count { get { return this.members.Count; } }
    public IHasState this[int index] 
    {
        get { return this.members[index]; } 
    }

    /// <summary>
    /// Creates an empty population with a given capacity
    /// </summary>
    public EventPopulation(int capacity)
    {
        this.isDirty = true;
        this.parameters = new List<IHasState>(capacity);
        this.members = new List<IHasState>(capacity);
        this.membersHashed =
            new HashSet<IHasState>(CompareById.Instance);
    }

    /// <summary>
    /// Creates a new population with an initial member
    /// </summary>
    /// <param name="capacity"></param>
    /// <param name="firstMember"></param>
    public EventPopulation(int capacity, IHasState firstMember)
        : this(capacity)
    {
        this.Add(firstMember);
    }

    /// <summary>
    /// Creates a new population from an existing one and an additional member
    /// </summary>
    public EventPopulation(EventPopulation source, IHasState newMember)
        : this(source.members.Capacity)
    {
        this.members.AddRange(source.members);
        foreach (IHasState member in source.membersHashed)
            this.membersHashed.Add(member);
        this.Add(newMember);
    }

    /// <summary>
    /// Adds an object to the population if the object does not already
    /// exist in the population
    /// </summary>
    /// <param name="obj"></param>
    public void Add(IHasState obj)
    {
        this.isDirty = true;
        this.members.Add(obj);
        this.membersHashed.Add(obj);
    }

    /// <summary>
    /// Returns true iff this population contains an object
    /// </summary>
    public bool Contains(IHasState obj)
    {
        return this.membersHashed.Contains(obj);
    }

    /// <summary>
    /// Returns an ordered enumeration of all members
    /// </summary>
    public IList<IHasState> Members
    {
        get
        {
            return this.members.AsReadOnly();
        }
    }

    /// <summary>
    /// Returns the population cast as a parameter list
    /// (This function will cache to prevent redundant casts)
    /// </summary>
    public IList<IHasState> AsParams()
    {
        if (this.isDirty == true)
        {
            this.parameters.Clear();
            this.parameters.AddRange(this.members);
            this.isDirty = false;
        }
        return this.parameters.AsReadOnly();
    }
}