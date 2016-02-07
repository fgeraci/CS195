//using UnityEngine;
//using System.Collections;

//using TreeSharpPlus;

//[LibraryIndex(2)]
//public class Evt_Threaten : SmartEvent
//{
//    public SmartCharacter char1;
//    public SmartCharacter char2;

//    [Name("Threaten")]
//    [Sentiment(Sentiment.Betrayal)]

//    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]
//    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]

//    [RelationRequired(0, 1, RelationName.IsSuspiciousOf)]

//    [RelationEffect(0, 1, ~RelationName.IsFriendOf)]
//    [RelationEffect(1, 0, ~RelationName.IsFriendOf)]

//    public Evt_Threaten(SmartObject char1, SmartObject char2)
//        : base(char1, char2)
//    {
//        this.char1 = (SmartCharacter)char1;
//        this.char2 = (SmartCharacter)char2;
//    }

//    public override TreeSharpPlus.Node BakeTree(Token token)
//    {
//        return new Sequence(
//            new LeafAffordance("TalkAngrily", this.char1, this.char2),
//            this.char1.Node_Set(char2.Id, ~RelationName.IsFriendOf),
//            this.char2.Node_Set(char1.Id, ~RelationName.IsFriendOf));
//    }
//}
