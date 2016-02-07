using UnityEngine;
using System;
using System.Collections.Generic;

public interface IConditionAttribute
{
    ICondition Condition { get; }
    int Order { get; }
}

/// <summary>
/// Events are given a library index and will only show up in libraries
/// with the same index number. Events can have arbitrarily many indices.
/// </summary>
[AttributeUsage(
    AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class LibraryIndexAttribute : Attribute
{
    public readonly int[] Indexes;

    public LibraryIndexAttribute(params int[] indexes)
    {
        this.Indexes = indexes;
    }
}

/// <summary>
/// Use this attribute to distinguish EventSignatures if a SmartEvent has
/// more than one constructor.
/// </summary>
[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
public class NameAttribute : Attribute
{
    public readonly string Name;

    public NameAttribute(string name)
    {
        this.Name = name;
    }
}

[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = true)]
public class StateRequiredAttribute : Attribute, IConditionAttribute
{
    private int order = int.MaxValue;
    public int Order { get { return this.order; } set { this.order = value; } }

    public readonly StateCondition Condition;
    ICondition IConditionAttribute.Condition 
    { 
        get { return Condition; } 
    }

    public StateRequiredAttribute(int index, params StateName[] tags)
    {
        this.Condition = new StateCondition(index, tags);
    }
}

[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = true)]
public class RelationRequiredAttribute : Attribute, IConditionAttribute
{
    private int order = int.MaxValue;
    public int Order { get { return this.order; } set { this.order = value; } }

    public readonly RelationCondition Condition;
    ICondition IConditionAttribute.Condition
    {
        get { return Condition; }
    }

    public RelationRequiredAttribute(
        int indexFrom, 
        int indexTo,
        params RelationName[] tags)
    {
        this.Condition =
            new RelationCondition(indexFrom, indexTo, tags);
    }
}

[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = true)]
public class RuleRequiredAttribute : Attribute, IConditionAttribute
{
    private int order = int.MaxValue;
    public int Order { get { return this.order; } set { this.order = value; } }

    public readonly RuleCondition Condition;
    ICondition IConditionAttribute.Condition
    {
        get { return Condition; }
    }

    public RuleRequiredAttribute(
        int indexFrom,
        int indexTo,
        params RuleName[] tags)
    {
        this.Condition =
            new RuleCondition(indexFrom, indexTo, tags);
    }
}

[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = true)]
public class StateEffectAttribute : Attribute, IConditionAttribute
{
    private int order = int.MaxValue;
    public int Order { get { return this.order; } set { this.order = value; } }

    public readonly StateCondition Condition;
    ICondition IConditionAttribute.Condition
    {
        get { return Condition; }
    }

    public StateEffectAttribute(
        int index,
        params StateName[] tags)
    {
        this.Condition = new StateCondition(index, tags);
    }
}

[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = true)]
public class RelationEffectAttribute : Attribute, IConditionAttribute
{
    private int order = int.MaxValue;
    public int Order { get { return this.order; } set { this.order = value; } }

    public readonly RelationCondition Condition;
    ICondition IConditionAttribute.Condition
    {
        get { return Condition; }
    }

    public RelationEffectAttribute(
        int indexFrom,
        int indexTo,
        params RelationName[] tags)
    {
        this.Condition =
            new RelationCondition(indexFrom, indexTo, tags);
    }
}

/// <summary>
/// Use this attribute to specify which arguments of the event should not be seen as active
/// participants of the event (e.g. the LookAt target in a LookAt event).
/// </summary>
[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
public class NonParticipantAttribute : Attribute
{
    public readonly int[] Indices;

    public NonParticipantAttribute(params int[] indices)
    {
        this.Indices = indices;
    }
}

/// <summary>
/// An attribute to list all the event types with which the given event should be merged
/// in the GUI, so it is displayed as only one event. Does not have any effect outside of the GUI.
/// </summary>
[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
public class MergeAttribute : Attribute
{
    public readonly Type[] Types;

    public MergeAttribute(params Type[] types)
    {
        this.Types = types;
    }
}

/// <summary>
/// An attribute to indicate that the event at the target type has a Merge attribute containing
/// the attributed event, so the GUI can retarget to that event. Does not have any effect outside
/// of the GUI.
/// </summary>
[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
public class MergeAtAttribute : Attribute
{
    public readonly Type Target;

    public MergeAtAttribute(Type target)
    {
        this.Target = target;
    }
}

/// <summary>
/// An attribute to indicate that all the arguments starting at FirstImplicitIndex should be implicitly
/// set when running the events, instead of being authored e.g. in the GUI.
/// </summary>
[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
public class IsImplicitAttribute : Attribute
{
    public readonly int FirstImplicitIndex;

    public IsImplicitAttribute(int firstImplicitIndex)
    {
        this.FirstImplicitIndex = firstImplicitIndex;
    }
}

/// <summary>
/// An attribute to indicate that this event should not be shown in the event sidebar of the GUI, for
/// the event libraries of the given index.
/// </summary>
[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
public class HideInGUIAttribute : Attribute
{
    public readonly int[] Indices;

    public HideInGUIAttribute(params int[] indices)
    {
        this.Indices = indices;
    }
}

/// <summary>
/// An attribute indicating that the two indices can be the same (symmetric)
/// </summary>
[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = true)]
public class CanEqualAttribute : Attribute
{
    public readonly int Index1;
    public readonly int Index2;

    public CanEqualAttribute(int index1, int index2)
    {
        this.Index1 = index1;
        this.Index2 = index2;
    }
}

/// <summary>
/// An attribute to indicate the name of the thumbnail folder in the Resources folder to retrieve it.
/// </summary>
[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
public class IconNameAttribute : Attribute
{
    public readonly string Name;

    public IconNameAttribute(string name)
    {
        this.Name = name;
    }
}

/// <summary>
/// Use this attribute to specify the sentiment of an event (if applicable)
/// </summary>
[AttributeUsage(
    AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
public class SentimentAttribute : Attribute
{
  public readonly string[] Sentiments;

  public SentimentAttribute(params string[] sentiments)
  {
    this.Sentiments = sentiments;
  }
}