using System;

/// <summary>
/// An interval, where values must be between the minimum and maximum value (both inclusive).
/// </summary>
public class Interval<T> : IValueRange<T> where T : IComparable<T>
{
    public T min { get; private set; }

    public T max { get; private set; }

    public Interval(T min, T max)
    {
        this.min = min;
        this.max = max;
    }

    public int IsInRange(T value)
    {
        if (value.CompareTo(min) < 0) 
        {
            return -1;
        }
        else if (value.CompareTo(max) > 0)
        {
            return 1;
        }
        return 0;
    }

    public override string ToString()
    {
        return "in interval [" + min.ToString() + ", " + max.ToString() + "]";
    }
}
