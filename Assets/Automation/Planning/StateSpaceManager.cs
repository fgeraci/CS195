using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Reflection;
using System.Linq;

public class StateSpaceManager {

	public IEnumerable<EventSignature> allEvents;
	public List<Transition> transitionListAll;
	public List<Transition> transitionListReduced;
	public List<Transition> endTransitions;
	public PlanSpace option;
	public bool initalized = false;

	public int noOfObjects;
	public SmartObject[] allSmartObjects;
	public DefaultState globalState;
	public DefaultState initalState;
	public List<EventID> IDsOfPlannedEvents;
	[HideInInspector]
	private StateName[] immutables;

    List<ReadOnlyPrototype> prototypeList;
	List<ReadOnlyPrototype> reducedPrototypeList;

	
	public StateSpaceManager() {
	}

	/// <summary>
	/// Initalizes the StateSpaceManager with all the events and objects in the scene.
	/// </summary>
	public void init()
	{
		allEvents = EventLibrary.Instance.GetSignatures();
        prototypeList = new List<ReadOnlyPrototype>();
		reducedPrototypeList = new List<ReadOnlyPrototype> ();
		transitionListAll = new List<Transition>();
		transitionListReduced = new List<Transition> ();
		allSmartObjects = ObjectManager.Instance.GetObjects().ToArray();
		noOfObjects = allSmartObjects.Length;
		for (int i=0; i<noOfObjects; i++)
		{
			prototypeList.Add(new ReadOnlyPrototype(allSmartObjects[i].State.AsReadOnly()));
		}
		globalState = new DefaultState(prototypeList.ToArray());
		initalState = new DefaultState (globalState);
		immutables = StateDefs.GetRoleStates ().ToArray();
		transitionListAll = createTransitionListWithSmartObjectSet (allSmartObjects);
		IDsOfPlannedEvents = new List<EventID> ();
		initalized = true;
	}

	/// <summary>
	/// The Objects in the world which are considered for planning.
	/// </summary>
	public List<ReadOnlyPrototype> theObjectSpace
	{
		get
		{
			if(option==PlanSpace.All)
				return globalState.worldstate.Prototypes.ToList();
			else
			{
				if(reducedPrototypeList!=null)
					return reducedPrototypeList;
				else
					Debug.LogError("You did not specify a reduced Object Space. All Objects are considered instead");
				return globalState.worldstate.Prototypes.ToList();
			}
		}
	}

	public void resetGlobalState()
	{
		globalState = new DefaultState (initalState);
		Rules.ClearCache ();
	}

	/// <summary>
	/// Changes the global state of the statespacemanager to the state after executing the transition "evnt".
	/// </summary>
	public void changeStateToPostconditionsOf(Transition evnt)
	{
		this.globalState = getGlobalStateAfterEvent (this.globalState, evnt);
		Rules.ClearCache ();
	}

	public DefaultState getGlobalStateAfterEvent(DefaultState curState, Transition transition)
	{
		return new DefaultState(curState.worldstate.Transform(transition.eventSig, transition.parameterIDs));
	}

	/// <summary>
	/// Returns the State which satisfies the preconditions of all events in a beat "beat" with the minimal number of bit changes from worldstate "curState".
	/// Out value "punishCost" is 10 for each event in the beat where the implicit parameters could not be set.
	/// </summary>
	public DefaultState getGlobalStateBeforeBeat(DefaultState curState, StoryBeat beat, out int punishCost)
	{
		punishCost = 0;
		DefaultState newState = new DefaultState(curState);
		foreach(StoryEvent e in beat.Events) {
			Transition implicitPopulated = new Transition(e);
			if(!implicitPopulated.populate(globalState.worldstate))
			{
				punishCost += 10;
				newState = getGlobalStateBeforeEvent(newState, implicitPopulated);
			} else
				getGlobalStateBeforeImplicitsPopulatedEvent(newState, implicitPopulated);
		}
		return newState;
	}

