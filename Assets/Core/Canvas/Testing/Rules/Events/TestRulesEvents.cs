using UnityEngine;
using System.Collections;

using TreeSharpPlus;

[LibraryIndex(-20)]
public class TestUnlockDoorFront : GenericEvent<SmartCharacter, SmartDoor, SmartObject, SmartObject>
{
    private SmartCharacter actor;
    private SmartDoor door;

    private SmartObject actorZone;
    private SmartObject doorFrontZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorFrontZone)
    {
        return new Sequence(
            new LeafAffordance("UnlockFront", actor, door),
            actor.Node_Set(actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(doorFrontZone.Id, RelationName.IsInZone));
    }

    [Name("TestUnlockDoorFront")]
    [StateRequired(0, StateName.RoleActor)]
    [StateRequired(1, StateName.RoleDoor, ~StateName.IsUnlocked)]
    [StateRequired(2, StateName.RoleTelevision)]
    [StateRequired(3, StateName.RoleTelevision)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.FrontZone)]

    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(2, StateName.IsUnlocked)]
    [RelationEffect(0, 2, ~RelationName.IsInZone)]
    [RelationEffect(0, 3, RelationName.IsInZone)]

    [IsImplicit(2)]
    public TestUnlockDoorFront(
        SmartCharacter actor, 
        SmartDoor door, 
        SmartObject actorZone, 
        SmartObject doorFrontZone)
        : base(actor, door, actorZone, doorFrontZone)
    {
        this.actor = actor;
        this.door = door;
        this.actorZone = actorZone;
        this.doorFrontZone = doorFrontZone;
    }
}

[LibraryIndex(-20)]
public class TestUnlockDoorRear : GenericEvent<SmartCharacter, SmartDoor, SmartObject, SmartObject>
{
    private SmartCharacter actor;
    private SmartDoor door;

    private SmartObject actorZone;
    private SmartObject doorRearZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorFrontZone)
    {
        return new Sequence(
            new LeafAffordance("UnlockRear", actor, door),
            actor.Node_Set(actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(doorFrontZone.Id, RelationName.IsInZone));
    }

    [Name("TestUnlockDoorRear")]
    [StateRequired(0, StateName.RoleActor)]
    [StateRequired(1, StateName.RoleDoor, ~StateName.IsUnlocked)]
    [StateRequired(2, StateName.RoleTelevision)]
    [StateRequired(3, StateName.RoleTelevision)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.RearZone)]

    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(2, StateName.IsUnlocked)]
    [RelationEffect(0, 2, ~RelationName.IsInZone)]
    [RelationEffect(0, 3, RelationName.IsInZone)]

    [IsImplicit(2)]
    public TestUnlockDoorRear(
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorRearZone)
        : base(actor, door, actorZone, doorRearZone)
    {
        this.actor = actor;
        this.door = door;
        this.actorZone = actorZone;
        this.doorRearZone = doorRearZone;
    }
}


[LibraryIndex(-20)]
public class TestLockDoorFront : GenericEvent<SmartCharacter, SmartDoor, SmartObject, SmartObject>
{
    private SmartCharacter actor;
    private SmartDoor door;

    private SmartObject actorZone;
    private SmartObject doorFrontZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorFrontZone)
    {
        return new Sequence(
            new LeafAffordance("LockFront", actor, door),
            actor.Node_Set(actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(doorFrontZone.Id, RelationName.IsInZone));
    }

    [Name("TestLockDoorFront")]
    [StateRequired(0, StateName.RoleActor)]
    [StateRequired(1, StateName.RoleDoor, StateName.IsUnlocked)]
    [StateRequired(2, StateName.RoleTelevision)]
    [StateRequired(3, StateName.RoleTelevision)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.FrontZone)]

    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(2, ~StateName.IsUnlocked)]
    [RelationEffect(0, 2, ~RelationName.IsInZone)]
    [RelationEffect(0, 3, RelationName.IsInZone)]

    [IsImplicit(2)]
    public TestLockDoorFront(
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorFrontZone)
        : base(actor, door, actorZone, doorFrontZone)
    {
        this.actor = actor;
        this.door = door;
        this.actorZone = actorZone;
        this.doorFrontZone = doorFrontZone;
    }
}

