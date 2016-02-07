using TreeSharpPlus;

[LibraryIndex(3)]
public class BreakAlliance : GenericEvent<SmartCharacter, SmartCharacter>
{
    protected override Node Root(
        Token token,
        SmartCharacter char1,
        SmartCharacter char2)
    {
        return new Sequence(
            ReusableActions.BreakAlliance(char1, char2));
    }

    [Name("BreakAlliance")]
    [StateRequired(0, StateName.RoleActor, StateName.CanBreakAlliances)]
    [StateRequired(1, StateName.RoleActor, StateName.CanBreakAlliances)]
    [RelationRequired(0, 1, RelationName.IsAlliedWith)]

    [RelationEffect(0, 1, ~RelationName.IsAlliedWith)]
    [RelationEffect(1, 0, ~RelationName.IsAlliedWith)]
    public BreakAlliance(
        SmartCharacter char1,
        SmartCharacter char2)
        : base(char1, char2) { }
}

[LibraryIndex(3)]
public class IncapacitateStealthily : GenericEvent<SmartCharacter, SmartCharacter>
{
    private SmartObject actorZone;
    private SmartObject targetZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartCharacter target)
    {
        return new Sequence(
            ReusableActions.IncapacitateFromBehind(actor, target),
            ReusableActions.MoveZones(actor, this.actorZone, this.targetZone));
    }

    [Name("IncapacitateStealthily")]
    [IconName("Evnt_Incapacitate")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]

    [StateEffect(1, StateName.IsIncapacitated)]
    [RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public IncapacitateStealthily(
        SmartCharacter actor,
        SmartCharacter target,
        SmartObject actorZone,
        SmartObject targetZone)
        : base(actor, target)
    {
        this.actorZone = actorZone;
        this.targetZone = targetZone;
    }
}

/// <summary>
/// The first two characters distract the third character and incapacitate him.
/// </summary>
[LibraryIndex(3)]
public class DistractAndIncapacitate : GenericEvent<SmartCharacter, SmartCharacter, SmartCharacter>
{
    private SmartObject distractorZone;
    private SmartObject targetZone;
    private SmartObject aggressorZone;

    protected override Node Root(
        Token token,
        SmartCharacter distractor,
        SmartCharacter target,
        SmartCharacter aggressor)
    {
        return new Sequence(
            ReusableActions.DistractAndIncapacitate(distractor, aggressor, target),
            ReusableActions.MoveZones(distractor, this.distractorZone, this.targetZone),
            ReusableActions.MoveZones(aggressor, this.aggressorZone, this.targetZone));
    }

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
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
	[RelationEffect(0, 3, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(2, 5, ~RelationName.IsInZone, Order = 2)]
	[RelationEffect(0, 4, RelationName.IsInZone, Order = 3)]
	[RelationEffect(2, 4, RelationName.IsInZone, Order = 4)]

    [CanEqual(3, 4)]
    [CanEqual(3, 5)]
    [CanEqual(4, 5)]
    [IsImplicit(3)]
    [NonParticipant(3, 4, 5)]
    public DistractAndIncapacitate(
        SmartCharacter distractor,
        SmartCharacter target,
        SmartCharacter aggressor,
        SmartObject distractorZone,
        SmartObject targetZone,
        SmartObject aggressorZone)
        : base(distractor, target, aggressor)
    {
        this.distractorZone = distractorZone;
        this.targetZone = targetZone;
        this.aggressorZone = aggressorZone;
    }
}

[LibraryIndex(3)]
public class GiveBriefcase : GenericEvent<SmartCharacter, SmartCharacter>
{
    private SmartObject actorZone;
    private SmartObject recipientZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartCharacter recipient)
    {
        return new Sequence(
            ReusableActions.GiveBriefcase(actor, recipient),
            ReusableActions.MoveZones(actor, this.actorZone, this.recipientZone));
    }

    [Name("GiveBriefcase")]
    [IconName("Evnt_GiveStolenMoney")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasBackpack)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, ~StateName.HasBackpack)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 1, RelationName.IsAlliedWith)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]

    [StateEffect(0, ~StateName.HasBackpack)]
    [StateEffect(1, StateName.HasBackpack)]
	[RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public GiveBriefcase(
        SmartCharacter actor,
        SmartCharacter recipient,
        SmartObject actorZone,
        SmartObject recipientZone)
        : base(actor, recipient)
    {
        this.actorZone = actorZone;
        this.recipientZone = recipientZone;
    }
}

[LibraryIndex(3)]
public class TakeWeaponFromIncapacitated : GenericEvent<SmartCharacter, SmartCharacter>
{
    private SmartObject takerZone;
    private SmartObject targetZone;

    protected override Node Root(
        Token token,
        SmartCharacter taker,
        SmartCharacter target)
    {
        return new Sequence(
            ReusableActions.TakeWeaponIncapacitated(taker, target),
            ReusableActions.MoveZones(taker, this.takerZone, this.targetZone));
    }

