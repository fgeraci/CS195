using UnityEngine;
using TreeSharpPlus;
using System;
using System.Collections;
using System.Collections.Generic;

public class SmartChair : SmartObject
{
    public override string Archetype { get { return "SmartChair"; } }

    void Awake()
    {
        base.Initialize(new BehaviorObject());
    }

    public Transform standPoint;
    public Transform facePoint;

    [Affordance]
    public Node Sit(SmartCharacter user)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated),
            this.Node_Require(StateName.RoleChair, ~StateName.IsOccupied),
            this.Node_Set(StateName.IsOccupied),
            new Race(
                new Sequence(
                    new LeafWait(15000),
                    new LeafInvoke(() => RunStatus.Failure)),
                user.ST_StandAtWaypoint(this.standPoint)),
            user.Node_NudgeTo(Val.V(() => this.standPoint.position)),
            new LeafInvoke(() => user.transform.rotation = this.standPoint.rotation),
            user.Node_Set(~StateName.IsStanding),
            user.Node_Set(this.Id, RelationName.IsSittingOn),
            new LeafInvoke(() => user.Character.SitDown()),
            new LeafWait(1000),
            new LeafInvoke(() => user.Character.NavOrientBehavior(OrientationBehavior.LookForward)));
    }

    [Affordance]
    public Node Stand(SmartCharacter user)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, ~StateName.IsStanding, ~StateName.IsIncapacitated),
            user.Node_Require(this.Id, RelationName.IsSittingOn),
            this.Node_Require(StateName.RoleChair, StateName.IsOccupied),
            new LeafInvoke(() => user.Character.StandUp()),
            new LeafWait(1000),
            user.Node_Set(StateName.IsStanding),
            user.Node_Set(this.Id, ~RelationName.IsSittingOn),
            this.Node_Set(~StateName.IsOccupied));
    }
}
