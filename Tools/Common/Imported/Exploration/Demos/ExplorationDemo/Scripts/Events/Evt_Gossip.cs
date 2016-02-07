//using UnityEngine;
//using System.Collections;

//using TreeSharpPlus;

//[LibraryIndex(2)]
//public class Evt_Gossip : SmartEvent
//{
//    public SmartCharacter gossiper;
//    public SmartCharacter listener;
//    public SmartCharacter subject;

//    [Name("Gossip")]
//    [Sentiment(Sentiment.Betrayal)]

//    // The subject isn't actually participating
//    [NonParticipant(2)]

//    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]
//    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]
//    [StateRequired(2, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]

//    // Can only gossip to a trusted friend
//    //[RelationRequired(0, 1, RelationName.IsFriendOf, ~RelationName.IsSuspiciousOf)]
//    [RelationRequired(0, 2, RelationName.IsSuspiciousOf)]

//    [RelationEffect(1, 2, RelationName.IsSuspiciousOf)]

//    public Evt_Gossip(SmartObject gossiper, SmartObject listener, SmartObject subject)
//        : base(gossiper, listener) // Don't involve the subject
//    {
//        this.gossiper = (SmartCharacter)gossiper;
//        this.listener = (SmartCharacter)listener;
//        this.subject = (SmartCharacter)subject;
//    }

//    public override TreeSharpPlus.Node BakeTree(Token token)
//    {
//        return
//            new Sequence(
//                new LeafAffordance("Talk", this.gossiper, this.listener),
//                this.listener.Node_Set(subject.Id, RelationName.IsSuspiciousOf));
//    }
//}
