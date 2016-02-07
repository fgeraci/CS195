using UnityEngine;
using TreeSharpPlus;

/// <summary>
/// Some sort of interesting object that can be looked at.
/// </summary>
public class SmartInterestingObject : SmartObject
{

    public override string Archetype
    {
        get { return "Interesting Object"; }
    }

    /// <summary>
    /// The point on the object at which to focus when looking at it.
    /// </summary>
    public Transform lookAtPoint;

    /// <summary>
    /// The point on which to stand when looking at the object.
    /// </summary>
    public Transform lookAtStandPoint;

    /// <summary>
    /// Lets the user, an actor, move to the optimal stand point and look
    /// at the interesting object from there.
    /// </summary>
    [Affordance]
    protected Node MoveAndLookAt(SmartObject user)
    {
        SmartCharacter character = (SmartCharacter)user;

        return new Sequence(
            this.Node_Require(StateName.RoleAttraction),
            character.Node_Require(StateName.RoleActor, StateName.IsStanding),
            character.Node_GoTo(lookAtStandPoint.position),
            LookAt(character)
            );
    }

    /// <summary>
    /// Lets the user, an actor, look at the object from wherever he currently is.
    /// </summary>
    [Affordance]
    protected Node LookAt(SmartObject user)
    {
        SmartCharacter character = (SmartCharacter)user;

        return new Sequence(
            this.Node_Require(StateName.RoleAttraction),
            character.Node_Require(StateName.RoleActor),
            character.Node_HeadLook(this.lookAtPoint.position)
            );
    }

    /// <summary>
    /// Lets the user, an actor, stop looking at the object.
    /// </summary>
    [Affordance]
    protected Node StopLookAt(SmartObject user)
    {
        SmartCharacter character = (SmartCharacter)user;

        return new Sequence(
            this.Node_Require(StateName.RoleAttraction),
            character.Node_Require(StateName.RoleActor),
            character.Node_HeadLookStop()
            );
    }
}
