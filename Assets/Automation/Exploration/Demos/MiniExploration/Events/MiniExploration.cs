//using TreeSharpPlus;

//[LibraryIndex(-101)]
//public class Mini_UnlockTellerDoor : GenericEvent<SmartCharacter, SmartDoor>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter actor,
//      SmartDoor door)
//  {
//    return new Sequence(
//        ReusableActions.UnlockDoorFront(actor, door));
//  }

//  [Name("Mini_UnlockTellerDoor")]
//  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
//  [StateRequired(1, StateName.RoleTellerDoor, ~StateName.IsUnlocked)]

//  [StateEffect(1, StateName.IsUnlocked)]

//  [IsImplicit(1)]
//  public Mini_UnlockTellerDoor(
//      SmartCharacter actor,
//      SmartDoor door)
//    : base(actor, door)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_UnlockManagerDoor : GenericEvent<SmartCharacter, SmartDoor>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter actor,
//      SmartDoor door)
//  {
//    return new Sequence(
//        ReusableActions.UnlockDoorFront(actor, door));
//  }

//  [Name("Mini_UnlockManagerDoor")]
//  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
//  [StateRequired(1, StateName.RoleManagerDoor, ~StateName.IsUnlocked)]

//  [StateEffect(1, StateName.IsUnlocked)]

//  [IsImplicit(1)]
//  public Mini_UnlockManagerDoor(
//      SmartCharacter actor,
//      SmartDoor door)
//    : base(actor, door)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_TellerButtonPress : GenericEvent<SmartCharacter, SmartButton, SmartDoor>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter actor,
//      SmartButton button,
//      SmartDoor tellerDoor)
//  {
//    return new Sequence(
//        ReusableActions.PressButton(actor, button),
//        button.Node_Set(StateName.IsUnlocked));
//  }

//  [Name("Mini_TellerButtonPress")]
//  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
//  [StateRequired(1, StateName.RoleTellerButton, ~StateName.IsUnlocked)]
//  [StateRequired(2, StateName.RoleTellerDoor, StateName.IsUnlocked)]

//  [StateEffect(1, StateName.IsUnlocked)]

//  [IsImplicit(1)]
//  public Mini_TellerButtonPress(
//      SmartCharacter actor,
//      SmartButton button,
//      SmartDoor tellerDoor)
//    : base(actor, button, tellerDoor)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_ManagerButtonPress : GenericEvent<SmartCharacter, SmartButton>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter actor,
//      SmartButton button)
//  {
//    return new Sequence(
//        ReusableActions.PressButton(actor, button),
//        button.Node_Set(StateName.IsUnlocked));
//  }

//  [Name("Mini_ManagerButtonPress")]
//  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, ~StateName.RoleTeller, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
//  [StateRequired(1, StateName.RoleManagerButton, ~StateName.IsUnlocked)]

//  [StateEffect(1, StateName.IsUnlocked)]

//  [IsImplicit(1)]
//  public Mini_ManagerButtonPress(
//      SmartCharacter actor,
//      SmartButton button)
//    : base(actor, button)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_OpenVaultDoor : GenericEvent<SmartCharacter, SmartVaultDoor, SmartButton, SmartButton, SmartDoor>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter actor,
//      SmartVaultDoor vaultDoor,
//      SmartButton tellerButton,
//      SmartButton managerButton,
//      SmartDoor managerDoor)
//  {
//    return new Sequence(
//        ReusableActions.OpenVault(actor, vaultDoor));
//  }

//  [Name("Mini_OpenVaultDoor")]
//  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
//  [StateRequired(1, StateName.RoleVaultDoor, ~StateName.IsUnlocked)]
//  [StateRequired(2, StateName.RoleTellerButton, StateName.IsUnlocked)]
//  [StateRequired(3, StateName.RoleManagerButton, StateName.IsUnlocked)]
//  [StateRequired(4, StateName.RoleManagerDoor, StateName.IsUnlocked)]

//  [StateEffect(1, StateName.IsUnlocked)]

