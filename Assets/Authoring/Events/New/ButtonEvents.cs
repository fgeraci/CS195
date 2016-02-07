using TreeSharpPlus;

// SPECIAL CASE TODOs:
//      - Character pressing a button he/she is currently guarding

[LibraryIndex(3)]
public class PressTellerButton : GenericEvent<SmartCharacter, SmartButton, SmartVaultDoor>
{
    private SmartObject actorZone;
    private SmartObject buttonZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartButton button,
        SmartVaultDoor vault)
    {
        return new Sequence(
            ReusableActions.PressButton(actor, button),
            actor.Node_Set(this.actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(this.buttonZone.Id, RelationName.IsInZone),
            vault.Node_Set(StateName.TellerButtonPressed));
    }

    [Name("PressButton")]
    [IconName("Evnt_ButtonPress")]
    [Merge(typeof(PressManagerButton))]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
    [StateRequired(1, StateName.RoleTellerButton)]
    [StateRequired(2, StateName.RoleVaultDoor)]
    [StateRequired(3, StateName.RoleZone)]
    [StateRequired(4, StateName.RoleZone)]

    [RelationRequired(0, 3, RelationName.IsInZone)]
    [RelationRequired(1, 4, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject, RuleName.CanManipulateObject)]

    [StateEffect(2, StateName.TellerButtonPressed)]

    [RelationEffect(0, 3, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(0, 4, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(3, 4)]
    [NonParticipant(3, 4)]
    public PressTellerButton(
        SmartCharacter actor,
        SmartButton button,
        SmartVaultDoor vault,
        SmartObject actorZone,
        SmartObject buttonZone)
        : base(actor, button, vault)
    {
        this.actorZone = actorZone;
        this.buttonZone = buttonZone;
    }
}

[LibraryIndex(3)]
public class PressManagerButton : GenericEvent<SmartCharacter, SmartButton, SmartVaultDoor>
{
    private SmartObject actorZone;
    private SmartObject buttonZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartButton button,
        SmartVaultDoor vault)
    {
        return new Sequence(
            ReusableActions.PressButton(actor, button),
            actor.Node_Set(this.actorZone.Id, ~RelationName.IsInZone),
            actor.Node_Set(this.buttonZone.Id, RelationName.IsInZone),
            vault.Node_Set(StateName.ManagerButtonPressed));
    }

    [Name("PressManagerButton")]
    [MergeAt(typeof(PressTellerButton))]

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
    [StateRequired(1, StateName.RoleManagerButton)]
    [StateRequired(2, StateName.RoleVaultDoor)]
    [StateRequired(3, StateName.RoleZone)]
    [StateRequired(4, StateName.RoleZone)]

    [RelationRequired(0, 3, RelationName.IsInZone)]
    [RelationRequired(1, 4, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject, RuleName.CanManipulateObject)]

    [StateEffect(2, StateName.ManagerButtonPressed)]

	[RelationEffect(0, 4, RelationName.IsInZone, Order = 2)]
	[RelationEffect(0, 3, ~RelationName.IsInZone, Order = 1)]
   

    [IsImplicit(2)]
    [CanEqual(3, 4)]
    [NonParticipant(3, 4)]
    public PressManagerButton(
        SmartCharacter actor,
        SmartButton button,
        SmartVaultDoor vault,
        SmartObject actorZone,
        SmartObject buttonZone)
        : base(actor, button, vault)
    {
        this.actorZone = actorZone;
        this.buttonZone = buttonZone;
    }
}