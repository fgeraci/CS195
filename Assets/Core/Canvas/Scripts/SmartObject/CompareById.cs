// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections;
using System.Collections.Generic;

public class CompareById :
    IComparer<IHasState>,
    IEqualityComparer<IHasState>
{
    private static CompareById instance;
    public static CompareById Instance
    {
        get
        {
            if (instance == null)
                instance = new CompareById();
            return instance;
        }
    }

    public int Compare(IHasState x, IHasState y)
    {
        return x.Id.CompareTo(y.Id);
    }

    public bool Equals(IHasState x, IHasState y)
    {
        return x.Id.Equals(y.Id);
    }

    public int GetHashCode(IHasState obj)
    {
        return (int)obj.Id;
    }
}