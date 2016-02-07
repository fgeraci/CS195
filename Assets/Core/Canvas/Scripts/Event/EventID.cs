using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// An ID to uniquely identify any scheduled event.
/// </summary
public sealed class EventID
{
    private static uint currentID = 0;

    /// <summary>
    /// The actual ID used by the EventID.
    /// </summary>
    public readonly uint ID;

    /// <summary>
    /// Gets a new EventID with an unused id.
    /// </summary>
    public EventID()
    {
        this.ID = currentID++;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != typeof(EventID))
        {
            return false;
        }
        return this.ID == ((EventID)obj).ID;
    }

    public override string ToString()
    {
        return "ID: " + ID.ToString();
    }
}
