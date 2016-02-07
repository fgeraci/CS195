using UnityEngine;
using TreeSharpPlus;

using RootMotion.FinalIK;

/// <summary>
/// A SmartObject that can be used to drop and retrieve props.
/// </summary>
public class SmartContainer : SmartObject 
{
    public override string Archetype
    {
        get { return "SmartContainer"; }
    }

    private Prop ContainedProp
    {
        get
        {
            if (this.Holder == null)
                return null;
            return this.Holder.CurrentProp;
        }
    }

    /// <summary>
    /// The holder for the dropped prop.
    /// </summary>
    public PropHolder Holder;

    /// <summary>
    /// The interaction object to use when picking up a gun.
    /// </summary>
    public InteractionObject InteractionTakeGun;

    [Affordance]
    protected Node PickupBackpack(SmartCharacter user)
    {
        return new Sequence(
            user.Node_GoToUpToRadius(Val.V(() =>
                this.ContainedProp.transform.position - 0.15f * user.transform.right), 1.0f),
            user.Behavior.Node_BodyAnimation("pickupleft", true),
            new LeafWait(500),
            new LeafInvoke(() => user.HoldPropLeftHand.Attach(this.Holder.Release())),
            new LeafWait(1000),
            this.Node_Set(~StateName.HasBackpack, ~StateName.IsOccupied),
            user.Node_Set(StateName.HasBackpack),
            new LeafWait(500));
    }

    [Affordance]
    protected Node PickupWeapon(SmartCharacter user)
    {
        return new Sequence(
            new Selector(
                new Sequence(
                    new LeafAssert(() => (user.transform.position - this.transform.position).magnitude < 1.0f),
                    new LeafInvoke(() => user.Character.NavStop())),
                user.Node_GoToUpToRadius(Val.V(() => this.transform.position), 1.0f)),
            user.Node_OrientTowards(Val.V(() =>this.transform.position)),
            user.Node_StartInteraction(FullBodyBipedEffector.RightHand, this.InteractionTakeGun),
            new LeafWait(500),
            new LeafInvoke(() => this.Holder.CurrentProp.FadeOut()),
            new LeafWait(500),
            new LeafInvoke(() => user.HoldPropRightHand.Attach(this.Holder.Release())),
            new LeafInvoke(() => user.GetRightProp().FadeIn()),
            user.Node_StopInteraction(FullBodyBipedEffector.RightHand),
            new LeafWait(1000),
            this.Node_Set(~StateName.HoldingWeapon, ~StateName.IsOccupied),
            user.Node_Set(StateName.HoldingWeapon, StateName.RightHandOccupied));
    }

    [Affordance]
    protected Node TeleportDropBackpack(SmartCharacter user)
    {
        return new Sequence(
            new LeafInvoke(() => this.transform.position = user.transform.position - 0.15f * user.transform.right
                + user.transform.forward),
            user.Behavior.Node_BodyAnimation("pickupleft", true),
            new LeafWait(500),
            new LeafInvoke(() => this.Holder.Attach(user.HoldPropLeftHand.Release())),
            new LeafWait(1500),
            this.Node_Set(StateName.HasBackpack, StateName.IsOccupied),
            user.Node_Set(~StateName.HasBackpack));
    }

    [Affordance]
    protected Node TeleportDropWeapon(SmartCharacter user)
    {
        return new Sequence(
            user.Node_StartInteraction(FullBodyBipedEffector.RightHand, user.InteractionDropGun),
            new LeafWait(500),
            new LeafInvoke(() => user.GetRightProp().FadeOut()),
            new LeafWait(500),
            new LeafInvoke(() => this.SetReceivePosition(user)),
            new LeafInvoke(() => this.Holder.Attach(user.HoldPropRightHand.Release())),
            new LeafInvoke(() => this.ContainedProp.FadeIn()),
            new LeafWait(500),
            user.Node_StopInteraction(FullBodyBipedEffector.RightHand),
            this.Node_Set(StateName.HoldingWeapon, StateName.IsOccupied),
            user.Node_Set(~StateName.HoldingWeapon, ~StateName.RightHandOccupied));
    }

    private void SetReceivePosition(SmartCharacter holder)
    {
        Transform propTrans = holder.GetRightProp().transform;
        Vector3 propPos = propTrans.position;
        float y = holder.transform.position.y;
        this.transform.position = new Vector3(propPos.x, y, propPos.z);
        float yAngle = propTrans.rotation.eulerAngles.y;
        Vector3 currentAngles = transform.rotation.eulerAngles;
        this.transform.rotation = 
            Quaternion.Euler(
                new Vector3(
                    currentAngles.x,
                    yAngle,
                    currentAngles.z));
    }