[LibraryIndex(-20)]
public class TestLockDoorRear : GenericEvent<SmartCharacter, SmartDoor, SmartObject, SmartObject>
{
    private SmartCharacter actor;
    private SmartDoor door;

    private SmartObject actorZone;
    private SmartObject doorRearZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorFrontZone)
    {
        return new Sequence(
            new LeafAffordance("LockRear", actor, door),
            actor.Node_Set(actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(doorFrontZone.Id, RelationName.IsInZone));
    }

    [Name("TestLockDoorRear")]
    [StateRequired(0, StateName.RoleActor)]
    [StateRequired(1, StateName.RoleDoor, StateName.IsUnlocked)]
    [StateRequired(2, StateName.RoleTelevision)]
    [StateRequired(3, StateName.RoleTelevision)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.RearZone)]

    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(2, ~StateName.IsUnlocked)]
    [RelationEffect(0, 2, ~RelationName.IsInZone)]
    [RelationEffect(0, 3, RelationName.IsInZone)]

    [IsImplicit(2)]
    public TestLockDoorRear(
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorRearZone)
        : base(actor, door, actorZone, doorRearZone)
    {
        this.actor = actor;
        this.door = door;
        this.actorZone = actorZone;
        this.doorRearZone = doorRearZone;
    }
}

[LibraryIndex(-20)]
public class TestGuardDoorRear : GenericEvent<SmartCharacter, SmartDoor, SmartObject, SmartObject>
{
    private SmartCharacter actor;
    private SmartDoor door;

    private SmartObject actorZone;
    private SmartObject doorRearZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorFrontZone)
    {
        return new Sequence(
            new LeafAffordance("GuardRear", actor, door),
            actor.Node_Set(actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(doorFrontZone.Id, RelationName.IsInZone));
    }

    [Name("TestGuardDoorRear")]
    [StateRequired(0, StateName.RoleActor)]
    [StateRequired(1, StateName.RoleDoor, ~StateName.IsGuarded)]
    [StateRequired(2, StateName.RoleTelevision)]
    [StateRequired(3, StateName.RoleTelevision)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.RearZone)]

    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(2, StateName.IsGuarded)]
    [RelationEffect(0, 2, ~RelationName.IsInZone)]
    [RelationEffect(0, 3, RelationName.IsInZone)]

    [IsImplicit(2)]
    public TestGuardDoorRear(
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorRearZone)
        : base(actor, door, actorZone, doorRearZone)
    {
        this.actor = actor;
        this.door = door;
        this.actorZone = actorZone;
        this.doorRearZone = doorRearZone;
    }
}

[LibraryIndex(-20)]
public class TestGuardDoorFront : GenericEvent<SmartCharacter, SmartDoor, SmartObject, SmartObject>
{
    private SmartCharacter actor;
    private SmartDoor door;

    private SmartObject actorZone;
    private SmartObject doorFrontZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorFrontZone)
    {
        return new Sequence(
            new LeafAffordance("GuardFront", actor, door),
            actor.Node_Set(actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(doorFrontZone.Id, RelationName.IsInZone));
    }

    [Name("TestGuardDoorFront")]
    [StateRequired(0, StateName.RoleActor)]
    [StateRequired(1, StateName.RoleDoor, ~StateName.IsGuarded)]
    [StateRequired(2, StateName.RoleTelevision)]
    [StateRequired(3, StateName.RoleTelevision)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.FrontZone)]

    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(2, StateName.IsGuarded)]
    [RelationEffect(0, 2, ~RelationName.IsInZone)]
    [RelationEffect(0, 3, RelationName.IsInZone)]

    [IsImplicit(2)]
    public TestGuardDoorFront(
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorFrontZone)
        : base(actor, door, actorZone, doorFrontZone)
    {
        this.actor = actor;
        this.door = door;
        this.actorZone = actorZone;
        this.doorFrontZone = doorFrontZone;
    }
}