//  [IsImplicit(1)]
//  public Mini_OpenVaultDoor(
//      SmartCharacter actor,
//      SmartVaultDoor vaultDoor,
//      SmartButton tellerButton,
//      SmartButton managerButton,
//      SmartDoor managerDoor)
//    : base(actor, vaultDoor, tellerButton, managerButton, managerDoor)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_CoerceIntoGivingKey : GenericEvent<SmartCharacter, SmartCharacter>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter aggressor,
//      SmartCharacter target)
//  {
//    return new Sequence(
//        ReusableActions.CoerceIntoGivingKey(aggressor, target));
//  }

//  [Name("Mini_CoerceIntoGivingKey")]
//  [Sentiment("Nonviolent")]
//  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
//  [StateRequired(1, StateName.RoleGuard, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]

//  [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

//  [StateEffect(1, ~StateName.HasKeys)]
//  [StateEffect(0, StateName.HasKeys)]

//  public Mini_CoerceIntoGivingKey(
//      SmartCharacter aggressor,
//      SmartCharacter target)
//    : base(aggressor, target)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_PickupMoneyFromCart : GenericEvent<SmartCharacter, SmartCart, SmartVaultDoor>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter actor,
//      SmartCart cart,
//      SmartVaultDoor vaultDoor)
//  {
//    return new Sequence(
//        ReusableActions.PickupMoneyFromCart(actor, cart));
//  }

//  [Name("Mini_PickupMoneyFromCart")]
//  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
//  [StateRequired(1, StateName.RoleContainer, StateName.IsImmobile, StateName.IsOccupied, StateName.HasBackpack)]
//  [StateRequired(2, StateName.RoleVaultDoor, StateName.IsUnlocked)]

//  [StateEffect(0, StateName.HasBackpack)]
//  [StateEffect(1, ~StateName.IsOccupied, ~StateName.HasBackpack)]

//  [IsImplicit(1)]
//  public Mini_PickupMoneyFromCart(
//      SmartCharacter actor,
//      SmartCart cart,
//      SmartVaultDoor vaultDoor)
//    : base(actor, cart, vaultDoor)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_DropWeapon : GenericEvent<SmartCharacter, SmartContainer>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter actor,
//      SmartContainer container)
//  {
//    return new Sequence(
//        ReusableActions.TeleportDropWeapon(actor, container));
//  }

//  [Name("Mini_DropWeapon")]
//  [Sentiment("Negligent")]
//  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
//  [StateRequired(1, StateName.RoleContainer, ~StateName.IsOccupied, ~StateName.IsImmobile)]

//  [StateEffect(0, ~StateName.RightHandOccupied, ~StateName.HoldingWeapon)]
//  [StateEffect(1, StateName.IsOccupied, StateName.HoldingWeapon)]

//  [IsImplicit(1)]
//  public Mini_DropWeapon(
//      SmartCharacter actor,
//      SmartContainer container)
//    : base(actor, container)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_PickupWeapon : GenericEvent<SmartCharacter, SmartContainer>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter actor,
//      SmartContainer container)
//  {
//    return new Sequence(
//        ReusableActions.PickupWeapon(actor, container));
//  }

//  [Name("Mini_PickupWeapon")]
//  [Sentiment("Brave")]
//  [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, ~StateName.RightHandOccupied)]
//  [StateRequired(1, StateName.RoleContainer, StateName.IsOccupied, StateName.HoldingWeapon)]

//  [StateEffect(0, StateName.RightHandOccupied, StateName.HoldingWeapon)]
//  [StateEffect(1, ~StateName.IsOccupied, ~StateName.HoldingWeapon)]

//  [IsImplicit(1)]
//  public Mini_PickupWeapon(
//      SmartCharacter actor,
//      SmartContainer container)
//    : base(actor, container)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_PickupWeaponTeller : GenericEvent<SmartCharacter, SmartContainer, SmartDoor>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter actor,
//      SmartContainer container,
//      SmartDoor door)
//  {
//    return new Sequence(
//        ReusableActions.PickupWeapon(actor, container));
//  }

//  [Name("Mini_PickupWeaponTeller")]
//  [Sentiment("Brave")]
//  [StateRequired(0, StateName.RoleActor, StateName.RoleTeller, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, ~StateName.RightHandOccupied)]
//  [StateRequired(1, StateName.RoleContainer, StateName.IsOccupied, StateName.HoldingWeapon)]
//  [StateRequired(2, StateName.RoleTellerDoor, StateName.IsUnlocked)]

