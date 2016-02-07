using UnityEngine;
using System.Collections.Generic;
using TreeSharpPlus;

[LibraryIndex(-2)]
public class TestCrowdEvent : CrowdEvent<SmartObject>
{

    public override Node BakeParticipantTree(SmartObject participant, object token)
    {
        return new Sequence(
            new LeafTrace(participant.name + " has joined the event!"),
            new LeafWait(10000),
            new LeafTrace(participant.name + " has finished his event!"));
    }

    [StateRequired(0, StateName.RoleCrowd)]
    public TestCrowdEvent(SmartCrowd crowd)
        : base(crowd.GetObjects()) { }
}