using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Stores the required metadata on top of an EventDescriptor to actually
/// instantiate the event and run it.
/// </summary>
public class EventSignature : EventDescriptor
{
    private readonly ConstructorInfo ctorInfo;

    public EventSignature(ConstructorInfo ctorInfo)
        : base(
            GetName(ctorInfo),
            ctorInfo.DeclaringType,
            GetRequiredTypes(ctorInfo),
            GetSentiments(ctorInfo),
            GetAttributes<StateCondition, StateRequiredAttribute>(ctorInfo),
            GetAttributes<RelationCondition, RelationRequiredAttribute>(ctorInfo),
            GetAttributes<RuleCondition, RuleRequiredAttribute>(ctorInfo),
            GetAttributes<StateCondition, StateEffectAttribute>(ctorInfo),
            GetAttributes<RelationCondition, RelationEffectAttribute>(ctorInfo),
            GetCanEqual(ctorInfo))
    {
        this.ctorInfo = ctorInfo;

        // Make sure every parameter is assignable to SmartObject
        foreach (ParameterInfo info in this.ctorInfo.GetParameters())
            DebugUtil.Assert(
                typeof(IHasState).IsAssignableFrom(info.ParameterType));
    }

    /// <summary>
    /// Given an ordered list of ids of parameters, returns an enumeration
    /// of only the ids that are actually considered participants used by
    /// this event.
    /// </summary>
    public IEnumerable<uint> FilterNonParticipantIds(IList<uint> ids)
    {
        HashSet<int> nonParticipants = new HashSet<int>();
        NonParticipantAttribute[] attrs =
            this.GetAttributes<NonParticipantAttribute>();

        if (attrs.Length > 0)
        {
            foreach (int index in attrs[0].Indices)
                nonParticipants.Add(index);
            for (int i = 0; i < ids.Count; i++)
                if (nonParticipants.Contains(i) == false)
                    yield return ids[i];
        }
        else
        {
            foreach (uint id in ids)
                yield return id;
        }
    }

    /// <summary>
    /// Shallow-copies the EventSignature
    /// </summary>
    public EventSignature(EventSignature other)
        : base(other)
    {
        this.ctorInfo = other.ctorInfo;
    }

    /// <summary>
    /// Helper function to get all the attributes of the given type.
    /// </summary>
    public T[] GetAttributes<T>()
    {
        object[] attributes = 
            this.ctorInfo.GetCustomAttributes(typeof(T), false);
        T[] result = new T[attributes.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = (T)attributes[i];
        return result;
    }

    private static string[] GetSentiments(ConstructorInfo ctorInfo)
    {
      SentimentAttribute[] attributes =
          GetAttributes<SentimentAttribute>(ctorInfo);
      if (attributes.Length > 0)
        return attributes[0].Sentiments;
      return new string[] { };
    }

	public int ExplicitParameterCount
	{
		get 
		{ 
			IsImplicitAttribute[] i = GetAttributes<IsImplicitAttribute>();
			if(i.Length==0)
				return ParameterCount;
			else
				return i[0].FirstImplicitIndex;
		}
	}

    /// <summary>
    /// Returns a listing of the constructor's parameter types
    /// </summary>
    public IEnumerable<Type> GetParameters()
    {
        return this.RequiredTypes;
    }

    /// <summary>
    /// Instantiates the SmartEvent in this signature given a list of
    /// parameters. Checks to make sure the parameters fit both in terms of
    /// type and in terms of narrative requirements.
    /// </summary>
    public SmartEvent Create(IList<IHasState> parameters)
    {
        DebugUtil.Assert(this.CheckTypes(parameters, true) == true);
        // TODO: Can't actually assert this without having all of the objects
        //DebugUtil.Assert(this.CheckRequirements(parameters, true) == true);

        object[] paramArray = parameters.Cast<object>().ToArray();
        return (SmartEvent)this.ctorInfo.Invoke(paramArray);
    }

    /// <summary>
    /// Instantiates the SmartEvent in this signature given a population. 
    /// Checks to make sure the parameters fit both in terms of type and
    /// in terms of narrative requirements.
    /// </summary>
    public SmartEvent Create(EventPopulation population)
    {
        return this.Create(population.AsParams());
    }

    #region Internals
    /// <summary>
    /// Helper function to get all the attributes of the given type.
    /// </summary>
    protected static T[] GetAttributes<T>(ConstructorInfo ctorInfo)
    {
        object[] attributes = ctorInfo.GetCustomAttributes(typeof(T), false);
        T[] result = new T[attributes.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = (T)attributes[i];
        return result;
    }

    /// <summary>
    /// Generic function for fetching condition (requirement/effect) attributes
    /// </summary>
    protected static T[] GetAttributes<T, U>(ConstructorInfo ctorInfo)
        where T : ICondition
        where U : IConditionAttribute
    {
        U[] attrs = GetAttributes<U>(ctorInfo);
        Array.Sort(attrs, (i, j) => i.Order.CompareTo(j.Order));

        T[] results = new T[attrs.Length];

        for (int i = 0; i < attrs.Length; i++)
            results[i] = (T)attrs[i].Condition;

        return results;
    }

    /// <summary>
    /// Finds all of the CanEqualAttribute values and returns them in an array
    /// </summary>
    protected static int[] GetCanEqual(ConstructorInfo ctorInfo)
    {
        CanEqualAttribute[] attrs = 
            GetAttributes<CanEqualAttribute>(ctorInfo);
        List<int> values = new List<int>();
        foreach (CanEqualAttribute attr in attrs)
        {
            values.Add(attr.Index1);
            values.Add(attr.Index2);
            values.Add(attr.Index2);
            values.Add(attr.Index1);
        }
        return values.ToArray();
    }

    /// <summary>
    /// Fetches the NameAttribute for the given constructor signature
    /// </summary>
    private static string GetName(ConstructorInfo ctorInfo)
    {
        object[] attrs = 
            ctorInfo.GetCustomAttributes(typeof(NameAttribute), false);
		if(attrs.Length==0)
			return ctorInfo.DeclaringType.Name;

        if (attrs.Length == 0)
        {
            return ctorInfo.DeclaringType.Name;
        }
        return ((NameAttribute)attrs[0]).Name;
    }

    /// <summary>
    /// Gets the required types from a ConstructorInfo
    /// </summary>
    private static Type[] GetRequiredTypes(ConstructorInfo ctorInfo)
    {
        return ctorInfo.GetParameters().Convert(p => p.ParameterType).ToArray();
    }

    /// <summary>
    /// Checks all of the authored state requirements for the event
    /// </summary>
    private bool CheckStateReqs(
        IList<IHasState> parameters, 
        bool logError = false)
    {
        foreach (StateCondition stateReq in this.StateReqs)
        {
            IHasState obj =
                (IHasState)parameters[stateReq.Index];
            if (obj.Require(stateReq.Tags) == false)
            {
                if (logError == true)
                    Debug.LogError(stateReq);
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks all of the authored relation requirements for the event
    /// </summary>
    private bool CheckRelationReqs(
        IList<IHasState> parameters, 
        bool logError = false)
    {
        foreach (RelationCondition relReq in this.RelationReqs)
        {
            IHasState objFrom = (IHasState)parameters[relReq.IndexFrom];
            IHasState objTo = (IHasState)parameters[relReq.IndexTo];

            if (objFrom.Require(objTo.State.Id, relReq.Tags) == false)
            {
                if (logError == true)
                    Debug.LogError(relReq);
                return false;
            }
        }
        return true;
    }
    #endregion
}
