using TreeSharpPlus;

// SPECIAL CASE TODOs:
//      - Locking/Unlocking doors by a character guarding that door (currently not allowed)

[LibraryIndex(3)]
public class UnlockDoorFront : GenericEvent<SmartCharacter, SmartDoor>
{
    private SmartObject actorZone;
    private SmartObject doorFrontZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door)
    {
        return new Sequence(
            ReusableActions.UnlockDoorFront(actor, door),
            actor.Node_Set(this.actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(this.doorFrontZone.Id, RelationName.IsInZone));
    }

    [Name("UnlockDoorFront")]
    [IconName("Evnt_UnlockDoor")]
    [StateRequired(0, StateName.RoleActor, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
    [StateRequired(1, StateName.RoleDoor, ~StateName.IsUnlocked)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.FrontZone)]

    [RuleRequired(0, 1, RuleName.CanManipulateObject)]
    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(1, StateName.IsUnlocked)]

	[RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]
    [RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public UnlockDoorFront(
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorFrontZone)
        : base(actor, door)
    {
        this.actorZone = actorZone;
        this.doorFrontZone = doorFrontZone;
    }
}

[LibraryIndex(3)]
public class UnlockDoorRear : GenericEvent<SmartCharacter, SmartDoor>
{
    private SmartObject actorZone;
    private SmartObject doorRearZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door)
    {
        return new Sequence(
            ReusableActions.UnlockDoorRear(actor, door),
            actor.Node_Set(this.actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(this.doorRearZone.Id, RelationName.IsInZone));
    }

    [Name("UnlockDoorRear")]
    [IconName("Evnt_UnlockDoor")]
    [StateRequired(0, StateName.RoleActor, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
    [StateRequired(1, StateName.RoleDoor, ~StateName.IsUnlocked)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.RearZone)]

    [RuleRequired(0, 1, RuleName.CanManipulateObject)]
    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(1, StateName.IsUnlocked)]

	[RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]
	[RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public UnlockDoorRear(
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorRearZone)
        : base(actor, door)
    {
        this.actorZone = actorZone;
        this.doorRearZone = doorRearZone;
    }
}

[LibraryIndex(3)]
public class LockDoorFront : GenericEvent<SmartCharacter, SmartDoor>
{
    private SmartObject actorZone;
    private SmartObject doorFrontZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door)
    {
        return new Sequence(
            ReusableActions.LockDoorFront(actor, door),
            actor.Node_Set(this.actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(this.doorFrontZone.Id, RelationName.IsInZone));
    }

    [Name("LockDoorFront")]
    [IconName("Evnt_LockDoor")]
    [StateRequired(0, StateName.RoleActor, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
    [StateRequired(1, StateName.RoleDoor, StateName.IsUnlocked)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.FrontZone)]

    [RuleRequired(0, 1, RuleName.CanManipulateObject)]
    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(1, ~StateName.IsUnlocked)]

	[RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]
	[RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public LockDoorFront(
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorFrontZone)
        : base(actor, door)
    {
        this.actorZone = actorZone;
        this.doorFrontZone = doorFrontZone;
    }
}

[LibraryIndex(3)]
public class LockDoorRear : GenericEvent<SmartCharacter, SmartDoor>
{
    private SmartObject actorZone;
    private SmartObject doorRearZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door)
    {
        return new Sequence(
            ReusableActions.LockDoorRear(actor, door),
            actor.Node_Set(this.actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(this.doorRearZone.Id, RelationName.IsInZone));
    }

    [Name("LockDoorRear")]
    [IconName("Evnt_LockDoor")]
    [StateRequired(0, StateName.RoleActor, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
    [StateRequired(1, StateName.RoleDoor, StateName.IsUnlocked)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.RearZone)]

    [RuleRequired(0, 1, RuleName.CanManipulateObject)]
    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(1, ~StateName.IsUnlocked)]
	[RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public LockDoorRear(
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorRearZone)
        : base(actor, door)
    {
        this.actorZone = actorZone;
        this.doorRearZone = doorRearZone;
    }
}

[LibraryIndex(3)]
public class GuardDoorFront : GenericEvent<SmartCharacter, SmartDoor>
{
    private SmartObject actorZone;
    private SmartObject doorFrontZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door)
    {
        return new Sequence(
            ReusableActions.GuardDoorFront(actor, door),
            actor.Node_Set(this.actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(this.doorFrontZone.Id, RelationName.IsInZone));
    }

    [Name("GuardDoorFront")]
    [IconName("Evnt_GuardDoor")]
    [StateRequired(0, StateName.RoleActor, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleDoor, ~StateName.IsGuarded)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.FrontZone)]

    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(0, StateName.IsImmobile, StateName.IsGuarding)]
    [StateEffect(1, StateName.IsGuarded)]
    [RelationEffect(0, 1, RelationName.IsGuarding)]
	[RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public GuardDoorFront(
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorFrontZone)
        : base(actor, door)
    {
        this.actorZone = actorZone;
        this.doorFrontZone = doorFrontZone;
    }
}

[LibraryIndex(3)]
public class GuardDoorRear : GenericEvent<SmartCharacter, SmartDoor>
{
    private SmartObject actorZone;
    private SmartObject doorRearZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door)
    {
        return new Sequence(
            ReusableActions.GuardDoorRear(actor, door),
            actor.Node_Set(this.actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(this.doorRearZone.Id, RelationName.IsInZone));
    }

    [Name("GuardDoorRear")]
    [IconName("Evnt_GuardDoor")]
    [StateRequired(0, StateName.RoleActor, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleDoor, ~StateName.IsGuarded)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.RearZone)]

    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(0, StateName.IsImmobile, StateName.IsGuarding)]
    [StateEffect(1, StateName.IsGuarded)]
    [RelationEffect(0, 1, RelationName.IsGuarding)]
	[RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]
	[RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public GuardDoorRear(
        SmartCharacter actor,
        SmartDoor door,
        SmartObject actorZone,
        SmartObject doorRearZone)
        : base(actor, door)
    {
        this.actorZone = actorZone;
        this.doorRearZone = doorRearZone;
    }
}

[LibraryIndex(3)]
public class StopGuardingDoor : GenericEvent<SmartCharacter, SmartDoor>
{
    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartDoor door)
    {
        return ReusableActions.StopGuardingDoor(actor, door);
    }

    [Name("StopGuardingDoor")]
    [StateRequired(0, StateName.RoleActor)]
    [StateRequired(1, StateName.RoleDoor, StateName.IsGuarded)]

    [RelationRequired(0, 1, RelationName.IsGuarding)]

    [StateEffect(0, ~StateName.IsImmobile, ~StateName.IsGuarding)]
    [StateEffect(1, ~StateName.IsGuarded)]
    [RelationEffect(0, 1, ~RelationName.IsGuarding)]

    [IsImplicit(1)]
    [NonParticipant(2, 3)]
    public StopGuardingDoor(
        SmartCharacter actor,
        SmartDoor door)
        : base(actor, door) { }
}

/// <summary>
/// The character opens the vault door after both buttons have been pressed. Also takes the manager door
/// as input, which must be unlocked.
/// </summary>
[LibraryIndex(3)]
public class OpenVault : GenericEvent<SmartCharacter, SmartVaultDoor>
{
    private SmartObject userZone;
    private SmartObject vaultFrontZone;

    protected override Node Root(
        Token token,
        SmartCharacter user,
        SmartVaultDoor vault)
    {
        return new Sequence(
            ReusableActions.OpenVault(user, vault),
            user.Node_Set(this.userZone.Id, ~RelationName.IsInZone),
            user.Node_Set(this.vaultFrontZone.Id, RelationName.IsInZone));
    }

    [Name("OpenVault")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsImmobile, ~StateName.IsIncapacitated)]
    [StateRequired(1, StateName.RoleVaultDoor, StateName.TellerButtonPressed, StateName.ManagerButtonPressed, ~StateName.IsOpen)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.FrontZone)]

    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(1, StateName.IsOpen, StateName.IsUnlocked)]
    
	[RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]
	[RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]

    [IsImplicit(1)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public OpenVault(
        SmartCharacter user,
        SmartVaultDoor vault,
        SmartObject userZone,
        SmartObject vaultFrontZone)
        : base(user, vault)
    {
        this.userZone = userZone;
        this.vaultFrontZone = vaultFrontZone;
    }
}