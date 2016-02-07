using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// You might be wondering why I wrote this class this horrible way. Why
/// didn't you just write a custom inspector, Alex? The answer is that
/// Unity doesn't let you bulk-edit custom inspector classes. Whereas
/// with this (hideous) approach, you can shift-select a bunch of 
/// SmartObjects at the same time and check off states for all of them.
/// Also, if you change the enum, this at least won't wipe everything.
/// 
/// Please accept my sincere apologies.
/// </summary>
[RequireComponent(typeof(SmartObject))]
public class StartingTags : MonoBehaviour 
{
    [System.Serializable]
    public class StartingRelationship
    {
        public SmartObject other;

        public bool IsSittingOn;
        public bool IsFriendOf;
        public bool IsAdjacentTo;
        public bool IsGuarding;
        public bool IsInZone;
        public bool FrontZone;
        public bool RearZone;
        public bool IsAlliedWith;
        public bool IsAttending;
    }

    // Roles
    public bool RoleActor;
    public bool RoleChair;
    public bool RoleTable;
    public bool RoleCrowd;
    public bool RoleVendorStand;
    public bool RoleWaypoint;
    public bool RoleTelevision;
    public bool RoleLamp;
    public bool RoleInterestingObject;
    public bool RoleContainer;
    public bool RoleDrinkDispenser;
    public bool RoleTeller;
    public bool RoleTrashcan;
    public bool RoleVaultDoor;
    public bool RoleDoor;
    public bool RoleTellerDoor;
    public bool RoleManagerDoor;
    public bool RoleTellerButton;
    public bool RoleManagerButton;
    public bool RoleGuard1;
    public bool RoleGuard2;
    public bool RoleGuard3;
    public bool RoleGuard4;
    public bool RoleManager;
//    public bool RoleTicketDispenser;
//    public bool RoleWorld;
	public bool RoleRobber;
	public bool RoleGuard;
    public bool RoleZone;
    public bool RoleBankCounter;

    // Ownership
    public bool HoldingBall;
    public bool HoldingWallet;
    public bool HoldingDrink;
    public bool HoldingWeapon;
    public bool HoldingMoney;
    public bool HasBackpack;
    public bool HasKeys;
    public bool RightHandOccupied;
    public bool LeftHandOccupied;

    // Conditions
    public bool IsIncapacitated;
    public bool IsDead;
    public bool IsStanding;
    public bool IsOccupied;
    public bool IsHaggling;
    public bool IsTurnedOn;
    public bool IsOpen;
    public bool IsGuarding;
    public bool IsUnlocked;
    public bool TellerButtonPressed;
    public bool ManagerButtonPressed;
    public bool AlarmRaised;
    public bool IsGuarded;
    public bool IsImmobile;
    public bool IsPressed;
    public bool IsGuardable;
    public bool CanBreakAlliances;

    // Mini Exploration Stuff
    public bool RoleMoneyContainer;
    public bool RoleWeaponContainer;

    // TODO: HACKS HACKS HACKS
    public bool InTellerZone;
    public bool NotCrowdEligible;

    public StartingRelationship[] Relationships;

