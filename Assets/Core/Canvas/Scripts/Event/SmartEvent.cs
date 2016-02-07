using UnityEngine;
using System;
using TreeSharpPlus;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public abstract class SmartEvent : IHasBehaviorEvent
{
    public BehaviorEvent Behavior { get; protected set; }

    /// <summary>
    /// The function that produces the tree to execute during this event. If
    /// the constructor provided a token object (or if you manually set one to
    /// the BehaviorEvent object), it will be passed in through here.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public abstract Node BakeTree(Token token);

    /// <summary>
    /// Arbitrary data attached to this SmartEvent. This token is passed to
    /// the tree when it's baked during BakeTree.
    /// </summary>
    public Token Token
    {
        get { return this.Behavior.Token; }
        set { this.Behavior.Token = value; }
    }

    /// <summary>
    /// Constructs a new SmartEvent
    /// </summary>
    /// <param name="priority">The event priority, for pre-empting other 
    /// events</param>
    /// <param name="participants">The participant list. These participants
    /// will be co-opted by the behavior system when the event starts.</param>
    public SmartEvent(params SmartObject[] participants)
    {
        this.Behavior = 
            new BehaviorEvent(
                this.BakeTree, 
                participants);
    }

    /// <summary>
    /// Starts this SmartEvent.
    /// </summary>
    public void StartEvent(float priority)
    {
        if (this.Behavior != null)
            this.Behavior.StartEvent(priority);
    }

    /// <summary>
    /// Stops this SmartEvent. May take multiple ticks.
    /// </summary>
    public RunStatus StopEvent()
    {
        if (this.Behavior != null)
            return this.Behavior.StopEvent();
        return RunStatus.Success;
    }
}
