using UnityEngine;
using TreeSharpPlus;
using RootMotion.FinalIK;

/// <summary>
/// A Smart Lamp that can be turned on and off.
/// </summary>
public class SmartLamp : SmartObject
{

    public override string Archetype
    {
        get { return "Lamp";  }
    }

    /// <summary>
    /// The light associated with this lamp.
    /// </summary>
    public Light light;

    /// <summary>
    /// The point to be used when turning on the lamp.
    /// </summary>
    public Transform standPoint;

    /// <summary>
    /// This lamp's light switch.
    /// </summary>
    public LightSwitch lightSwitch;

    void Start()
    {
        this.lightSwitch.Initialize(this.Require(StateName.IsTurnedOn));
    }

    void Update()
    {
        this.light.enabled = this.Require(StateName.IsTurnedOn);
    }

    /// <summary>
    /// Lets the user, an actor, turn on the lamp.
    /// </summary>
    [Affordance]
    public Node TurnOn(SmartObject user)
    {
        SmartCharacter character = (SmartCharacter)user;
        return ST_Switch(character, StateName.IsTurnedOn);
    }

    /// <summary>
    /// Lets the user, an actor, turn off the lamp.
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
            this.Node_Require(StateName.RoleLamp, ~switchTo),
            user.Node_GoTo(standPoint.position),
            user.Node_OrientTowards(lightSwitch.transform.position),
            user.Node_StartInteraction(FullBodyBipedEffector.RightHand, lightSwitch.interactionObject),
            user.Node_WaitForFinish(FullBodyBipedEffector.RightHand),
            user.Node_StopInteraction(FullBodyBipedEffector.RightHand),
            new LeafInvoke(() => lightSwitch.Set(switchTo > 0)),
            this.Node_Set(switchTo)
            );
    }
}
