using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using TreeSharpPlus;

public class TestSmartAgent : SmartObject
{
    public string Name;

    public override string Archetype
    {
        get { return "TestAgent"; }
    }

    public IEnumerable<RunStatus> ThreeTickTerminate()
    {
        Debug.Log("Terminating " + this.Name + "... (1)");
        yield return RunStatus.Running;

        Debug.Log("Terminating " + this.Name + "... (2)");
        yield return RunStatus.Running;

        Debug.Log("Terminating " + this.Name + "... (3)");
        yield return RunStatus.Success;
    }

    void Start()
    {
        base.Initialize(
            new BehaviorAgent(
                new DecoratorCatch((Func<RunStatus>)new IteratorRunner(this.ThreeTickTerminate).Run,
                    new DecoratorLoop(
                        new LeafInvoke(() => Debug.Log("Ticking " + this.Name + "..."))))));
        this.Behavior.Token = this;
    }
}



