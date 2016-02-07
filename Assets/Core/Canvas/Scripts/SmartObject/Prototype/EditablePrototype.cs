// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

public class EditablePrototype : 
    IHasState, IHasEditableState, IEquatable<EditablePrototype>
{
    private EditableState state;

    public EditableState State
    {
        get { return this.state; }
    }

    IState IHasState.State
    {
        get { return this.state; }
    }

    IEditableState IHasEditableState.State
    {
        get { return this.state; }
    }

    public uint Id { get { return this.state.Id; } }

    public EditablePrototype(EditableState state)
    {
        // Make a clone so two protypes don't share the same editable state
        this.state = new EditableState(state);
    }

    public EditablePrototype(EditablePrototype other)
    {
        // Make a clone so two protypes don't share the same editable state
        this.state = new EditableState(other.state);
    }

    public EditablePrototype(ReadOnlyPrototype other)
    {
        // Make a clone so two protypes don't share the same editable state
        this.state = new EditableState(other.State);
    }

    public ReadOnlyPrototype AsReadOnly()
    {
        return new ReadOnlyPrototype(this.state.AsReadOnly());
    }

    /// <summary>
    /// Compares two prototypes, accounting for Id.
    /// </summary>
    public bool Equals(EditablePrototype other)
    {
        if (this.state.Id != other.Id)
            return false;
        return this.state.Equals(other.State);
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
    public void Set(params StateName[] states)
    {
        this.state.Set(states);
    }

    /// <summary>
    /// Sets the given values relative to the object id
    /// </summary>
    public void Set(uint id, params RelationName[] relations)
    {
        this.state.Set(id, relations);
    }

    public EditablePrototype SetCopy(ChangeSet changes)
    {
        return new EditablePrototype(this.state.SetCopy(changes));
    }

    /// <summary>
    /// Blocking this off because (a) you should never hash the contents
    /// of a mutable object, and (b) we should never want to hash a state 
    /// object based on its reference. We could check and see if IsReadOnly
    /// is true here, but better to err on the side of caution.
    /// 
    /// Use PrototypeReadOnly instead.
    /// </summary>
    public override int GetHashCode()
    {
        throw new ApplicationException("GetHashCode() on Prototype");
    }
}