	/// <summary>
	/// Returns the State which satisfies the preconditions of a complete event "transition" where the implicit parameters
	/// are already populated with the minimal number of bit changes from worldstate "curState".
	/// </summary>
	public DefaultState getGlobalStateBeforeImplicitsPopulatedEvent(DefaultState curState, Transition transition)
	{
		IList<ReadOnlyPrototype> newState = curState.worldstate.Prototypes;
		EditablePrototype changing;
		foreach (StateCondition scond in transition.eventSig.StateReqs)
		{
			int index = curState.worldstate.GetIndexById(transition.eventIndexToID(scond.Index));
			changing = new EditablePrototype(newState[index]);
			changing.Set (scond.Tags);
			newState[index] = changing.AsReadOnly();
		}
		foreach (RelationCondition rcond in transition.eventSig.RelationReqs)
		{
			int indexFrom = curState.worldstate.GetIndexById(transition.eventIndexToID(rcond.IndexFrom));
			changing = new EditablePrototype(newState[indexFrom]);
			changing.Set (transition.eventIndexToID(rcond.IndexTo), rcond.Tags);
			newState[indexFrom] = changing.AsReadOnly();
		}
		return new DefaultState(newState.Cast<IHasState>());
	}

	/// <summary>
	/// Returns the State which satisfies the preconditions of a complete event "transition" where the implicit parameters
	/// are not populated yet with the minimal number of bit changes from worldstate "curState".
	/// </summary>
	public DefaultState getGlobalStateBeforeEvent(DefaultState curState, Transition transition)
	{
		IList<ReadOnlyPrototype> newState = curState.worldstate.Prototypes;
		EditablePrototype changing;

		foreach (StateCondition scond in transition.eventSig.StateReqs)
		{
			if(scond.Index < transition.eventSig.ExplicitParameterCount)
			{
				int index = curState.worldstate.GetIndexById(transition.eventIndexToID(scond.Index));
				changing = new EditablePrototype(newState[index]);
				changing.Set (scond.Tags);
				newState[index] = changing.AsReadOnly();
			}
		}
		foreach (RelationCondition rcond in transition.eventSig.RelationReqs)
		{
			if(rcond.IndexFrom < transition.eventSig.ExplicitParameterCount && rcond.IndexTo < transition.eventSig.ExplicitParameterCount)
			{
				int indexFrom = curState.worldstate.GetIndexById(transition.eventIndexToID(rcond.IndexFrom));
				changing = new EditablePrototype(newState[indexFrom]);
				changing.Set (transition.eventIndexToID(rcond.IndexTo), rcond.Tags);
				newState[indexFrom] = changing.AsReadOnly();
			}
		}
		return new DefaultState(newState.Cast<IHasState>());
	}

	/// <summary>
	/// Creates the relevant masks for a set of transitions.
	/// </summary>
	public RelevanceMask[] createRelevanceMasks(params Transition[] transitions)
	{
		List<RelevanceMask> masks = new List<RelevanceMask> ();
		foreach(Transition t in transitions)
		{
			int nrRoles = t.eventSig.ExplicitParameterCount;
			for (int i=0; i<nrRoles; i++)
				masks.Add(new RelevanceMask(t, i));
		}
		return masks.ToArray ();
	}

	/// <summary>
	/// Generates the list of all possible Transitions from the state "currentState". Which objects are allowed to be used in this transitions is depending on "option".
	/// </summary>
	public List<DefaultAction> generateTransitions(DefaultState currentState)
	{

		List<DefaultAction> transitions = new List<DefaultAction> ();
		List<Transition> optionList;
		switch(option)
		{
			case PlanSpace.Reduced: optionList = transitionListReduced;break;
			default: optionList = transitionListAll;break;
		}
		foreach (Transition trans in optionList)
		{
			Transition tran = new Transition(trans.eventSig, trans.parameterIDs);
			if(tran.populate(currentState.worldstate))
            {
				tran.state = getGlobalStateAfterEvent(currentState,tran);
				tran.cost = tran.eventSig.Cost;
				transitions.Add(tran);
			}
		}
		return transitions;
	}

