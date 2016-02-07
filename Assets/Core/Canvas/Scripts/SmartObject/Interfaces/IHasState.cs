// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections;
using System.Collections.Generic;

public interface IHasState
{
    uint Id { get; }

    IState State { get; }

    bool Require(params StateName[] states);
    bool Require(uint id, params RelationName[] relations);
}

public interface IHasEditableState : IHasState
{
    // This overwrites the State in IHasState
    // Might be a horrible mistake. Have fun!
    new IEditableState State { get; }

    void Set(params StateName[] states);
    void Set(uint id, params RelationName[] relations);
}
