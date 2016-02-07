using UnityEngine;
using System.Collections.Generic;
using TreeSharpPlus;

/// <summary>
/// A special type of SmartEvent that uses the ForEach node and is particularly
/// suited for handling crowd events.
/// </summary>
/// <typeparam name="T">The type of Smart Object to be used.</typeparam>
public abstract class CrowdEvent<T> : SmartEvent
    where T : SmartObject
{
    public abstract Node BakeParticipantTree(T participant, object token);

    //Function not used in CrowdEvent, so I seal it here.
    public sealed override Node BakeTree(Token token)
    {
        return null;
    }

    public CrowdEvent(IEnumerable<T> participants)
    {
        this.Behavior =
            new CrowdBehaviorEvent<T>(
                this.BakeParticipantTree,
                participants);
    }
}
