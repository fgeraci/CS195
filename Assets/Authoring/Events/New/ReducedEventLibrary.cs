using TreeSharpPlus;

[LibraryIndex(4)]
public class BreakAlliance_ : GenericEvent<SmartCharacter, SmartCharacter>
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
	[StateRequired(0, StateName.RoleRobber, StateName.CanBreakAlliances)]
	[StateRequired(1, StateName.RoleRobber, StateName.CanBreakAlliances)]
	[RelationRequired(0, 1, RelationName.IsAlliedWith)]
	
	[RelationEffect(0, 1, ~RelationName.IsAlliedWith)]
	[RelationEffect(1, 0, ~RelationName.IsAlliedWith)]
	public BreakAlliance_(
		SmartCharacter char1,
		SmartCharacter char2)
	: base(char1, char2) { }
}

[LibraryIndex(4)]
public class IncapacitateStealthily_ : GenericEvent<SmartCharacter, SmartCharacter>
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
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
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
	public IncapacitateStealthily_(
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
[LibraryIndex(4)]
public class DistractAndIncapacitate_ : GenericEvent<SmartCharacter, SmartCharacter, SmartCharacter>
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
	
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
	[StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
	[StateRequired(2, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
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
	public DistractAndIncapacitate_(
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

[LibraryIndex(4)]
public class TakeKeysFromIncapacitated_ : GenericEvent<SmartCharacter, SmartCharacter>
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
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
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
	public TakeKeysFromIncapacitated_(
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

[LibraryIndex(4)]
public class Escape_ : GenericEvent<SmartCharacter>
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
	public Escape_(
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
[LibraryIndex(4)]
public class TalkSecretivelyRobbers_ : GenericEvent<SmartCharacter, SmartCharacter>
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
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
	[StateRequired(1, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated)]
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
	public TalkSecretivelyRobbers_(SmartCharacter char1, SmartCharacter char2, SmartObject char1Zone, SmartObject char2Zone)
		: base(char1, char2)
	{
		this.char1Zone = char1Zone;
		this.char2Zone = char2Zone;
	}
}
[LibraryIndex(4)]
public class UnlockDoorFront_ : GenericEvent<SmartCharacter, SmartDoor>
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
	[StateRequired(0, StateName.RoleRobber, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
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
	public UnlockDoorFront_(
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

[LibraryIndex(4)]
public class LockDoorFront_ : GenericEvent<SmartCharacter, SmartDoor>
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
	public LockDoorFront_(
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

/// <summary>
/// The character opens the vault door after both buttons have been pressed. Also takes the manager door
/// as input, which must be unlocked.
/// </summary>
[LibraryIndex(4)]
public class OpenVault_ : GenericEvent<SmartCharacter, SmartVaultDoor>
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
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsImmobile, ~StateName.IsIncapacitated)]
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
	public OpenVault_(
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

[LibraryIndex(4)]
public class PickupMoneyFromCart_ : GenericEvent<SmartCharacter, SmartCart>
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
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
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
	public PickupMoneyFromCart_(
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

[LibraryIndex(4)]
public class PickupWeapon_ : GenericEvent<SmartCharacter, SmartContainer>
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
	[StateRequired(0, StateName.RoleActor, ~StateName.RoleRobber, ~StateName.RoleGuard, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, ~StateName.RightHandOccupied)]
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
	public PickupWeapon_(
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

[LibraryIndex(4)]
public class PressTellerButton_ : GenericEvent<SmartCharacter, SmartButton, SmartVaultDoor>
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
	
	[Name("PressTellerButton")]
	[IconName("Evnt_ButtonPress")]
	[Merge(typeof(PressManagerButton))]
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
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
	public PressTellerButton_(
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

[LibraryIndex(4)]
public class PressManagerButton_ : GenericEvent<SmartCharacter, SmartButton, SmartVaultDoor>
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
	//[MergeAt(typeof(PressTellerButton_))]
	[IconName("Evnt_ButtonPress")]
	
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
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
	public PressManagerButton_(
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

[LibraryIndex(4)]
public class CoerceIntoGivingKey_ : GenericEvent<SmartCharacter, SmartCharacter>
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
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
	[StateRequired(1, StateName.RoleGuard, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
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
	public CoerceIntoGivingKey_(
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

[LibraryIndex(4)]
public class CoerceIntoPressingTellerButton_ : GenericEvent<SmartCharacter, SmartCharacter, SmartButton>
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
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
	[StateRequired(1, StateName.RoleActor, ~StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
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
	public CoerceIntoPressingTellerButton_(
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

[LibraryIndex(4)]
public class CoerceIntoPressingManagerButton_ : GenericEvent<SmartCharacter, SmartCharacter, SmartButton>
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
	
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
	[StateRequired(1, StateName.RoleActor, ~StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
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
	public CoerceIntoPressingManagerButton_(
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

[LibraryIndex(4)]
public class CoerceIntoUnlockDoorFront_ : GenericEvent<SmartCharacter, SmartCharacter, SmartDoor>
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
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
	[StateRequired(1, StateName.RoleGuard, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
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
	public CoerceIntoUnlockDoorFront_(
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

[LibraryIndex(4)]
public class CoerceTellerAtCounterIntoPressingButton_ : GenericEvent<SmartCharacter, SmartCharacter, SmartButton, SmartBankCounter>
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
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
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
	public CoerceTellerAtCounterIntoPressingButton_(
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

[LibraryIndex(4)]
public class CoerceIntoDropWeapon_ : GenericEvent<SmartCharacter, SmartCharacter, SmartContainer>
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
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
	[StateRequired(1, StateName.RoleGuard, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
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
	public CoerceIntoDropWeapon_(
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

[LibraryIndex(4)]
public class CoerceIntoSurrender_ : GenericEvent<SmartCharacter, SmartCharacter>
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
	[StateRequired(1, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, ~StateName.HoldingWeapon)]
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
	public CoerceIntoSurrender_(
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

[LibraryIndex(4)]
public class CoerceIntoDropWeaponSurrender_ : GenericEvent<SmartCharacter, SmartCharacter, SmartContainer>
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
	[StateRequired(1, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
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
	public CoerceIntoDropWeaponSurrender_(
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

[LibraryIndex(4)]
public class CoerceIntoMovingToTellerZone_ : GenericEvent<SmartCharacter, SmartCharacter, SmartWaypoint>
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
	[StateRequired(0, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
	[StateRequired(1, StateName.RoleGuard, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
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
	public CoerceIntoMovingToTellerZone_(
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