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
public class _PlasenciaTags : MonoBehaviour
{
    [System.Serializable]
    public class StartingRelationship
    {
        public SmartObject other;

        public bool IsSittingOn;
        public bool IsFriendOf;
        public bool IsAdjacentTo;
        public bool IsInZone;
        public bool FrontZone;
        public bool RearZone;
        public bool IsAttending;
    }

    // Roles
    public bool RoleActor;
    public bool RoleChair;
    public bool RoleTable;
    public bool RoleCrowd;
    public bool RoleVendorStand;
    public bool RoleWaypoint;
    public bool RoleInterestingObject;
    public bool RoleContainer;
    public bool RoleTeller;
    public bool RoleVaultDoor;
    public bool RoleDoor;
    public bool RoleRobber;
    public bool RoleGuard;
    public bool RoleZone;
    public bool RoleGatheringJew;

    // Ownership
    public bool HoldingBall;
    public bool HoldingDrink;
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
            (this.RoleInterestingObject) ? StateName.RoleAttraction : ~StateName.RoleAttraction,
            (this.RoleContainer) ? StateName.RoleContainer : ~StateName.RoleContainer,
            (this.RoleTeller) ? StateName.RoleTeller : ~StateName.RoleTeller,
            (this.RoleVaultDoor) ? StateName.RoleVaultDoor : ~StateName.RoleVaultDoor,
            (this.RoleDoor) ? StateName.RoleDoor : ~StateName.RoleDoor,
            //            (this.RoleTicketDispenser) ? StateName.RoleTicketDispenser : ~StateName.RoleTicketDispenser,
            //            (this.RoleWorld) ? StateName.RoleWorld : ~StateName.RoleWorld,
            (this.RoleRobber) ? StateName.RoleRobber : ~StateName.RoleRobber,
            (this.RoleGuard) ? StateName.RoleGuard : ~StateName.RoleGuard,
          //  (this.RoleGatheringJew) ? StateName.RoleGatheringJew : ~StateName.RoleGatheringJew,

            (this.HoldingBall) ? StateName.HoldingBall : ~StateName.HoldingBall,
            (this.HoldingDrink) ? StateName.HoldingDrink : ~StateName.HoldingDrink,
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
            (this.IsUnlocked) ? StateName.IsUnlocked : ~StateName.IsUnlocked,
            (this.TellerButtonPressed) ? StateName.TellerButtonPressed : ~StateName.TellerButtonPressed,
            (this.ManagerButtonPressed) ? StateName.ManagerButtonPressed : ~StateName.ManagerButtonPressed,
            (this.AlarmRaised) ? StateName.AlarmRaised : ~StateName.AlarmRaised,
            (this.IsGuarded) ? StateName.IsGuarded : ~StateName.IsGuarded,
            (this.IsImmobile) ? StateName.IsImmobile : ~StateName.IsImmobile,
            (this.RoleZone) ? StateName.RoleZone : ~StateName.RoleZone,
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
                (relationship.IsInZone) ? RelationName.IsInZone : ~RelationName.IsInZone,
                (relationship.FrontZone) ? RelationName.FrontZone : ~RelationName.FrontZone,
                (relationship.RearZone) ? RelationName.RearZone : ~RelationName.RearZone,
                (relationship.IsAttending) ? RelationName.IsAttending : ~RelationName.IsAttending);
    }
}