//  [StateEffect(0, StateName.RightHandOccupied, StateName.HoldingWeapon)]
//  [StateEffect(1, ~StateName.IsOccupied, ~StateName.HoldingWeapon)]

//  [IsImplicit(1)]
//  public Mini_PickupWeaponTeller(
//      SmartCharacter actor,
//      SmartContainer container,
//      SmartDoor tellerDoor)
//    : base(actor, container, tellerDoor)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_IncapacitateStealthily : GenericEvent<SmartCharacter, SmartCharacter, SmartDoor>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter actor,
//      SmartCharacter target,
//      SmartDoor tellerDoor)
//  {
//    return new Sequence(
//        ReusableActions.IncapacitateFromBehind(actor, target));
//  }

//  [Name("Mini_IncapacitateStealthily")]
//  [Sentiment("Violent")]
//  [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
//  [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
//  [StateRequired(2, StateName.RoleTellerDoor, StateName.IsUnlocked)]

//  [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

//  [StateEffect(1, StateName.IsIncapacitated)]

//  [IsImplicit(2)]
//  public Mini_IncapacitateStealthily(
//      SmartCharacter actor,
//      SmartCharacter target,
//      SmartDoor tellerDoor)
//    : base(actor, target, tellerDoor)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_IncapacitateStealthilySpecial : GenericEvent<SmartCharacter, SmartCharacter>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter actor,
//      SmartCharacter target)
//  {
//    return new Sequence(
//        ReusableActions.IncapacitateFromBehind(actor, target));
//  }

//  [Name("Mini_IncapacitateStealthilySpecial")]
//  [Sentiment("Violent")]
//  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
//  [StateRequired(1, StateName.RoleActor, StateName.RoleGuard, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]

//  [StateEffect(1, StateName.IsIncapacitated)]

//  public Mini_IncapacitateStealthilySpecial(
//      SmartCharacter actor,
//      SmartCharacter target)
//    : base(actor, target)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_TakeKeysFromIncapacitated : GenericEvent<SmartCharacter, SmartCharacter>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter taker,
//      SmartCharacter target)
//  {
//    return new Sequence(
//        ReusableActions.TakeKeysIncapacitated(taker, target));
//  }

//  [Name("Mini_TakeKeysFromIncapacitated")]
//  [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
//  [StateRequired(1, StateName.RoleActor, StateName.IsStanding, StateName.IsIncapacitated, StateName.HasKeys)]

//  [StateEffect(0, StateName.HasKeys)]
//  [StateEffect(1, ~StateName.HasKeys)]

//  public Mini_TakeKeysFromIncapacitated(
//      SmartCharacter taker,
//      SmartCharacter target)
//    : base(taker, target)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_TalkTo : GenericEvent<SmartCharacter, SmartCharacter>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter char1,
//      SmartCharacter char2)
//  {
//    return new Sequence(
//        new Race(
//            char2.Node_OrientTowards(Val.V(() => char1.transform.position)),
//            new LeafWait(2000)),
//        new LeafAffordance("TalkSecretively", char1, char2),
//        char1.Node_Set(char2.Id, RelationName.IsFriendOf),
//        char2.Node_Set(char1.Id, RelationName.IsFriendOf));
//  }

//  [Name("Mini_TalkTo")]
//  [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
//  [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
//  [RelationRequired(0, 1, ~RelationName.IsFriendOf)]

//  [RelationEffect(0, 1, RelationName.IsFriendOf)]
//  [RelationEffect(1, 0, RelationName.IsFriendOf)]

//  public Mini_TalkTo(
//      SmartCharacter char1,
//      SmartCharacter char2)
//    : base(char1, char2)
//  {
//  }
//}

//[LibraryIndex(-101)]
//public class Mini_Leave : GenericEvent<SmartCharacter, SmartWaypoint>
//{
//  protected override Node Root(
//      Token token,
//      SmartCharacter character,
//      SmartWaypoint waypoint)
//  {
//    SteeringController steering =
//        character.GetComponent<SteeringController>();

//    return new Sequence(
//        new LeafInvoke(() => character.Character.Body.NavGoTo(waypoint.transform.position)));
//  }

