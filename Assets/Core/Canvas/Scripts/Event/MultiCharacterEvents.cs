using UnityEngine;
using TreeSharpPlus;
using System.Collections.Generic;

// This file contains various events for multiple characters.


public static class ReusedTrees
{
    /// <summary>
    /// Tree for DropAndReturn events. Uses three affordances, these should depend on the type of object that is being dropped
    /// (e.g. ball, wallet, ...)
    /// </summary>
    public static Node DropAndReturnThing(
        SmartCharacter dropper, 
        SmartCharacter returner, 
        SmartContainer container, 
        SmartWaypoint waypoint,
        string dropAffordance, 
        string pickupAffordance, 
        string giveAffordance)
    {
        Vector3 dropperGoal = waypoint.transform.position;

        //Sequence:
        //Dropper walks away and drops object. Returner alerts dropper, dropper turns reacts and turns towards him,
        //returner picks up object and goes to dropper to give it to him, dropper thanks returner.
        return new Sequence(
            returner.Node_OrientTowards(dropper.transform.position),
            new LeafAffordance(dropAffordance, dropper, container),
            new SequenceParallel(
                dropper.Node_GoTo(Val.V(() => dropperGoal)),
                new Sequence(
                    new LeafWait(1300),
                    new LeafInvoke(() => dropperGoal = dropper.transform.position + dropper.GetComponent<NavMeshAgent>().desiredVelocity),
                    dropper.Node_OrientTowards(returner.transform.position)),
                new SequenceParallel(
                    returner.Behavior.ST_PlayHandGesture("cheer", 2000),
                    new LeafAffordance(pickupAffordance, returner, container))),
            new LeafAffordance(giveAffordance, returner, dropper),
            dropper.Behavior.ST_PlayFaceGesture("lookaway", 1000),
            dropper.Behavior.ST_PlayFaceGesture("headnod", 1000));
    }
}

[LibraryIndex(-2)]
public class TalkNormally : GenericEvent<SmartCharacter, SmartCharacter>
{
    protected override Node Root(Token token, SmartCharacter char1, SmartCharacter char2)
    {
        return new LeafAffordance("Talk", char1, char2);
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, StateName.IsStanding)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding)]
    [HideInGUI(3)]
    public TalkNormally(SmartCharacter char1, SmartCharacter char2)
        :base(char1, char2) { }
}

[LibraryIndex(-2)]
public class TalkHappily : GenericEvent<SmartCharacter, SmartCharacter>
{
    protected override Node Root(Token token, SmartCharacter char1, SmartCharacter char2)
    {
        return new LeafAffordance("TalkHappily", char1, char2);
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, StateName.IsStanding)]
    [StateRequired(1, StateName.RoleActor, ~StateName.RoleTeller, StateName.IsStanding)]
    [HideInGUI(3)]
    public TalkHappily(SmartCharacter char1, SmartCharacter char2)
        : base(char1, char2) { }
}

[LibraryIndex(-2)]
public class TalkSecretively : GenericEvent<SmartCharacter, SmartCharacter>
{
    protected override Node Root(Token token, SmartCharacter char1, SmartCharacter char2)
    {
        return new LeafAffordance("TalkSecretively", char1, char2);
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, StateName.IsStanding)]
    [StateRequired(1, StateName.RoleActor, ~StateName.RoleTeller, StateName.IsStanding)]
    [HideInGUI(3)]
    public TalkSecretively(SmartCharacter char1, SmartCharacter char2)
        : base(char1, char2) { }
}

[LibraryIndex(-2)]
public class DropAndReturnWallet : GenericEvent<SmartCharacter, SmartCharacter, SmartContainer, SmartWaypoint>
{
    protected override Node Root(Token token, SmartCharacter dropper, SmartCharacter returner, SmartContainer container, SmartWaypoint waypoint)
    {
        return ReusedTrees.DropAndReturnThing(
            dropper, 
            returner, 
            container, 
            waypoint, 
            "DropWallet", 
            "PickupWallet", 
            "GiveWallet");
    }

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, StateName.HoldingWallet)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingWallet, ~StateName.HoldingBall)]
    [StateRequired(2, StateName.RoleContainer, ~StateName.HoldingWallet, ~StateName.HoldingBall)]
    [StateRequired(3, StateName.RoleWaypoint)]
    public DropAndReturnWallet(SmartCharacter dropper, SmartCharacter returner, SmartContainer container, SmartWaypoint waypoint)
        : base(dropper, returner, container, waypoint) { }
}

