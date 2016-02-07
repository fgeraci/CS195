using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TreeSharpPlus;


public class DemoPlanning : MonoBehaviour {

	bool spawning = true;
	StateSpaceManager stateSpaceManager;
	EventPlanner ePlanner;
	Transition startEvent; 
	Transition endEvent;
	public SmartObject actor0, actor1, chair, table;


	// Use this for initialization
	void Awake () {
		stateSpaceManager = new StateSpaceManager ();
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown(KeyCode.P)) {

//			foreach (EventSignature sig in EventLibrary.Instance.GetSignatures())
//				Debug.Log(sig.ToString());

			actor0.Set (StateName.RoleActor, StateName.IsStanding);
			actor1.Set (StateName.RoleActor, StateName.IsStanding);
			chair.Set (StateName.RoleChair);
			table.Set (StateName.RoleTable, StateName.HoldingBall);

			ePlanner = new EventPlanner(stateSpaceManager); //, AuthoredEventManager.instance);

			EventSignature sitDown = EventLibrary.Instance.GetSignaturesOfType(typeof(SitDown)).First();
			EventSignature take = EventLibrary.Instance.GetSignaturesOfType(typeof(Take)).First();
			EventSignature give = EventLibrary.Instance.GetSignaturesOfType(typeof(Give)).First();
//			Transition startEvent = new Transition(sitDown, actor0.Id, chair.Id);
//			Transition endEvent = new Transition(sitDown, actor1.Id, chair.Id);

			StoryArc arc = new StoryArc(
								new StoryBeat(new StoryEvent(give, uint.MaxValue, uint.MaxValue)),
								new StoryBeat(new StoryEvent(take, uint.MaxValue, uint.MaxValue))
								);

			//arc = ePlanner.completeStoryArcUpTo(arc.Beats.Length, arc, null, PlanSpace.All);
			if(ePlanner.plan.Count>0)
				ePlanner.plan.Pop ();

			while(ePlanner.plan.Count>0) {
				Transition cur = (Transition)ePlanner.plan.Pop();
				Debug.Log(cur.eventSig.ToString() + "with arguments " + + cur.eventIndexToID(0)+ " "+ cur.eventIndexToID(1));
			}
			foreach(StoryBeat b in arc.Beats) {
				Debug.Log("new Beat:");
				foreach(StoryEvent e in b.Events)
					Debug.Log(e.Signature.ToString() + " with arguments: " + e.Participants[0] + e.Participants[1]);
			}
		}
	}
}

// 1 for Planning
[LibraryIndexAttribute(1)]
public class SitDown : SmartEvent
{
	public string Name
	{
		get 
		{
            return (string)Token.Get(this.Behavior.Token);
		}
		set
		{
			this.Behavior.Token = new Token(value);
		}
	}

	[Name("SitDown")]
	[StateRequired(0, StateName.IsStanding, StateName.RoleActor)]
	[StateRequired(1, ~StateName.IsOccupied, StateName.RoleChair)]
	[RelationRequired(0, 1, ~RelationName.IsSittingOn)]
	[StateEffect(0, ~StateName.IsStanding)]
	[StateEffect(1, StateName.IsOccupied)]
	[RelationEffect(0, 1, RelationName.IsSittingOn)]
	public SitDown(
		SmartObject obj1,
		SmartObject obj2)
		: base(obj1, obj2)
	{
	}

    public override Node BakeTree(Token token)
	{
		return new LeafInvoke(() => Debug.Log("Sitting down"));
	}
}

[LibraryIndexAttribute(1)]
public class StandUp : SmartEvent
{
	public string Name
	{
		get
		{
			return (string)Token.Get(this.Behavior.Token);
		}
		set
		{
			this.Behavior.Token = new Token(value);
		}
	}

	[Name("StandUp")]
	[StateRequired(0, StateName.RoleActor, ~StateName.IsStanding)]
	[StateRequired(1, StateName.RoleChair, StateName.IsOccupied)]
	[RelationRequired(0, 1, RelationName.IsSittingOn)]
	[StateEffect(0, StateName.IsStanding)]
	[StateEffect(1, ~StateName.IsOccupied)]
	[RelationEffect(0, 1, ~RelationName.IsSittingOn)]
	public StandUp(
		SmartObject obj1,
		SmartObject obj2)
		: base(obj1, obj2)
	{
	}