//  [Name("Mini_Leave")]
//  [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
//  [StateRequired(1, StateName.RoleWaypoint)]

//  [StateEffect(0, StateName.IsImmobile, StateName.HasLeft)]

//  [IsImplicit(1)]
//  public Mini_Leave(
//      SmartCharacter character,
//      SmartWaypoint waypoint)
//    : base(character, waypoint)
//  {
//  }
//}

//502
using TreeSharpPlus;

[LibraryIndex(-101)]
public class Mini_UnlockTellerDoor : GenericEvent<SmartCharacter, SmartDoor>
{
  protected override Node Root(
      Token token,
      SmartCharacter actor,
      SmartDoor door)
  {
    return new Sequence(
        ReusableActions.UnlockDoorFront(actor, door));
  }

  [Name("Mini_UnlockTellerDoor")]
  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
  [StateRequired(1, StateName.RoleTellerDoor, ~StateName.IsUnlocked)]

  [StateEffect(1, StateName.IsUnlocked)]

  [IsImplicit(1)]
  public Mini_UnlockTellerDoor(
      SmartCharacter actor,
      SmartDoor door)
    : base(actor, door)
  {
  }
}

[LibraryIndex(-101)]
public class Mini_CoerceIntoUnlockTellerDoor : GenericEvent<SmartCharacter, SmartCharacter, SmartDoor>
{
  protected override Node Root(
      Token token,
      SmartCharacter aggressor,
      SmartCharacter target,
      SmartDoor door)
  {
      return new Sequence(
          ReusableActions.CoerceIntoUnlockDoorFront(aggressor, target, door));
  }

