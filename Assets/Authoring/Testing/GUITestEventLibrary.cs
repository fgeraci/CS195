using UnityEngine;
using TreeSharpPlus;

[LibraryIndexAttribute(-2)]
public class TestGoToEvent : SmartEvent
{
    SmartCharacter character;

    SmartWaypoint waypoint;

    [Name("Go To")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding)]
    [StateRequired(1, StateName.RoleWaypoint)]
    public TestGoToEvent(SmartCharacter character, SmartWaypoint waypoint)
        :base(character, waypoint)
    {
        this.character = character;
        this.waypoint = waypoint;
    }

    public override Node BakeTree(Token token)
    {
        return waypoint.Approach(character);
    }
}

[LibraryIndexAttribute(-2)]
public class TestSitEvent : SmartEvent
{
    SmartCharacter character;

    SmartChair chair;

    [Name("Sit Down")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding)]
    [StateRequired(1, StateName.RoleChair, ~StateName.IsOccupied)]
    [StateEffect(0, ~StateName.IsStanding)]
    [StateEffect(1, StateName.IsOccupied)]
    public TestSitEvent(SmartCharacter character, SmartChair chair)
        :base(character, chair)
    {
        this.character = character;
        this.chair = chair;
    }

    public override Node BakeTree(Token token)
    {
        return chair.Sit(character);
    }
}