    [Affordance]
    protected Node DropWallet(SmartCharacter user)
    {
        return ST_Drop(user, StateName.HoldingWallet, user.HoldPropPocket);
    }

    [Affordance]
    protected Node PickupWallet(SmartCharacter user)
    {
        return new Sequence(
            ST_PickupRightHand(user, StateName.HoldingWallet),
            user.ST_PutWalletInPocket(),
            user.Node_Set(~StateName.RightHandOccupied));
    }

    [Affordance]
    protected Node DropBall(SmartCharacter user)
    {
        return ST_DropRightHand(user, StateName.HoldingBall);
    }

    [Affordance]
    protected Node PickupBall(SmartCharacter user)
    {
        return ST_PickupRightHand(user, StateName.HoldingBall);
    }

    [Affordance]
    protected Node PickupMoney(SmartCharacter user)
    {
        return ST_PickupLeftHand(user, StateName.HasBackpack);
    }

    [Affordance]
    protected Node DropMoney(SmartCharacter user)
    {
        return ST_DropLeftHand(user, StateName.HasBackpack);
    }

    [Affordance]
    protected Node DropWeapon(SmartCharacter user)
    {
        return ST_DropRightHand(user, StateName.HoldingWeapon);
    }

    #region Internal subtrees
    protected Node ST_Drop(SmartCharacter user, StateName holdingState, PropHolder holder)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, holdingState, StateName.IsStanding),
            this.Node_Require(StateName.RoleContainer, ~StateName.IsOccupied),
            new LeafInvoke(() => this.Holder.Attach(holder.Release())),
            this.Node_Set(holdingState, StateName.IsOccupied),
            user.Node_Set(~holdingState));
    }

    protected Node ST_DropLeftHand(SmartCharacter user, StateName holdingState)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.LeftHandOccupied, holdingState, StateName.IsStanding),
            this.Node_Require(StateName.RoleContainer, ~StateName.IsOccupied),
            new LeafInvoke(() => this.Holder.Attach(user.HoldPropLeftHand.Release())),
            this.Node_Set(holdingState, StateName.IsOccupied),
            user.Node_Set(~holdingState, ~StateName.LeftHandOccupied));
    }

    protected Node ST_DropRightHand(SmartCharacter user, StateName holdingState)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.RightHandOccupied, holdingState, StateName.IsStanding),
            this.Node_Require(StateName.RoleContainer, ~StateName.IsOccupied),
            new LeafInvoke(() => this.transform.position = user.transform.position),
            new LeafInvoke(() => this.Holder.Attach(user.HoldPropRightHand.Release())),
            this.Node_Set(holdingState, StateName.IsOccupied),
            user.Node_Set(~holdingState, ~StateName.RightHandOccupied));
    }

    protected Node ST_PickupRightHand(SmartCharacter user, StateName holdingState)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, ~StateName.RightHandOccupied, StateName.IsStanding),
            this.Node_Require(StateName.RoleContainer, holdingState, StateName.IsOccupied),
            user.Node_GoToUpToRadius(Val.V(() => 
                this.ContainedProp.transform.position - 0.15f * user.transform.right), 0.1f),
            user.Behavior.Node_BodyAnimation("pickupright", true),
            new LeafWait(500),
            new LeafInvoke(() => user.HoldPropRightHand.Attach(this.Holder.Release())),
            this.Node_Set(~holdingState, ~StateName.IsOccupied),
            user.Node_Set(holdingState, StateName.RightHandOccupied),
            new LeafWait(500));
    }

    protected Node ST_PickupLeftHand(SmartCharacter user, StateName holdingState)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, ~StateName.LeftHandOccupied, StateName.IsStanding),
            this.Node_Require(StateName.RoleContainer, holdingState, StateName.IsOccupied),
            user.Node_GoToUpToRadius(Val.V(() => 
                this.ContainedProp.transform.position + 0.15f * user.transform.right), 0.1f),
            user.Behavior.Node_BodyAnimation("pickupleft", true),
            new LeafWait(500),
            new LeafInvoke(() => user.HoldPropLeftHand.Attach(this.Holder.Release())),
            this.Node_Set(~holdingState, ~StateName.IsOccupied),
            user.Node_Set(holdingState, StateName.LeftHandOccupied),
            new LeafWait(500));
    }
    #endregion
}
