// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections;
using System.Collections.Generic;

public class StateComparer : IEqualityComparer<ReadOnlyState>
{
    public bool Equals(ReadOnlyState x, ReadOnlyState y)
    {
        return x.Equals(y);
    }

    public int GetHashCode(ReadOnlyState obj)
    {
        return obj.GetHashCode();
    }
}