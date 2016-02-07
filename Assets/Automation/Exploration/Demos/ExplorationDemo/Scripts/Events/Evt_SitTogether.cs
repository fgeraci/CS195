//using UnityEngine;
//using System.Collections;

//using TreeSharpPlus;

//[LibraryIndex(999)]
//public class Evt_SitTogether : SmartEvent
//{
//    public SmartCharacter actor1;
//    public SmartCharacter actor2;
//    public SmartChair chair1;
//    public SmartChair chair2;

//    [Name("SitTogether")]

//    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]
//    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]
//    [StateRequired(2, StateName.RoleChair, ~StateName.IsOccupied)]
//    [StateRequired(3, StateName.RoleChair, ~StateName.IsOccupied)]
//    [RelationRequired(2, 3, RelationName.IsAdjacentTo)]
//    [RelationRequired(3, 2, RelationName.IsAdjacentTo)]

//    [StateEffect(0, ~StateName.IsStanding)]
//    [StateEffect(1, ~StateName.IsStanding)]
//    [StateEffect(2, StateName.IsOccupied)]
//    [StateEffect(3, StateName.IsOccupied)]
//    [RelationEffect(0, 2, RelationName.IsSittingOn)]
//    [RelationEffect(1, 3, RelationName.IsSittingOn)]

//    public Evt_SitTogether(SmartObject actor1, SmartObject actor2, SmartObject chair1, SmartObject chair2)
//        : base(actor1, actor2, chair1, chair2)
//    {
//        this.actor1 = (SmartCharacter)actor1;
//        this.actor2 = (SmartCharacter)actor2;
//        this.chair1 = (SmartChair)chair1;
//        this.chair2 = (SmartChair)chair2;
//    }

//    public override TreeSharpPlus.Node BakeTree(object token)
//    {
//        return new SequenceParallel(
//            new LeafAffordance("Sit", this.actor1, this.chair1),
//            new LeafAffordance("Sit", this.actor2, this.chair2));
//    }
//}
