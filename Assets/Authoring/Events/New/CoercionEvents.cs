using TreeSharpPlus;

[LibraryIndex(3)]
public class CoerceIntoGivingKey : GenericEvent<SmartCharacter, SmartCharacter>
{
    private SmartObject aggressorZone;
    private SmartObject targetZone;

    protected override Node Root(
        Token token,
        SmartCharacter aggressor,
        SmartCharacter target)
    {
        return new Sequence(
            ReusableActions.CoerceIntoGivingKey(aggressor, target),
            ReusableActions.MoveZones(aggressor, aggressorZone, targetZone));
    }

    [Name("CoerceIntoGivingKey")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]

    [StateEffect(0, StateName.HasKeys)]
    [StateEffect(1, ~StateName.HasKeys)]
    
	[RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]
    [RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public CoerceIntoGivingKey(
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartObject aggressorZone,
        SmartObject targetZone)
        : base(aggressor, target)
    {
        this.aggressorZone = aggressorZone;
        this.targetZone = targetZone;
    }
}

[LibraryIndex(3)]
public class CoerceIntoPressingTellerButton : GenericEvent<SmartCharacter, SmartCharacter, SmartButton>
{
    private SmartVaultDoor vault;
    private SmartObject aggressorZone;
    private SmartObject targetbuttonZone;

    protected override Node Root(
        Token token,
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartButton button)
    {
        return new Sequence(
            ReusableActions.CoerceIntoPressingButton(aggressor, target, button),
            aggressor.Node_Set(this.aggressorZone.Id, ~RelationName.IsInZone),
            aggressor.Node_Set(this.targetbuttonZone.Id, RelationName.IsInZone),
            this.vault.Node_Set(StateName.TellerButtonPressed));
    }

    [Name("CoerceIntoPressingButton")]
    [Merge(typeof(CoerceIntoPressingManagerButton))]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
    [StateRequired(2, StateName.RoleTellerButton)]
    [StateRequired(3, StateName.RoleVaultDoor)]
    [StateRequired(4, StateName.RoleZone)]
    [StateRequired(5, StateName.RoleZone)]