    [Name("TakeWeaponFromIncapacitated")]
    [IconName("Evnt_TakeWeapon")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, ~StateName.RightHandOccupied)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, StateName.IsIncapacitated, StateName.HoldingWeapon)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]

    [StateEffect(0, StateName.RightHandOccupied, StateName.HoldingWeapon)]
    [StateEffect(1, ~StateName.RightHandOccupied, ~StateName.HoldingWeapon)]
	[RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]
	[RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]

    [IsImplicit(2)]
    [CanEqual(2, 3)]
    [NonParticipant(2, 3)]
    public TakeWeaponFromIncapacitated(
        SmartCharacter taker,
        SmartCharacter target,
        SmartObject takerZone,
        SmartObject targetZone)
        : base(taker, target)
    {
        this.takerZone = takerZone;
        this.targetZone = targetZone;
    }
}

[LibraryIndex(3)]
public class TakeKeysFromIncapacitated : GenericEvent<SmartCharacter, SmartCharacter>
{
    private SmartObject takerZone;
    private SmartObject targetZone;

    protected override Node Root(
        Token token,
        SmartCharacter taker,
        SmartCharacter target)
    {
        return new Sequence(
            ReusableActions.TakeKeysIncapacitated(taker, target),
            ReusableActions.MoveZones(taker, this.takerZone, this.targetZone));
    }

    [Name("TakeKeysFromIncapacitated")]
    [IconName("Evnt_TakeKey")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, StateName.IsIncapacitated, StateName.HasKeys)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

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
    public TakeKeysFromIncapacitated(
        SmartCharacter taker,
        SmartCharacter target,
        SmartObject takerZone,
        SmartObject targetZone)
        : base(taker, target)
    {
        this.takerZone = takerZone;
        this.targetZone = targetZone;
    }
}

[LibraryIndex(3)]
public class TakeKeysStealthily : GenericEvent<SmartCharacter, SmartCharacter>
{
    private SmartObject actorZone;
    private SmartObject targetZone;

    protected override Node Root(
        Token token,
        SmartCharacter actor,
        SmartCharacter target)
    {
        return new Sequence(
            ReusableActions.TakeKeysFromBehind(actor, target),
            ReusableActions.MoveZones(actor, this.actorZone, this.targetZone));
    }

    [Name("TakeKeysStealthily")]
    [IconName("Evnt_StealKey")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
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
    public TakeKeysStealthily(
        SmartCharacter actor,
        SmartCharacter target,
        SmartObject actorZone,
        SmartObject targetZone)
        : base(actor, target)
    {
        this.actorZone = actorZone;
        this.targetZone = targetZone;
    }
}


[LibraryIndex(3)]
public class Escape : GenericEvent<SmartCharacter>
{
    SmartWaypoint waypoint;

    protected override Node Root(
        Token token,
        SmartCharacter character)
    {
        SteeringController steering = 
            character.GetComponent<SteeringController>();

        return new Sequence(
            new LeafInvoke(() => steering.maxSpeed = 5.5f),
            new LeafInvoke(() => character.Character.Body.NavGoTo(this.waypoint.transform.position)));
    }

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
    [StateRequired(1, StateName.RoleWaypoint, StateName.RoleGuard1)]

    [IsImplicit(1)]
    [NonParticipant(1)]
    public Escape(
        SmartCharacter character, 
        SmartWaypoint waypoint)
        : base(character)
    {
        this.waypoint = waypoint;
    }
}

/// <summary>
/// Secretive conversation between two characters.
/// </summary>
[LibraryIndex(3)]
public class TalkSecretivelyRobbers : GenericEvent<SmartCharacter, SmartCharacter>
{
    private SmartObject char1Zone;

    private SmartObject char2Zone;

    protected override Node Root(Token token, SmartCharacter char1, SmartCharacter char2)
    {
        return new Sequence(
            new Race(
                char2.Node_OrientTowards(Val.V(() => char1.transform.position)),
                new LeafWait(2000)),
            new LeafAffordance("TalkSecretively", char1, char2),
            ReusableActions.MoveZones(char1, char1Zone, char2Zone));
    }

    [Name("TalkSecretively")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated)]
    [StateRequired(2, StateName.RoleZone)]
    [StateRequired(3, StateName.RoleZone)]

    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 3, RelationName.IsInZone)]

    [RuleRequired(0, 1, RuleName.CanAccessObject)]

    [RelationEffect(0, 2, ~RelationName.IsInZone, Order = 1)]
    [RelationEffect(0, 3, RelationName.IsInZone, Order = 2)]

    [CanEqual(2, 3)]
    [IsImplicit(2)]
    [NonParticipant(2, 3)]
    public TalkSecretivelyRobbers(SmartCharacter char1, SmartCharacter char2, SmartObject char1Zone, SmartObject char2Zone)
        : base(char1, char2)
    {
        this.char1Zone = char1Zone;
        this.char2Zone = char2Zone;
    }
}

[LibraryIndex(3)]
public class StartDuck : GenericEvent<SmartCharacter>
{
    protected override Node Root(Token token, SmartCharacter character)
    {
        return new Sequence(
            new LeafWait(2000),
            character.Behavior.Node_BodyAnimation("duck", true));
    }

    [StateRequired(0, StateName.RoleActor)]
    public StartDuck(SmartCharacter character)
        : base(character) { }
}

[LibraryIndex(3)]
public class StopDuck : GenericEvent<SmartCharacter>
{
    protected override Node Root(Token token, SmartCharacter character)
    {
        return character.Behavior.Node_BodyAnimation("duck", false);
    }

    [StateRequired(0, StateName.RoleActor)]
    public StopDuck(SmartCharacter character)
        : base(character) { }
}
