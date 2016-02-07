// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System.Linq;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WorldData
{
    public StateData InitialState;
    public EventData[] Events;

    public WorldData()
    {
        this.InitialState = null;
        this.Events = null;
    }

    public WorldData(StateData initialState, EventData[] events)
    {
        this.InitialState = initialState;
        this.Events = events;
    }
}
