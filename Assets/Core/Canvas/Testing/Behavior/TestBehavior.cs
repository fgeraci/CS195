using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using TreeSharpPlus;

public class TestBehavior : MonoBehaviour 
{
    private int ticks = 0;
    private bool first = false;

    public TestSmartObject obj1;
    public TestSmartObject obj2;
    public TestSmartObject obj3;
    public TestSmartAgent agent;

    void Update()
    {
        if (this.first == true)
            return;

        this.TestAgentTerminate();
        this.TestSingleEvent();
        this.TestEventUsurp1();
        this.TestEventUsurp2();
        this.TestEventUsurp3();

        this.first = true;
    }

    private static void Readout(
        string text,
        IHasBehaviorEvent[] events, 
        params IHasBehaviorObject[] objects)
    {
        string output = text + "\n\n";
        foreach (IHasBehaviorObject obj in objects)
        {
            output += obj.Behavior.Token.ToString() + ": " + obj.Behavior.Status;

            output += " C: ";
            if (obj.Behavior.CurrentEvent == null)
                output += "(null)";
            else
                output += obj.Behavior.CurrentEvent.Token;

            output += " P: ";
            if (obj.Behavior.PendingEvent == null)
                output += "(null)";
            else
                output += obj.Behavior.PendingEvent.Token;

            output += "\n";
        }
        foreach (IHasBehaviorEvent evt in events)
            output += evt.Behavior.Token.ToString() 
                + ": " + evt.Behavior.Status + "\n";
        Debug.Log(output);
    }

    private static void Readout(
        string text,
        params IHasBehaviorObject[] objects)
    {
        string output = text + "\n\n";
        foreach (IHasBehaviorObject obj in objects)
            output += obj.Behavior.Token.ToString() 
                + ": " + obj.Behavior.Status + "\n";
        Debug.Log(output);
    }

    private void Tick(
        int count, 
        IHasBehaviorEvent[] events,
        params IHasBehaviorObject[] objects)
    {
        for (int i = 0; i < count; i++)
        {
            BehaviorManager.Instance.Update(Time.fixedDeltaTime);
            Readout("Tick " + this.ticks++ + " results", events, objects);
        }
    }

    private void Tick(
        int count,
        params IHasBehaviorObject[] objects)
    {
        for (int i = 0; i < count; i++)
        {
            BehaviorManager.Instance.Update(Time.fixedDeltaTime);
            Readout("Tick " + this.ticks++ + " results", objects);
        }
    }

    void TestAgentTerminate()
    {
        // Make sure the agent should start, stop, and restart appropriately

        Debug.Log("TestAgentTerminate");
        this.ticks = 0;

        Readout("Beginning", this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);

        this.agent.Behavior.StartBehavior();
        Readout("Agent.StartBehavior() Results", this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);

        this.Tick(4, this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);

        this.agent.Behavior.StopBehavior();
        Readout("Agent.StopBehavior() Results", this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Terminating);

        this.Tick(3, this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Idle);

        this.Tick(1, this.agent);

        this.agent.Behavior.StartBehavior();
        Readout("Agent.StartBehavior() Results", this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);

        this.Tick(4, this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);

        this.agent.Behavior.StopBehavior();
        Readout("Agent.StopBehavior() Results", this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Terminating);

        this.agent.Behavior.StartBehavior();
        Readout("Agent.StartBehavior() Results", this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Restarting);

        this.Tick(1, this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Restarting);

        this.agent.Behavior.StartBehavior();
        Readout("Agent.StartBehavior() Results", this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Restarting);

        this.agent.Behavior.StopBehavior();
        Readout("Agent.StopBehavior() Results", this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Terminating);

        this.Tick(1, this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Terminating);

        this.agent.Behavior.StartBehavior();
        Readout("Agent.StartBehavior() Results", this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Restarting);

        this.Tick(1, this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);

        this.Tick(3, this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);

        this.agent.Behavior.StopBehavior();
        Readout("Agent.StopBehavior() Results", this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Terminating);

        this.Tick(3, this.agent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Idle);

        this.Tick(2, this.agent);

        Debug.LogWarning("TestAgentTerminate Passed!");
    }

    void TestSingleEvent()
    {
        // The agent should begin ticking. Then the event should start and pre-empt the agent.
        // Then the event should end and the agent should resume ticking.

        Debug.Log("TestSingleEvent");

        TestSmartEvent event1 = new TestSmartEvent(obj1, agent);
        event1.Name = "Event1";

        Readout("Beginning", new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running || this.agent.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Instantiated);

        this.agent.StartBehavior();
        Readout("Agent.StartBehavior() Results", new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Instantiated);

        this.Tick(2, new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Instantiated);

        event1.StartEvent(0.1f);
        Readout("event1.StartEvent() Results", new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Initializing);

        this.Tick(1, new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Pending);

        this.Tick(1, new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Terminating);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Pending);

