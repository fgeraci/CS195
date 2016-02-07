// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections;
using System.Collections.Generic;

public interface IState : IEquatable<IState>
{
    uint Id { get; }

    bool Require(IList<StateName> states);
    bool Require(uint id, IList<RelationName> relations);

    IState SetCopy(ChangeSet changes);

    // Do not use these without permission
    long GetStateBits();
    long GetRelationBits(uint id);
    IEnumerable<Tuple<uint, long>> GetStoredRelations();
    int GetChecksum();
}

public interface IEditableState : IState, IEquatable<IEditableState>
{
    void Set(IList<StateName> states);
    void Set(uint id, IList<RelationName> relations);

    // Do not use these without permission
    void SetStateBits(long state);
    void SetRelationBits(uint id, long relation);
}