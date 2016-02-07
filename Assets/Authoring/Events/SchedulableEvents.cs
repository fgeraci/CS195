using TreeSharpPlus;
using UnityEngine;
using RootMotion.FinalIK;

public static class ReusableCleanup
{
    /// <summary>
    /// Tries attaching release's current prop to attach, if it exists. Does nothing otherwise.
    /// </summary>
    public static void TryAttach(PropHolder attach, PropHolder release)
    {
        if (release.CurrentProp != null)
        {
            attach.Attach(release.Release());
        }
    }
}

/// <summary>
/// Event where the character gets a drink from the dispenser, drinks it and throws it away.
/// </summary>
[LibraryIndex(3)]
public class GetDrinkAndDrink : GenericEvent<SmartCharacter, SmartDrinkDispenser, SmartTrashcan>
{
    protected override Node Root(Token token, SmartCharacter user, SmartDrinkDispenser dispenser, 
        SmartTrashcan can)
    {
        Vector3 oldPosition = new Vector3();

        return new Sequence(
            new LeafInvoke(() => oldPosition = user.transform.position),
            new DecoratorCatch(
                () => //if terminates, make sure our right hand is not holding anything
                {
                    if (user.HoldPropRightHand.CurrentProp != null)
                        GameObject.Destroy(user.HoldPropRightHand.CurrentProp.gameObject);
                    user.Set(~StateName.HoldingDrink, ~StateName.RightHandOccupied);
                    user.Character.StopInteraction(FullBodyBipedEffector.RightHand);
                },
                new Sequence(   
                    new LeafAffordance("GetDrink", user, dispenser),
                    user.Behavior.ST_PlayFaceGesture("drink", 4000),
                    new LeafAffordance("DropDrink", user, can))),
            user.Behavior.ST_PlayHandGesture("mouthwipe", 3000),
            user.Node_GoTo(Val.V(() => oldPosition)));
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [StateRequired(1, StateName.RoleDispenser)]
    [StateRequired(2, StateName.RoleTrashcan)]
    [HideInGUI(3)]
    public GetDrinkAndDrink(SmartCharacter user, SmartDrinkDispenser dispenser, SmartTrashcan can)
        : base(user, dispenser, can) { }
}

/// <summary>
/// Event where the character brings the filled out form to the teller.
/// </summary>
[LibraryIndex(3)]
public class DeliverForm : GenericEvent<SmartCharacter, SmartBankCounter, SmartCharacter>
{
    private void CleanupForm(SmartCharacter user, SmartBankCounter counter, SmartCharacter teller)
    {
        ReusableCleanup.TryAttach(user.HoldPropHidden, user.HoldPropRightHand);
        ReusableCleanup.TryAttach(user.HoldPropHidden, counter.HoldPropIntermediate);
        ReusableCleanup.TryAttach(user.HoldPropHidden, counter.HoldPropStorage);
        ReusableCleanup.TryAttach(user.HoldPropHidden, teller.HoldPropRightHand);
    }

    protected override Node Root(Token token, SmartCharacter user, SmartBankCounter counter, SmartCharacter teller)
    {
        return new Sequence(
            user.Node_GoTo(counter.CustomerStandPoint.position),
            user.Node_OrientTowards(counter.TellerStandPoint.position),
            teller.Node_GoTo(counter.TellerStandPoint.position),
            teller.Node_OrientTowards(counter.CustomerStandPoint.position),
            user.ST_TalkWithoutApproach(teller),
            new DecoratorCatch(
                () => CleanupForm(user, counter, teller),
                new Sequence(
                    new LeafAffordance("CustomerPutHidden", user, counter),
                    new LeafAffordance("TellerPutHidden", teller, counter),
                    new LeafInvoke(() => user.HoldPropHidden.Attach(counter.HoldPropStorage.Release())))),
            user.Node_GoTo(counter.LeavePoint.position));
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [StateRequired(1, StateName.RoleBankCounter)]
    [StateRequired(2, StateName.RoleTeller, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated,
        StateName.IsStanding)]
    //[RelationRequired(2, 1, RelationName.IsAttending)]
    [HideInGUI(3)]
    public DeliverForm(SmartCharacter user, SmartBankCounter counter, SmartCharacter teller)
        : base(user, counter, teller) { }
}

/// <summary>
/// Event where the character fills out a form at the table.
/// </summary>
[LibraryIndex(3)]
public class FillOutForm : GenericEvent<SmartCharacter, SmartTable>
{
    private RunStatus Cleanup(SmartCharacter user, SmartTable table)
    {
        user.Character.StopInteraction(FullBodyBipedEffector.RightHand);
        user.Character.HeadLookStop();
        user.Character.HandAnimation("writing", false);
        ReusableCleanup.TryAttach(user.HoldPropHidden, user.HoldPropRightHand);
        ReusableCleanup.TryAttach(user.HoldPropHidden, table.PropHolder);
        return user.Character.NavStop();
    }

