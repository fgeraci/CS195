// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EdgeData
{
    public uint EdgeId;
    public uint IdFrom;
    public uint IdTo;

    public string[] EventNames;
    public uint[][] EventParticipants;

    public EdgeData()
    {
        this.EdgeId = uint.MaxValue;
        this.IdFrom = uint.MaxValue;
        this.IdTo = uint.MaxValue;

        this.EventNames = null;
        this.EventParticipants = null;
    }

    public EdgeData(uint id, ExplorationEdge edge)
    {
        this.EdgeId = id;

        this.IdFrom = edge.Source.Id;
        this.IdTo = edge.Target.Id;

        this.EventNames = new string[edge.Events.Length];
        this.EventParticipants = new uint[edge.Events.Length][];
        for (int i = 0; i < edge.Events.Length; i++)
        {
            TransitionEvent evt = edge.Events[i];
            EventNames[i] = evt.Descriptor.Name;
            EventParticipants[i] = evt.Participants;
        }
    }
}
