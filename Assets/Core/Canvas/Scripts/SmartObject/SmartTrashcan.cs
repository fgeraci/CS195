using UnityEngine;
using TreeSharpPlus;
using RootMotion.FinalIK;

/// <summary>
/// A trash can where objects can be thrown into.
/// </summary>
public class SmartTrashcan : SmartObject
{
    public override string Archetype
    {
        get { return this.GetType().Name; }
    }

    /// <summary>
    /// The stand point to throw trash into the can
    /// </summary>
    public Transform StandPoint;

    /// <summary>
    /// The interaction object to reach to when dropping trash
    /// </summary>
    public InteractionObject DropTrash;

    /// <summary>
    /// The user drops his drink into the trash can.
    /// </summary>
    [Affordance]
    protected Node DropDrink(SmartCharacter user)
    {
        Prop toDestroy = null;

        return new Sequence(
            this.Node_Require(StateName.RoleTrashcan),
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, StateName.HoldingDrink, StateName.RightHandOccupied),
            new LeafInvoke(() => toDestroy = user.HoldPropRightHand.CurrentProp),
            user.ST_StandAtWaypoint(this.StandPoint),
            user.ST_DropAndDestroy(DropTrash),
            user.Node_Set(~StateName.HoldingDrink, ~StateName.RightHandOccupied));            
    }
}
