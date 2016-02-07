using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using TreeSharpPlus;

// No event library index since we don't want this in any library
public class TestSmartEvent : SmartEvent 
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

    [Name("MyTestEvent")]
    public TestSmartEvent(
        SmartObject obj1,
        SmartObject obj2)
        : base(obj1, obj2)
    {
        // Don't need to actually do anything with obj1 or obj2 since
        // they don't do anything during the event itself, just need
        // to occupy them
    }

    private IEnumerable<RunStatus> ThreeTickTerminate()
    {
        Debug.Log("Event Terminating ... (1)");
        yield return RunStatus.Running;

        Debug.Log("Event Terminating ... (2)");
        yield return RunStatus.Running;

        Debug.Log("Event Terminating ... (3)");
        yield return RunStatus.Success;
    }

    public override Node BakeTree(Token token)
    {
        return new DecoratorCatch((Func<RunStatus>)new IteratorRunner(this.ThreeTickTerminate).Run,
                new DecoratorLoop(8,
                    new LeafInvoke(() => Debug.Log(this.Name + " Ticking ..."))));
    }
}