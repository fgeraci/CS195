using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class CompareByMatrix : IComparer<WorldState>, IEqualityComparer<WorldState>
{
    public int Compare(WorldState x, WorldState y)
    {
        long[] xMatrix = x.GetStateMatrix();
        long[] yMatrix = y.GetStateMatrix();

        DebugUtil.Assert(xMatrix.Length == xMatrix.Length);
        int sum = 0;
        for (int i = 0; i < xMatrix.Length; i++)
            sum += HammingWeight(xMatrix[i] ^ yMatrix[i]);
        return sum;
    }

    private static int HammingWeight(long val)
    {
        return HW3(val);
    }

    // Three different options for calculating hamming weight:
    // See http://en.wikipedia.org/wiki/Hamming_weight

    private static int HW1(long x)
    {
		int count = 0;
        while (x > 0)
		{
            x &= (x - 1);
			count++;
		}
		return count;
    }

    private const ulong m1 = 0x5555555555555555;
    private const ulong m2 = 0x3333333333333333;
    private const ulong m4 = 0x0f0f0f0f0f0f0f0f;
    private const ulong m8 = 0x00ff00ff00ff00ff;
    private const ulong m16 = 0x0000ffff0000ffff;
    private const ulong m32 = 0x00000000ffffffff;
    private const ulong hff = 0xffffffffffffffff;
    private const ulong h01 = 0x0101010101010101;

    // Use me if your machine has slow multiplication
    // See: http://www.agner.org/optimize/
    private static int HW2(long val)
    {
        ulong x = (ulong)val;
        x -= (x >> 1) & m1;
        x = (x & m2) + ((x >> 2) & m2);
        x = (x + (x >> 4)) & m4; 
        x += x >> 8;
        x += x >> 16;
        x += x >> 32;
        return (int)(x & 0x7f);
    }

    // Use me if your machine has fast multiplication
    // See: http://www.agner.org/optimize/
    private static int HW3(long val)
    {
        ulong x = (ulong)val;
        x -= (x >> 1) & m1;
        x = (x & m2) + ((x >> 2) & m2);
        x = (x + (x >> 4)) & m4;
        return (int)((x * h01) >> 56);
    }

    public bool Equals(WorldState x, WorldState y)
    {
        return x.Equals(y);
    }

    public int GetHashCode(WorldState obj)
    {
        return obj.GetHashCode();
    }
}
