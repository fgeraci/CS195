// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections;
using System.Collections.Generic;

// You can use ~ to negate these values in parameter lists.
// Never ever use bitwise & or | directly with these enums!

[System.Serializable]
public enum StateName : long
{
    // Roles
    RoleActor           = 1L << 0,
    RoleChair           = 1L << 1,
    RoleTable           = 1L << 2,
    RoleCrowd           = 1L << 3,
    RoleVendorStand     = 1L << 4,
    RoleWaypoint        = 1L << 5,
    RoleLamp            = 1L << 6,
    RoleTelevision      = 1L << 7,
    RoleAttraction      = 1L << 8,
    RoleContainer       = 1L << 9,
    RoleDispenser       = 1L << 10,
    RoleTeller          = 1L << 11,
    RoleTrashcan        = 1L << 12,
    RoleDoor            = 1L << 13,
    RoleVaultDoor       = 1L << 14,
    RoleTellerDoor      = 1L << 15,
    RoleManagerDoor     = 1L << 16,
    RoleTellerButton    = 1L << 17,
    RoleManagerButton   = 1L << 18,
    RoleGuard1          = 1L << 19,
    RoleGuard2          = 1L << 20,
    RoleGuard3          = 1L << 21,
    RoleGuard4          = 1L << 22,
    RoleManager         = 1L << 23,
    RoleRobber			= 1L << 24,
    RoleGuard           = 1L << 25,
    RoleZone            = 1L << 26,
    RoleBankCounter     = 1L << 27,

    ALL_ROLES           = (1 << 28) - 1,

    // Ownership
    HoldingBall         = 1L << 28,
    HoldingWallet       = 1L << 29,
    HoldingDrink        = 1L << 30,
    HoldingWeapon       = 1L << 31,
    HoldingMoney        = 1L << 32, // UNUSED
    HasBackpack         = 1L << 33,
    HasKeys             = 1L << 34,
    RightHandOccupied   = 1L << 35,
    LeftHandOccupied    = 1L << 36,

    // Conditions
    IsIncapacitated     = 1L << 37,
    IsDead              = 1L << 38,
    IsStanding          = 1L << 39,
    IsOccupied          = 1L << 40,
    IsTurnedOn          = 1L << 41,
    IsOpen              = 1L << 42,
    IsGuarding          = 1L << 43,
    IsUnlocked          = 1L << 44,
    TellerButtonPressed = 1L << 45,
    ManagerButtonPressed = 1L << 46,
    AlarmRaised         = 1L << 47,
    IsGuarded           = 1L << 48,
    IsImmobile          = 1L << 49,
    IsPressed           = 1L << 50,
    IsGuardable         = 1L << 51,
    CanBreakAlliances   = 1L << 52,

    // Temporary stuff for simple world exploration
    RoleMoneyContainer  = 1L << 53,
    RoleWeaponContainer = 1L << 54,
    ManagerDoorUnlocked = 1L << 55,
    TellerDoorUnlocked  = 1L << 56,
    HasLeft             = 1L << 57,


    InTellerZone        = 1L << 61,
    NotCrowdEligible    = 1L << 62,

    // Do not use negative numbers!
    INVALID             = 1L << 63,
}

[System.Serializable]
public enum RelationName : long
{
    IsSittingOn      = 1L << 0,
    IsFriendOf       = 1L << 1,
    IsAdjacentTo     = 1L << 2,
    IsSuspiciousOf   = 1L << 3,
    IsGuarding       = 1L << 4,
    IsInZone         = 1L << 5,
    FrontZone        = 1L << 6,
    RearZone         = 1L << 7,
    IsAlliedWith     = 1L << 8,
    IsAttending      = 1L << 9,

    // Do not use negative numbers!
    INVALID          = 1L << 63,
}

public static class StateDefs
{
    /// <summary>
    /// Returns a list of all of the Role enums
    /// </summary>
    public static IEnumerable<StateName> GetRoleStates()
    {
        StateName[] allStates =
            (StateName[])Enum.GetValues(typeof(StateName));

        foreach (StateName state in allStates)
            if (((long)state & (long)StateName.ALL_ROLES) != 0)
                yield return state;
        yield break;
    }

    public static string StateString(IState state, int minLength)
    {
        StateName[] allStates =
            (StateName[])Enum.GetValues(typeof(StateName));

        string output = "";

        foreach (StateName name in allStates)
            if (name != StateName.ALL_ROLES && name != StateName.INVALID)
                output += PrettyPrint(name, state.Require(new [] { name }));

        for (int i = output.Length; i < minLength; i++)
            output += " ";

        return output;
    }

    public static string RelationString(uint id, IState state, int minLength)
    {
        RelationName[] allStates =
            (RelationName[])Enum.GetValues(typeof(RelationName));

        string output = "";

        foreach (RelationName name in allStates)
            if (name != RelationName.INVALID)
                output += PrettyPrint(name, state.Require(id, new[] { name }));

        for (int i = output.Length; i < minLength; i++)
            output += " ";

        return output;
    }

    public static string PrettyPrint(StateName state, bool value)
    {
        return (value == true) ? state.ToString() : "";

        //switch (state)
        //{
        //    case StateName.RoleActor:       return value ? " Rac" : "";
        //    case StateName.RoleChair:       return value ? " Rch" : "";
        //    case StateName.RoleTable:       return value ? " Rta" : "";
        //    case StateName.RoleCrowd:       return value ? " Rcr" : "";
        //    case StateName.RoleVendorStand: return value ? " Rvs" : "";
        //    case StateName.RoleWaypoint:    return value ? " Rwp" : "";
        //    case StateName.RoleLamp:        return value ? " Rla" : "";
        //    case StateName.RoleContainer:   return value ? " Rla" : "";
        //    case StateName.RoleTelevision:  return value ? " Rtv" : "";
        //    case StateName.RoleAttraction:  return value ? " Rat" : "";
        //    case StateName.HoldingBall:     return value ? " Hba" : "";
        //    case StateName.HoldingWallet:   return value ? " Hwa" : "";
        //    case StateName.IsStanding:      return value ? " Ist" : "";
        //    case StateName.IsIncapacitated: return value ? " Iic" : "";
        //    case StateName.IsDead:          return value ? " Idd" : "";
        //    case StateName.IsOccupied:      return value ? " Ioc" : "";
        //    case StateName.IsTurnedOn:      return value ? " Ito" : "";
        //    default:                        return value ? " Xxx" : "";
        //}
    }

    public static string PrettyPrint(RelationName state, bool value)
    {
        return (value == true) ? state.ToString() : "";

        //switch (state)
        //{
        //    case RelationName.IsSittingOn:    return value ? " Iso" : "";
        //    case RelationName.IsFriendOf:     return value ? " Ifo" : "";
        //    case RelationName.IsAdjacentTo:   return value ? " Iat" : "";
        //    case RelationName.IsGuarding: return value ? " Igd" : "";
        //    case RelationName.IsInZone:       return value ? " Iir" : "";
        //    case RelationName.FrontZone:  return value ? " Dfz" : "";
        //    case RelationName.RearZone:   return value ? " Drz" : "";
        //    default: return value ? " Xxx" : "";
        //}
    }
}