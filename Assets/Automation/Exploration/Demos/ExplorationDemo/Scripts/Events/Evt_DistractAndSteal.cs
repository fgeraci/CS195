//using UnityEngine;
//using System.Collections;

//using TreeSharpPlus;

//[LibraryIndex(2)]
//public class Evt_DistractAndSteal : SmartEvent
//{
//    public SmartCharacter con;
//    public SmartCharacter thief;
//    public SmartCharacter victim;

//    [Name("DistractAndSteal")]
//    [Sentiment(Sentiment.Betrayal)]

//    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]
//    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingWallet, ~StateName.IsIncapacitated)]
//    [StateRequired(2, StateName.RoleActor, StateName.IsStanding, StateName.HoldingWallet, ~StateName.IsIncapacitated)]

//    // Thief and con need to be friends
//    [RelationRequired(0, 1, RelationName.IsFriendOf)]
//    [RelationRequired(1, 0, RelationName.IsFriendOf)]

//    // Can't steal from friends
//    [RelationRequired(0, 2, ~RelationName.IsFriendOf)]
//    [RelationRequired(1, 2, ~RelationName.IsFriendOf)]

//    [StateEffect(1, StateName.HoldingWallet)]
//    [StateEffect(2, ~StateName.HoldingWallet)]

//    public Evt_DistractAndSteal(SmartObject con, SmartObject thief, SmartObject victim)
//        : base(con, thief, victim)
//    {
//        this.con = (SmartCharacter)con;
//        this.thief = (SmartCharacter)thief;
//        this.victim = (SmartCharacter)victim;
//    }

//    public override TreeSharpPlus.Node BakeTree(Token token)
//    {
//        return
//            new Sequence(
//                new SequenceParallel(
//                    this.con.Node_GoToUpToRadius(new Val<Vector3>(() => this.victim.transform.position), 5.0f),
//                    this.thief.Node_GoTo(new Val<Vector3>(() => this.victim.transform.position + (this.victim.transform.right * 5.0f)))),
//                new SequenceParallel(
//                    new Sequence(
//                        new LeafAffordance("TalkSecretive", this.con, this.victim),
//                        this.con.Node_GoToUpToRadius(new Val<Vector3>(() => this.victim.transform.position), 5.0f)),
//                    new Sequence(
//                        new LeafAffordance("Steal", this.thief, this.victim),
//                        this.thief.Node_GoToUpToRadius(new Val<Vector3>(() => this.victim.transform.position), 5.0f))));
//    }
//}
