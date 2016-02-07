using UnityEngine;
using System;
using System.Collections.Generic;

public class StoryEvent 
{
    public readonly EventSignature Signature;
    public readonly uint[] Participants;
    public readonly EventID ID;

    public StoryEvent(
        EventSignature signature,
        params uint[] parameters)
        :this(signature, new EventID(), parameters)
    {
    }

    public StoryEvent(
        EventSignature signature,
        EventID id,
        params uint[] parameters)
    {
        this.Signature = signature;
        this.Participants = parameters;
        this.ID = id;
    }


    public StoryEvent(
        EventSignature signature,
        params SmartObject[] parameters)
        :this(signature, new EventID(), parameters)
    {
    }

    public StoryEvent(
        EventSignature signature,
        EventID id,
        params SmartObject[] parameters)
    {
        this.Signature = signature;
        this.Participants = new uint[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
            this.Participants[i] = parameters[i].Id;
        this.ID = id;
    }


    public StoryEvent(StoryEvent other)
    {
        this.Signature = new EventSignature(other.Signature);
        this.Participants = new uint[other.Participants.Length];
        Array.Copy(
            other.Participants, 
            this.Participants, 
            other.Participants.Length);
        this.ID = other.ID;
    }
}
