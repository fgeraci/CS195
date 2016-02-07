using TreeSharpPlus;
using UnityEngine;
using RootMotion.FinalIK;

/// <summary>
/// A Smart Button that can be pressed by the user.
/// </summary>
public class SmartButton : SmartObject
{
    public override string Archetype
    {
        get { return this.GetType().Name; }
    }

    [System.Serializable]
    public class ButtonPress
    {
        public Transform PhysicalButton;
        public Vector3 PressDirection;
    }

    public Transform ButtonTransform;

    /// <summary>
    /// Point to stand at while pressing the button.
    /// </summary>
    public Transform StandPoint;

    /// <summary>
    /// The interaction object to use when pressing the button.
    /// </summary>
    public InteractionObject InteractionPress;

    /// <summary>
    /// The actual physical pressing of the button where it moves inwards.
    /// </summary>
    public ButtonPress PhysicalPress;


    void Start()
    {
        ButtonTransform.GetComponent<Renderer>().material = 
            new Material(ButtonTransform.GetComponent<Renderer>().material);
        this.ButtonTransform.GetComponent<Renderer>().material.color = Color.red;
    }

    /// <summary>
    /// Lets the user press the button.
    /// </summary>
    [Affordance]
    protected Node Press(SmartCharacter user)
    {
        return new Sequence(
            user.ST_StandAtWaypoint(StandPoint),
            user.Node_StartInteraction(FullBodyBipedEffector.LeftHand, InteractionPress),
            user.Node_WaitForTrigger(FullBodyBipedEffector.LeftHand),
            new DecoratorLoop(
                10,
                new LeafInvoke(() => PhysicalPress.PhysicalButton.position 
                                += PhysicalPress.PressDirection.normalized * 0.003f)),
            new LeafInvoke(() => this.ButtonTransform.GetComponent<Renderer>().material.color = Color.green),
            new DecoratorLoop(
                10,
                new LeafInvoke(() => PhysicalPress.PhysicalButton.position 
                                -= PhysicalPress.PressDirection.normalized * 0.003f)),
            user.Node_StopInteraction(FullBodyBipedEffector.LeftHand),
            user.Node_HeadLookStop());
    }

}
