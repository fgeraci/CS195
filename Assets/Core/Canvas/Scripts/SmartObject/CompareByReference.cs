using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class CompareByReference :
    IEqualityComparer<IHasState>
{
    private static CompareByReference instance;
    public static CompareByReference Instance
    {
        get
        {
            if (instance == null)
                instance = new CompareByReference();
            return instance;
        }
    }

    public bool Equals(IHasState x, IHasState y)
    {
        return object.ReferenceEquals(x, y);
    }

    public int GetHashCode(IHasState obj)
    {
        return RuntimeHelpers.GetHashCode(obj);
    }
}