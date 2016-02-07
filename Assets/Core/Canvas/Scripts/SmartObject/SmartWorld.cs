using UnityEngine;


/// <summary>
/// A simple smart object that is only used to be globally available and keep track of certain states.
/// </summary>
public class SmartWorld : SmartObject 
{
    public override string Archetype
    {
        get { return this.GetType().Name; }
    }
}
