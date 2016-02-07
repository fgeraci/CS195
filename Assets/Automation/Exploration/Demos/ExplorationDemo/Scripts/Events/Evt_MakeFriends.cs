//using UnityEngine;
//using System.Collections;

//using TreeSharpPlus;

//[LibraryIndex(2)]
//public class Evt_MakeFriends : SmartEvent
//{
//    public SmartCharacter char1;
//    public SmartCharacter char2;

//    [Name("MakeFriends")]
//    [Sentiment(Sentiment.Friendship)]

//    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]
//    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]
//    [RelationRequired(0, 1, ~RelationName.IsFriendOf)]
//    [RelationRequired(1, 0, ~RelationName.IsFriendOf)]

//    [RelationEffect(0, 1, RelationName.IsFriendOf)]
//    [RelationEffect(1, 0, RelationName.IsFriendOf)]

//    public Evt_MakeFriends(SmartObject char1, SmartObject char2)
//        : base(char1, char2)
//    {
//        this.char1 = (SmartCharacter)char1;
//        this.char2 = (SmartCharacter)char2;
//    }

//    public override TreeSharpPlus.Node BakeTree(Token token)
//    {
//        return new Sequence(
//            new LeafAffordance("TalkHappily", this.char1, this.char2),
//            this.char1.Node_Set(char2.Id, RelationName.IsFriendOf),
//            this.char2.Node_Set(char1.Id, RelationName.IsFriendOf));
//    }
//}
