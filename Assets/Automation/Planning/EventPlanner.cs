using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeSharpPlus;

class EventPlanner
{
	//set the limits of the planner with these constants
	public const int TIMEOUT = 180;
	public const int MAXNODES = 5000;

	BestFirstSearchPlanner planner;
	List<PlanningDomainBase> domains;
	StateSpaceManager stateSpaceManager;
	StoryEvent[] startEvents;
	StoryEvent[] endEvents;
	Transition[] endTransitions;
	SmartObject[] startEndBeatParticipants;
	SmartObject[] storyArcParticipants;
	
	public List<EventID> IDs;
	public Stack<DefaultAction> plan;

	public EventPlanner(StateSpaceManager m)
	{
		planner = new BestFirstSearchPlanner ();
		domains = new List<PlanningDomainBase>();
		plan = new Stack<DefaultAction> ();
		stateSpaceManager = m;
		if(!stateSpaceManager.initalized)
			stateSpaceManager.init ();
		IDs = m.IDsOfPlannedEvents;
	}

	/// <summary>
	/// Plans through a whole StoryArc. The returned StoryArc keeps all the previous Events in it and is globally consistent. If that's not possible, an Error is thrown.
	/// </summary>
	public StoryArc planGlobal(StoryArc arc, IEnumerable<SmartObject> reducedObjectSpace, PlanSpace option, ref bool success)
	{
		return planGlobalUpTo (arc.Beats.Count (), arc, reducedObjectSpace, option, ref success);
	}

