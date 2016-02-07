// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public interface ICondition { }

/// <summary>
/// A metadata holder describing the world state ramifications of an Event.
/// Does not hold enough information to actually execute the event. For that,
/// use EventSignature.
/// </summary>
public class EventDescriptor
{
    public readonly string Name;
    public readonly Type Type;
    public readonly Type[] RequiredTypes;
    public readonly string[] Sentiments;

    public readonly StateCondition[] StateReqs;
    public readonly RelationCondition[] RelationReqs;
    public readonly RuleCondition[] RuleReqs;

    public readonly StateCondition[] StateEffects;
    public readonly RelationCondition[] RelationEffects;

    public readonly StateName[][] StateRequirements;

    private readonly Dictionary<int, HashSet<int>> canEqual;
    public int[] CanEqualPairs
    {
        get
        {
            List<int> values = new List<int>();
            foreach (var pair in this.canEqual)
            {
                foreach (int val in pair.Value)
                {
                    values.Add(pair.Key);
                    values.Add(val);
                }
            }
            return values.ToArray();
        }
    }

    public int ParameterCount
    {
        get { return this.RequiredTypes.Length; }
    }

	public int Cost
	{
		get 
		{ 
			int i=0;
			foreach (StateCondition seffect in this.StateEffects) {
				i += seffect.Tags.Count();
			}
			foreach (RelationCondition reffect in this.RelationEffects) {
				i += reffect.Tags.Count();
			}
			return i;
		}
	}

    /// <summary>
    /// Creates a new EventDescriptor from metadata
    /// </summary>
    public EventDescriptor(
        string name,
        Type type,
        Type[] requiredTypes,
        string[] sentiments,
        StateCondition[] stateReqs,
        RelationCondition[] relationReqs,
        RuleCondition[] ruleReqs,
        StateCondition[] stateEffects,
        RelationCondition[] relationEffects,
        int[] canEqualPairs)
    {
        this.Name = name;

        this.Type = type;
        this.RequiredTypes = requiredTypes;
        this.Sentiments = sentiments;

        this.StateReqs = stateReqs;
        this.RelationReqs = relationReqs;
        this.RuleReqs = ruleReqs;
        this.StateEffects = stateEffects;
        this.RelationEffects = relationEffects;

        this.StateRequirements = this.CollectStateRequirements();
        this.canEqual = this.PopulateCanEqual(canEqualPairs);
    }

    /// <summary>
    /// Populates the CanEqual dictionary from an array of int pairs
    /// </summary>
    private Dictionary<int, HashSet<int>> PopulateCanEqual(int[] pairs)
    {
        Dictionary<int, HashSet<int>> dict = new Dictionary<int, HashSet<int>>();

        for (int i = 0; i < pairs.Length; i += 2)
        {
            int key = pairs[i];
            int value = pairs[i + 1];

            if (dict.ContainsKey(key) == false)
                dict[key] = new HashSet<int>();
            dict[key].Add(value);
        }

        return dict;
    }

    /// <summary>
    /// Deep-copies the EventDescriptor
    /// </summary>
    public EventDescriptor(EventDescriptor other)
    {
        this.Name = other.Name;
        this.Type = other.Type;
		this.RequiredTypes = other.RequiredTypes;

        this.StateRequirements = 
            other.StateRequirements.Convert(
                l => (StateName[])l.Clone()).ToArray();

        this.RequiredTypes = other.RequiredTypes;

        this.StateReqs = 
            (StateCondition[])other.StateReqs.Clone();
        this.RelationReqs = 
            (RelationCondition[])other.RelationReqs.Clone();
        this.RuleReqs =
            (RuleCondition[])other.RuleReqs.Clone();

        this.StateEffects = 
            (StateCondition[])other.StateEffects.Clone();
        this.RelationEffects = 
            (RelationCondition[])other.RelationEffects.Clone();

        this.canEqual = new Dictionary<int, HashSet<int>>();
        foreach (var pair in other.canEqual)
        {
            this.canEqual.Add(pair.Key, new HashSet<int>());
            foreach (int value in pair.Value)
                this.canEqual[pair.Key].Add(value);
        }
    }

    /// <summary>
    /// Returns true iff this event allows the two indexes to be equal
    /// </summary>
    public bool CanEqual(int index1, int index2)
    {
        HashSet<int> values;
        if (this.canEqual.TryGetValue(index1, out values) == false)
            return false;
        return values.Contains(index2);
    }

