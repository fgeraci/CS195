using TreeSharpPlus;

// SPECIAL CASE TODOs:
//      - Incapacitating a character guarding a door

[LibraryIndex(3)]
public class PickupMoneyFromCart : GenericEvent<SmartCharacter, SmartCart>
{
    private SmartObject actorZone;
    private SmartObject cartZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartCart cart)
    {
        return new Sequence(
            ReusableActions.PickupMoneyFromCart(actor, cart),
            ReusableActions.MoveZones(actor, actorZone, cartZone));
    }

    [Name("PickupMoneyFromCart")]
    [IconName("Evnt_TakeStolenMoney")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
    [StateRequired(1, StateName.RoleContainer, StateName.IsImmobile, StateName.HasBackpack)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]

    [StateEffect(0, StateName.HasBackpack)]
    [StateEffect(1, ~StateName.IsOccupied, ~StateName.HasBackpack)]
	[RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public PickupMoneyFromCart(
        SmartCharacter actor,
        SmartCart cart,
        SmartObject actorZone,
        SmartObject cartZone)
        : base(actor, cart)
    {
        this.actorZone = actorZone;
        this.cartZone = cartZone;
    }
}

[LibraryIndex(3)]
public class PickupBackpack : GenericEvent<SmartCharacter, SmartContainer>
{
    private SmartObject actorZone;
    private SmartObject containerZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartContainer container)
    {
        return new Sequence(
            ReusableActions.PickupBriefcase(actor, container),
            ReusableActions.MoveZones(actor, actorZone, containerZone));
    }

    [Name("PickupBackpack")]
    [IconName("Evnt_TakeStolenMoney")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
    [StateRequired(1, StateName.RoleContainer, ~StateName.IsImmobile, StateName.HasBackpack)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]

    [StateEffect(0, StateName.HasBackpack)]
    [StateEffect(1, ~StateName.IsOccupied, ~StateName.HasBackpack)]
    [RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]
    [RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public PickupBackpack(
        SmartCharacter actor,
        SmartContainer container,
        SmartObject actorZone,
        SmartObject containerZone)
        : base(actor, container)
    {
        this.actorZone = actorZone;
        this.containerZone = containerZone;
    }
}

[LibraryIndex(3)]
public class DropBackpack : GenericEvent<SmartCharacter, SmartContainer>
{
    private SmartObject actorZone;
    private SmartObject containerZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartContainer container)
    {
        return new Sequence(
            ReusableActions.TeleportDropBackpack(actor, container),
            ReusableActions.MoveZones(actor, actorZone, containerZone));
    }

    [Name("DropBackpack")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasBackpack)]
    [StateRequired(1, StateName.RoleContainer, ~StateName.IsOccupied, ~StateName.IsImmobile)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.IsInZone)]

    [StateEffect(0, ~StateName.HasBackpack)]
    [StateEffect(1, StateName.IsOccupied, StateName.HasBackpack)]
	[RelationEffect(1, 2, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(1, 3, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public DropBackpack(
        SmartCharacter actor,
        SmartContainer container,
        SmartObject actorZone,
        SmartObject containerZone)
        : base(actor, container)
    {
        this.actorZone = actorZone;
        this.containerZone = containerZone;
    }
}

[LibraryIndex(3)]
public class PickupWeapon : GenericEvent<SmartCharacter, SmartContainer>
{
    private SmartObject actorZone;
    private SmartObject containerZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartContainer container)
    {
        return new Sequence(
            ReusableActions.PickupWeapon(actor, container),
            ReusableActions.MoveZones(actor, actorZone, containerZone));
    }

    [Name("PickupWeapon")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, ~StateName.RightHandOccupied)]
    [StateRequired(1, StateName.RoleContainer, StateName.IsOccupied, StateName.HoldingWeapon)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]

    [StateEffect(0, StateName.RightHandOccupied, StateName.HoldingWeapon)]
    [StateEffect(1, ~StateName.IsOccupied, ~StateName.HoldingWeapon)]
	[RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public PickupWeapon(
        SmartCharacter actor,
        SmartContainer container,
        SmartObject actorZone,
        SmartObject containerZone)
        : base(actor, container)
    {
        this.actorZone = actorZone;
        this.containerZone = containerZone;
    }
}

[LibraryIndex(3)]
public class DropWeapon : GenericEvent<SmartCharacter, SmartContainer>
{
    private SmartObject actorZone;
    private SmartObject containerZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartContainer container)
    {
        return new Sequence(
            ReusableActions.TeleportDropWeapon(actor, container),
            ReusableActions.MoveZones(actor, actorZone, containerZone));
    }

    [Name("DropWeapon")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleContainer, ~StateName.IsOccupied, ~StateName.IsImmobile)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.IsInZone)]

    [StateEffect(0, ~StateName.RightHandOccupied, ~StateName.HoldingWeapon)]
    [StateEffect(1, StateName.IsOccupied, StateName.HoldingWeapon)]
	[RelationEffect(1, 2, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(1, 3, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public DropWeapon(
        SmartCharacter actor,
        SmartContainer container,
        SmartObject actorZone,
        SmartObject containerZone)
        : base(actor, container)
    {
        this.actorZone = actorZone;
        this.containerZone = containerZone;
    }
}