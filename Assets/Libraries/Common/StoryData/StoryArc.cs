using UnityEngine;
using System;
using TreeSharpPlus;
using System.Collections.Generic;

public class StoryArc
{
    public readonly StoryBeat[] Beats;
    public readonly uint[] Participants;

    public StoryArc(params StoryBeat[] beats)
    {
        this.Beats = beats;
        this.Participants = this.ReadBeats(beats);
    }

    private uint[] ReadBeats(StoryBeat[] beats)
    {
        HashSet<uint> seenIds = new HashSet<uint>();
        foreach (StoryBeat beat in beats)
            foreach (StoryEvent evt in beat.Events)
                foreach (uint id in evt.Participants)
                    seenIds.Add(id);
        uint[] result = new uint[seenIds.Count];
        seenIds.CopyTo(result);
        return result;
    }

    public StoryArc(StoryArc other)
    {
        this.Beats = new StoryBeat[other.Beats.Length];
        for (int i = 0; i < other.Beats.Length; i++)
            this.Beats[i] = new StoryBeat(other.Beats[i]);

        this.Participants = new uint[other.Participants.Length];
        Array.Copy(
            other.Participants, 
            this.Participants, 
            other.Participants.Length);
    }
}
