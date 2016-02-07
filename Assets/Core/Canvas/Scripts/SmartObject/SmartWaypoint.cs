using UnityEngine;
using TreeSharpPlus;
using System;
using System.Collections;
using System.Collections.Generic;

public class SmartWaypoint : SmartObject
{
    public override string Archetype { get { return "SmartWaypoint"; } }

    /// <summary>
    /// Use this boolean to indicate that this SmartWaypoint was system generated, and
    /// thus needs to be persisted for Serialization.
    /// </summary>
    public bool IsSystemGenerated { get; private set; }

    /// <summary>
    /// Set this target if the objects should go to the set point instead of the transform
    /// of the waypoint itself.
    /// </summary>
    public Transform OtherTarget;

    private Transform target;

    /// <summary>
    /// Generates a SmartWaypoint at the given position with the role RoleWaypoint and sets
    /// SystemGenerated to true.
    /// </summary>
    public static SmartWaypoint GenerateWaypoint(Vector3 position)
    {
        GameObject newGO = new GameObject("Generated Waypoint");
        SmartWaypoint result = newGO.AddComponent<SmartWaypoint>();
        result.Set(StateName.RoleWaypoint);
        result.IsSystemGenerated = true;
        result.transform.position = position;
        return result;
    }

    void Awake()
    {
        base.Initialize(new BehaviorObject());
        target = OtherTarget == null ? this.transform : OtherTarget;
    }

    [Affordance]
    public Node Approach(SmartCharacter user)
    {
        return new Sequence(
            user.Node_Require(StateName.RoleActor, StateName.IsStanding),
            user.Node_GoTo(target.position));
    }
}
