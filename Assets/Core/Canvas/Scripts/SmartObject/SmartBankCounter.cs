using UnityEngine;
using TreeSharpPlus;
using System;
using RootMotion.FinalIK;
using System.Collections.Generic;

/// <summary>
/// A bank counter, where actors can either go to put money or get money as well
/// as deliver the forms they filled out.
/// </summary>
public class SmartBankCounter : SmartObject
{

    public override string Archetype
    {
        get { return "SmartBar"; }
    }

    /// <summary>
    /// The prefab for the bank's money prop.
    /// </summary>
    public GameObject MoneyPrefab;

    /// <summary>
    /// The point for the teller to stand during the interaction.
    /// </summary>
    public Transform TellerStandPoint;

    /// <summary>
    /// The point for the customer to stand during the interaction.
    /// </summary>
    public Transform CustomerStandPoint;

    /// <summary>
    /// The point for the customer to go to after the interaction.
    /// </summary>
    public Transform LeavePoint;

    /// <summary>
    /// The prop holder for the money storage.
    /// </summary>
    public PropHolder HoldPropStorage;

    /// <summary>
    /// The prop holder for intermediate interaction (placed there by teller,
    /// taken by customer).
    /// </summary>
    public PropHolder HoldPropIntermediate;

    /// <summary>
    /// Interaction for the money storage.
    /// </summary>
    public InteractionObject InteractionStorage;

    /// <summary>
    /// Interaction for intermediate holder for teller.
    /// </summary>
    public InteractionObject InteractionIntermediateTeller;

    /// <summary>
    /// Interaction for intermediate holder for customer.
    /// </summary>
    public InteractionObject InteractionIntermediateCustomer;

    /// <summary>
    /// Tells the user to attend the counter.
    /// </summary>
    [Affordance]
    protected Node Attend(SmartCharacter user)
    {
        return new Sequence(
            user.Node_GoTo(TellerStandPoint.position),
            user.Node_OrientTowards(CustomerStandPoint.position),
            user.Node_Set(StateName.IsImmobile),
            user.Node_Set(this.Id, RelationName.IsAttending),
            this.Node_Set(StateName.IsOccupied));
    }

    /// <summary>
    /// Tells the user to stop attending the counter.
    /// </summary>
    [Affordance]
    protected Node Leave(SmartCharacter user)
    {
        return new Sequence(
            user.Node_Set(~StateName.IsImmobile),
            this.Node_Set(~StateName.IsOccupied));
    }

    /// <summary>
    /// Lets the user stand in front of the counter.
    /// </summary>
    [Affordance]
    protected Node StandInFront(SmartCharacter user)
    {
        return new Sequence(
            user.Node_GoTo(CustomerStandPoint.position),
            user.Node_OrientTowards(TellerStandPoint.position),
            user.Node_NudgeTo(CustomerStandPoint.position));
    }

    /// <summary>
    /// The user takes money from the teller's storage.
    /// </summary>
    [Affordance]
    protected Node TellerTakeMoney(SmartCharacter user)
    {
        return new Sequence(
            this.Node_Require(StateName.RoleTeller),
            user.Node_Require(StateName.RoleTeller, StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated),
            this.Node_Require(user.Id, RelationName.IsAdjacentTo),
            new LeafInvoke(() => GenerateMoney(HoldPropStorage)),
            user.Node_GoTo(TellerStandPoint.position),
            user.Node_OrientTowards(CustomerStandPoint.position),
            user.ST_Pickup(HoldPropStorage, InteractionStorage),
            user.ST_Put(HoldPropIntermediate, InteractionIntermediateTeller),
            this.Node_Set(StateName.HasBackpack));
    }

    /// <summary>
    /// The user takes money from the intermediate storage.
    /// </summary>
    [Affordance]
    protected Node CustomerTakeMoney(SmartCharacter user)
    {
        return new Sequence(
            this.Node_Require(StateName.RoleTeller),
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.RightHandOccupied, 
                ~StateName.HoldingWallet, ~StateName.IsIncapacitated),
            user.ST_Pickup(HoldPropIntermediate, InteractionIntermediateCustomer),
            user.ST_PutWalletInPocket(),
            user.Node_Set(StateName.HoldingWallet));
    }

    /// <summary>
    /// The user puts money into the teller storage.
    /// </summary>
    [Affordance]
    protected Node TellerPutMoney(SmartCharacter user)
    {
        return new Sequence(
            this.Node_Require(StateName.RoleTeller, StateName.HoldingWallet),
            user.Node_Require(StateName.RoleTeller, StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated),
            this.Node_Require(user.Id, RelationName.IsAdjacentTo),
            user.Node_GoTo(TellerStandPoint.position),
            user.Node_OrientTowards(CustomerStandPoint.position),
            user.ST_Pickup(HoldPropIntermediate, InteractionIntermediateTeller),
            user.ST_Put(HoldPropStorage, InteractionStorage),
            this.Node_Set(~StateName.HoldingWallet));
    }

    /// <summary>
    /// The user puts money into the intermediate storage.
    /// </summary>
    [Affordance]
    protected Node CustomerPutMoney(SmartCharacter user)
    {
        return new Sequence(
            this.Node_Require(StateName.RoleTeller),
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, StateName.HoldingWallet),
            user.ST_TakeWalletFromPocket(),
            user.ST_Put(HoldPropIntermediate, InteractionIntermediateCustomer),
            user.Node_Set(~StateName.HoldingWallet),
            this.Node_Set(StateName.HoldingWallet));
    }

    /// <summary>
    /// The user puts his hidden prop into the intermediate storage.
    /// </summary>
    [Affordance]
    protected Node CustomerPutHidden(SmartCharacter user)
    {
        return new Sequence(
            new LeafInvoke(() => user.HoldPropRightHand.Attach(user.HoldPropHidden.Release())),
            new LeafInvoke(() => user.GetRightProp().FadeIn()),
            user.ST_Put(HoldPropIntermediate, InteractionIntermediateCustomer));
    }

    /// <summary>
    /// The user puts the prop into the storage.
    /// </summary>
    [Affordance]
    protected Node TellerPutHidden(SmartCharacter user)
    {
        return new Sequence(
            user.Node_GoTo(TellerStandPoint.position),
            user.Node_OrientTowards(CustomerStandPoint.position),
            user.ST_Pickup(HoldPropIntermediate, InteractionIntermediateTeller),
            new LeafInvoke(() => user.GetRightProp().FadeOut()),
            user.ST_Put(HoldPropStorage, InteractionStorage));
    }

    /// <summary>
    /// Generates a money prop if missing.
    /// </summary>
    private void GenerateMoney(PropHolder holder)
    {
        if (holder.CurrentProp != null)
        {
            return;
        }
        GameObject newGO = (GameObject)GameObject.Instantiate(MoneyPrefab);
        Prop prop = newGO.GetComponent<Prop>();
        this.HoldPropStorage.Attach(prop);
    }
}
