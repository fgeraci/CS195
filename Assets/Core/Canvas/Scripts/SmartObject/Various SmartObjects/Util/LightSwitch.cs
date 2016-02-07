using UnityEngine;
using RootMotion.FinalIK;

/// <summary>
/// A simple light switch for a lamp. The switch can contain a physical switch,
/// which is rotated when it is turned on/off.
/// </summary>
public class LightSwitch : MonoBehaviour 
{
    /// <summary>
    /// The interaction object associated with this light switch, indicating
    /// both the pose and the reach target for the user's hand.
    /// </summary>
    public InteractionObject interactionObject;

    /// <summary>
    /// The light switch can also have a physical switch as a separate transform, 
    /// which is rotated when turned on/off. This field is optional.
    /// </summary>
    public Transform physicalSwitch;

    /// <summary>
    /// The angle that the physical switch is rotated by.
    /// </summary>
    private const float turnAngle = 10.0f;

    private bool state;

    /// <summary>
    /// Initializes the light switch to the given state.
    /// </summary>
    public void Initialize(bool switchedOn)
    {
        if (physicalSwitch != null)
        {
            physicalSwitch.Rotate(Vector3.forward, switchedOn ? turnAngle : -turnAngle);
        }
        this.state = switchedOn;
    }

    /// <summary>
    /// Sets the light switch to the given state. Also rotates the physical switch if it
    /// exists and the state changes.
    /// </summary>
    public void Set(bool switchedOn)
    {
        if (this.state != switchedOn && physicalSwitch != null)
        {
            physicalSwitch.Rotate(Vector3.forward, (switchedOn ? 2 : -2) * turnAngle);
        }
        this.state = switchedOn;
    }

}