    [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

    // Target and Button have to be in the same zone
    [RelationRequired(0, 4, RelationName.IsInZone)]
    [RelationRequired(1, 5, RelationName.IsInZone)]
    [RelationRequired(2, 5, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]
    [RuleRequired(1, 2, RuleName.CanManipulateObject)]

    [StateEffect(3, StateName.TellerButtonPressed)]
    [RelationEffect(0, 4, ~RelationName.IsInZone, Order = 1)]
    [RelationEffect(0, 5, RelationName.IsInZone, Order = 2)]

    [IsImplicit(3)]
    [CanEqual(4, 5)]
    [NonParticipant(3, 4, 5)]
    public CoerceIntoPressingTellerButton(
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartButton button,
        SmartVaultDoor vault,
        SmartObject aggressorZone,
        SmartObject targetbuttonZone)
        : base(aggressor, target, button)
    {
        this.vault = vault;
        this.aggressorZone = aggressorZone;
        this.targetbuttonZone = targetbuttonZone;
    }
}

[LibraryIndex(3)]
public class CoerceIntoPressingManagerButton : GenericEvent<SmartCharacter, SmartCharacter, SmartButton>
{
    private SmartVaultDoor vault;
    private SmartObject aggressorZone;
    private SmartObject targetbuttonZone;

    protected override Node Root(
        Token token,
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartButton button)
    {
        return new Sequence(
            ReusableActions.CoerceIntoPressingButton(aggressor, target, button),
            aggressor.Node_Set(this.aggressorZone.Id, ~RelationName.IsInZone),
            aggressor.Node_Set(this.targetbuttonZone.Id, RelationName.IsInZone),
            this.vault.Node_Set(StateName.ManagerButtonPressed));
    }

    [Name("CoerceIntoPressingManagerButton")]
    [MergeAt(typeof(CoerceIntoPressingTellerButton))]

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
    [StateRequired(2, StateName.RoleManagerButton)]
    [StateRequired(3, StateName.RoleVaultDoor)]
    [StateRequired(4, StateName.RoleZone)]
    [StateRequired(5, StateName.RoleZone)]

    [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

    // Target and Button have to be in the same zone
    [RelationRequired(0, 4, RelationName.IsInZone)]
    [RelationRequired(1, 5, RelationName.IsInZone)]
    [RelationRequired(2, 5, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]
    [RuleRequired(1, 2, RuleName.CanManipulateObject)]

    [StateEffect(3, StateName.ManagerButtonPressed)]
    
	[RelationEffect(0, 4, ~RelationName.IsInZone, Order = 1)]
    [RelationEffect(0, 5, RelationName.IsInZone, Order = 2)]

    [IsImplicit(3)]
    [CanEqual(4, 5)]
    [NonParticipant(3, 4, 5)]
    public CoerceIntoPressingManagerButton(
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartButton button,
        SmartVaultDoor vault,
        SmartObject aggressorZone,
        SmartObject targetbuttonZone)
        : base(aggressor, target, button)
    {
        this.vault = vault;
        this.aggressorZone = aggressorZone;
        this.targetbuttonZone = targetbuttonZone;
    }
}

[LibraryIndex(3)]
public class CoerceIntoUnlockDoorFront : GenericEvent<SmartCharacter, SmartCharacter, SmartDoor>
{
    private SmartObject aggressorZone;
    private SmartObject targetZonedoorFrontZone;

    protected override Node Root(
        Token token,
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartDoor door)
    {
        return new Sequence(
            ReusableActions.CoerceIntoUnlockDoorFront(aggressor, target, door),
            aggressor.Node_Set(this.aggressorZone.Id, ~RelationName.IsInZone),
            aggressor.Node_Set(this.targetZonedoorFrontZone.Id, RelationName.IsInZone));
    }

    [Name("CoerceIntoUnlockDoorFront")]
    [IconName("Evnt_CoerceIntoUnlockingDoor")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
    [StateRequired(2, StateName.RoleDoor, ~StateName.IsUnlocked)]
    [StateRequired(3, StateName.RoleZone)]
    [StateRequired(4, StateName.RoleZone)]

    [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

    // Target has to be in the door's front zone
    [RelationRequired(0, 3, RelationName.IsInZone)]
    [RelationRequired(1, 4, RelationName.IsInZone)]
    [RelationRequired(2, 4, RelationName.FrontZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]
    [RuleRequired(1, 2, RuleName.CanManipulateObject)]

    [StateEffect(2, StateName.IsUnlocked)]
    
	[RelationEffect(0, 3, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(0, 4, RelationName.IsInZone, Order = 2)]

    [IsImplicit(3)]
    [CanEqual(3, 4)]
    [NonParticipant(3, 4)]
    public CoerceIntoUnlockDoorFront(
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartDoor door,
        SmartObject aggressorZone,
        SmartObject targetZonedoorFrontZone)
        : base(aggressor, target, door)
    {
        this.aggressorZone = aggressorZone;
        this.targetZonedoorFrontZone = targetZonedoorFrontZone;
    }
}

[LibraryIndex(3)]
public class CoerceIntoUnlockDoorRear : GenericEvent<SmartCharacter, SmartCharacter, SmartDoor>
{
    private SmartObject aggressorZone;
    private SmartObject targetZonedoorRearZone;

    protected override Node Root(
        Token token,
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartDoor door)
    {
        return new Sequence(
            ReusableActions.CoerceIntoUnlockDoorRear(aggressor, target, door),
            aggressor.Node_Set(this.aggressorZone.Id, ~RelationName.IsInZone),
            aggressor.Node_Set(this.targetZonedoorRearZone.Id, RelationName.IsInZone));
    }

    [Name("CoerceIntoUnlockDoorRear")]
    [IconName("Evnt_CoerceIntoUnlockingDoor")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
    [StateRequired(2, StateName.RoleDoor)]
    [StateRequired(3, StateName.RoleZone)]
    [StateRequired(4, StateName.RoleZone)]

    [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

    // Target has to be in the door's rear zone
    [RelationRequired(0, 3, RelationName.IsInZone)]
    [RelationRequired(1, 4, RelationName.IsInZone)]
    [RelationRequired(2, 4, RelationName.RearZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]
    [RuleRequired(1, 2, RuleName.CanManipulateObject)]

    [StateEffect(2, StateName.IsUnlocked)]
	[RelationEffect(0, 4, RelationName.IsInZone, Order = 2)]
	[RelationEffect(0, 3, ~RelationName.IsInZone, Order = 1)]

    [IsImplicit(3)]
    [CanEqual(3, 4)]
    [NonParticipant(3, 4)]
    public CoerceIntoUnlockDoorRear(
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartDoor door,
        SmartObject aggressorZone,
        SmartObject targetZonedoorRearZone)
        : base(aggressor, target, door)
    {
        this.aggressorZone = aggressorZone;
        this.targetZonedoorRearZone = targetZonedoorRearZone;
    }
}

/// <summary>
/// The first two characters distract the third character and incapacitate him.
/// </summary>
[LibraryIndex(3)]
public class CoerceAndIncapacitate :
    GenericEvent<
        SmartCharacter,
        SmartCharacter,
        SmartCharacter>
{
    private SmartObject coercerZone;
    private SmartObject targetZone;
    private SmartObject aggressorZone;

    protected override Node Root(
        Token token,
        SmartCharacter coercer,
        SmartCharacter target,
        SmartCharacter aggressor)
    {
        return new Sequence(
            ReusableActions.CoerceAndIncapacitate(coercer, aggressor, target),
            coercer.Node_Set(this.coercerZone.Id, ~RelationName.IsInZone),
            coercer.Node_Set(this.targetZone.Id, RelationName.IsInZone),
            aggressor.Node_Set(this.aggressorZone.Id, ~RelationName.IsInZone),
            aggressor.Node_Set(this.targetZone.Id, RelationName.IsInZone));
    }

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
    [StateRequired(2, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(3, StateName.RoleZone)]
    [StateRequired(4, StateName.RoleZone)]
    [StateRequired(5, StateName.RoleZone)]

    [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]
    [RelationRequired(2, 1, ~RelationName.IsAlliedWith)]
    [RelationRequired(0, 2, RelationName.IsAlliedWith)]

    [RelationRequired(0, 3, RelationName.IsInZone)]
    [RelationRequired(1, 4, RelationName.IsInZone)]
    [RelationRequired(2, 5, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]
    [RuleRequired(2, 1, RuleName.CanAccessObject)]

    [StateEffect(1, StateName.IsIncapacitated)]
    
	[RelationEffect(0, 4, RelationName.IsInZone, Order = 3)]
	[RelationEffect(2, 4, RelationName.IsInZone, Order = 4)]
	[RelationEffect(0, 3, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(2, 5, ~RelationName.IsInZone, Order = 2)]
	
    [CanEqual(3, 4)]
    [CanEqual(3, 5)]
    [CanEqual(4, 5)]
    [IsImplicit(3)]
    [NonParticipant(3, 4, 5)]
    public CoerceAndIncapacitate(
        SmartCharacter coercer,
        SmartCharacter target,
        SmartCharacter aggressor,
        SmartObject coercerZone,
        SmartObject targetZone,
        SmartObject aggressorZone)
        : base(
            coercer,
            target,
            aggressor)
    {
        this.coercerZone = coercerZone;
        this.targetZone = targetZone;
        this.aggressorZone = aggressorZone;
    }
}

[LibraryIndex(3)]
public class CoerceTellerAtCounterIntoPressingButton : GenericEvent<SmartCharacter, SmartCharacter, SmartButton, SmartBankCounter>
{
    private SmartVaultDoor vault;
    private SmartObject aggressorZone;
    private SmartObject counterFrontZone;
    private SmartObject targetbuttonZone;

    protected override Node Root(
        Token token,
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartButton button,
        SmartBankCounter counter)
    {
        return new Sequence(
            ReusableActions.CoerceIntoPressingButtonAtCounter(aggressor, target, button, counter),
            aggressor.Node_Set(this.aggressorZone.Id, ~RelationName.IsInZone),
            aggressor.Node_Set(this.counterFrontZone.Id, RelationName.IsInZone),
            this.vault.Node_Set(StateName.TellerButtonPressed));
    }

    [Name("CoerceTellerAtCounterIntoPressingButton")]
    [IconName("Evnt_CoerceIntoPressingButton")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleTeller, StateName.IsStanding, ~StateName.IsIncapacitated, StateName.IsImmobile)]
    [StateRequired(2, StateName.RoleTellerButton)]
    [StateRequired(3, StateName.RoleBankCounter)]
    [StateRequired(4, StateName.RoleVaultDoor)]
    [StateRequired(5, StateName.RoleZone)]
    [StateRequired(6, StateName.RoleZone)]
    [StateRequired(7, StateName.RoleZone)]

    [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

    [RelationRequired(0, 5, RelationName.IsInZone)]
    [RelationRequired(3, 6, RelationName.FrontZone)]

    // Teller and Button have to be in the same zone
    [RelationRequired(1, 7, RelationName.IsInZone)]
    [RelationRequired(2, 7, RelationName.IsInZone)]

    [RuleRequired(0, 6, RuleName.CanAccessZone)]
    [RuleRequired(1, 2, RuleName.CanManipulateObject)]

    [StateEffect(4, StateName.TellerButtonPressed)]
    [StateEffect(1, ~StateName.IsImmobile)]
    [StateEffect(3, ~StateName.IsOccupied)]
    [RelationEffect(1, 3, ~RelationName.IsAttending)]
	[RelationEffect(0, 6, RelationName.IsInZone, Order = 2)]
	[RelationEffect(0, 5, ~RelationName.IsInZone, Order = 1)]

    [IsImplicit(3)]
    [CanEqual(5, 6)]
    [NonParticipant(4, 5, 6, 7)]
    public CoerceTellerAtCounterIntoPressingButton(
        SmartCharacter aggressor,
        SmartCharacter teller,
        SmartButton button,
        SmartBankCounter counter,
        SmartVaultDoor vault,
        SmartObject aggressorZone,
        SmartObject counterFrontZone,
        SmartObject targetbuttonZone)
        : base(aggressor, teller, button, counter)
    {
        this.vault = vault;
        this.aggressorZone = aggressorZone;
        this.counterFrontZone = counterFrontZone;
        this.targetbuttonZone = targetbuttonZone;
    }
}

[LibraryIndex(3)]
public class CoerceIntoDropBriefcase : GenericEvent<SmartCharacter, SmartCharacter, SmartContainer>
{
    private SmartObject aggressorZone;
    private SmartObject targetZone;
    private SmartObject containerZone;

    protected override Node Root(
        Token token,
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartContainer container)
    {
        return new Sequence(
            ReusableActions.CoerceIntoTeleportDropBriefcase(aggressor, target, container),
            ReusableActions.MoveZones(aggressor, this.aggressorZone, this.targetZone),
            ReusableActions.MoveZones(container, this.containerZone, this.targetZone));
    }

    [Name("CoerceIntoDropBriefcase")]
    //[IconName("Evnt_CoerceIntoDroppingMoney")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasBackpack)]
    [StateRequired(2, StateName.RoleContainer, ~StateName.IsOccupied, ~StateName.IsImmobile)]
    [StateRequired(3, StateName.RoleZone)]
    [StateRequired(4, StateName.RoleZone)]
    [StateRequired(5, StateName.RoleZone)]

    [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

    // Target has to be in the door's rear zone
    [RelationRequired(0, 3, RelationName.IsInZone)]
    [RelationRequired(1, 4, RelationName.IsInZone)]
    [RelationRequired(2, 5, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]

    [StateEffect(1, ~StateName.HasBackpack)]
    [StateEffect(2, StateName.IsOccupied, StateName.HasBackpack)]
    
    [RelationEffect(0, 4, RelationName.IsInZone)]
	[RelationEffect(0, 3, ~RelationName.IsInZone)]
	[RelationEffect(2, 4, RelationName.IsInZone, Order = 2)]
	[RelationEffect(2, 5, ~RelationName.IsInZone, Order = 1)]

    [IsImplicit(3)]
    [CanEqual(3, 4)]
    [CanEqual(3, 5)]
    [CanEqual(4, 5)]
    [NonParticipant(3, 4, 5)]
    public CoerceIntoDropBriefcase(
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartContainer container,
        SmartObject aggressorZone,
        SmartObject targetZone,
        SmartObject containerZone)
        : base(aggressor, target, container)
    {
        this.aggressorZone = aggressorZone;
        this.targetZone = targetZone;
        this.containerZone = containerZone;

    }
}

[LibraryIndex(3)]
public class CoerceIntoDropWeapon : GenericEvent<SmartCharacter, SmartCharacter, SmartContainer>
{
    private SmartObject aggressorZone;
    private SmartObject targetZone;
    private SmartObject containerZone;

    protected override Node Root(
        Token token,
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartContainer container)
    {
        return new Sequence(
            ReusableActions.CoerceIntoTeleportDropWeapon(aggressor, target, container),
            ReusableActions.MoveZones(aggressor, this.aggressorZone, this.targetZone),
            ReusableActions.MoveZones(container, this.containerZone, this.targetZone));
    }

    [Name("CoerceIntoDropWeapon")]
    [IconName("Evnt_CoerceIntoDroppingWeapon")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(2, StateName.RoleContainer, ~StateName.IsOccupied, ~StateName.IsImmobile)]
    [StateRequired(3, StateName.RoleZone)]
    [StateRequired(4, StateName.RoleZone)]
    [StateRequired(5, StateName.RoleZone)]

    [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

    // Target has to be in the door's rear zone
    [RelationRequired(0, 3, RelationName.IsInZone)]
    [RelationRequired(1, 4, RelationName.IsInZone)]
    [RelationRequired(2, 5, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]

    [StateEffect(1, ~StateName.RightHandOccupied, ~StateName.HoldingWeapon)]
    [StateEffect(2, StateName.IsOccupied, StateName.HoldingWeapon)]

	[RelationEffect(0, 4, RelationName.IsInZone, Order = 3)]
	[RelationEffect(0, 3, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(2, 4, RelationName.IsInZone, Order = 4)]
	[RelationEffect(2, 5, ~RelationName.IsInZone, Order = 2)]
  

    [IsImplicit(3)]
    [CanEqual(3, 4)]
    [CanEqual(3, 5)]
    [CanEqual(4, 5)]
    [NonParticipant(3, 4, 5)]
    public CoerceIntoDropWeapon(
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartContainer container,
        SmartObject aggressorZone,
        SmartObject targetZone,
        SmartObject containerZone)
        : base(aggressor, target, container)
    {
        this.aggressorZone = aggressorZone;
        this.targetZone = targetZone;
        this.containerZone = containerZone;
    }
}

[LibraryIndex(3)]
public class CoerceIntoSurrender : GenericEvent<SmartCharacter, SmartCharacter>
{
    private SmartObject aggressorZone;
    private SmartObject targetZone;

    protected override Node Root(
        Token token,
        SmartCharacter aggressor,
        SmartCharacter target)
    {
        return new Sequence(
            ReusableActions.CoerceIntoSurrender(aggressor, target),
            aggressor.Node_Set(StateName.IsImmobile),
            ReusableActions.MoveZones(aggressor, this.aggressorZone, this.targetZone));
    }

    [Name("CoerceIntoSurrender")]
    //[IconName("Evnt_CoerceIntoSurrender")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, ~StateName.HoldingWeapon)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]

    [StateEffect(0, StateName.IsImmobile)]
    [StateEffect(1, StateName.IsImmobile)]

    [RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]
    [RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]


    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public CoerceIntoSurrender(
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartObject aggressorZone,
        SmartObject targetZone)
        : base(aggressor, target)
    {
        this.aggressorZone = aggressorZone;
        this.targetZone = targetZone;
    }
}

[LibraryIndex(3)]
public class CoerceIntoDropWeaponSurrender : GenericEvent<SmartCharacter, SmartCharacter, SmartContainer>
{
    private SmartObject aggressorZone;
    private SmartObject targetZone;
    private SmartObject containerZone;

    protected override Node Root(
        Token token,
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartContainer container)
    {
        return new Sequence(
            ReusableActions.CoerceIntoDropWeaponAndSurrender(aggressor, target, container),
            aggressor.Node_Set(StateName.IsImmobile),
            ReusableActions.MoveZones(aggressor, this.aggressorZone, this.targetZone),
            ReusableActions.MoveZones(container, this.containerZone, this.targetZone));
    }

    [Name("CoerceIntoDropWeaponAndSurrender")]
    [IconName("Evnt_CoerceIntoDroppingWeapon")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(2, StateName.RoleContainer, ~StateName.IsOccupied, ~StateName.IsImmobile)]
    [StateRequired(3, StateName.RoleZone)]
    [StateRequired(4, StateName.RoleZone)]
    [StateRequired(5, StateName.RoleZone)]

    [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

    // Target has to be in the door's rear zone
    [RelationRequired(0, 3, RelationName.IsInZone)]
    [RelationRequired(1, 4, RelationName.IsInZone)]
    [RelationRequired(2, 5, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]

    [StateEffect(1, ~StateName.RightHandOccupied, ~StateName.HoldingWeapon, StateName.IsImmobile)]
    [StateEffect(2, StateName.IsOccupied, StateName.HoldingWeapon, StateName.IsImmobile)]

    [RelationEffect(0, 4, RelationName.IsInZone, Order = 3)]
    [RelationEffect(0, 3, ~RelationName.IsInZone, Order = 1)]
    [RelationEffect(2, 4, RelationName.IsInZone, Order = 4)]
    [RelationEffect(2, 5, ~RelationName.IsInZone, Order = 2)]


    [IsImplicit(3)]
    [CanEqual(3, 4)]
    [CanEqual(3, 5)]
    [CanEqual(4, 5)]
    [NonParticipant(3, 4, 5)]
    public CoerceIntoDropWeaponSurrender(
        SmartCharacter aggressor,
        SmartCharacter target,
        SmartContainer container,
        SmartObject aggressorZone,
        SmartObject targetZone,
        SmartObject containerZone)
        : base(aggressor, target, container)
    {
        this.aggressorZone = aggressorZone;
        this.targetZone = targetZone;
        this.containerZone = containerZone;
    }
}

[LibraryIndex(3)]
public class CoerceIntoMovingToTellerZone : GenericEvent<SmartCharacter, SmartCharacter, SmartWaypoint>
{
    private SmartObject aggressorZone;
    private SmartObject victimZone;
    private SmartObject targetZone;

    protected override Node Root(
        Token token, 
        SmartCharacter aggressor, 
        SmartCharacter victim, 
        SmartWaypoint target)
    {
        return new Sequence(
            ReusableActions.CoerceIntoMoveToTellerZone(aggressor, victim, target),
            ReusableActions.MoveZones(aggressor, this.aggressorZone, this.victimZone),
            ReusableActions.MoveZones(victim, this.victimZone, this.targetZone));
    }

    [IconName("Evnt_CoerceIntoMovingIntoRoom")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
    [StateRequired(2, StateName.RoleWaypoint, StateName.InTellerZone)]
    [StateRequired(3, StateName.RoleZone)]
    [StateRequired(4, StateName.RoleZone)]
    [StateRequired(5, StateName.RoleZone)]

    [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

    // Target has to be in the counter's rear zone
    [RelationRequired(0, 3, RelationName.IsInZone)]
    [RelationRequired(1, 4, RelationName.IsInZone)]
    [RelationRequired(2, 5, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]
    [RuleRequired(1, 2, RuleName.CanAccessObject)]

    [RelationEffect(0, 3, ~RelationName.IsInZone, Order = 1)]
    [RelationEffect(1, 4, ~RelationName.IsInZone, Order = 2)]
    [RelationEffect(0, 4, RelationName.IsInZone, Order = 3)]
    [RelationEffect(1, 5, RelationName.IsInZone, Order = 4)]

    [IsImplicit(2)]
    [CanEqual(3, 4)]
    [CanEqual(3, 5)]
    [CanEqual(4, 5)]
    [NonParticipant(2, 3, 4, 5)]
    public CoerceIntoMovingToTellerZone(
        SmartCharacter aggressor,
        SmartCharacter victim,
        SmartWaypoint target,
        SmartObject aggressorZone,
        SmartObject victimZone,
        SmartObject targetZone)
        : base(aggressor, victim, target)
    {
        this.aggressorZone = aggressorZone;
        this.victimZone = victimZone;
        this.targetZone = targetZone;
    }
}