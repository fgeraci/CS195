using TreeSharpPlus;
using System.Linq;
using UnityEngine;
using RootMotion.FinalIK;

// This file contains the narratively significant crowd events.

/// <summary>
/// Event where the crowd flees to a given point.
/// </summary>
[LibraryIndex(3, 4)]
public class Flee : CrowdEvent<SmartCharacter>
{
    private SmartWaypointArea area;

    public override Node BakeParticipantTree(SmartCharacter participant, object token)
    {
        float oldSpeed = 0.0f;
        SteeringController steering = participant.GetComponent<SteeringController>();

        return new Sequence(
            new LeafWait(Random.Range(0, 1000)),
            participant.Behavior.Node_BodyAnimation("duck", false),
            new LeafInvoke(() => oldSpeed = steering.maxSpeed),
            new LeafInvoke(() => steering.maxSpeed = 5.5f),
            new LeafInvoke(() => participant.Character.Body.NavGoTo(area.GetWaypoint().position)));
    }

    [StateRequired(0, StateName.RoleCrowd)]
    [StateRequired(1, StateName.RoleWaypoint)]
    [IsImplicit(1)]
    public Flee(SmartCrowd crowd, SmartWaypointArea waypoint)
        : base(crowd.GetObjectsByState(StateName.RoleActor, ~StateName.HoldingWeapon, ~StateName.RoleTeller, ~StateName.RoleManager, ~StateName.NotCrowdEligible).Cast<SmartCharacter>())
    {
        this.area = waypoint;
    }
}

/// <summary>
/// Event where the shooter fires a warning shot and the crowd reacts.
/// </summary>
[LibraryIndex(3)]
public class WarningShot : CrowdEvent<SmartCharacter>
{
    private SmartCharacter shooter;

    private GameObject explosion;

    public override Node BakeParticipantTree(SmartCharacter participant, object token)
    {
        if (participant != shooter)
        {
            return new Sequence(
                new LeafWait(Val.V(() => 
                    900 + (long)(participant.transform.position - shooter.transform.position).magnitude * 50)),
                participant.Behavior.Node_BodyAnimation("duck", true));
        }
        else
        {
            GameObject particles = null;

            return new Sequence(
                participant.Node_StartInteraction(FullBodyBipedEffector.RightHand, participant.InteractionPointGunUpwards),
                new LeafWait(1000),
                new LeafInvoke(() => participant.GetRightProp().CreateEffect(0)),
                new LeafWait(2000),
                participant.Node_StopInteraction(FullBodyBipedEffector.RightHand),
                new LeafInvoke(() => GameObject.Destroy(particles)));
        }
    }

    [StateRequired(0, StateName.RoleActor, StateName.IsStanding, StateName.HoldingWeapon, ~StateName.IsIncapacitated)]
    [StateRequired(1, StateName.RoleCrowd)]
    public WarningShot(SmartCharacter shooter, SmartCrowd crowd)
        : base(crowd.GetObjectsByState(StateName.RoleActor, ~StateName.HoldingWeapon, ~StateName.NotCrowdEligible).
                Cast<SmartCharacter>().Union(new SmartCharacter[] { shooter }))
    {
        this.shooter = shooter;
        foreach (SmartCrowd c in ObjectManager.Instance.GetObjectsByType<SmartCrowd>())
        {
            c.EventCollection.SetEnabled(false);
        }
        this.explosion = Resources.Load<GameObject>("WarningShot");
    }
}