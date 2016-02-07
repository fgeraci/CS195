using UnityEngine;
using TreeSharpPlus;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class AffordanceAttribute : Attribute
{
    public bool Reflexive { get; private set; }
    public AffordanceAttribute(bool reflexive = false)
    {
        this.Reflexive = reflexive;
    }
}

[RequireComponent(typeof(AssignId))]
public abstract class SmartObject : 
    MonoBehaviour, IHasBehaviorObject, IHasState, IHasEditableState
{
    public StatusIcon StatusIcon = null;
    public Texture2D Portrait = null;

    private class Affordance
    {
        public string name;
        public bool isReflexive;
        public Type requiredType;
        public MethodInfo method;

        public Affordance(
            string name,
            bool isReflexive,
            Type requiredType,
            MethodInfo method)
        {
            this.name = name;
            this.isReflexive = isReflexive;
            this.requiredType = requiredType;
            this.method = method;
        }

        public Node BakeTask(
            SmartObject parent,
            SmartObject obj)
        {
            return (Node)this.method.Invoke(parent, new[] { obj });
        }
    }

    public abstract string Archetype { get; }

    public BehaviorObject Behavior { get; protected set; }

    private EditableState state;

    public EditableState State 
    { 
        get { return this.state; } 
    }

    IState IHasState.State
    {
        get { return this.state; }
    }

    IEditableState IHasEditableState.State 
    { 
        get { return this.state; } 
    }

    public uint Id
    {
        get { return this.state.Id; }
    }

    void Awake()
    {
        this.Initialize(new BehaviorObject());
    }

    private Dictionary<string, Affordance> affordances = null;
    private Dictionary<string, Node> activeTasks = null;

    protected virtual void Initialize(BehaviorObject obj)
    {
        this.InitializeState();
        this.Register();
        this.RegisterAffordances();

        this.Behavior = obj;
        this.Behavior.StartBehavior();
    }

    protected void InitializeState()
    {
        AssignId id = this.GetComponent<AssignId>();
        this.state = new EditableState(id.Id);
    }

    protected void Register()
    {
        ObjectManager.Instance.RegisterSmartObject(this);
    }

    protected Node GetTask(string name)
    {
        if (this.activeTasks.ContainsKey(name) == true)
            return this.activeTasks[name];
        return null;
    }

    private RunStatus StartTask(string name, SmartObject user)
    {
        if (this.activeTasks.ContainsKey(name) == true)
            throw new ApplicationException(
                "Attempting to clobber existing active task");

        DebugUtil.Assert(
            this.affordances.ContainsKey(name), "Not found: " + name);
        Affordance affordance = this.affordances[name];

        if (affordance == null)
            throw new ApplicationException(
                "Attempting to activate nonexistent affordance");
        if (affordance.requiredType.IsAssignableFrom(user.GetType()) == false)
            throw new ApplicationException(
                "Wrong user type for affordance execution -- "
                + "Owner: " + this
                + ", User: " + user
                + ", Affordance: " + name);
        Node node = affordance.BakeTask(this, user);
        this.activeTasks[name] = node;
        node.Start();
        return RunStatus.Running;
    }

    private RunStatus TickTask(string name, Node node)
    {
        RunStatus result = node.Tick();
        if (result != RunStatus.Running)
            this.activeTasks.Remove(name);
        return result;
    }

    private void RegisterAffordances()
    {
        // Find all of the functions with the affordance attribute and 
        // bake them into the registry so they can be invoked
        this.affordances = new Dictionary<string, Affordance>();
        this.activeTasks = new Dictionary<string, Node>();

        MethodInfo[] methods =
            this.GetType().GetMethods(
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Instance);
        foreach (MethodInfo method in methods)
        {
            AffordanceAttribute affordance = null;
            foreach (object attr in method.GetCustomAttributes(false))
                if (attr is AffordanceAttribute)
                    affordance = (AffordanceAttribute)attr;

            if (affordance != null)
                this.AddAffordance(method, affordance);
        }
    }

    private void AddAffordance(
        MethodInfo method,
        AffordanceAttribute attr)
    {
        Type requiredType = method.GetParameters()[0].ParameterType;
        bool compatibleType = 
            typeof(SmartObject).IsAssignableFrom(requiredType);

        if (method == null
            || method.ReturnType != typeof(Node)
            || method.GetParameters().Length != 1
            || compatibleType == false)
            throw new ApplicationException(
                this.gameObject.name 
                + ": Wrong function signature for affordance");
        string name = method.Name;

        Affordance affordance = new Affordance(
            name,
            attr.Reflexive,
            requiredType,
            method);

        this.affordances.Add(name, affordance);
    }

    public IEnumerable<string> GetAffordances(SmartObject user)
    {
        foreach (Affordance affordance in this.affordances.Values)
            yield return affordance.name;
        yield break;
    }

    public RunStatus RunAffordance(string name, SmartObject user)
    {
        Node task = this.GetTask(name);
        if (task == null)
            return this.StartTask(name, user);
        return this.TickTask(name, task);
    }

    /// <summary>
    /// Starts this Smart Object's autonomous behavior, if any
    /// </summary>
    public void StartBehavior()
    {
        if (this.Behavior != null)
            this.Behavior.StartBehavior();
    }

    /// <summary>
    /// Stops this Smart Object's autonomous behavior, if any
    /// </summary>
    public RunStatus StopBehavior()
    {
        if (this.Behavior != null)
            return this.Behavior.StopBehavior();
        return RunStatus.Success;
    }

    /// <summary>
    /// Requires a collection of states be the given values
    /// </summary>
    public bool Require(params StateName[] states)
    {
        return this.State.Require(states);
    }

    /// <summary>
    /// Requires that we have the given values relative to the object id
    /// </summary>
    public bool Require(uint id, params RelationName[] relations)
    {
        return this.State.Require(id, relations);
    }

    /// <summary>
    /// Sets a collection of states to the given values
    /// </summary>
    public void Set(params StateName[] states)
    {
        this.State.Set(states);
    }

    /// <summary>
    /// Sets the icon string if we have a status icon holder
    /// </summary>
    public void SetIcon(string name)
    {
        Debug.Log("Setting icon to:" + name);
        if (this.StatusIcon != null)
            this.StatusIcon.Icon = name;
    }

    /// <summary>
    /// Sets the given values relative to the object id
    /// </summary>
    public void Set(uint id, params RelationName[] relations)
    {
        this.State.Set(id, relations);
    }

    /// <summary>
    /// Sets the icon above the object's head, if we have a billboard
    /// </summary>
    public Node Node_Icon(Val<string> name)
    {
        return new LeafInvoke(
            () => this.SetIcon((name == null) ? null : name.Value));
    }

    /// <summary>
    /// Requires a collection of states be the given values
    /// </summary>
    public Node Node_Require(params StateName[] states)
    {
        return new LeafAssert(() => this.State.Require(states));
    }

    /// <summary>
    /// Requires that we have the given values relative to the object id
    /// </summary>
    public Node Node_Require(uint id, params RelationName[] relations)
    {
        return new LeafAssert(() => this.State.Require(id, relations));
    }

    /// <summary>
    /// Requires a collection of states be the given values
    /// </summary>
    public Node Node_Set(params StateName[] states)
    {
        return new LeafInvoke(() => this.State.Set(states));
    }

    /// <summary>
    /// Requires that we have the given values relative to the object id
    /// </summary>
    public Node Node_Set(uint id, params RelationName[] relations)
    {
        return new LeafInvoke(() => this.State.Set(id, relations));
    }
}
