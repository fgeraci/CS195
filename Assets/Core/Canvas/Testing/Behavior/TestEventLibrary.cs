using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using TreeSharpPlus;

public class TestEventLibrary : MonoBehaviour 
{
    public SmartObject obj1;
    public SmartObject obj2;

	// Use this for initialization
	void Start() 
    {
        foreach (EventSignature sig in EventLibrary.Instance.GetSignatures())
            Debug.Log(sig.ToString());

        EventSignature sig2 = EventLibrary.Instance.GetSignaturesOfType(typeof(DummyEvent2)).First();
        EventSignature sig3 = EventLibrary.Instance.GetSignaturesOfType(typeof(DummyEvent3)).First();

        obj1.State.Set(new[] { StateName.RoleActor, StateName.IsStanding, StateName.HoldingBall });
        obj2.State.Set(new[] { StateName.RoleChair, StateName.IsOccupied });

        IHasState[] allObjs = new IHasState[] { obj1, obj2 };

        // Should fail because param 0 needs to be a float
        Debug.Log("sig2.Check(new object[] { obj1, obj1, obj2 }, true), should fail because of parameters...");
        Debug.Log(sig2.CheckTypes(new IHasState[] { obj1, obj1, obj2 }));
        Debug.Log(sig2.CheckRequirements(new IHasState[] { obj1, obj1, obj2 }, allObjs));

        Debug.Log("sig2.Check(new object[] { 0.1f, obj1, obj2 }, true), should fail because of state...");
        Debug.Log(sig2.CheckTypes(new IHasState[] { obj1, obj2 }));
        Debug.Log(sig2.CheckRequirements(new IHasState[] { obj1, obj1, obj2 }, allObjs));

        obj2.State.Set(new[] { ~StateName.IsOccupied });

        Debug.Log("sig2.Check(new object[] { 0.1f, obj1, obj2 }, true), should succeed now...");
        Debug.Log(sig2.CheckTypes(new IHasState[] { obj1, obj2 }));
        Debug.Log(sig2.CheckRequirements(new IHasState[] { obj1, obj1, obj2 }, allObjs));

        Debug.Log("sig3.Check(new object[] { 0.1f, obj1, obj2 }, true), should fail because of relation...");
        Debug.Log(sig3.CheckTypes(new IHasState[] { obj1, obj2 }));
        Debug.Log(sig2.CheckRequirements(new IHasState[] { obj1, obj1, obj2 }, allObjs));

        obj1.State.Set(obj2.Id, new[] { RelationName.IsFriendOf });

        Debug.Log("sig3.Check(new object[] { 0.1f, obj1, obj2 }, true), should succeed now...");
        Debug.Log(sig3.CheckTypes(new IHasState[] { obj1, obj2 }));
        Debug.Log(sig2.CheckRequirements(new IHasState[] { obj1, obj1, obj2 }, allObjs));
	}
}

// -1 for Debug
[LibraryIndexAttribute(-1)]
public class DummyEvent1 : SmartEvent
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

    // This should break because param 0 isn't a SmartObject
    [Name("DummyEvent1")]
    [StateRequired(0, ~StateName.IsOccupied, StateName.RoleActor)]
    [RelationRequired(0, 2, RelationName.IsFriendOf)]
    [RelationRequired(1, 3, RelationName.IsFriendOf)]
    public DummyEvent1(
        float foo,
        SmartObject obj1,
        SmartObject obj2)
        : base(obj1, obj2)
    {
    }

    public override Node BakeTree(Token token)
    {
        return
            new DecoratorLoop(3,
                new LeafInvoke(() => Debug.Log(this.Name + " Ticking ...")));
    }
}

[LibraryIndexAttribute(-1)]
public class DummyEvent2 : SmartEvent
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

    [Name("DummyEvent2")]
    [StateRequired(0, StateName.RoleActor, StateName.IsStanding)]
    [StateRequired(1, StateName.RoleChair, ~StateName.IsOccupied)]
    public DummyEvent2(
        SmartObject obj1,
        SmartObject obj2)
        : base(obj1, obj2)
    {
    }

    public override Node BakeTree(Token token)
    {
        return
            new DecoratorLoop(3,
                new LeafInvoke(() => Debug.Log(this.Name + " Ticking ...")));
    }
}

[LibraryIndexAttribute(-1)]
public class DummyEvent3 : SmartEvent
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

    [Name("DummyEvent3")]
    [RelationRequired(0, 1, RelationName.IsFriendOf)]
    public DummyEvent3(
        SmartObject obj1,
        SmartObject obj2)
        : base(obj1, obj2)
    {
    }

    public override Node BakeTree(Token token)
    {
        return
            new DecoratorLoop(3,
                new LeafInvoke(() => Debug.Log(this.Name + " Ticking ...")));
    }
}