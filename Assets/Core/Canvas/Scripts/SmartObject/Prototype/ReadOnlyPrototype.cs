// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

public class ReadOnlyPrototype : IHasState, IEquatable<ReadOnlyPrototype>
{
    private readonly ReadOnlyState state;

    public ReadOnlyState State
    {
        get { return this.state; }
    }

    IState IHasState.State
    {
        get { return this.state; }
    }

    public uint Id { get { return this.state.Id; } }

    public ReadOnlyPrototype(ReadOnlyState state)
    {
        // No need to clone here since it's read-only
        this.state = state;
    }

    public ReadOnlyPrototype(ReadOnlyPrototype other)
    {
        // No need to clone here since it's read-only
        this.state = other.state;
    }

    /// <summary>
    /// Compares two prototypes, accounting for Id.
    /// </summary>
    public bool Equals(ReadOnlyPrototype other)
    {
        if (this.state.Id != other.Id)
            return false;
        return this.state.Equals(other.state);
    }

    /// <summary>
    /// Requires a collection of states be the given values
    /// </summary>
    public bool Require(params StateName[] states)
    {
        return this.state.Require(states);
    }

    /// <summary>
    /// Requires that we have the given values relative to the object id
    /// </summary>
    public bool Require(uint id, params RelationName[] relations)
    {
        return this.state.Require(id, relations);
    }

    /// <summary>
    /// Sets a collection of states to the given values
    /// </summary>
    public ReadOnlyPrototype SetCopy(ChangeSet changes)
    {
        return new ReadOnlyPrototype(this.state.SetCopy(changes));
    }

    public override int GetHashCode()
    {
        return this.state.GetHashCode();
    }
}