    public override Node BakeTree(Token token)
	{
		return new LeafInvoke(() => Debug.Log("Standing Up"));
	}
}

[LibraryIndexAttribute(1)]
public class Take : SmartEvent
{
    public string Name
    {
        get
        {
            return (string)Token.Get(this.Behavior.Token);
        }
        set
        {
            this.Behavior.Token = new Token(value);
        }
    }

	[Name("Take")]
	[StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingBall)]
	[StateRequired(1, StateName.RoleTable, StateName.HoldingBall)]
	[StateEffect(0, StateName.HoldingBall)]
	[StateEffect(1, ~StateName.HoldingBall)]
	public Take(
		SmartObject obj1,
		SmartObject obj2)
		: base(obj1, obj2)
	{
	}
	
	public override Node BakeTree(Token token)
	{
		return new LeafInvoke(() => Debug.Log("Taking object"));
	}
}

[LibraryIndexAttribute(1)]
public class Place : SmartEvent
{
    public string Name
    {
        get
        {
            return (string)Token.Get(this.Behavior.Token);
        }
        set
        {
            this.Behavior.Token = new Token(value);
        }
    }

	[Name("Place")]
	[StateRequired(0, StateName.RoleActor, StateName.IsStanding, StateName.HoldingBall)]
	[StateRequired(1, StateName.RoleTable, ~StateName.HoldingBall)]
	[StateEffect(0, ~StateName.HoldingBall)]
	[StateEffect(1, StateName.HoldingBall)]
	public Place(
		SmartObject obj1,
		SmartObject obj2)
		: base(obj1, obj2)
	{
	}

    public override Node BakeTree(Token token)
	{
		return new LeafInvoke(() => Debug.Log("Placing object"));
	}
}

[LibraryIndexAttribute(1)]
public class Give : SmartEvent
{
    public string Name
    {
        get
        {
            return (string)Token.Get(this.Behavior.Token);
        }
        set
        {
            this.Behavior.Token = new Token(value);
        }
    }

	[Name("Give")]
	[StateRequired(0, StateName.RoleActor, StateName.IsStanding, StateName.HoldingBall)]
	[StateRequired(1, StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingBall)]
	[StateEffect(0, ~StateName.HoldingBall)]
	[StateEffect(1, StateName.HoldingBall)]
	public Give(
		SmartObject obj1,
		SmartObject obj2)
		: base(obj1, obj2)
	{
	}

    public override Node BakeTree(Token token)
	{
		return new LeafInvoke(() => Debug.Log("Give object to other Character"));
	}
}

[LibraryIndexAttribute(1)]
public class Clap : SmartEvent
{
    public string Name
    {
        get
        {
            return (string)Token.Get(this.Behavior.Token);
        }
        set
        {
            this.Behavior.Token = new Token(value);
        }
    }
	
	[Name("Clap")]
	[StateRequired(0, StateName.RoleActor, StateName.IsStanding, ~StateName.HoldingBall)]
	public Clap(
		SmartObject obj1)
		: base(obj1)
	{
	}

    public override Node BakeTree(Token token)
	{
		return new LeafInvoke(() => Debug.Log("Clapping"));
	}
}

[LibraryIndexAttribute(1)]
public class EatBall : SmartEvent
{
    public string Name
    {
        get
        {
            return (string)Token.Get(this.Behavior.Token);
        }
        set
        {
            this.Behavior.Token = new Token(value);
        }
    }
	
	[Name("EatBall")]
	[StateRequired(0, StateName.RoleActor, StateName.IsStanding, StateName.HoldingBall)]
	[StateEffect(0, ~StateName.HoldingBall)]
	public EatBall(
		SmartObject obj1)
		: base(obj1)
	{
	}

    public override Node BakeTree(Token token)
	{
		return new LeafInvoke(() => Debug.Log("Give object to other Character"));
	}
}