	/// <summary>
	/// Creates a Transition list for Objects in "smartobjectset" and saves it to transitionListReduced.
	/// </summary>
	public void setListReduced(SmartObject[] smartobjectset)
	{
		reducedPrototypeList.Clear ();
		foreach(SmartObject o in smartobjectset)
		{
			reducedPrototypeList.Add (globalState.worldstate.GetPrototypeById(o.Id));
		}
		transitionListReduced = createTransitionListWithSmartObjectSet (smartobjectset);
		Debug.Log ("Transition list with reduced object space has length: " + transitionListReduced.Count ());
	}


	/// <summary>
	/// Creates a Transition list for Objects with ids and saves it to transitionListReduced.
	/// </summary>
	public void setListReduced(uint[] ids)
	{
		SmartObject[] smartobjectset = new SmartObject[ids.Length];
		for(int i=0; i< ids.Length; i++)
			smartobjectset[i] = ObjectManager.Instance.GetObjectById(ids[i]);
		setListReduced (smartobjectset);
	}
	
	/// <summary>
	/// Creates a Transition for every possible event in the given scene. For every event there are multiple Transitions with all
	/// possible argument-combinations, where arguments are only from the smartobjectset. These are returned in a transition list.
	/// </summary>
	List<Transition> createTransitionListWithSmartObjectSet(SmartObject[] smartobjectset)
	{
		List<Transition> transitionList = new List<Transition> ();
		foreach (EventSignature sig in allEvents)
		{
			if(sig.AffectsWorldState())
			{
				int length = sig.ExplicitParameterCount;
				List<SmartObject>[] possibleArguments = new List<SmartObject>[length];
                for (int i = 0; i < length; i++)
                {
                    possibleArguments[i] = new List<SmartObject>();
                    List<StateName> filter = new List<StateName>();
                    foreach (StateName s in immutables)
                    {
                        foreach (StateCondition scond in sig.StateReqs)
                        {
                            if (scond.Index == i && scond.Tags.Contains(s))
                                filter.Add(s);
                        }
                    }
                    possibleArguments[i] = Filter.ByState(smartobjectset, filter.ToArray()).Cast<SmartObject>().ToList();
                }

				//Create instances with all possible combinations of arguments
				IEnumerable<IEnumerable<SmartObject>> allCombs = CombinationHelper.CartesianProduct(possibleArguments);
				foreach (IEnumerable<SmartObject> args in allCombs)
				{
					if(CombinationHelper.ContainsDuplicates<SmartObject>(args))
						continue;
					Transition newTransition = new Transition(sig, args.ToArray());
					transitionList.Add(newTransition);
				}
			}
		}
		return transitionList;
	}

	/// <summary>
	/// Gets all possible populations for a event e that fulfill the required roles.
	/// </summary>
	public IEnumerable<IEnumerable<uint>> getRelaxedPopulations(StoryEvent e)
	{
		int length = e.Signature.ExplicitParameterCount;
		IEnumerable<IHasState> candidates = theObjectSpace.Cast<IHasState>();

		List<uint>[] possibleArguments = new List<uint>[length];
		for (int i = 0; i < length; i++)
		{
			possibleArguments[i] = new List<uint>();
			if(e.Participants[i] == uint.MaxValue)
			{
				List<StateName> filter = new List<StateName>();
				foreach (StateName s in immutables)
				{
					foreach (StateCondition scond in e.Signature.StateReqs)
					{
						if (scond.Index == i && scond.Tags.Contains(s))
							filter.Add(s);
					}
				}
				IEnumerable<IHasState> StateList = Filter.ByState(candidates, filter.ToArray());
				foreach(IHasState s in StateList)
					possibleArguments[i].Add (s.Id);
			} else {
				possibleArguments[i].Add (e.Participants[i]);
			}
		}


		IEnumerable<IEnumerable<uint>> eventpopulations = CombinationHelper.CartesianProduct(possibleArguments);
		IEnumerable<IEnumerable<uint>> noDuplicates = new List<IEnumerable<uint>> ();

		foreach(IEnumerable<uint> ep in eventpopulations)
		{
			if(!CombinationHelper.ContainsDuplicates(ep))
				((List<IEnumerable<uint>>)noDuplicates).Add(ep);
		}

		return noDuplicates;
	}

