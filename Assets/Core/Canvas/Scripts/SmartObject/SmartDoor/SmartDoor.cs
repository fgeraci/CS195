using UnityEngine;
using TreeSharpPlus;

/// <summary>
/// A Smart Door, which allows a user to lock, unlock and guard it.
/// </summary>
public class SmartDoor : SmartObject 
{
    public override string Archetype
    {
        get { return this.GetType().Name; }
    }
    
    /// <summary>
    /// Waypoint to use when (un)locking from front.
    /// </summary>
    public Transform UnlockWaypointFront = null;
    
    /// <summary>
    /// Waypoint to use when (un)locking from rear.
    /// </summary>
    public Transform UnlockWaypointRear = null;

    /// <summary>
    /// Waypoint to use when guarding from front.
    /// </summary>
    public Transform GuardWaypointFront = null;
    
    /// <summary>
    /// Waypoint to use when guarding from rear.
    /// </summary>
    public Transform GuardWaypointRear = null;

    private SlidingDoor slidingDoor = null;

    private bool setLock = false;

    void Update()
    {
        if (this.setLock == false && this.StatusIcon != null)
        {
            bool isUnlocked =
                this.State.Require(new StateName[] { StateName.IsUnlocked });
            if (isUnlocked == false)
                this.StatusIcon.Icon = "lock";
            this.setLock = true;
        }

        if (this.slidingDoor == null)
        {
            this.slidingDoor = this.GetComponent<SlidingDoor>();
        }

        if (this.slidingDoor != null)
        {
            this.slidingDoor.Locked =
                !this.State.Require(new[] { StateName.IsUnlocked });
        }
    }

    /// <summary>
    /// Lets the user unlock the door from the front.
    /// </summary>
    [Affordance]
    protected Node UnlockFront(SmartCharacter user)
    {
        return new Sequence(
            user.ST_StandAtWaypoint(this.UnlockWaypointFront),
            user.Node_Icon("key"),
            user.Node_PlayHandGesture("EnterCode", 3200),
            user.Node_Icon(null),
            this.Node_Icon(null),
            this.Node_Set(StateName.IsUnlocked));
    }

    /// <summary>
    /// Lets the user unlock the door from the rear.
    /// </summary>
    [Affordance]
    protected Node UnlockRear(SmartCharacter user)
    {
        return new Sequence(
            user.ST_StandAtWaypoint(this.UnlockWaypointRear),
            user.Node_Icon("key"),
            user.Node_PlayHandGesture("EnterCode", 3200),
            user.Node_Icon(null),
            this.Node_Icon(null),
            this.Node_Set(StateName.IsUnlocked));
    }

    /// <summary>
    /// Lets the user lock the door from the front.
    /// </summary>
    [Affordance]
    protected Node LockFront(SmartCharacter user)
    {
        return new Sequence(
            user.ST_StandAtWaypoint(this.UnlockWaypointFront),
            user.Node_Icon("key"),
            user.Node_PlayHandGesture("EnterCode", 3200),
            user.Node_Icon(null),
            this.Node_Icon("lock"),
            this.Node_Set(~StateName.IsUnlocked));
    }

    /// <summary>
    /// Lets the user lock the door from the rear.
    /// </summary>
    [Affordance]
    protected Node LockRear(SmartCharacter user)
    {
        return new Sequence(
            user.ST_StandAtWaypoint(this.UnlockWaypointRear),
            user.Node_Icon("key"),
            user.Node_PlayHandGesture("EnterCode", 3200),
            user.Node_Icon(null),
            this.Node_Icon("lock"),
            this.Node_Set(~StateName.IsUnlocked));
    }

    /// <summary>
    /// Lets the user guard the door from the front.
    /// </summary>
    [Affordance]
    protected Node GuardFront(SmartCharacter user)
    {
        return new Sequence(
            user.Node_GoTo(this.GuardWaypointFront.position),
            user.Node_Orient(this.GuardWaypointFront.rotation),
            user.Node_Set(StateName.IsGuarding, StateName.IsImmobile),
            user.Node_Set(this.Id, RelationName.IsGuarding),
            this.Node_Set(StateName.IsGuarded));
    }

    /// <summary>
    /// Lets the user guard the door from the rear.
    /// </summary>
    [Affordance]
    protected Node GuardRear(SmartCharacter user)
    {
        return new Sequence(
            user.Node_GoTo(this.GuardWaypointRear.position),
            user.Node_Orient(this.GuardWaypointRear.rotation),
            user.Node_Set(StateName.IsGuarding, StateName.IsImmobile),
            user.Node_Set(this.Id, RelationName.IsGuarding),
            this.Node_Set(StateName.IsGuarded));
    }

    /// <summary>
    /// Lets the user stop guarding the door (no matter if guarding from front or rear).
    /// </summary>
    [Affordance]
    protected Node StopGuarding(SmartCharacter user)
    {
        return new Sequence(
            user.Node_Set(~StateName.IsGuarding, ~StateName.IsImmobile),
            user.Node_Set(this.Id, ~RelationName.IsGuarding),
            this.Node_Set(~StateName.IsGuarded));
    }
}