    protected override Node Root(Token token, SmartCharacter user, SmartTable table)
    {
        return new DecoratorCatch(
            () => Cleanup(user, table),
            new LeafAffordance("FillOutForm", user, table));
            //new LeafAffordance("FillOutForm", user, table));
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [StateRequired(1, StateName.RoleTable)]
    [HideInGUI(3)]
    public FillOutForm(SmartCharacter user, SmartTable table)
        : base(user, table) { }
}

/// <summary>
/// Event where the character talks to the manager.
/// </summary>
[LibraryIndex(3)]
public class TalkToManager : GenericEvent<SmartCharacter, SmartCharacter>
{
    private RunStatus CleanUp(SmartCharacter cust, SmartCharacter manager)
    {
        cust.GetComponent<BodyMecanim>().ResetAnimation();
        manager.GetComponent<BodyMecanim>().ResetAnimation();
        return cust.Character.NavStop();
    }

    protected override Node Root(Token token, SmartCharacter customer, SmartCharacter manager)
    {
        Vector3 oldCustomerPosition = new Vector3();
        return new DecoratorCatch(
            () => CleanUp(customer, manager),
            new Sequence(
                new LeafInvoke(() => oldCustomerPosition = customer.transform.position),
                new LeafAffordance("TalkSecretively", customer, manager),
                customer.Node_GoTo(Val.V(() => oldCustomerPosition))));
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [StateRequired(1, StateName.RoleManager, StateName.IsStanding, ~StateName.IsIncapacitated)]
    [RelationRequired(0, 2, RelationName.IsInZone)]
    [RelationRequired(1, 2, RelationName.IsInZone)]
    [HideInGUI(3)]
    [NonParticipant(2)]
    public TalkToManager(SmartCharacter customer, SmartCharacter manager, SmartObject zone)
        : base(customer, manager) { }
}

/// <summary>
/// Event where the character gets a ticket from the dispenser and then enters the bank.
/// </summary>
[LibraryIndex(3)]
public class EnterBank : GenericEvent<SmartCharacter, SmartWaypoint>
{
    protected override Node Root(Token token, SmartCharacter user, SmartWaypoint waypoint)
    {
        return new LeafAffordance("Approach", user, waypoint);
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [StateRequired(1, StateName.RoleWaypoint, ~StateName.NotCrowdEligible)]
    [HideInGUI(3)]
    public EnterBank(SmartCharacter user, SmartWaypoint waypoint)
        : base(user, waypoint) { }
}

/// <summary>
/// Event where the character leaves the bank through the given waypoint's OtherTarget.
/// </summary>
[LibraryIndex(3)]
public class ExitBank : GenericEvent<SmartCharacter, SmartWaypoint>
{
    protected override Node Root(Token token, SmartCharacter user, SmartWaypoint waypoint)
    {
        return new LeafAffordance("Approach", user, waypoint);
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [StateRequired(1, StateName.RoleWaypoint, ~StateName.NotCrowdEligible)]
    [HideInGUI(3)]
    public ExitBank(SmartCharacter user, SmartWaypoint waypoint)
        : base(user, waypoint) { }
}

/// <summary>
/// Normal conversation between two characters.
/// </summary>
[LibraryIndex(3)]
public class TalkNormallyCrowd : GenericEvent<SmartCharacter, SmartCharacter>
{
    protected override Node Root(Token token, SmartCharacter char1, SmartCharacter char2)
    {
        return new LeafAffordance("Talk", char1, char2);
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [StateRequired(1, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [HideInGUI(3)]
    public TalkNormallyCrowd(SmartCharacter char1, SmartCharacter char2)
        : base(char1, char2) { }
}
/// <summary>
/// Happy conversation between two characters.
/// </summary>
[LibraryIndex(3)]
public class TalkHappilyCrowd : GenericEvent<SmartCharacter, SmartCharacter>
{
    protected override Node Root(Token token, SmartCharacter char1, SmartCharacter char2)
    {
        return new LeafAffordance("TalkHappily", char1, char2);
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [StateRequired(1, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [HideInGUI(3)]
    public TalkHappilyCrowd(SmartCharacter char1, SmartCharacter char2)
        : base(char1, char2) { }
}

/// <summary>
/// Secretive conversation between two characters.
/// </summary>
[LibraryIndex(3)]
public class TalkSecretivelyCrowd : GenericEvent<SmartCharacter, SmartCharacter>
{
    protected override Node Root(Token token, SmartCharacter char1, SmartCharacter char2)
    {
        return new LeafAffordance("TalkSecretively", char1, char2);
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [StateRequired(1, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [HideInGUI(3)]
    public TalkSecretivelyCrowd(SmartCharacter char1, SmartCharacter char2)
        : base(char1, char2) { }
}

/// <summary>
/// The first character calls the second to come over, and then they happily converse.
/// </summary>
[LibraryIndex(3)]
public class CallAndConverseCrowd : GenericEvent<SmartCharacter, SmartCharacter>
{
    protected override Node Root(Token token, SmartCharacter caller, SmartCharacter callee)
    {
        return new Sequence(
            caller.Node_OrientTowards(callee.transform.position),
            new SequenceParallel(
                caller.Behavior.ST_PlayHandGesture("callover", 3000),
                new Sequence(
                    new LeafWait(1300),
                    callee.Node_OrientTowards(caller.transform.position))),
            new LeafAffordance("TalkHappily", callee, caller));
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [StateRequired(1, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [HideInGUI(3)]
    public CallAndConverseCrowd(SmartCharacter caller, SmartCharacter callee)
        : base(caller, callee) { }
}

/// <summary>
/// Character sits down, does something interesting and then stands up again.
/// </summary>
[LibraryIndex(3)]
public class SitAndStand : GenericEvent<SmartCharacter, SmartChair>
{

    private RunStatus Cleanup(SmartCharacter user)
    {
        RunStatus result = user.Character.StandUp();
        if (result != RunStatus.Running)
            return user.Character.NavStop();
        else
        {
            user.Character.NavStop();
            return result;
        }
    }

    protected override Node Root(Token token, SmartCharacter user, SmartChair chair)
    {
        return new DecoratorCatch(
            //() => user.Character.StandUp(),
            () => Cleanup(user),
            new Sequence(
                new LeafAffordance("Sit", user, chair),
                //new LeafAffordance("Sit", user, chair),
                    new DecoratorCatch(
                        () => ReusableCleanup.TryAttach(user.HoldPropHidden, user.HoldPropRightHand),
                        new Sequence(
                            new LeafWait(10000))),
                new LeafAffordance("Stand", user, chair)));
    }

    [StateRequired(0, StateName.RoleActor, ~StateName.RoleTeller, ~StateName.RoleManager,
        ~StateName.RoleGuard1, ~StateName.RoleGuard2, ~StateName.RoleGuard3, ~StateName.RoleGuard4,
        StateName.IsStanding, ~StateName.RightHandOccupied, ~StateName.IsIncapacitated, ~StateName.NotCrowdEligible)]
    [StateRequired(1, StateName.RoleChair)]
    [HideInGUI(3)]
    public SitAndStand(SmartCharacter user, SmartChair chair)
        : base(user, chair) { }
}