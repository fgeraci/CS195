using UnityEngine;
using System.Collections;

public class TestSmartObject : SmartObject 
{
    public string Name;

    public override string Archetype
    {
        get { return "Test"; }
    }

    void Awake()
    {
        base.Initialize(new BehaviorObject());
        this.Behavior.Token = this;
    }
}
