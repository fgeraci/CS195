using UnityEngine;
using System.Collections;

public interface IArea 
{

    /// <summary>
    /// Returns whether the given area contains the given position.
    /// </summary>
    bool Contains(Vector3 position);

}
