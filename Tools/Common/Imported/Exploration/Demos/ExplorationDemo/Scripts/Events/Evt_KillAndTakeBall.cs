using UnityEngine;
using System.Collections;

using TreeSharpPlus;

[LibraryIndex(2)]
public class Evt_KillAndTakeBall : SmartEvent
{
    public SmartCharacter thief;
    public SmartCharacter victim;

    [Name("KillAndTakeBall")]
    [Sentiment("Betrayal")]

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, StateName.HoldingBall, ~StateName.IsIncapacitated)]

    // Can't steal from friends
    [RelationRequired(1, 0, ~RelationName.IsFriendOf)]

    [StateEffect(0, StateName.HoldingBall)]
    [StateEffect(1, ~StateName.HoldingBall, StateName.IsIncapacitated, StateName.IsDead)]

    public Evt_KillAndTakeBall(SmartObject thief, SmartObject victim)
        : base(thief, victim)
    {
        this.thief = (SmartCharacter)thief;
        this.victim = (SmartCharacter)victim;
    }

    public override TreeSharpPlus.Node BakeTree(Token token)
    {
        return
            new Sequence(
                new LeafAffordance("TalkAngrily", this.thief, this.victim),
                new LeafAffordance("Kill", this.thief, this.victim),
                new LeafAffordance("TakeBall", this.thief, this.victim));
    }
}
