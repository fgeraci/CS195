using TreeSharpPlus;

[LibraryIndex(3)]
public class AttendCounter : GenericEvent<SmartCharacter, SmartBankCounter>
{
    private SmartObject tellerZone;
    private SmartObject counterRearZone;

    protected override Node Root(
        Token token,
        SmartCharacter teller,
        SmartBankCounter counter)
    {
        return new Sequence(
            ReusableActions.AttendCounter(teller, counter),
            teller.Node_Set(this.tellerZone.Id, ~RelationName.IsInZone),
            teller.Node_Set(this.counterRearZone.Id, RelationName.IsInZone));
    }

    [Name("AttendCounter")]
    [StateRequired(0, StateName.RoleTeller, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
    [StateRequired(1, StateName.RoleBankCounter, ~StateName.IsOccupied)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.RearZone)]

    [RuleRequired(0, 3, RuleName.CanAccessZone)]

    [StateEffect(0, StateName.IsImmobile)]
    [StateEffect(1, StateName.IsOccupied)]
    [RelationEffect(0, 1, RelationName.IsAttending)]
	[RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public AttendCounter(
        SmartCharacter teller,
        SmartBankCounter counter,
        SmartObject tellerZone,
        SmartObject counterRearZone)
        : base(teller, counter)
    {
        this.tellerZone = tellerZone;
        this.counterRearZone = counterRearZone;
    }
}

[LibraryIndex(3)]
public class LeaveCounter : GenericEvent<SmartCharacter, SmartBankCounter>
{
    protected override Node Root(
        Token token,
        SmartCharacter teller,
        SmartBankCounter counter)
    {
        return ReusableActions.LeaveCounter(teller, counter);
    }

    [Name("LeaveCounter")]
    [StateRequired(0, StateName.RoleActor, StateName.IsImmobile)]
    [StateRequired(1, StateName.RoleBankCounter, StateName.IsOccupied)]

    [RelationRequired(0, 1, RelationName.IsAttending)]

    [StateEffect(0, ~StateName.IsImmobile)]
    [StateEffect(1, ~StateName.IsOccupied)]
    [RelationEffect(0, 1, ~RelationName.IsAttending)]

    [IsImplicit(1)]
    public LeaveCounter(
        SmartCharacter teller,
        SmartBankCounter counter)
        : base(teller, counter) { }
}