	/// <summary>
	/// Finds all the Subclasses of a class and returns them in a list
	/// </summary>
	List<TBaseType> FindSubClassesOf<TBaseType>()
	{   
		var baseType = typeof(TBaseType);
		var assembly = baseType.Assembly;
		Type[] types = assembly.GetTypes ();
		List<TBaseType> result = new List<TBaseType> ();
		foreach (Type t in types)
		{
			if(t.IsSubclassOf(baseType) && !t.IsAbstract)
				result.Add((TBaseType)Activator.CreateInstance(t));
		}

		return result;
	}
}

/// <summary>
/// A RelevanceMask holds the information on which bits in the worldstate are important for the object with a certain id.
/// </summary>
public class RelevanceMask {

	public uint id;
	public bool stateRelevant = false;
	public bool relationsRelevant = false;

	//Every Bit that should be considered for planning (is relevant), is a 1
	public long stateMask = 0L;
	private Dictionary<uint, long> relationMasks;

	/// <summary>
	/// Creates a Relevance mask for Transition t and its parameter i
	/// </summary>
	public RelevanceMask(Transition t, int i)
	{
		this.id = t.parameterIDs [i];
		relationMasks = new Dictionary<uint, long> ();
		foreach (StateCondition scond in t.eventSig.StateReqs)
		{
			if (scond.Index<t.eventSig.ExplicitParameterCount)
			{
				if (t.parameterIDs[scond.Index] == this.id)
				{
					stateRelevant = true;
					foreach (StateName tag in scond.Tags)
					{
						if(tag >= 0)
							stateMask |= (long)tag;
						else
							stateMask |= ~((long)tag);
					}
				}
			}
		}
		foreach(RelationCondition rcond in t.eventSig.RelationReqs)
		{
			if (rcond.IndexFrom < t.eventSig.ExplicitParameterCount && rcond.IndexTo < t.eventSig.ExplicitParameterCount)
			{
				if (t.parameterIDs[rcond.IndexFrom] == this.id)
				{
					relationsRelevant = true;
					relationMasks.Add(t.parameterIDs[rcond.IndexTo], 0L);
					foreach (RelationName tag in rcond.Tags)
					{
						if(tag >= 0)
							relationMasks[t.parameterIDs[rcond.IndexTo]] |= (long)tag;
						else
							relationMasks[t.parameterIDs[rcond.IndexTo]] |= ~((long)tag);
					}
				}
			}
		}
	}

	public long GetRelationBits(uint id)
	{
		long result;
		if (this.relationMasks.TryGetValue(id, out result) == false)
			return 0x0L;
		return result;
	} 

}

/// <summary>
/// A Transition is an event instance. Transitions are used for planning as the edges of the search graph to get from one world state to another one.
/// </summary>
public class Transition : DefaultAction {

	public EventSignature eventSig;
	public List<uint> parameterIDs;

	public uint eventIndexToID(int index) {
		return parameterIDs [index];
	}

	public Transition(EventSignature _eventSig, IEnumerable<uint> _parameters)
	{
		eventSig = _eventSig;
		parameterIDs = _parameters.ToList();
		if(_parameters.Count() < eventSig.ParameterCount)
		{
			for(int i =0; i < (eventSig.ParameterCount - _parameters.Count()); i++)
			{
				parameterIDs.Add(uint.MaxValue);
			}
		}
	}

