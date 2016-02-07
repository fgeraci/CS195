//using UnityEngine;
//using System.Collections;

//using TreeSharpPlus;

//[LibraryIndex(2)]
//public class Evt_Mug : SmartEvent
//{
//    public SmartCharacter thief;
//    public SmartCharacter victim;

//    [Name("Mug")]
//    [Sentiment(Sentiment.Betrayal)]

//    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingBall, ~StateName.HoldingWallet, ~StateName.IsIncapacitated)]
//    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, StateName.HoldingWallet)]

//    [RelationRequired(0, 1, ~RelationName.IsFriendOf)] // Can't steal from friends

//    [StateEffect(0, StateName.HoldingWallet)]
//    [StateEffect(1, StateName.IsIncapacitated, ~StateName.HoldingWallet)]

//    public Evt_Mug(SmartObject thief, SmartObject victim)
//        : base(thief, victim)
//    {
//        this.thief = (SmartCharacter)thief;
//        this.victim = (SmartCharacter)victim;
//    }

//    public override TreeSharpPlus.Node BakeTree(Token token)
//    {
//        return
//            new Sequence(
//                new Selector(
//                    this.victim.Node_Require(StateName.IsIncapacitated),
//                    new LeafAffordance("Incapacitate", this.thief, this.victim)),
//                new LeafAffordance("TakeWallet", this.thief, this.victim));
//    }
//}