	/// <summary>
	/// Plans through a StoryArc up to level "planningUpTo". The returned StoryArc keeps all the previous Events in it and is globally consistent. If that's not possible, an Error is thrown.
	/// </summary>
	public StoryArc planGlobalUpTo(int planningUpTo, StoryArc arc, IEnumerable<SmartObject> reducedObjectSpace, PlanSpace option, ref bool success)
	{
		Debug.Log ("Start Planning Global"); 
		StoryArc newArc = new StoryArc (arc);

		//Stacks are used to save: The arcs we planned, from where we started, Beatlevel we plan to. So we can go back if something went wrong
		Stack<StoryArc> arcStack = new Stack<StoryArc> ();
		Stack<int> fromStack = new Stack<int> ();
		Stack<int> ToStack = new Stack<int> ();
		Stack<List<EventID>> IdListStack = new Stack<List<EventID>>();
		Stack<List<BeatPopulation>> BeatPopulationSetStack = new Stack<List<BeatPopulation>> ();
		Stack<int> xthTryStack = new Stack<int> ();

		List<StoryBeat> newBeats = new List<StoryBeat>();
		List<Transition> upgoing = new List<Transition> ();
		Transition cur;
		int maxFails = (planningUpTo * (planningUpTo - 1)) / 2;
		int maxTrys = 1;
		int nrFails = 0;
		int curPlanningFrom = 0;
		int curPlanningUpTo = planningUpTo;
		int curTry = 0;

		arcStack.Push (newArc);
		fromStack.Push (curPlanningFrom);
		ToStack.Push (curPlanningUpTo);
		
		xthTryStack.Push (curTry);

		domains.Add (new EventDomain (stateSpaceManager));
		EventDomain domain = (EventDomain)domains [0];

		if(PlanSpace.Reduced == option)
			stateSpaceManager.setListReduced (reducedObjectSpace.ToArray ());
		stateSpaceManager.option = option;
		BeatPopulationSetStack.Push (getBestPopulations(curPlanningFrom, arc, maxTrys));
		List<Transition> bestChoice = new List<Transition>();

		// while "from" isn't the last Beat yet we continue planning
		while(ToStack.Peek() != fromStack.Peek())
		{
			arc = arcStack.Peek();
			curPlanningFrom = fromStack.Peek();
			curPlanningUpTo = ToStack.Peek ();
			//--------------------------------
			//PLANNING WITH PARAMETERFILLING
			curTry = xthTryStack.Peek();
			StoryBeat BeatWithNewPop = BeatPopulationSetStack.Peek().ElementAt(curTry).ToStoryBeat();
			StoryBeat[] remainingBeats = arc.Beats;
			remainingBeats[curPlanningFrom] = BeatWithNewPop;
			arc = new StoryArc(remainingBeats);
			//--------------------------------

			//Convert the EndEvents from Events to Transitions
			endEvents = arc.Beats[curPlanningFrom].Events;
			endTransitions = new Transition[endEvents.Length];
			for (int i=0;i<endEvents.Length;i++)
				endTransitions[i] = new Transition(endEvents[i]);
			if(curTry==0)
				bestChoice = endTransitions.ToList();
			//Adding the relevant states of the end-Transitions, so heuristic only depending on these states can be used to compute plan
			domain.relevanceMasks = stateSpaceManager.createRelevanceMasks(endTransitions);
			planner.init (ref domains, MAXNODES);

			stateSpaceManager.resetGlobalState ();
			SimulateUpTo (stateSpaceManager, curPlanningFrom, arc);
			//Set start state to the state after the Beat we are planning from
			DefaultState startState = new DefaultState (stateSpaceManager.globalState);

			//end state is set to the state after the startEvents and the necessary preconditions changed
			DefaultState endState = new DefaultState (startState);
			List<RelevanceMask> relev = domain.relevanceMasks.ToList();

			//end state is changed according to the upgoing conditions for global consistency
			foreach(Transition t in upgoing)
			{
				endState = stateSpaceManager.getGlobalStateBeforeEvent(endState,t);
				foreach(RelevanceMask mask in stateSpaceManager.createRelevanceMasks(t))
						relev.Add(mask);
			}
			domain.relevanceMasks = relev.ToArray();

			//conditions from upgoing are overwritten if they conflict with preconditions for current beat
			foreach(Transition t in endTransitions)
			{
				endState = stateSpaceManager.getGlobalStateBeforeEvent(endState,t);
			}

			stateSpaceManager.endTransitions = endTransitions.ToList();
			foreach(Transition t in upgoing)
			{
				stateSpaceManager.endTransitions.Add(t);
			}

			plan.Clear();
			success = planner.computePlan (ref startState, ref endState, ref plan, TIMEOUT);

			//---------------------------------------------------------------
			if (success)
			{
				plan.Pop (); 		//pop start event
				upgoing.Clear();
				int oldNrBeats = arc.Beats.Count();
				int newNrEvents = plan.Count;
				//add Beats up to the starting level of planning
				for(int i=0; i<curPlanningFrom; i++)
				{
					newBeats.Add(arc.Beats[i]);
				}
				//add planned beats
				List<EventID> IdList = new List<EventID>();
				while(plan.Count>0)
				{
					cur = ((Transition)plan.Pop());
					newBeats.Add(new StoryBeat(cur.ToStoryEvent()));
					IdList.Add(newBeats.Last().Events.First().ID);
				}
				//add Beats after the planning
				for(int i=curPlanningFrom; i<oldNrBeats; i++)
				{
					newBeats.Add(arc.Beats[i]);
				}
				newArc = new StoryArc(newBeats.ToArray());
				IdListStack.Push(IdList);
				arcStack.Push(newArc);
				fromStack.Push(curPlanningFrom + 1 + newNrEvents);
				ToStack.Push (curPlanningUpTo + newNrEvents);
				if(fromStack.Peek() != ToStack.Peek())
					BeatPopulationSetStack.Push (getBestPopulations(fromStack.Peek(), newArc, maxTrys));
				xthTryStack.Push(0);
				newBeats.Clear();
				upgoing.Clear();
			}
			else
			{
				if(curTry + 1>=maxTrys || curTry >= BeatPopulationSetStack.Peek().Count-1)
				{
					nrFails++;
					//Propagate the Transitions we need to consider upwards if planning fails
					if (upgoing.Count == 0)
						foreach(Transition t in bestChoice)
							upgoing.Add (t);

					if(fromStack.Peek() > 0)
					{
						IdListStack.Pop ();
						fromStack.Pop ();
						arcStack.Pop();
						ToStack.Pop ();
						BeatPopulationSetStack.Pop();
						xthTryStack.Pop();
					}
					if(fromStack.Peek()==0 || nrFails>maxFails)
					{
						return arc;
					}
				}
				else {
					int temp = xthTryStack.Peek();
					temp++;
					xthTryStack.Pop();
					xthTryStack.Push (temp);
				}
			}
		}
		List<EventID> IDsFromOneLevel;
		while(IdListStack.Count > 0)
		{
			IDsFromOneLevel = IdListStack.Pop ();
			foreach(EventID i in IDsFromOneLevel)
				IDs.Add(i);
		}
		return arcStack.Peek();
	}

