// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections;
using System.Collections.Generic;

public class EditableState : IState, IEditableState, IEquatable<EditableState>
{
    public uint Id { get { return this.id; } }

    private long state;
    private int? checkSum = null;

    private readonly Dictionary<uint, long> relations;
    private readonly uint id;

    private EditableState(uint id, long stateBits)
    {
        this.id = id;
        this.state = stateBits;
        this.relations = new Dictionary<uint, long>();
    }

    public EditableState(uint id)
        : this(id, 0x0L) { }

    internal EditableState(IState other)
        : this(other.Id, other.GetStateBits())
    {
        other.GetStoredRelations().ForEach(
            (Tuple<uint, long> v) => this.relations.Add(v.Item1, v.Item2));
    }

    public ReadOnlyState AsReadOnly()
    {
        return new ReadOnlyState(this);
    }

    public bool Equals(IState other)
    {
        return this.CheckEquality(other);
    }

    public bool Equals(IEditableState other)
    {
        return this.CheckEquality(other);
    }

    public bool Equals(EditableState other)
    {
        return this.CheckEquality(other);
    }

    public bool Require(IList<StateName> states)
    {
        return RequireTags(
            this.state,
            states.Convert<StateName, long>(item => (long)item));
    }

    public bool Require(uint id, IList<RelationName> relations)
    {
        if (this.relations.ContainsKey(id) == false)
            this.relations.Add(id, 0x0L);
        return RequireTags(
            this.relations[id],
            relations.Convert<RelationName, long>(item => (long)item));
    }

    public void Set(IList<StateName> states)
    {
        this.checkSum = null;
        this.state =
            SetTags(
                this.state,
                states.Convert<StateName, long>(item => (long)item));
    }

    public void Set(uint id, IList<RelationName> relations)
    {
        this.checkSum = null;
        if (this.relations.ContainsKey(id) == false)
            this.relations.Add(id, 0x0L);
        this.relations[id] =
            SetTags(
                this.relations[id],
                relations.Convert<RelationName, long>(item => (long)item));
    }

    /// <summary>
    /// Creates a copy of this State with the changes applied
    /// </summary>
    public EditableState SetCopy(ChangeSet changeSet)
    {
        EditableState copy = new EditableState(this);
        copy.Set(changeSet.GetStates());
        foreach (var relation in changeSet.GetRelations())
            copy.Set(relation.Item1, relation.Item2);
        return copy;
    }

    IState IState.SetCopy(ChangeSet changeSet)
    {
        return this.SetCopy(changeSet);
    }

    /// <summary>
    /// Hashes the contents of the State, independent of reference
    /// </summary>
    public int GetChecksum()
    {
        // Cache the hash, since we are read-only
        if (this.checkSum.HasValue == false)
        {
            long total = this.GetStateBits();
            foreach (var relation in this.GetStoredRelations())
                if (relation.Item2 != 0x0L)
                    total = total ^ relation.Item2;
            this.checkSum = (int)(total ^ (total >> 32));
        }
        return this.checkSum.Value;
    }

    #region Direct Bit Access
    public void SetStateBits(long state)
    {
        this.checkSum = null;
        this.state = state;
    }

    public void SetRelationBits(uint id, long relation)
    {
        this.checkSum = null;
        this.relations[id] = relation;
    }

    public long GetStateBits()
    {
        return this.state;
    }

    public long GetRelationBits(uint id)
    {
        long result;
        if (this.relations.TryGetValue(id, out result) == false)
            return 0x0L;
        return result;
    }

    public IEnumerable<Tuple<uint, long>> GetStoredRelations()
    {
        foreach (KeyValuePair<uint, long> kv in this.relations)
            yield return new Tuple<uint, long>(kv.Key, kv.Value);
        yield break;
    }
    #endregion

    #region Internals
    /// <summary>
    /// Compares two states by content
    /// </summary>
    private bool CheckEquality(IState other)
    {
        if (this.GetStateBits() != other.GetStateBits())
            return false;

        Dictionary<uint, long> otherDict = new Dictionary<uint, long>();
        other.GetStoredRelations().ForEach(
            (Tuple<uint, long> v) => otherDict.Add(v.Item1, v.Item2));

        return this.relations.DictionaryEqual(otherDict);
    }

    /// <summary>
    /// Helper function to read bitmasks for tags
    /// </summary>
    private static bool RequireTags(long current, IList<long> tags)
    {
        long mask = 0x0L;
        long value = 0x0L;

        foreach (long tag in tags)
        {
            if (tag >= 0)
            {
                mask |= tag;
                value |= tag;
            }
            else
            {
                mask |= ~tag;
            }
        }

        return (current & mask) == value;
    }

    /// <summary>
    /// Helper function to set bitmasks for tags
    /// </summary>
    private static long SetTags(long current, IList<long> tags)
    {
        foreach (long tag in tags)
        {
            if (tag >= 0)
            {
                current |= tag;
            }
            else
            {
                current &= tag;
            }
        }

        return current;
    }
    #endregion

    /// <summary>
    /// Blocking this off because (a) you should never hash the contents
    /// of a mutable object, and (b) we should never want to hash a state 
    /// object based on its reference. We could check and see if IsReadOnly
    /// is true here, but better to err on the side of caution.
    /// 
    /// Use StateReadOnly instead.
    /// </summary>
    public override int GetHashCode()
    {
        throw new ApplicationException("GetHashCode() on State");
    }
}