[LibraryIndex(-2)]
public class DropAndReturnBall : GenericEvent<SmartCharacter, SmartCharacter, SmartContainer, SmartWaypoint>
{
    protected override Node Root(Token token, SmartCharacter dropper, SmartCharacter returner, SmartContainer container, SmartWaypoint waypoint)
    {
        return ReusedTrees.DropAndReturnThing(
            dropper, 
            returner, 
            container, 
            waypoint, 
            "DropBall", 
            "PickupBall", 
            "GiveBall");
    }

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, StateName.HoldingBall)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingWallet, ~StateName.HoldingBall)]
    [StateRequired(2, StateName.RoleContainer, ~StateName.HoldingBall, ~StateName.HoldingWallet)]
    [StateRequired(3, StateName.RoleWaypoint)]
    public DropAndReturnBall(SmartCharacter dropper, SmartCharacter returner, SmartContainer container, SmartWaypoint waypoint)
        : base(dropper, returner, container, waypoint) { } 
}


[LibraryIndexAttribute(-2)]
public class CallAndConverse : GenericEvent<SmartCharacter, SmartCharacter>
{
    protected override Node Root(Token token, SmartCharacter caller, SmartCharacter callee)
    {
        return new Sequence(
            caller.Node_OrientTowards(callee.transform.position),
            new SequenceParallel(
                caller.Behavior.ST_PlayHandGesture("cheer", 3000),
                new Sequence(
                    new LeafWait(1300),
                    callee.Node_OrientTowards(caller.transform.position))),
            new LeafAffordance("TalkHappily", caller, callee));
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, StateName.IsStanding)]
    [StateRequired(1, StateName.RoleActor, ~StateName.RoleTeller, StateName.IsStanding)]
    [HideInGUI(3)]
    public CallAndConverse(SmartCharacter caller, SmartCharacter callee)
        : base(caller, callee) { } 
}


[LibraryIndex(-2)]
public class SitDownAndConverse : GenericEvent<SmartCharacter, SmartCharacter, SmartChair, SmartChair>
{
    protected override Node Root(Token token, SmartCharacter char1, SmartCharacter char2, SmartChair chair1, SmartChair chair2)
    {
        return new Sequence(
            new SequenceParallel(
                chair1.Sit(char1),
                chair2.Sit(char2)),
            char1.Node_HeadLook(char2.MarkerHead.position),
            char2.Node_HeadLook(char1.MarkerHead.position),
            char1.ST_TalkWithoutApproach(char2),
            char1.Node_HeadLookStop(),
            char2.Node_HeadLookStop());
    }


    [StateRequired(0, StateName.RoleActor, StateName.IsStanding)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding)]
    [StateRequired(2, StateName.RoleChair, ~StateName.IsOccupied)]
    [StateRequired(3, StateName.RoleChair, ~StateName.IsOccupied)]
    [RelationRequired(2, 3, RelationName.IsAdjacentTo)]
    [StateEffect(0, ~StateName.IsStanding)]
    [StateEffect(1, ~StateName.IsStanding)]
    [StateEffect(2, StateName.IsOccupied)]
    [StateEffect(3, StateName.IsOccupied)]
    [RelationEffect(0, 2, RelationName.IsSittingOn)]
    [RelationEffect(1, 3, RelationName.IsSittingOn)]
    public SitDownAndConverse(SmartCharacter char1, SmartCharacter char2,
        SmartChair chair1, SmartChair chair2)
        : base(char1, char2, chair1, chair2) { } 
}



[LibraryIndex(-2)]
public class Wait : GenericEvent<SmartObject>
{
    protected override Node Root(Token token, SmartObject participant)
    {
        return new DecoratorLoop(new LeafWait(1000));
    }

    public Wait(SmartObject waiter)
        : base(waiter) { }
}

[LibraryIndex(-2)]
public class LookAtIndefinitely : GenericEvent<SmartCharacter>
{
    private SmartObject obj;