    /// <summary>
    /// Typechecks the parameter list to make sure the constructor
    /// can be properly called
    /// </summary>
    public bool CheckTypes(
        IList<IHasState> parameters,
        bool logError = false)
    {
        if (parameters.Count != this.ParameterCount)
            return false;

        for (int i = 0; i < parameters.Count; i++)
        {
            Type paramType = this.RequiredTypes[i];
            if (paramType.IsAssignableFrom(parameters[i].GetType()) == false)
            {
                if (logError == true)
                    UnityEngine.Debug.Log(i);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks state, relations, and rules on a given parameter set. Note that
    /// this function does not do typechecking. Use CheckTypes() first.
    /// </summary>
    /// <param name="parameters">The current parameter candidates</param>
    /// <param name="allWorldObjs">All of the objects in the world</param>
    /// <param name="logError">true if this function should complain</param>
    /// <returns>true iff the parameters can successfully satisfy this event
    /// with respect to their state</returns>
    public bool CheckRequirements(
        IList<IHasState> parameters,
        IEnumerable<IHasState> allWorldObjs,
        bool logError = false)
    {
        // UNCOMMENT FOR FEEDBACK ON EVENT FAILURE
#if !EXTERNAL
//        string names = "( ";
//        foreach (IHasState member in parameters)
//            names += ObjectManager.Instance.GetObjectById(member.Id).gameObject.name + " ";
//        UnityEngine.Debug.Log(names + ")");
//
//        bool state = this.CheckStateReqs(parameters, logError);
//        bool relations = this.CheckRelationReqs(parameters, logError);
//        bool rules = this.CheckRuleReqs(parameters, allWorldObjs, logError);
//        UnityEngine.Debug.Log(state + " " + relations + " " + rules);
#endif

        return
            this.CheckStateReqs(parameters, logError)
            && this.CheckRelationReqs(parameters, logError)
            && this.CheckRuleReqs(parameters, allWorldObjs, logError);
    }

    /// <summary>
    /// Checks to see if an object can fit in a particular index by type
    /// </summary>
    public bool CheckType(int index, object obj)
    {
        Type paramType = this.RequiredTypes[index];
        return paramType.IsAssignableFrom(obj.GetType());
    }

    /// <summary>
    /// Checks to see if an IHasState meets a slot's state requirements
    /// </summary>
    public bool CheckStateRequirements(
        int index,
        IHasState obj,
        IEnumerable<StateName> filter = null)
    {
        IList<StateName> requirements = this.StateRequirements[index];
        DebugUtil.Assert(
            requirements != null,
            "Requirements on slot " + index + " of " + this.Name);

        // Filter by the given states if we have any
        if (filter != null)
            requirements = Filter.StateList(requirements, filter);

        return obj.State.Require(requirements);
    }

    /// <summary>
    /// Given a list of objects, returns all of the objects that can fit
    /// in that slot according to their state
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IHasState> FilterByStateMatch(
        int index,
        IEnumerable<IHasState> candidates,
        IEnumerable<StateName> filter = null)
    {
        return candidates.Where(
            (IHasState o) => this.CheckStateRequirements(index, o, filter));
    }

    /// <summary>
    /// Returns true iff this event has effects on the world state
    /// </summary>
    public bool AffectsWorldState()
    {
        return (StateEffects.Count() > 0 || RelationEffects.Count() > 0);
    }

    #region Internals
    /// <summary>
    /// Collects all of the requirements for each parameter index
    /// </summary>
    private StateName[][] CollectStateRequirements()
    {
        List<List<StateName>> requirements =
            new List<List<StateName>>(this.ParameterCount);

        for (int i = 0; i < this.ParameterCount; i++)
            requirements.Add(new List<StateName>());

        foreach (StateCondition req in this.StateReqs)
        {
            // Make sure the index is valid
            DebugUtil.Assert(req.Index >= 0);
            DebugUtil.Assert(req.Index < this.ParameterCount);
            requirements[req.Index].AddRange(req.Tags);
        }

        return requirements.Convert(l => l.ToArray()).ToArray();
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
                    UnityEngine.Debug.LogError(stateReq);
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
                    UnityEngine.Debug.LogError(relReq);
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks all of the authored relation requirements for the event
    /// </summary>
    // TODO: Why is this public again? Should be private.
    public bool CheckRuleReqs(
        IList<IHasState> parameters,
        IEnumerable<IHasState> allWorldObjs,
        bool logError = false)
    {
        foreach (RuleCondition ruleReq in this.RuleReqs)
        {
            IHasState objFrom = (IHasState)parameters[ruleReq.IndexFrom];
            IHasState objTo = (IHasState)parameters[ruleReq.IndexTo];

            foreach (RuleName rule in ruleReq.Rules)
            {
                bool isRuleSafisfied =
                    Rules.IsRuleSatisfied(allWorldObjs, rule, objFrom, objTo);

                if (isRuleSafisfied == false)
                {
                    if (logError == true)
                        UnityEngine.Debug.LogError(ruleReq);
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Returns a formatted string of the required types
    /// </summary>
    private string PrintRequiredTypes()
    {
        string output = "(";
        for (int i = 0; i < this.RequiredTypes.Length; i++)
        {
            output += this.RequiredTypes[i];
            if (i < this.RequiredTypes.Length - 1)
                output += ", ";
        }
        return output + ")";
    }

    /// <summary>
    /// Returns a formatted string of the given types
    /// </summary>
    private string PrintTypes(IList<object> parameters)
    {
        string output = "(";
        for (int i = 0; i < parameters.Count; i++)
        {
            output += parameters[i].GetType();
            if (i < parameters.Count - 1)
                output += ", ";
        }
        return output + ")";
    }

    public override string ToString()
    {
        return this.Type.ToString() + this.PrintRequiredTypes();
    }
    #endregion
}
