using UnityEngine;
using TreeSharpPlus;

public static class ReusableActions
{
    public static Node MoveZones(
        SmartObject mover, 
        SmartObject fromZone, 
        SmartObject toZone)
    {
        return new Sequence(
            mover.Node_Set(fromZone.Id, ~RelationName.IsInZone),
            mover.Node_Set(toZone.Id, RelationName.IsInZone));
    }

    public static Node BreakAlliance(SmartCharacter char1, SmartCharacter char2)
    {
        return new Sequence(
            char1.Node_Set(char2.Id, ~RelationName.IsAlliedWith),
            char2.Node_Set(char1.Id, ~RelationName.IsAlliedWith));
    }

    public static Node IncapacitateFromBehind(SmartCharacter aggressor, SmartCharacter target)
    {
        return new LeafAffordance("IncapacitateFromBehind", aggressor, target);
    }

    public static Node TakeKeysFromBehind(SmartCharacter aggressor, SmartCharacter target)
    {
        return new LeafAffordance("TakeKeysFromBehind", aggressor, target);
    }

    public static Node Distract(SmartCharacter distractor, SmartCharacter target)
    {
        return new Sequence
        (
            distractor.Node_Icon("speaking"),
            distractor.Behavior.ST_PlayHandGesture("callover", 3000),
            target.Node_GoToUpToRadius(Val.V(() => distractor.transform.position), 4.0f),
            new LeafAffordance("Talk", target, distractor),
            distractor.Node_Icon(null)
        );
    }

    public static Node DistractAndIncapacitate(
        SmartCharacter distractor, 
        SmartCharacter aggressor,
        SmartCharacter target)
    {
        return new Sequence(
            distractor.Node_GoToUpToRadius(Val.V(() => target.transform.position), 10f),
            new SequenceParallel(
                ReusableActions.Distract(distractor, target),
                new Sequence(
                    new LeafWait(17000),
                    aggressor.Node_GoTo(Val.V(() => target.WaypointBack.position)))),
            ReusableActions.IncapacitateFromBehind(aggressor, target));
    }

    public static Node PressButton(SmartCharacter user, SmartButton button)
    {
        return new LeafAffordance("Press", user, button);
    }

    private static Node ApproachUpTo(SmartCharacter approacher, SmartCharacter target, float distance)
    {
        return new Sequence(
            approacher.Node_GoToUpToRadius(Val.V(() => target.transform.position), distance),
            approacher.Node_OrientTowards(Val.V(() => target.transform.position)));
    }

    private static Node Coerce(
        SmartCharacter aggressor,
        SmartCharacter target,
        Val<float> distance)
    {
        return new Sequence(
            aggressor.Node_GoToUpToRadius(Val.V(() => target.transform.position), distance),
            aggressor.Node_OrientTowards(Val.V(() => target.transform.position)),
            aggressor.Node_Icon("stickup"),
            new SequenceParallel(
                aggressor.Behavior.ST_PlayHandGesture("pistolaim", 4000),
                new Sequence(
                    new LeafWait(2000),
                    target.Node_OrientTowards(Val.V(() => aggressor.transform.position)))));

    }

    private static Node BeginCoerce(
        SmartCharacter coercer,
        SmartCharacter target)
    {
        return new Sequence(
            coercer.Node_OrientTowards(Val.V(() => target.transform.position)),
            coercer.Node_Icon("shouting"),
            new LeafInvoke(() => coercer.trackTarget = target.transform),
            new LeafAffordance("RaiseGun", coercer, coercer),
            target.Node_OrientTowards(Val.V(() => coercer.transform.position)),
            new LeafWait(2000),
            coercer.Node_Icon(null));
    }

    private static Node EndCoerce(SmartCharacter coercer)
    {
        return new Sequence(
            new LeafInvoke(() => coercer.trackTarget = null),
            new LeafAffordance("LowerGun", coercer, coercer));
    }

    public static Node CoerceAndIncapacitate(
        SmartCharacter coercer,
        SmartCharacter aggressor,
        SmartCharacter target)
    {
        return new Sequence(
            ApproachUpTo(coercer, target, 6.0f),
            BeginCoerce(coercer, target),
            IncapacitateFromBehind(aggressor, target),
            EndCoerce(coercer));
    }

    public static Node CoerceIntoGivingKey(
        SmartCharacter coercer,
        SmartCharacter target)
    {
        return new Sequence(
            ApproachUpTo(coercer, target, 2.0f),
            BeginCoerce(coercer, target),
            new LeafAffordance("CoerceGiveKey", target, coercer),
            EndCoerce(coercer),
            target.Node_Set(~StateName.HasKeys),
            coercer.Node_Set(StateName.HasKeys));
    }

    public static Node CoerceIntoPressingButton(
        SmartCharacter coercer, 
        SmartCharacter target, 
        SmartButton button)
    {
        return new Sequence(
            ApproachUpTo(coercer, target, 3.0f),
            BeginCoerce(coercer, target),
            new LeafWait(3000),
            PressButton(target, button),
            EndCoerce(coercer));
    }

    public static Node CoerceIntoPressingButtonAtCounter(
        SmartCharacter coercer,
        SmartCharacter target,
        SmartButton button,
        SmartBankCounter counter)
    {
        return new Sequence(
            StandInFrontOfCounter(coercer, counter),
            BeginCoerce(coercer, target),
            new LeafWait(2000),
            LeaveCounter(target, counter),
            new LeafWait(1000),
            PressButton(target, button),
            EndCoerce(coercer));
    }