    protected override Node Root(Token token, SmartCharacter looker)
    {
        Transform lookAt = (obj is SmartCharacter) ? ((SmartCharacter)obj).MarkerHead :  obj.transform;
        return new DecoratorCatch(
            () => looker.Character.HeadLookStop(),
            new DecoratorLoop(
                looker.Node_HeadLook(lookAt.position)));
    }

    [StateRequired(0, StateName.RoleActor)]
    [StateRequired(1, ~StateName.RoleCrowd)]
    [NonParticipantAttribute(1)]
    public LookAtIndefinitely(SmartCharacter looker, SmartObject obj)
        :base(looker)
    {
        this.obj = obj;
    }
}

[LibraryIndex(-2)]
public class TurnOnTV : GenericEvent<SmartCharacter, SmartTelevision>
{
    protected override Node Root(Token token, SmartCharacter user, SmartTelevision tv)
    {
        return new LeafAffordance("TurnOn", user, tv);
    }

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding)]
    [StateRequired(1, StateName.RoleTelevision, ~StateName.IsTurnedOn)]
    [StateEffect(1, StateName.IsTurnedOn)]
    public TurnOnTV(SmartCharacter user, SmartTelevision tv)
        : base(user, tv) { } 
}

[LibraryIndex(-2)]
public class TurnOffTV : GenericEvent<SmartCharacter, SmartTelevision>
{
    protected override Node Root(Token token, SmartCharacter user, SmartTelevision tv)
    {
        return new LeafAffordance("TurnOff", user, tv);
    }

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding)]
    [StateRequired(1, StateName.RoleTelevision, StateName.IsTurnedOn)]
    [StateEffect(1, ~StateName.IsTurnedOn)]
    public TurnOffTV(SmartCharacter user, SmartTelevision tv)
        : base(user, tv) { }
}

[LibraryIndex(-2)]
public class TurnOnLamp : GenericEvent<SmartCharacter, SmartLamp>
{
    protected override Node Root(Token token, SmartCharacter user, SmartLamp lamp)
    {
        return new LeafAffordance("TurnOn", user, lamp);
    }

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding)]
    [StateRequired(1, StateName.RoleLamp, ~StateName.IsTurnedOn)]
    [StateEffect(1, StateName.IsTurnedOn)]
    public TurnOnLamp(SmartCharacter user, SmartLamp lamp)
        : base(user, lamp) { }
}

[LibraryIndex(-2)]
public class TurnOffLamp : GenericEvent<SmartCharacter, SmartLamp>
{
    protected override Node Root(Token token, SmartCharacter user, SmartLamp lamp)
    {
        return new LeafAffordance("TurnOff", user, lamp);
    }

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding)]
    [StateRequired(1, StateName.RoleLamp, StateName.IsTurnedOn)]
    [StateEffect(1, ~StateName.IsTurnedOn)]
    public TurnOffLamp(SmartCharacter user, SmartLamp lamp)
        : base(user, lamp) { }
}

[LibraryIndex(-2)]
public class TakeBall : GenericEvent<SmartCharacter, SmartTable>
{
    protected override Node Root(Token token, SmartCharacter user, SmartTable table)
    {
        return new LeafAffordance("TakeBall", user, table);
    }

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingBall, ~StateName.HoldingWallet, ~StateName.HoldingDrink)]
    [StateRequired(1, StateName.RoleTable, StateName.HoldingBall)]
    [StateEffect(0, StateName.HoldingBall)]
    [StateEffect(1, ~StateName.HoldingBall)]
    public TakeBall(SmartCharacter user, SmartTable table)
        : base(user, table) { }
}

[LibraryIndex(-2)]
public class PlaceBall : GenericEvent<SmartCharacter, SmartTable>
{
    protected override Node Root(Token token, SmartCharacter user, SmartTable table)
    {
        return new LeafAffordance("PlaceBall", user, table);
    }

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, StateName.HoldingBall)]
    [StateRequired(1, StateName.RoleTable, ~StateName.HoldingBall, ~StateName.HoldingWallet)]
    [StateEffect(0, ~StateName.HoldingBall)]
    [StateEffect(1, StateName.HoldingBall)]
    public PlaceBall(SmartCharacter user, SmartTable table)
        : base(user, table) { }
}