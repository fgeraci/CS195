using TreeSharpPlus;
using UnityEngine;
using RootMotion.FinalIK;

/// <summary>
/// A Smart Ticket Dispenser, allowing a user to get a ticket.
/// </summary>
public class SmartTicketDispenser : SmartObject 
{
    public override string Archetype
    {
        get { return this.GetType().Name; }
    }

    /// <summary>
    /// The first edge of the area to go to afterwards.
    /// </summary>
    public Transform GoToEdge1;

    /// <summary>
    /// The second edge of the area to go to afterwards.
    /// </summary>
    public Transform GoToEdge2;

    /// <summary>
    /// The point for the user to stand at when getting a ticket.
    /// </summary>
    public Transform StandPoint;

    /// <summary>
    /// The interaction object to take the dispensed ticket.
    /// </summary>
    public InteractionObject InteractionTake;

    /// <summary>
    /// The prop holder for the ticket which is dispensed.
    /// </summary>
    public PropHolder TicketHolder;

    /// <summary>
    /// The prefab for the dispensed tickets. Must have some sort of Prop.
    /// </summary>
    public GameObject TicketPrefab;

    private GroundRectangle rect;

    void Start()
    {
        rect = new GroundRectangle(GoToEdge1.position, GoToEdge2.position);
    }

    /// <summary>
    /// Lets the user get a ticket.
    /// </summary>
    [Affordance]
    protected Node GetTicket(SmartCharacter user)
    {
        return new Sequence(
            new LeafInvoke(() => GenerateTicket()),
            user.Node_GoTo(StandPoint.position),
            user.Node_OrientTowards(TicketHolder.transform.position),
            user.ST_Pickup(TicketHolder, InteractionTake),
            new DecoratorCatch(
                () => GameObject.Destroy(user.HoldPropRightHand.CurrentProp.gameObject),
                user.Node_GoTo(rect.RandomPoint(GoToEdge1.position.y))),
            new LeafInvoke(() => GameObject.Destroy(user.HoldPropRightHand.CurrentProp.gameObject)));
    }

    /// <summary>
    /// Generates a ticket if none exists.
    /// </summary>
    private void GenerateTicket()
    {
        if (TicketHolder.CurrentProp == null)
        {
            GameObject newProp = (GameObject)GameObject.Instantiate(TicketPrefab,
                TicketHolder.transform.position, Quaternion.identity);
            TicketHolder.Attach(newProp.GetComponent<Prop>());
        }
    }
}
