using UnityEngine;
using TreeSharpPlus;
using RootMotion.FinalIK;

/// <summary>
/// A Smart Television, that can be turned on and off and is a special version
/// of the SmartInterestingObject.
/// </summary>
public class SmartTelevision : SmartInterestingObject
{

    public override string Archetype
    {
        get { return "Television"; }
    }

    /// <summary>
    /// The mesh where this television's image will be displayed.
    /// </summary>
    public MeshRenderer displayMesh;

    /// <summary>
    /// The material to be used when the tv is on.
    /// </summary>
    public Material onMaterial;

    /// <summary>
    /// The material to be used when the tv is off.
    /// </summary>
    public Material offMaterial;

    /// <summary>
    /// The point to be used when turning on the television.
    /// </summary>
    public Transform turnOnOffStandPoint;

    /// <summary>
    /// The television's onOffSwitch.
    /// </summary>
    public InteractionObject onOffSwitch;

    void Update()
    {
        SetMaterial();
    }

    /// <summary>
    /// Lets the user, an actor, turn on the television.
    /// </summary>
    [Affordance]
    public Node TurnOn(SmartObject user)
    {
        SmartCharacter character = (SmartCharacter)user;
        return ST_Switch(character, StateName.IsTurnedOn);
    }

    /// <summary>
    /// Lets the user, an actor, turn off the television.
    /// </summary>
    [Affordance]
    public Node TurnOff(SmartObject user)
    {
        SmartCharacter character = (SmartCharacter)user;
        return ST_Switch(character, ~StateName.IsTurnedOn);
    }

    /// <summary>
    /// Subtree for the user switching the lamp on/off.
    /// </summary>
    /// <param name="switchTo">THe desired state of StateName.IsTurnedOn after executing the tree.</param>
    private Node ST_Switch(SmartCharacter user, StateName switchTo)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding),
            this.Node_Require(StateName.RoleTelevision, ~switchTo),
            user.Node_GoTo(turnOnOffStandPoint.position),
            user.Node_OrientTowards(onOffSwitch.transform.position),
            user.Node_StartInteraction(FullBodyBipedEffector.LeftHand, onOffSwitch),
            user.Node_WaitForTrigger(FullBodyBipedEffector.LeftHand),
            user.Node_StopInteraction(FullBodyBipedEffector.LeftHand),
            this.Node_Set(switchTo)
            );
    }

    /// <summary>
    /// Sets the tv's material depending on whether it is turned on or off.
    /// </summary>
    private void SetMaterial()
    {
        displayMesh.material =
            this.Require(StateName.IsTurnedOn) ?
            this.onMaterial :
            this.offMaterial;
    }
}
