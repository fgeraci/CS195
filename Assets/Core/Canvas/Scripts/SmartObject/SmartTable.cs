using UnityEngine;
using TreeSharpPlus;
using System;
using System.Collections;
using System.Collections.Generic;

using RootMotion.FinalIK;

public class SmartTable : SmartObject
{
    public override string Archetype { get { return "SmartTable"; } }

    void Awake()
    {
        base.Initialize(new BehaviorObject());
    }

    public InteractionObject PropPickup = null;
    public PropHolder PropHolder = null;
    public Transform StandPoint = null;

    [Affordance]
    protected Node TakeBall(SmartCharacter user)
    {
        return ST_Take(user, StateName.HoldingBall, new LeafWait(0));
    }

    [Affordance]
    protected Node PlaceBall(SmartCharacter user)
    {
        return ST_Place(user, StateName.HoldingBall, new LeafWait(0));
    }

    [Affordance]
    protected Node TakeWallet(SmartCharacter user)
    {
        return ST_Take(user, StateName.HoldingWallet, user.ST_PutWalletInPocket());
    }

    [Affordance]
    protected Node PlaceWallet(SmartCharacter user)
    {
        return ST_Place(user, StateName.HoldingWallet, user.ST_TakeWalletFromPocket());
    }

    [Affordance]
    protected Node FillOutForm(SmartCharacter user)
    {
        Vector3 oldPosition = new Vector3();
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.RightHandOccupied),
            this.Node_Require(StateName.RoleTable, ~StateName.HoldingBall, ~StateName.HoldingWallet),
            new LeafInvoke(() => oldPosition = user.transform.position),
            new Race(
                new Sequence(
                    new LeafWait(15000),
                    new LeafInvoke(() => RunStatus.Failure)),
                user.ST_StandAtWaypoint(this.StandPoint)),
            user.Node_NudgeTo(Val.V(() => this.StandPoint.position)),
            new LeafInvoke(() => user.transform.rotation = this.StandPoint.rotation),
            new LeafInvoke(() => user.HoldPropRightHand.Attach(user.HoldPropHidden.Release())),
            new LeafInvoke(() => user.GetRightProp().FadeIn()),
            user.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.PropPickup),
            user.Node_WaitForTrigger(FullBodyBipedEffector.RightHand),
            new LeafInvoke(() => this.PropHolder.Attach(user.HoldPropRightHand.Release())),
            user.Node_StopInteraction(FullBodyBipedEffector.RightHand),
            user.Behavior.ST_PlayHandGesture("writing", 6000),
            user.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.PropPickup),
            user.Node_WaitForTrigger(FullBodyBipedEffector.RightHand),
            new LeafInvoke(() => user.HoldPropRightHand.Attach(this.PropHolder.Release())),
            user.Node_StopInteraction(FullBodyBipedEffector.RightHand),
            new LeafWait(500),
            new LeafInvoke(() => user.GetRightProp().FadeOut()),
            new LeafWait(500),
            new LeafInvoke(() => user.HoldPropHidden.Attach(user.HoldPropRightHand.Release())),
            user.Node_GoTo(Val.V(() => oldPosition)));
    }

    protected Node ST_Take(SmartCharacter user, StateName holdingState, Node afterTake)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingBall, ~StateName.HoldingWallet, ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleTable, holdingState),
            user.Node_GoTo(this.StandPoint.position),
            new SequenceParallel(
                user.Node_HeadLook(this.PropPickup.transform.position),
                user.Node_OrientTowards(this.PropPickup.transform.position)),
            new DecoratorCatch(
                () => { user.Character.StopInteraction(FullBodyBipedEffector.RightHand); user.Character.HeadLookStop(); },
                new Sequence(
                   user.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.PropPickup),
                   user.Node_WaitForTrigger(FullBodyBipedEffector.RightHand),
                   new LeafInvoke(() => user.HoldPropRightHand.Attach(this.PropHolder.Release())),
                   user.Node_HeadLookStop())),
            this.Node_Set(~holdingState),
            afterTake,
            user.Node_Set(holdingState),
            user.Node_StartInteraction(FullBodyBipedEffector.RightHand, user.InteractionHoldRight),
            user.Node_WaitForTrigger(FullBodyBipedEffector.RightHand),
            new LeafWait(300));
    }

    protected Node ST_Place(SmartCharacter user, StateName holdingState, Node beforePlace)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, holdingState, ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleTable, ~StateName.HoldingBall, ~StateName.HoldingWallet, ~StateName.HoldingDrink),
            user.Node_GoTo(this.StandPoint.position),
            beforePlace,
            new SequenceParallel(
                user.Node_HeadLook(this.PropPickup.transform.position),
                user.Node_OrientTowards(this.PropPickup.transform.position)),
            new DecoratorCatch(
                () => { user.Character.StopInteraction(FullBodyBipedEffector.RightHand); user.Character.HeadLookStop(); },
                new Sequence(
                   user.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.PropPickup),
                   user.Node_WaitForTrigger(FullBodyBipedEffector.RightHand),
                    new LeafInvoke(() => this.PropHolder.Attach(user.HoldPropRightHand.Release())),
                   user.Node_HeadLookStop())),
            user.Node_Set(~holdingState),
            this.Node_Set(holdingState),
            user.Node_StopInteraction(FullBodyBipedEffector.RightHand),
            new LeafWait(1000));
    }
}
