// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections.Generic;

public class TransitionEvent
{
    public readonly EventDescriptor Descriptor;
    public readonly uint[] Participants;

    public TransitionEvent(
        EventDescriptor descriptor,
        params uint[] participants)
    {
        this.Descriptor = descriptor;
        this.Participants = participants;
    }

    public TransitionEvent(TransitionEvent other)
    {
        this.Descriptor = new EventDescriptor(other.Descriptor);
        this.Participants = (uint[])other.Participants.Clone();
    }
}