[LibraryIndex(-20)]
public class TestStopGuardingDoor : GenericEvent<SmartCharacter, SmartDoor>
{
    private SmartCharacter actor;
    private SmartDoor door;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door)
    {
        return new Sequence(
            new LeafAffordance("StopGuarding", actor, door),
            new LeafTrace("Stopped guarding."));
    }

    [Name("TestStopGuardingDoor")]
    [StateRequired(0, StateName.RoleActor)]
    [StateRequired(1, StateName.RoleDoor, StateName.IsGuarded)]

    [RelationRequired(0, 1, RelationName.IsGuarding)]

    [StateEffect(1, ~StateName.IsGuarded)]
    [RelationEffect(0, 1, ~RelationName.IsGuarding)]

    [IsImplicit(1)]
    public TestStopGuardingDoor(
        SmartCharacter actor,
        SmartDoor door)
        : base(actor, door)
    {
        this.actor = actor;
        this.door = door;
    }
}

[LibraryIndex(-20)]
public class TestIsInZone : GenericEvent<SmartCharacter, SmartObject>
{
    private SmartCharacter actor;
    private SmartObject actorZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartObject actorZone)
    {
        return new LeafTrace("In zone!");
    }

    [Name("TestIsInZone")]
    [RelationRequired(0, 1, RelationName.IsInZone)]

    [IsImplicit(2)]
    public TestIsInZone(
        SmartCharacter actor,
        SmartObject actorZone)
        : base(actor, actorZone)
    {
        this.actor = actor;
        this.actorZone = actorZone;
    }
}

[LibraryIndex(-20)]
public class TestGoWaypoint : GenericEvent<SmartCharacter, SmartWaypoint, SmartObject, SmartObject>
{
    private SmartCharacter actor;
    private SmartWaypoint waypoint;

    private SmartObject actorZone;
    private SmartObject waypointZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartWaypoint waypoint,
        SmartObject actorZone,
        SmartObject waypointZone)
    {
        return new Sequence(
            new LeafAffordance("Approach", actor, waypoint),
            actor.Node_Set(actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(waypointZone.Id, RelationName.IsInZone));
    }

    [Name("TestGoWaypoint")]
    [StateRequired(0, StateName.RoleActor)]
    [StateRequired(1, StateName.RoleWaypoint)]
    [StateRequired(2, StateName.RoleTelevision)]
    [StateRequired(3, StateName.RoleTelevision)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.IsInZone)]

    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [RelationEffect(0, 2, ~RelationName.IsInZone)]
    [RelationEffect(0, 3, RelationName.IsInZone)]

    [IsImplicit(2)]
    public TestGoWaypoint(
        SmartCharacter actor,
        SmartWaypoint waypoint,
        SmartObject actorZone,
        SmartObject waypointZone)
        : base(actor, waypoint, actorZone, waypointZone)
    {
        this.actor = actor;
        this.waypoint = waypoint;
        this.actorZone = actorZone;
        this.waypointZone = waypointZone;
    }
}

[LibraryIndex(-20)]
public class TestIncapacitate : GenericEvent<SmartCharacter, SmartCharacter, SmartObject, SmartObject>
{
    private SmartCharacter actor;
    private SmartCharacter target;

    private SmartObject actorZone;
    private SmartObject targetZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartCharacter target,
        SmartObject actorZone,
        SmartObject targetZone)
    {
        return new Sequence(
            new LeafAffordance("Incapacitate", actor, target),
            actor.Node_Set(actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(targetZone.Id, RelationName.IsInZone));
    }

    [Name("TestIncapacitate")]
    [StateRequired(0, StateName.RoleActor)]
    [StateRequired(1, StateName.RoleActor)]
    [StateRequired(2, StateName.RoleTelevision)]
    [StateRequired(3, StateName.RoleTelevision)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]

    [RelationEffect(0, 2, ~RelationName.IsInZone)]
    [RelationEffect(0, 3, RelationName.IsInZone)]

    [IsImplicit(2)]
    public TestIncapacitate(
        SmartCharacter actor,
        SmartCharacter target,
        SmartObject actorZone,
        SmartObject targetZone)
        : base(actor, target, actorZone, targetZone)
    {
        this.actor = actor;
        this.target = target;
        this.actorZone = actorZone;
        this.targetZone = targetZone;
    }
}