	/// <summary>
	/// Simulate the state of the StateSpaceManager up to a certain Beat (specified by level)
	/// </summary>
	public void SimulateUpTo(StateSpaceManager stateSpaceManager, int startLevel, StoryArc story)
	{
		stateSpaceManager.resetGlobalState ();
		for(int i=0; i< startLevel; i++)
		{	
			foreach(StoryEvent ev in story.Beats[i].Events)
			{
				Rules.ClearCache();
				Transition implicitPopulated = new Transition(ev);
				implicitPopulated.populate(stateSpaceManager.globalState.worldstate);
				stateSpaceManager.changeStateToPostconditionsOf(implicitPopulated);
			}
		}
	}
	/// <summary>
	/// Returns a list of the "x" best Beatpopulations for the storybeat in arc "arc" at position "curPlanningFrom".
	/// </summary>
	private List<BeatPopulation> getBestPopulations(int curPlanningFrom, StoryArc arc, int x)
	{
		SimulateUpTo(stateSpaceManager, curPlanningFrom, arc);
		DefaultState planningFrom = stateSpaceManager.globalState;
		List<BeatPopulation> pop = createBeatPopulationSet(arc.Beats[curPlanningFrom]);
		int i = 0;
		List<BeatPopulation> xBest = new List<BeatPopulation> ();
		foreach (BeatPopulation bp in pop)
		{
			StoryBeat distanceDummy = bp.ToStoryBeat();
			int punishCost; //This is used to punish Populations which don't satisfy the current WorldState
			DefaultState planningTo = stateSpaceManager.getGlobalStateBeforeBeat(planningFrom, distanceDummy, out punishCost);
			bp.cost = punishCost + domains.First().costBetweenWorldStates(ref planningFrom, ref planningTo, 0);
			if(bp.cost==0) {
				xBest.Add(bp);
				i++;
				if(i>=x)
					return xBest;
			}
		}
		pop = pop.OrderBy(o=>o.cost).ToList();
		return pop;
	}

	/// <summary>
	/// Returns the list of all Beatpopulations for a storybeat "b".
	/// </summary>
	private List<BeatPopulation> createBeatPopulationSet(StoryBeat b)
	{
		List<BeatPopulation> populationSet = new List<BeatPopulation>();
		StoryBeat beat = new StoryBeat (b);
		List<IEnumerable<IEnumerable<uint>>> eventpopulations = new List<IEnumerable<IEnumerable<uint>>>();
		foreach (StoryEvent e in beat.Events)
		{
			eventpopulations.Add(stateSpaceManager.getRelaxedPopulations(e));
		}
		IEnumerable<IEnumerable<IEnumerable<uint>>> allCombs = CombinationHelper.CartesianProduct(eventpopulations);

		foreach(IEnumerable<IEnumerable<uint>> comb in allCombs)
		{
			IEnumerable<uint> flattened = comb.SelectMany(o => o);
			if(!CombinationHelper.ContainsDuplicates(flattened))
				populationSet.Add(new BeatPopulation(comb, b));
		}
		if (populationSet.Count () == 0)
		{
			String enames = "";
			foreach(StoryEvent ev in b.Events)
				enames += (ev.Signature.ToString() + ", ");
			Debug.LogError("For the Story Beat containing: " + enames + "no Population of the Beat can be found. You might want to split the Beat up to find a population.");
		}
		return populationSet;
	}

