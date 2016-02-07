using UnityEngine;
using TreeSharpPlus;
using RootMotion.FinalIK;

/// <summary>
/// A Smart Drink Dispenser, where a user can get a drink.
/// </summary>
public class SmartDrinkDispenser : SmartObject
{

    public override string Archetype
    {
        get { return "SmartDrinkDispenser"; }
    }

    /// <summary>
    /// Interaction object to pick up the dispensed drink.
    /// </summary>
    public InteractionObject DrinkPickup;

    /// <summary>
    /// Interaction object for the button to be pressed for a drink.
    /// </summary>
    public InteractionObject DrinkButton;

    /// <summary>
    /// The prefab for the drink that is instantiated when dispensed.
    /// </summary>
    public GameObject DrinkPrefab;

    /// <summary>
    /// The holder for the drink to be attached to when dispensed.
    /// </summary>
    public PropHolder DrinkHolder;

    /// <summary>
    /// The point for the user to stand when getting a drink.
    /// </summary>
    public Transform StandPoint;

    /// <summary>
    /// The point for the user to go to when done.
    /// </summary>
    public Transform LeavePoint;

    /// <summary>
    /// Lets the user get a drink from the dispenser.
    /// </summary>
    [Affordance]
    protected Node GetDrink(SmartCharacter user)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleDispenser),
            user.Node_GoTo(this.StandPoint.position),
            user.Node_OrientTowards(this.DrinkButton.transform.position),
            new DecoratorCatch(
                () => //if terminates, make sure the right hand interaction and headlook is stopped
                {
                    user.Character.StopInteraction(FullBodyBipedEffector.RightHand);
                    user.Character.HeadLookStop();
                },
                new Sequence(
                    user.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.DrinkButton),
                    user.Node_WaitForTrigger(FullBodyBipedEffector.RightHand),
                    new SequenceParallel(
                        //user.Node_HeadLook(this.DrinkPickup.transform.position),
                        user.Node_OrientTowards(this.DrinkPickup.transform.position)),
                    user.Node_StopInteraction(FullBodyBipedEffector.RightHand),
                    new LeafWait(1000),
                    new LeafInvoke(() => GenerateDrink()),
                    new LeafWait(500))),
            user.ST_Pickup(DrinkHolder, DrinkPickup),
            user.Node_Set(StateName.HoldingDrink, StateName.RightHandOccupied),
            user.Node_GoTo(this.LeavePoint.position));

    }

    /// <summary>
    /// Generate a drink if none exists currently.
    /// </summary>
    private void GenerateDrink()
    {
        if (this.DrinkHolder.CurrentProp != null)
        {
            return;
        }
        GameObject newGO = (GameObject) GameObject.Instantiate(DrinkPrefab);
        Prop drink = newGO.GetComponent<Prop>();
        this.DrinkHolder.Attach(drink);
        drink.FadeIn();
    }

}
