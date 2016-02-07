using UnityEngine;
using System;
using System.Collections;
using System.Linq;

public class StoryBeat
{
    public readonly StoryEvent[] Events;

    public StoryBeat(params StoryEvent[] events)
    {
        this.Events = events;
    }

    public StoryBeat(StoryBeat other)
    {
        this.Events = new StoryEvent[other.Events.Length];
        for (int i = 0; i < other.Events.Length; i++ )
            this.Events[i] = new StoryEvent(other.Events[i]);
    }
}