	/// <summary>
	/// returns a story arc where all implicit parameters are not set.
	/// </summary>
	public StoryArc depopulateArc (StoryArc arc) {
		foreach (StoryBeat b in arc.Beats) {
			foreach(StoryEvent e in b.Events) {
				if(e.Signature.ExplicitParameterCount < e.Participants.Length) {
					for(int i = e.Signature.ExplicitParameterCount; i < e.Participants.Length; i++)
						e.Participants[i] = uint.MaxValue;
				}
			}
		}
		return arc;
	}
}


public enum PlanSpace {
	All = 0,
	Reduced
}

/// <summary>
/// A class holding the population (smart object ids) for all event instances of a beat and its cost.
/// </summary>
class BeatPopulation {

	public float cost;
	StoryBeat beat;
	public readonly IEnumerable<IEnumerable<uint>> beatpopulation;

	public BeatPopulation(IEnumerable<IEnumerable<uint>> oneCombs, StoryBeat b) {
		beatpopulation = oneCombs;
		beat = b;
	}

	public StoryBeat ToStoryBeat() {
		List<StoryEvent> newEvents = new List<StoryEvent>();
		int i = 0;
		foreach(StoryEvent e in beat.Events) {
			newEvents.Add (new StoryEvent(e.Signature, beatpopulation.ElementAt(i).ToArray()));
			i++;
		}
		return new StoryBeat (newEvents.ToArray ());
	}
}


class EventDomain : PlanningDomainBase
{
	//weighted A* is used with the weight specified with this constant
	const int WEIGHT = 5;
	StateSpaceManager manager;
	//relevant bits for cost calculation (only the ones which are important to satisfy the precondition are relevant).
	public RelevanceMask[] relevanceMasks;
	int noStatesInGS;
	
	public EventDomain(StateSpaceManager manager){
		this.manager = manager;
		noStatesInGS = manager.noOfObjects;
	}

	float countSetBits(long n)
	{
		float count = 0;
		while (n>0)
		{
			n &= (n-1) ;
			count++;
		}
		return count;
	}

	/// <summary>
	/// Cost from one Default State to another one. Cost is determined by different bits which are relevant.
	/// </summary>
	public override float estimateTotalCost (ref DefaultState currentStates, ref DefaultState idealGoalStates, float currentg)
	{
		Profiler.BeginSample ("estimate total cost");
		long cS, gS, cSR, gSR;
		float noTransitionsNeeded = 0;
		int nrRelevantPrototypes = relevanceMasks.Length;

		for(int i = 0; i<nrRelevantPrototypes;i++)
		{
			if(relevanceMasks[i].stateRelevant)
			{
	            cS = (currentStates.worldstate.GetPrototypeById(relevanceMasks[i].id)).State.GetStateBits() & relevanceMasks[i].stateMask;
				gS = (idealGoalStates.worldstate.GetPrototypeById(relevanceMasks[i].id)).State.GetStateBits() & relevanceMasks[i].stateMask;
				noTransitionsNeeded += countSetBits(cS ^ gS);
			}
			if(relevanceMasks[i].relationsRelevant)
			{
				for(uint j=0; j<noStatesInGS; j++)
				{
					cSR = (currentStates).worldstate.GetPrototypeById(relevanceMasks[i].id).State.GetRelationBits(j) & relevanceMasks[i].GetRelationBits(j);
					gSR = (idealGoalStates).worldstate.GetPrototypeById(relevanceMasks[i].id).State.GetRelationBits(j) & relevanceMasks[i].GetRelationBits(j);
					noTransitionsNeeded += countSetBits(cSR ^ gSR);
				}
			}
		}
		Profiler.EndSample ();
		return noTransitionsNeeded * WEIGHT + currentg;
	}