	public Transition(EventSignature _eventSig, params SmartObject[] objs)
	{
		eventSig = _eventSig;
		parameterIDs = new List<uint>();
		if(objs.Length < eventSig.ParameterCount)
		{
			foreach(SmartObject o in objs)
				parameterIDs.Add(o.Id);
			for(int i =0; i < (eventSig.ParameterCount - objs.Length); i++)
				parameterIDs.Add(uint.MaxValue);
		} else
		{
			for(int i=0; i<objs.Length;i++)
				parameterIDs.Add (objs[i].Id);
		}
	}

	public Transition(StoryEvent ev)
	{
		eventSig = new EventSignature(ev.Signature);
		parameterIDs = new List<uint>();
		foreach(uint id in ev.Participants)
			parameterIDs.Add (id);
	}

	public Transition(Transition other)
	{
		base.state = other.state.Clone ();
		eventSig = new EventSignature(other.eventSig);
		parameterIDs = new List<uint>();
		foreach(uint id in other.parameterIDs)
			parameterIDs.Add (id);
	}
	
	public override DefaultAction Clone ()
	{
		return new Transition (this);
	}

	public StoryEvent ToStoryEvent()
	{
		return new StoryEvent (new EventSignature(eventSig), parameterIDs.ToArray());
	}

	/// <summary>
	/// Typechecks the parameter list to make sure the constructor
	/// can be properly called
	/// </summary>
    public bool CheckParameters(params EditablePrototype[] prots)
	{
		return eventSig.CheckTypes(prots);
	}
	
	/// <summary>
	/// Checks the authored state and relation requirements for the event
	/// </summary>
	public bool CheckRequirements(
        IList<IHasState> parameters,
        IList<IHasState> allWorldObjs)
	{
		return this.eventSig.CheckRequirements(parameters, allWorldObjs);
	}

	/// <summary>
	/// Populates the transition with its implicit parameter according to a worldstate.
	/// The Transition's explicit parameters need to be populated in order to populate the implicit ones.
	/// </summary>
	public bool populate (WorldState worldstate)
	{
		IEnumerable<IHasState> objsToConsider = worldstate.Prototypes.Cast<IHasState> ();
		IEnumerable<IHasState> allWorldObjs = objsToConsider;
		int explicitCount = eventSig.ExplicitParameterCount;
		int paraCount = eventSig.ParameterCount;
				
		Dictionary<int, IHasState> fixedSlots = new Dictionary<int, IHasState> ();
		for(int i=0; i < explicitCount; i++)
		{
			fixedSlots.Add (i,allWorldObjs.ElementAt(worldstate.GetIndexById(parameterIDs[i])));
		}
		IEnumerable<EventPopulation> populations = EventPopulator.GetValidPopulations (eventSig, objsToConsider, allWorldObjs, fixedSlots);
		Profiler.EndSample ();
		if(populations.Count() ==0) {
			return false;
		}
		IList<IHasState> parameters = populations.First().AsParams();
		parameterIDs.Clear ();
		for (int j=0; j < paraCount; j++)
		{
			parameterIDs.Add(parameters.ElementAt (j).Id);
		}
		return true;

	}
	
}

public static class CombinationHelper {

	/// <summary>
	/// Produces the Cartesian Product of an unknown number of lists
	/// </summary>
	public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
	{
		IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
		return sequences.Aggregate(
			emptyProduct,
			(accumulator, sequence) =>
			from accseq in accumulator
			from item in sequence
			select accseq.Concat(new[] {item}));
	}

	/// <summary>
	/// Returns whether a collection contains any duplicates
	/// </summary>
	public static bool ContainsDuplicates<T>(this IEnumerable<T> e)
	{
		var set = new HashSet<T>();
		foreach (var item in e)
		{
			if (!set.Add(item))
				return true;
		}
		return false;
	}
}