    void Start()
    {
        SmartObject obj = this.GetComponent<SmartObject>();

        obj.Set(
            (this.RoleActor) ? StateName.RoleActor : ~StateName.RoleActor,
            (this.RoleChair) ? StateName.RoleChair : ~StateName.RoleChair,
            (this.RoleTable) ? StateName.RoleTable : ~StateName.RoleTable,
            (this.RoleCrowd) ? StateName.RoleCrowd : ~StateName.RoleCrowd,
            (this.RoleVendorStand) ? StateName.RoleVendorStand : ~StateName.RoleVendorStand,
            (this.RoleWaypoint) ? StateName.RoleWaypoint : ~StateName.RoleWaypoint,
            (this.RoleTelevision) ? StateName.RoleTelevision : ~StateName.RoleTelevision,
            (this.RoleLamp) ? StateName.RoleLamp : ~StateName.RoleLamp,
            (this.RoleInterestingObject) ? StateName.RoleAttraction : ~StateName.RoleAttraction,
            (this.RoleContainer) ? StateName.RoleContainer : ~StateName.RoleContainer,
            (this.RoleDrinkDispenser) ? StateName.RoleDispenser : ~StateName.RoleDispenser,
            (this.RoleTeller) ? StateName.RoleTeller : ~StateName.RoleTeller,
            (this.RoleTrashcan) ? StateName.RoleTrashcan : ~StateName.RoleTrashcan,
            (this.RoleVaultDoor) ? StateName.RoleVaultDoor : ~StateName.RoleVaultDoor,
            (this.RoleDoor) ? StateName.RoleDoor : ~StateName.RoleDoor,
            (this.RoleTellerDoor) ? StateName.RoleTellerDoor : ~StateName.RoleTellerDoor,
            (this.RoleManagerDoor) ? StateName.RoleManagerDoor : ~StateName.RoleManagerDoor,
            (this.RoleTellerButton) ? StateName.RoleTellerButton : ~StateName.RoleTellerButton,
            (this.RoleManagerButton) ? StateName.RoleManagerButton : ~StateName.RoleManagerButton,
            (this.RoleGuard1) ? StateName.RoleGuard1 : ~StateName.RoleGuard1,
            (this.RoleGuard2) ? StateName.RoleGuard2 : ~StateName.RoleGuard2,
            (this.RoleGuard3) ? StateName.RoleGuard3 : ~StateName.RoleGuard3,
            (this.RoleGuard4) ? StateName.RoleGuard4 : ~StateName.RoleGuard4,
            (this.RoleManager) ? StateName.RoleManager : ~StateName.RoleManager,
//            (this.RoleTicketDispenser) ? StateName.RoleTicketDispenser : ~StateName.RoleTicketDispenser,
//            (this.RoleWorld) ? StateName.RoleWorld : ~StateName.RoleWorld,
			(this.RoleRobber) ? StateName.RoleRobber : ~StateName.RoleRobber,
			(this.RoleGuard) ? StateName.RoleGuard : ~StateName.RoleGuard,

            (this.HoldingBall) ? StateName.HoldingBall : ~StateName.HoldingBall,
            (this.HoldingWallet) ? StateName.HoldingWallet : ~StateName.HoldingWallet,
            (this.HoldingDrink) ? StateName.HoldingDrink : ~StateName.HoldingDrink,
            (this.HoldingWeapon) ? StateName.HoldingWeapon : ~StateName.HoldingWeapon,
            (this.HoldingMoney) ? StateName.HoldingMoney : ~StateName.HoldingMoney,
            (this.HasBackpack) ? StateName.HasBackpack : ~StateName.HasBackpack,
            (this.HasKeys) ? StateName.HasKeys : ~StateName.HasKeys,
            (this.RightHandOccupied) ? StateName.RightHandOccupied : ~StateName.RightHandOccupied,
            (this.LeftHandOccupied) ? StateName.LeftHandOccupied : ~StateName.LeftHandOccupied,

            (this.IsIncapacitated) ? StateName.IsIncapacitated : ~StateName.IsIncapacitated,
            (this.IsDead) ? StateName.IsDead : ~StateName.IsDead,
            (this.IsStanding) ? StateName.IsStanding : ~StateName.IsStanding,
            (this.IsOccupied) ? StateName.IsOccupied : ~StateName.IsOccupied,
            (this.IsTurnedOn) ? StateName.IsTurnedOn : ~StateName.IsTurnedOn,
            (this.IsOpen) ? StateName.IsOpen : ~StateName.IsOpen,
            (this.IsGuarding) ? StateName.IsGuarding : ~StateName.IsGuarding,
            (this.IsUnlocked) ? StateName.IsUnlocked : ~StateName.IsUnlocked,
            (this.TellerButtonPressed) ? StateName.TellerButtonPressed : ~StateName.TellerButtonPressed,
            (this.ManagerButtonPressed) ? StateName.ManagerButtonPressed : ~StateName.ManagerButtonPressed,
            (this.AlarmRaised) ? StateName.AlarmRaised : ~StateName.AlarmRaised,
            (this.IsGuarded) ? StateName.IsGuarded : ~StateName.IsGuarded,
            (this.IsImmobile) ? StateName.IsImmobile : ~StateName.IsImmobile,
            (this.RoleZone) ? StateName.RoleZone : ~StateName.RoleZone,
            (this.RoleBankCounter) ? StateName.RoleBankCounter : ~StateName.RoleBankCounter,
            (this.IsPressed) ? StateName.IsPressed : ~StateName.IsPressed,
            (this.IsGuardable) ? StateName.IsGuardable : ~StateName.IsGuardable,
            (this.CanBreakAlliances) ? StateName.CanBreakAlliances : ~StateName.CanBreakAlliances,

            // Mini Exploration temp stuff
            (this.RoleWeaponContainer) ? StateName.RoleWeaponContainer : ~StateName.RoleWeaponContainer,
            (this.RoleMoneyContainer) ? StateName.RoleMoneyContainer : ~StateName.RoleMoneyContainer,
            (this.InTellerZone) ? StateName.InTellerZone : ~StateName.InTellerZone,
            (this.NotCrowdEligible) ? StateName.NotCrowdEligible : ~StateName.NotCrowdEligible);


        foreach (StartingRelationship relationship in this.Relationships)
            obj.Set(relationship.other.Id,
                (relationship.IsSittingOn) ? RelationName.IsSittingOn : ~RelationName.IsSittingOn,
                (relationship.IsFriendOf) ? RelationName.IsFriendOf : ~RelationName.IsFriendOf,
                (relationship.IsAdjacentTo) ? RelationName.IsAdjacentTo : ~RelationName.IsAdjacentTo,
                (relationship.IsGuarding) ? RelationName.IsGuarding : ~RelationName.IsGuarding,
                (relationship.IsInZone) ? RelationName.IsInZone : ~RelationName.IsInZone,
                (relationship.FrontZone) ? RelationName.FrontZone : ~RelationName.FrontZone,
                (relationship.RearZone) ? RelationName.RearZone : ~RelationName.RearZone,
                (relationship.IsAlliedWith) ? RelationName.IsAlliedWith : ~RelationName.IsAlliedWith,
                (relationship.IsAttending) ? RelationName.IsAttending : ~RelationName.IsAttending);
    }
}