        this.Tick(1, new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Terminating);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Pending);

        this.Tick(1, new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Pending);

        this.Tick(1, new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);

        this.Tick(7, new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);

        this.Tick(1, new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Detaching);

        this.Tick(1, new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);

        this.Tick(2, new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);

        this.Tick(2, new IHasBehaviorEvent[] { event1 }, this.obj1, this.agent);

        Debug.LogWarning("TestSingleEvent Passed!");
    }

    void TestEventUsurp1()
    {
        // The agent should begin ticking. The Event 1 should start and pre-empt the agent.
        // Event 2 should pre-empt Event 1. Event 3 should pre-empt Event 2 before Event 2
        // can start. Event 1 should terminate properly, then Event 3 should execute fully.

        Debug.Log("TestEventUsurp");
        this.ticks = 0;

        TestSmartEvent event1 = new TestSmartEvent(obj1, agent);
        event1.Name = "Event1";

        TestSmartEvent event2 = new TestSmartEvent(obj1, obj2);
        event2.Name = "Event2";

        TestSmartEvent event3 = new TestSmartEvent(obj1, obj2);
        event3.Name = "Event3";

        Readout("Beginning", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.agent.StartBehavior();
        Readout("Agent.StartBehavior() Results", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(2, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        event1.StartEvent(0.1f);
        Readout("event1.StartEvent() Results", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Initializing);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == event1.Behavior);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Terminating);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == event1.Behavior);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(2, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == event1.Behavior);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        this.Tick(2, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        event2.StartEvent(0.2f);
        Readout("event2.StartEvent() Results", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Initializing);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        event3.StartEvent(0.3f);
        Readout("event3.StartEvent() Results", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Initializing);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        // Tick 10
        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Detaching);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Initializing);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);


        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Initializing);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Terminating);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Terminating);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        // Tick 15
        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Detaching);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(8, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Detaching);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(2, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(2, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);

        Debug.LogWarning("TestEventUsurp1 Passed!");
    }

    void TestEventUsurp2()
    {
        // The agent should begin ticking. The Event 1 should start and pre-empt the agent.
        // Event 2 should pre-empt Event 1. Event 3 should pre-empt Event 2 before Event 2
        // can start. Event 1 should terminate properly, then Event 3 should execute fully.

        Debug.Log("TestEventUsurp");
        this.ticks = 0;

        TestSmartEvent event1 = new TestSmartEvent(obj1, agent);
        event1.Name = "Event1";

        TestSmartEvent event2 = new TestSmartEvent(obj1, obj2);
        event2.Name = "Event2";

        TestSmartEvent event3 = new TestSmartEvent(obj1, obj2);
        event3.Name = "Event3";

        Readout("Beginning", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);

        this.agent.StartBehavior();
        Readout("Agent.StartBehavior() Results", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);

        this.Tick(2, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);

        event1.StartEvent(0.1f);
        Readout("event1.StartEvent() Results", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);

        this.Tick(7, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);

        // Everything prior to this point it the same as TestEventUsurp()

        event2.StartEvent(0.2f);
        Readout("event2.StartEvent() Results", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Initializing);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        Debug.Log("Here");

        // Tick 10
        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Terminating);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        this.Tick(2, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Detaching);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        event3.StartEvent(0.3f);
        Readout("event3.StartEvent() Results", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Detaching);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Initializing);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Detaching);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Initializing);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event2.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Initializing);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        // Tick 15
        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(7, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Detaching);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        // Tick 25
        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(2, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(2, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);

        Debug.LogWarning("TestEventUsurp2 Passed!");
    }

    void TestEventUsurp3()
    {
        // The agent should begin ticking. The Event 1 should start and pre-empt the agent.
        // Event 2 should pre-empt Event 1. Event 3 should pre-empt Event 2 before Event 2
        // can start. Event 1 should terminate properly, then Event 3 should execute fully.

        Debug.Log("TestEventUsurp");
        this.ticks = 0;

        TestSmartEvent event1 = new TestSmartEvent(obj1, agent);
        event1.Name = "Event1";

        TestSmartEvent event2 = new TestSmartEvent(obj1, obj2);
        event2.Name = "Event2";

        TestSmartEvent event3 = new TestSmartEvent(obj1, obj2);
        event3.Name = "Event3";

        Readout("Beginning", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);

        this.agent.StartBehavior();
        Readout("Agent.StartBehavior() Results", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);

        this.Tick(2, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);

        event1.StartEvent(0.1f);
        Readout("event1.StartEvent() Results", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);

        this.Tick(7, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);

        // Same as the other two Usurp tests

        event2.StartEvent(0.2f);
        Readout("event2.StartEvent() Results", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Initializing);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Instantiated);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        event3.StartEvent(0.3f);
        Readout("event3.StartEvent() Results", new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Initializing);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Initializing);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Terminating);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        // Tick 10
        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Terminating);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Detaching);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Terminating);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Detaching);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event1.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == event1.Behavior);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Pending);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);


        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        // Tick 15
        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(6, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Running);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.InEvent);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Detaching);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == event3.Behavior);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(1, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(2, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);
        DebugUtil.Assert(this.obj1.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.obj2.Behavior.Status == BehaviorStatus.Idle);
        DebugUtil.Assert(this.agent.Behavior.Status == BehaviorStatus.Running);
        DebugUtil.Assert(event1.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event2.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(event3.Behavior.Status == EventStatus.Finished);
        DebugUtil.Assert(this.obj1.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.agent.Behavior.PendingEvent == null);
        DebugUtil.Assert(this.obj1.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.obj2.Behavior.CurrentEvent == null);
        DebugUtil.Assert(this.agent.Behavior.CurrentEvent == null);

        this.Tick(2, new IHasBehaviorEvent[] { event1, event2, event3 }, this.obj1, this.obj2, this.agent);

        Debug.LogWarning("TestEventUsurp3 Passed!");
    }
}
