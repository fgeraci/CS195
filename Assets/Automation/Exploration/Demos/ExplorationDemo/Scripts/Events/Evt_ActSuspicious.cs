//using UnityEngine;
//using System.Collections;

//using TreeSharpPlus;

//[LibraryIndex(2)]
//public class Evt_ActSuspicious : SmartEvent
//{
//    public SmartCharacter observer;
//    public SmartCharacter subject;
//    public SmartChair chair;

//    [Name("ActSuspicious")]

//    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]
//    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]
//    [StateRequired(2, StateName.RoleChair)]

//    [RelationEffect(0, 1, RelationName.IsSuspiciousOf)]

//    public Evt_ActSuspicious(SmartObject observer, SmartObject subject, SmartObject chair)
//        : base(observer, subject, chair)
//    {
//        this.observer = (SmartCharacter)observer;
//        this.subject = (SmartCharacter)subject;
//        this.chair = (SmartChair)chair;
//    }

//    public override TreeSharpPlus.Node BakeTree(Token token)
//    {
//        return
//            new Sequence(
//                new SelectorParallel(
//                    new DecoratorLoop(
//                        this.observer.Node_OrientTowards(Val.Val(() => this.subject.transform.position))),
//                    new Sequence(
//                        new LeafAffordance("Sit", this.subject, this.chair),
//                        new LeafAffordance("Stand", this.subject, this.chair))),
//                observer.Node_Set(subject.Id, RelationName.IsSuspiciousOf));
//    }
//}
