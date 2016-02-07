using UnityEngine;

/// <summary>
/// A simple smart object that is only used for storing state.
/// </summary>
public class EmptySmartObject : SmartObject
{
    public override string Archetype
    {
        get { return this.GetType().Name; }
    }
}
