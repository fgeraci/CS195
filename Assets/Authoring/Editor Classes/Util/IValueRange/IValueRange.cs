
/// <summary>
/// An interface to determine whether some value is within a given range, and if not, on which side of the range it is (too small or too large).
/// </summary>
/// <typeparam name="T">The value type of the range.</typeparam>
public interface IValueRange<T> 
{
    /// <summary>
    /// Checks whether the given value is in the range. Returns a negative number if the value is too small, 0 if the value is within the range
    /// and a positive number if the value is too large.
    /// </summary>
    int IsInRange(T value);
}
