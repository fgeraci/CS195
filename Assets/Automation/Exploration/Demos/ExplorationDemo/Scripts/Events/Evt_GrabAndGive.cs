using UnityEngine;
using System.Collections;

using TreeSharpPlus;

[LibraryIndex(2)]
public class Evt_GrabAndGive : SmartEvent
{
    public SmartCharacter giver;
    public SmartCharacter recipient;
    public SmartTable table;

    [Name("GrabAndGive")]
    [Sentiment("Friendship")]

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingBall, ~StateName.IsIncapacitated)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingBall, ~StateName.IsIncapacitated)]
    [StateRequired(2, StateName.RoleTable, StateName.HoldingBall, ~StateName.IsIncapacitated)]

    [StateEffect(1, StateName.HoldingBall)]
    [StateEffect(2, ~StateName.HoldingBall)]

    public Evt_GrabAndGive(SmartCharacter giver, SmartCharacter recipient, SmartTable table)
        : base(giver, recipient, table)
    {
        this.giver = giver;
        this.recipient = recipient;
        this.table = table;
    }

    public override TreeSharpPlus.Node BakeTree(Token token)
    {
        return new Sequence(
            new LeafAffordance("TakeBall", this.giver, this.table),
            new LeafAffordance("GiveBall", this.giver, this.recipient));
    }
}
