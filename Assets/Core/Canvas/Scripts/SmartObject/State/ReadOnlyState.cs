// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A read-only wrapper for EditableState that prevents editing
/// </summary>
public class ReadOnlyState : IState, IEquatable<ReadOnlyState>
{
    private readonly EditableState contained;

    public uint Id { get { return contained.Id; } }

    public ReadOnlyState(IState source)
    {
        this.contained = new EditableState(source);
    }

    public bool Require(IList<StateName> states)
    {
        return this.contained.Require(states);
    }

    public bool Require(uint id, IList<RelationName> relations)
    {
        return this.contained.Require(id, relations);
    }

    public ReadOnlyState SetCopy(ChangeSet changeSet)
    {
        return new ReadOnlyState(this.contained.SetCopy(changeSet));
    }

    IState IState.SetCopy(ChangeSet changeSet)
    {
        return new ReadOnlyState(this.contained.SetCopy(changeSet));
    }

    public bool Equals(ReadOnlyState other)
    {
        return this.contained.Equals(other.contained);
    }

    public bool Equals(IState other)
    {
        return this.contained.Equals(other);
    }

    public int GetChecksum()
    {
        return this.contained.GetChecksum();
    }

    public long GetStateBits()
    {
        return this.contained.GetStateBits();
    }

    public long GetRelationBits(uint id)
    {
        return this.contained.GetRelationBits(id);
    }

    public IEnumerable<Tuple<uint, long>> GetStoredRelations()
    {
        return this.contained.GetStoredRelations();
    }

    public override int GetHashCode()
    {
        return this.contained.GetChecksum();
    }
}