    public static Node CoerceIntoUnlockDoorFront(
        SmartCharacter coercer,
        SmartCharacter target,
        SmartDoor door)
    {
        return new Sequence(
            ApproachUpTo(coercer, target, 3.0f),
            BeginCoerce(coercer, target),
            UnlockDoorFront(target, door),
            EndCoerce(coercer));
    }

    public static Node CoerceIntoUnlockDoorRear(
        SmartCharacter coercer,
        SmartCharacter target,
        SmartDoor door)
    {
        return new Sequence(
            ApproachUpTo(coercer, target, 3.0f),
            BeginCoerce(coercer, target),
            UnlockDoorRear(target, door),
            EndCoerce(coercer));
    }

    public static Node CoerceIntoTeleportDropBriefcase(
        SmartCharacter coercer,
        SmartCharacter target,
        SmartContainer container)
    {
        return new Sequence(
            ApproachUpTo(coercer, target, 6.0f),
            BeginCoerce(coercer, target),
            TeleportDropBackpack(target, container),
            EndCoerce(coercer));
    }

    public static Node CoerceIntoTeleportDropWeapon(
        SmartCharacter coercer,
        SmartCharacter target,
        SmartContainer container)
    {
        return new Sequence(
            ApproachUpTo(coercer, target, 6.0f),
            BeginCoerce(coercer, target),
            TeleportDropWeapon(target, container),
            EndCoerce(coercer));
    }

    public static Node CoerceIntoSurrender(
        SmartCharacter coercer,
        SmartCharacter target)
    {
        return new Sequence(
            ApproachUpTo(coercer, target, 6.0f),
            BeginCoerce(coercer, target),
            Surrender(target));
    }

    public static Node CoerceIntoDropWeaponAndSurrender(
        SmartCharacter coercer,
        SmartCharacter target,
        SmartContainer container)
    {
        return new Sequence(
            ApproachUpTo(coercer, target, 6.0f),
            BeginCoerce(coercer, target),
            TeleportDropWeapon(target, container),
            Surrender(target));
    }

    public static Node CoerceIntoMoveToTellerZone(
        SmartCharacter coercer,
        SmartCharacter victim,
        SmartWaypoint target)
    {
        return new Sequence(
            ApproachUpTo(coercer, victim, 6.0f),
            BeginCoerce(coercer, victim),
            new LeafAffordance("Approach", victim, target),
            EndCoerce(coercer));
    }

    public static Node UnlockDoorFront(
        SmartCharacter actor,
        SmartDoor door)
    {
        return new LeafAffordance("UnlockFront", actor, door);
    }

    public static Node UnlockDoorRear(
        SmartCharacter actor,
        SmartDoor door)
    {
        return new LeafAffordance("UnlockRear", actor, door);
    }

    public static Node LockDoorFront(
        SmartCharacter actor,
        SmartDoor door)
    {
        return new LeafAffordance("LockFront", actor, door);
    }

    public static Node LockDoorRear(
        SmartCharacter actor,
        SmartDoor door)
    {
        return new LeafAffordance("LockRear", actor, door);
    }

    public static Node GuardDoorFront(
        SmartCharacter actor,
        SmartDoor door)
    {
        return new LeafAffordance("GuardFront", actor, door);
    }

    public static Node GuardDoorRear(
        SmartCharacter actor,
        SmartDoor door)
    {
        return new LeafAffordance("GuardRear", actor, door);
    }

    public static Node StopGuardingDoor(
        SmartCharacter actor,
        SmartDoor door)
    {
        return new LeafAffordance("StopGuarding", actor, door);
    }

    public static Node OpenVault(
        SmartCharacter user,
        SmartVaultDoor vault)
    {
        return new LeafAffordance("Open", user, vault);
    }

    public static Node AttendCounter(
        SmartCharacter teller,
        SmartBankCounter counter)
    {
        return new LeafAffordance("Attend", teller, counter);
    }

    public static Node LeaveCounter(
        SmartCharacter teller,
        SmartBankCounter counter)
    {
        return new LeafAffordance("Leave", teller, counter);
    }

    public static Node StandInFrontOfCounter(
        SmartCharacter user,
        SmartBankCounter counter)
    {
        return new LeafAffordance("StandInFront", user, counter);
    }

    public static Node PickupBriefcase(
        SmartCharacter actor,
        SmartContainer container)
    {
        return new LeafAffordance("PickupBackpack", actor, container);
    }

    public static Node PickupMoneyFromCart(
        SmartCharacter actor,
        SmartCart container)
    {
        return new LeafAffordance("PickupMoney", actor, container);
    }

    public static Node PickupWeapon(
        SmartCharacter actor,
        SmartContainer container)
    {
        return new LeafAffordance("PickupWeapon", actor, container);
    }

    public static Node TeleportDropBackpack(
        SmartCharacter actor,
        SmartContainer container)
    {
        return new LeafAffordance("TeleportDropBackpack", actor, container);
    }

    public static Node TeleportDropWeapon(
        SmartCharacter actor,
        SmartContainer container)
    {
        return new LeafAffordance("TeleportDropWeapon", actor, container);
    }

    public static Node Surrender(
        SmartCharacter actor)
    {
        return new LeafAffordance("Surrender", actor, actor);
    }

    public static Node GiveBriefcase(
        SmartCharacter actor,
        SmartCharacter recipient)
    {
        return new LeafAffordance("GiveBriefcase", actor, recipient);
    }

    public static Node TakeWeaponIncapacitated(
        SmartCharacter taker,
        SmartCharacter target)
    {
        return new LeafAffordance("GetGunIncapacitated", taker, target);
    }

    public static Node TakeKeysIncapacitated(
        SmartCharacter taker,
        SmartCharacter target)
    {
        return new LeafAffordance("GetKeyIncapacitated", taker, target);
    }
}