  [Name("Mini_CoerceIntoUnlockTellerDoor")]
  [Sentiment("Nonviolent")]
  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
  [StateRequired(1, StateName.RoleGuard, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
  [StateRequired(2, StateName.RoleTellerDoor, ~StateName.IsUnlocked)]

  [StateEffect(2, StateName.IsUnlocked)]

  [IsImplicit(2)]
  public Mini_CoerceIntoUnlockTellerDoor(
      SmartCharacter aggressor,
      SmartCharacter target,
      SmartDoor door)
    : base(aggressor, target, door)
  {
  }
}

[LibraryIndex(-101)]
public class Mini_CoerceIntoTellerButtonPress : GenericEvent<SmartCharacter, SmartCharacter, SmartDoor, SmartButton>
{
  protected override Node Root(
      Token token,
      SmartCharacter aggressor,
      SmartCharacter target,
      SmartDoor door,
      SmartButton button)
  {
    return new Sequence(
        ReusableActions.CoerceIntoPressingButton(aggressor, target, button));
  }


  [Name("Mini_CoerceIntoTellerButtonPress")]
  [Sentiment("Nonviolent")]
  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
  [StateRequired(1, StateName.RoleGuard, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
  [StateRequired(2, StateName.RoleTellerDoor, StateName.IsUnlocked)]
  [StateRequired(3, StateName.RoleTellerButton, ~StateName.IsUnlocked)]

  [StateEffect(3, StateName.IsUnlocked)]

  [IsImplicit(2)]
  public Mini_CoerceIntoTellerButtonPress(
      SmartCharacter aggressor,
      SmartCharacter target,
      SmartDoor door,
      SmartButton button)
    : base(aggressor, target, door, button)
  {
  }
}

[LibraryIndex(-101)]
public class Mini_UnlockManagerDoor : GenericEvent<SmartCharacter, SmartDoor>
{
  protected override Node Root(
      Token token,
      SmartCharacter actor,
      SmartDoor door)
  {
    return new Sequence(
        ReusableActions.UnlockDoorFront(actor, door));
  }

  [Name("Mini_UnlockManagerDoor")]
  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]
  [StateRequired(1, StateName.RoleManagerDoor, ~StateName.IsUnlocked)]

  [StateEffect(1, StateName.IsUnlocked)]

  [IsImplicit(1)]
  public Mini_UnlockManagerDoor(
      SmartCharacter actor,
      SmartDoor door)
    : base(actor, door)
  {
  }
}

[LibraryIndex(-101)]
public class Mini_TellerButtonPress : GenericEvent<SmartCharacter, SmartButton, SmartDoor>
{
  protected override Node Root(
      Token token,
      SmartCharacter actor,
      SmartButton button,
      SmartDoor tellerDoor)
  {
    return new Sequence(
        ReusableActions.PressButton(actor, button),
        button.Node_Set(StateName.IsUnlocked));
  }

  [Name("Mini_TellerButtonPress")]
  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
  [StateRequired(1, StateName.RoleTellerButton, ~StateName.IsUnlocked)]
  [StateRequired(2, StateName.RoleTellerDoor, StateName.IsUnlocked)]

  [StateEffect(1, StateName.IsUnlocked)]

  [IsImplicit(1)]
  public Mini_TellerButtonPress(
      SmartCharacter actor,
      SmartButton button,
      SmartDoor tellerDoor)
    : base(actor, button, tellerDoor)
  {
  }
}

[LibraryIndex(-101)]
public class Mini_ManagerButtonPress : GenericEvent<SmartCharacter, SmartButton>
{
  protected override Node Root(
      Token token,
      SmartCharacter actor,
      SmartButton button)
  {
    return new Sequence(
        ReusableActions.PressButton(actor, button),
        button.Node_Set(StateName.IsUnlocked));
  }

  [Name("Mini_ManagerButtonPress")]
  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, ~StateName.RoleTeller, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
  [StateRequired(1, StateName.RoleManagerButton, ~StateName.IsUnlocked)]

  [StateEffect(1, StateName.IsUnlocked)]

  [IsImplicit(1)]
  public Mini_ManagerButtonPress(
      SmartCharacter actor,
      SmartButton button)
    : base(actor, button)
  {
  }
}

[LibraryIndex(-101)]
public class Mini_OpenVaultDoor : GenericEvent<SmartCharacter, SmartVaultDoor, SmartButton, SmartButton, SmartDoor>
{
  protected override Node Root(
      Token token,
      SmartCharacter actor,
      SmartVaultDoor vaultDoor,
      SmartButton tellerButton,
      SmartButton managerButton,
      SmartDoor managerDoor)
  {
    return new Sequence(
        ReusableActions.OpenVault(actor, vaultDoor));
  }

  [Name("Mini_OpenVaultDoor")]
  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
  [StateRequired(1, StateName.RoleVaultDoor, ~StateName.IsUnlocked)]
  [StateRequired(2, StateName.RoleTellerButton, StateName.IsUnlocked)]
  [StateRequired(3, StateName.RoleManagerButton, StateName.IsUnlocked)]
  [StateRequired(4, StateName.RoleManagerDoor, StateName.IsUnlocked)]

  [StateEffect(1, StateName.IsUnlocked)]

  [IsImplicit(1)]
  public Mini_OpenVaultDoor(
      SmartCharacter actor,
      SmartVaultDoor vaultDoor,
      SmartButton tellerButton,
      SmartButton managerButton,
      SmartDoor managerDoor)
    : base(actor, vaultDoor, tellerButton, managerButton, managerDoor)
  {
  }
}

[LibraryIndex(-101)]
public class Mini_CoerceIntoGivingKey : GenericEvent<SmartCharacter, SmartCharacter>
{
  protected override Node Root(
      Token token,
      SmartCharacter aggressor,
      SmartCharacter target)
  {
    return new Sequence(
        ReusableActions.CoerceIntoGivingKey(aggressor, target));
  }

  [Name("Mini_CoerceIntoGivingKey")]
  [Sentiment("Nonviolent")]
  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
  [StateRequired(1, StateName.RoleGuard, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HasKeys)]

  [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

  [StateEffect(1, ~StateName.HasKeys)]
  [StateEffect(0, StateName.HasKeys)]

  public Mini_CoerceIntoGivingKey(
      SmartCharacter aggressor,
      SmartCharacter target)
    : base(aggressor, target)
  {
  }
}

[LibraryIndex(-101)]
public class Mini_PickupMoneyFromCart : GenericEvent<SmartCharacter, SmartCart, SmartVaultDoor>
{
  protected override Node Root(
      Token token,
      SmartCharacter actor,
      SmartCart cart,
      SmartVaultDoor vaultDoor)
  {
    return new Sequence(
        ReusableActions.PickupMoneyFromCart(actor, cart));
  }

  [Name("Mini_PickupMoneyFromCart")]
  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
  [StateRequired(1, StateName.RoleContainer, StateName.IsImmobile, StateName.IsOccupied, StateName.HasBackpack)]
  [StateRequired(2, StateName.RoleVaultDoor, StateName.IsUnlocked)]

  [StateEffect(0, StateName.HasBackpack)]
  [StateEffect(1, ~StateName.IsOccupied, ~StateName.HasBackpack)]

  [IsImplicit(1)]
  public Mini_PickupMoneyFromCart(
      SmartCharacter actor,
      SmartCart cart,
      SmartVaultDoor vaultDoor)
    : base(actor, cart, vaultDoor)
  {
  }
}

[LibraryIndex(-101)]
public class Mini_DropWeapon : GenericEvent<SmartCharacter, SmartContainer>
{
  protected override Node Root(
      Token token,
      SmartCharacter actor,
      SmartContainer container)
  {
    return new Sequence(
        ReusableActions.TeleportDropWeapon(actor, container));
  }

  [Name("Mini_DropWeapon")]
  [Sentiment("Negligent")]
  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
  [StateRequired(1, StateName.RoleContainer, ~StateName.IsOccupied, ~StateName.IsImmobile)]

  [StateEffect(0, ~StateName.RightHandOccupied, ~StateName.HoldingWeapon)]
  [StateEffect(1, StateName.IsOccupied, StateName.HoldingWeapon)]

  [IsImplicit(1)]
  public Mini_DropWeapon(
      SmartCharacter actor,
      SmartContainer container)
    : base(actor, container)
  {
  }
}

//[LibraryIndex(-101)]
//public class Mini_PickupWeapon : GenericEvent<SmartCharacter, SmartContainer>
//{
//    protected override Node Root(
//        Token token,
//        SmartCharacter actor,
//        SmartContainer container)
//    {
//        return new Sequence(
//            ReusableActions.PickupWeapon(actor, container));
//    }

//    [Name("Mini_PickupWeapon")]
//    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, ~StateName.RightHandOccupied)]
//    [StateRequired(1, StateName.RoleContainer, StateName.IsOccupied, StateName.HoldingWeapon)]

//    [StateEffect(0, StateName.RightHandOccupied, StateName.HoldingWeapon)]
//    [StateEffect(1, ~StateName.IsOccupied, ~StateName.HoldingWeapon)]

//    [IsImplicit(1)]
//    public Mini_PickupWeapon(
//        SmartCharacter actor,
//        SmartContainer container)
//        : base(actor, container)
//    {
//    }
//}

[LibraryIndex(-101)]
public class Mini_PickupWeaponTeller : GenericEvent<SmartCharacter, SmartContainer, SmartDoor>
{
  protected override Node Root(
      Token token,
      SmartCharacter actor,
      SmartContainer container,
      SmartDoor door)
  {
    return new Sequence(
        ReusableActions.PickupWeapon(actor, container));
  }

  [Name("Mini_PickupWeaponTeller")]
  [Sentiment("Brave")]
  [StateRequired(0, StateName.RoleActor, StateName.RoleTeller, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, ~StateName.RightHandOccupied)]
  [StateRequired(1, StateName.RoleContainer, StateName.IsOccupied, StateName.HoldingWeapon)]
  [StateRequired(2, StateName.RoleTellerDoor, StateName.IsUnlocked)]

  [StateEffect(0, StateName.RightHandOccupied, StateName.HoldingWeapon)]
  [StateEffect(1, ~StateName.IsOccupied, ~StateName.HoldingWeapon)]

  [IsImplicit(1)]
  public Mini_PickupWeaponTeller(
      SmartCharacter actor,
      SmartContainer container,
      SmartDoor tellerDoor)
    : base(actor, container, tellerDoor)
  {
  }
}

[LibraryIndex(-101)]
public class Mini_IncapacitateStealthily : GenericEvent<SmartCharacter, SmartCharacter, SmartDoor>
{
  protected override Node Root(
      Token token,
      SmartCharacter actor,
      SmartCharacter target,
      SmartDoor tellerDoor)
  {
    return new Sequence(
        ReusableActions.IncapacitateFromBehind(actor, target));
  }

  [Name("Mini_IncapacitateStealthily")]
  [Sentiment("Violent")]
  [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
  [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
  [StateRequired(2, StateName.RoleTellerDoor, StateName.IsUnlocked)]

  [RelationRequired(0, 1, ~RelationName.IsAlliedWith)]

  [StateEffect(1, StateName.IsIncapacitated)]

  [IsImplicit(2)]
  public Mini_IncapacitateStealthily(
      SmartCharacter actor,
      SmartCharacter target,
      SmartDoor tellerDoor)
    : base(actor, target, tellerDoor)
  {
  }
}

[LibraryIndex(-101)]
public class Mini_IncapacitateStealthilySpecial : GenericEvent<SmartCharacter, SmartCharacter>
{
  protected override Node Root(
      Token token,
      SmartCharacter actor,
      SmartCharacter target)
  {
    return new Sequence(
        ReusableActions.IncapacitateFromBehind(actor, target));
  }

  [Name("Mini_IncapacitateStealthilySpecial")]
  [Sentiment("Violent")]
  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile, StateName.HoldingWeapon)]
  [StateRequired(1, StateName.RoleActor, StateName.RoleGuard, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]

  [StateEffect(1, StateName.IsIncapacitated)]

  public Mini_IncapacitateStealthilySpecial(
      SmartCharacter actor,
      SmartCharacter target)
    : base(actor, target)
  {
  }
}

[LibraryIndex(-101)]
public class Mini_TakeKeysFromIncapacitated : GenericEvent<SmartCharacter, SmartCharacter>
{
  protected override Node Root(
      Token token,
      SmartCharacter taker,
      SmartCharacter target)
  {
    return new Sequence(
        ReusableActions.TakeKeysIncapacitated(taker, target));
  }

  [Name("Mini_TakeKeysFromIncapacitated")]
  [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
  [StateRequired(1, StateName.RoleActor, StateName.IsStanding, StateName.IsIncapacitated, StateName.HasKeys)]

  [StateEffect(0, StateName.HasKeys)]
  [StateEffect(1, ~StateName.HasKeys)]

  public Mini_TakeKeysFromIncapacitated(
      SmartCharacter taker,
      SmartCharacter target)
    : base(taker, target)
  {
  }
}

//[LibraryIndex(-101)]
//public class Mini_TalkTo : GenericEvent<SmartCharacter, SmartCharacter>
//{
//    protected override Node Root(
//        Token token,
//        SmartCharacter char1,
//        SmartCharacter char2)
//    {
//        return new Sequence(
//            new Race(
//                char2.Node_OrientTowards(Val.V(() => char1.transform.position)),
//                new LeafWait(2000)),
//            new LeafAffordance("TalkSecretively", char1, char2),
//            char1.Node_Set(char2.Id, RelationName.IsFriendOf),
//            char2.Node_Set(char1.Id, RelationName.IsFriendOf));
//    }

//    [Name("Mini_TalkTo")]
//    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
//    [StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
//    [RelationRequired(0, 1, ~RelationName.IsFriendOf)]

//    [RelationEffect(0, 1, RelationName.IsFriendOf)]
//    [RelationEffect(1, 0, RelationName.IsFriendOf)]

//    public Mini_TalkTo(
//        SmartCharacter char1,
//        SmartCharacter char2)
//        : base(char1, char2)
//    {
//    }
//}

[LibraryIndex(-101)]
public class Mini_Leave : GenericEvent<SmartCharacter, SmartWaypoint>
{
  protected override Node Root(
      Token token,
      SmartCharacter character,
      SmartWaypoint waypoint)
  {
    SteeringController steering =
        character.GetComponent<SteeringController>();

    return new Sequence(
        new LeafInvoke(() => character.Character.Body.NavGoTo(waypoint.transform.position)));
  }

  [Name("Mini_Leave")]
  [StateRequired(0, StateName.RoleActor, StateName.RoleRobber, StateName.IsStanding, ~StateName.IsIncapacitated, ~StateName.IsImmobile)]
  [StateRequired(1, StateName.RoleWaypoint)]

  [StateEffect(0, StateName.IsImmobile, StateName.HasLeft)]

  [IsImplicit(1)]
  public Mini_Leave(
      SmartCharacter character,
      SmartWaypoint waypoint)
    : base(character, waypoint)
  {
  }
}