	/// <summary>
	/// Cost from one Default State to another one. Cost is determined by different bits of the whole state.
	/// </summary>
	public override float costBetweenWorldStates (ref DefaultState currentStates, ref DefaultState idealGoalStates, float currentg)
	{
		long cS, gS, cSR, gSR;
		float noTransitionsNeeded = 0;
		for(int i = 0; i<noStatesInGS;i++)
		{
			cS = ((currentStates).worldstate.Prototypes[i]).State.GetStateBits();
			gS = ((idealGoalStates).worldstate.Prototypes[i]).State.GetStateBits();
			noTransitionsNeeded += countSetBits(cS ^ gS);

			for(uint j=0; j<noStatesInGS; j++)
			{
				cSR = (currentStates).worldstate.Prototypes[i].State.GetRelationBits(j);
				gSR = (idealGoalStates).worldstate.Prototypes[i].State.GetRelationBits(j);
				noTransitionsNeeded += countSetBits(cSR ^ gSR);
			}
		}
		return noTransitionsNeeded * WEIGHT + currentg;
	}

	/// <summary>
	/// Returns true if the current State is a Goal State.
	/// </summary>
	public override bool isAGoalState (ref DefaultState currentStates, ref DefaultState idealGoalState)
	{
		bool finished = true;
		List<Transition> endTransitions = manager.endTransitions;
		foreach (Transition trans in endTransitions)
		{
			Transition tran = new Transition(trans.eventSig, trans.parameterIDs);
			finished = tran.populate(currentStates.worldstate);
			if(!finished)
				return false;
		}

		long cS, gS, cSR, gSR;
		int nrRelevantPrototypes = relevanceMasks.Length;

		for(int i = 0; i<nrRelevantPrototypes;i++)
		{
            if (relevanceMasks[i].stateRelevant)
			{
				cS = (currentStates.worldstate.GetPrototypeById(relevanceMasks[i].id)).State.GetStateBits() & relevanceMasks[i].stateMask;
				gS = (idealGoalState.worldstate.GetPrototypeById(relevanceMasks[i].id)).State.GetStateBits() & relevanceMasks[i].stateMask;
            	finished = finished && (countSetBits(cS ^ gS)==0);
				if(!finished)
					return false;
            }
			if(relevanceMasks[i].relationsRelevant)
			{
				for(uint j=0; j<noStatesInGS; j++)
				{
					cSR = currentStates.worldstate.GetPrototypeById(relevanceMasks[i].id).State.GetRelationBits(j) & relevanceMasks[i].GetRelationBits(j);
					gSR = idealGoalState.worldstate.GetPrototypeById(relevanceMasks[i].id).State.GetRelationBits(j)& relevanceMasks[i].GetRelationBits(j);
					finished = finished && (countSetBits(cSR ^ gSR)==0);
					if(!finished)
						return false;
				}
			}
		}
		return finished;
	}
	
	public override float evaluateDomain (ref DefaultState state)
	{	
		return 1.0f;
	}
	
	public override bool equals (DefaultState s1, DefaultState s2, bool isStart)
	{
		bool result = true;
		for(int i=0; i<noStatesInGS; i++)
		{
            result = result && ((s1.worldstate.Prototypes[i].State.GetStateBits() ^ s2.worldstate.Prototypes[i].State.GetStateBits()) == 0x0L);
			for(uint j=0; j<noStatesInGS; j++)
            	result = result && ((s1.worldstate.Prototypes[i].State.GetRelationBits(j) ^ s2.worldstate.Prototypes[i].State.GetRelationBits(j)) == 0x0L);
		}
		return result;
	}

	public int numtimescalled =0;

	/// <summary>
	/// generates the list of possible transitions from the current World State.
	/// </summary>
	public override void generateTransitions (ref DefaultState currentState, ref DefaultState previousState, ref DefaultState idealGoalState, ref List<DefaultAction> transitions)
	{
		transitions = manager.generateTransitions (currentState);
	}
	
	
}