using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Priority_Queue;

//
// Copyright (c) 2009-2010 Shawn Singh, Mubbasir Kapadia, Petros Faloutsos, Glenn Reinman
// See license.txt for complete license.
//

/// @file BestFirstSearchPlanner.h
/// @brief Declares and implements a templatized best-first search planner.
///

//For deriving different planners and selecting the one we are using

 public class Planner : MonoBehaviour
 {
	public float goalTime;
	
	public int MaxNumberOfNodes;
	public float maxTime;
	
	public Transform root;
	public Transform currentStateTransform;			
	
	public Transform goalStateTransform;
	[HideInInspector] public Vector3 currentGoal;
	
	public Material goalCompletedMaterial;
	public Material goalMaterial;
	
	[HideInInspector] public bool initialized;	
	
	[HideInInspector] public bool isPlanComputed;
	
	[HideInInspector] public bool planChanged;
	
	[HideInInspector] public bool goalReached;
	
	
	public virtual bool IsEmpty() { return false; }
	
	public virtual bool HasExpired() { return false; }
	
	public virtual bool ThereStillTime() { return true; }
	
	public virtual void RecomputePlan() { } 
		
	public virtual int GetNumberOfDomains()
	{
        return 0;
    }
		
	public virtual bool ComputePlan() { return false; }
	
};

 interface IPlannerInterface<StateMap>
{
	 void init(ref List<PlanningDomainBase> newPlanningDomain, int maxNumNodesToExpand);
	
	 bool _computePlan(ref DefaultState startState, ref DefaultState idealGoalState, StateMap map, ref DefaultState actualStateReached, float maxTime);
}


	/**
	 * @brief The default class used to describe a transition between states, if it was not specified by the user.
	 *
	 * @see
	 *   - Documentation of the BestFirstSearchPlanner class, which describes how states and actions are used.
	 */
public class DefaultAction {
	public float cost;
	public DefaultState state;

	public DefaultAction(){
	}
	internal DefaultAction (DefaultAction other) {
		this.state = other.state.Clone();
	}
	public virtual DefaultAction Clone(){
		return new DefaultAction (this);
	}
}

public class DefaultState {

	public WorldState worldstate;

	public DefaultState(WorldState w) {
		this.worldstate = new WorldState (w);
	}
	
	public DefaultState(IEnumerable<IHasState> objs) {
		this.worldstate = new WorldState(objs);
	}

	public DefaultState(DefaultState other) {
		this.worldstate = new WorldState (other.worldstate);
	}

	public virtual DefaultState Clone() {
		return new DefaultState (this);
	}

	public override int GetHashCode () {
		return this.worldstate.GetHashCode ();
	}

	public bool Equals (DefaultState other)
	{
		return this.worldstate.Equals (other.worldstate);
	}

	public override string ToString ()
	{
		return this.worldstate.ToString();
	}

}

/**
 * @brief Internal helper class used to describe a node in the best-first search.
 *
 * This class is not intended to be used directly.  Instead, use the BestFirstSearchPlanner that
 * uses this class for internal representation.
 *
 * @see
 *   - Documentation of the BestFirstSearchPlanner class, which describes how states and actions are used.
 */


public class BestFirstSearchNode : PriorityQueueNode {
	
	public BestFirstSearchNode() { }
	public BestFirstSearchNode(float _g, float _f, DefaultState _previousStateRef, DefaultAction _nextActionRef) 
		{ g = _g; f = _f; previousState = _previousStateRef; action = _nextActionRef; alreadyExpanded = false;	}

	public BestFirstSearchNode(float _g, float _f, ref DefaultState _previousStateRef, ref DefaultState _nextStateRef) 
		{
            g = _g; f = _f; previousState = _previousStateRef; alreadyExpanded = false;
			action = new DefaultAction();
			action.cost = 0.0f;
			action.state = _nextStateRef;
		}

	public float g;
	public float f;
	public DefaultState previousState;
	public DefaultAction action;
	public bool alreadyExpanded;
	};


	/**
	 * @brief A functor class used to compare the costs of two BestFirstSearchNodes.
	 *
	 * @see
	 *   - Documentation of the BestFirstSearchPlanner class, which describes how states and actions are used.
	 */
	public class CompareCosts  {
	    BestFirstSearchNode node1, node2;
 
        public static int CompareCost(BestFirstSearchNode n1, BestFirstSearchNode n2)
        { 
			if (n1.f != n2.f) 
            	 if(n1.f > n2.f)
					return 1;
				else  
					return -1;
			else
				if(n1.g > n2.g) return 1;
				else if(n1.g < n2.g) return -1;
				else return 0;
        }
	};

    public static class Extensions
    {
        public static bool IsDefault<T>(this T value) where T : struct
        {
            bool isDefault = value.Equals(default(T));

                return isDefault;
        }
    };



	/**
	 * @brief A base class for implementing a planning domain used by the BestFirstSearchPlanner.
	 *
	 * To use the BestFirstSearchPlanner, you must provide a class that implements the search heuristic, state transitions, and
	 * the meaning of a goal state.  To make such a class, you can inherit this class and implement the functionality.
	 * Because the BestFirstSearchPlanner is a template class, it is not necessary to
	 * inherit this class directly, but you may find it helpful to do so.  The real requirement is that
	 * your planning domain class has at least the same functionality that is contained in this class.
	 *
	 * <b>Note:</b> You should not instantiate this class directly, even though it is also templatized, because 
	 * you may wish to have different planning domains that use the same data type to represent state.  Instead,
	 * inherit this class and then implement the functionality in your derived class.
	 *
	 * @see
	 *   - Refer to the details of each function in this class.
	 *   - Documentation of the BestFirstSearchPlanner class, which describes how states and actions are used.
	 */
public class PlanningDomainBase {
		/**
		 * @brief Returns true if state is a valid goal state for the planning problem.
		 *
		 * The parameter idealGoalState is only a reference to the goal state that the user specified
		 * when calling BestFirstSearchPlanner::computePlan().  Depending on your planning task,
		 * you may want to ignore this value, for example, if you have multiple goals and only need
		 * to reach one of those goal states.
		 *
		 * In most cases, however, this parameter is a helpful optimization, that allows you to
		 * compute "distance" or "cost" to the goal state without having to store extra data in
		 * your planning domain class.  In some cases, this allows you to use the same instance of the
		 * planning domain for all agents that are performing the same planning task, even though 
		 * they have different start and goal states.
		 *
		 */
        public virtual bool isAGoalState(ref DefaultState state, ref DefaultState idealGoalState)
        {
            return state.Equals(idealGoalState);
        }

		/**
		 * @brief Populates the list of actions that can be taken from the current state.
		 *
		 * When you implement this function, make sure that you initialize both the (1) cost and (2) new state 
		 * of the action being generated.
		 *
		 * You may wish to use the previousState or the idealGoalState as additional information
		 * that helps you decide how to generate actions.  The classic example of this is when the
		 * state space is a graph, you should avoid generating an action that will take you back to 
		 * the previous graph node.  In another example, you may choose to generate goal-dependent
		 * actions.
		 *
		 */
        public virtual void generateTransitions(ref DefaultState currentState, ref DefaultState previousState, ref DefaultState idealGoalState, ref List<DefaultAction> transitions)
        {

		Debug.Log ("This is overridden");
            /*
	          DefaultAction<PlanningState> newAction = new DefaultAction<PlanningState>();
	          foreach(PlanningAction action in transitions) {
	            newAction.cost = (action as DefaultAction<PlanningState>).cost;
	            newAction.state = (action as DefaultAction<PlanningState>).state;
	            transitions.( newAction );
	          }
	       */
		}
	
		// added new function which is used by ARAStar -- what is the difference between this and generateTransitions ? 
		public virtual void generateNeighbors(DefaultState currentState, ref List<DefaultState> neighbors)
		{
		}
		
		public virtual void generatePredecessors(DefaultState currentState, ref List<DefaultAction> actionList)
		{
		}
	
		/**
		 * @brief Computes the heuristic function (often denoted as f) that estimates the "goodness" of a state towards the goal.
		 *
		 * Note carerfully that this function should compute f, not h.  For generalized best-first search, there is no explicit concept of h.
		 * For example, to implement A*, this function would return f, where f = currentg + h.  In this case, h is the estimated distance or cost from the current state to the goal state.
		 *
		 */
		public virtual float estimateTotalCost(ref DefaultState currentState, ref DefaultState idealGoalState, float currentg )
          {   /*
	          // if distance() is admissible, then this implementation is an A* search.
	          float h = Vector3.Distance((currentState as Transform).position, (idealGoalState as Transform).position);
	          float f = currentg + h;  
	          return f;
	          */
			  return 0; 
	      }

		public virtual float costBetweenWorldStates (ref DefaultState currentStates, ref DefaultState idealGoalStates, float currentg) {
		return 0;
		}


	public virtual float evaluateDomain(ref DefaultState state)
		{
			return 0;
		}
	
		public virtual bool CheckStateCollisions(DefaultState state)
		{
			return false;
		}
	
		public virtual bool CheckTransitionCollisions(DefaultState state, DefaultState prevState, bool sampling = true)
		{
			return false;
		}
	
	
		public virtual float ComputeEstimate(ref DefaultState _from, ref DefaultState _to, string estimateType)
		{
			return 0.0f;
		}

		// ComputeEstimate is split up into 2 separate functions -- ComputeEstimate should be deprecated 
		public virtual float ComputeGEstimate(DefaultState _from, DefaultState _to)
		{
			return 0.0f;
		}
	
		public virtual float ComputeHEstimate(DefaultState _form, DefaultState _to)
		{
			return 0.0f;	
		}
	
	
		public virtual void generatePredecesors(ref DefaultState currentState, ref DefaultState previousState, ref DefaultState idealGoalState, ref List<DefaultAction> transitions)
		{
		}
	
		public virtual bool equals(DefaultState s1, DefaultState s2, bool isStart)
		{
			return false;
		}
	
		public virtual bool equals(float value1, float value2)
		{
			return false;
		}
	
		public virtual DefaultAction generateAction (DefaultState previousState, DefaultState nextState)
		{
			return new DefaultAction();
		}
		
		public bool includeStart = true;
	};


	/**
	 * @brief A state-space planning algorithm that uses a generalized best-first search.
	 *
	 * This class implements an easy-to-use, generalized best-first search.  This class has no knowledge of the actual planning task.
	 * To use this class, you must provide information about the planning task using three template parameters:
	 *   - PlanningDomain is a simple user-defined class that defines the search heuristic, state transitions, and the meaning of a goal state.
	 *   - PlanningState is the data type used to represent a state.
	 *   - PlanningAction is an optional data type used to represent an action.  If an action data type is not specified, the compact DefaultAction &lt;PlanningState&gt; class is used.
	 *
	 * Depending on how you define the action costs and heuristic function, the search can work like A*, near-optimal best-first search,
	 * greedy best-first, or any other best-first search technique.  An example of using this planner is given below.
	 *
	 * <h3> Background </h3>
	 *
	 * To compute a plan, the planner needs the following information (i.e., the planning domain):
	 *   - A state space, which is the space of all possible states that can occur in the problem,
	 *   - An action space, which describes the space of actions that transition from one state to another,
	 *   - Costs associated with each action,
	 *   - A heuristic to estimate the "goodness" of a given state, denoted as f, which tells the planner what to search next.
	 *   - A notion of what it means to be a goal state.
	 *
	 * The user implicitly provides all this information with the template parameters when instantiating the planner.
	 *
	 * <h3> How to use this class </h3>
	 *
	 * Using this class is very straightforward:
	 *
	 *   -# Choose the data types you will use to represent state and action.
	 *   -# Implement a class that describes the state and action spaces, providing functionality that is the same as SteerLib::PlanningDomainBase.
	 *   -# Instantiate the BestFirstSearchPlanner specifying the template parameters for your planning domain class, the data type for a state, and (optionally) the 
	 *      data type for an action.
	 *   -# Initialize the planner using init().
	 *   -# Call computePlan(), specifying your start state and goal state.
	 *
	 * <b> Notes: </b>
	 *
	 *   - The data type you use for a State must implement the assignment "=", equals "==", and comparison "<" operators
	 *     so that it can be used by this planner.  It may be a good idea to test these operators using an STL set and an STL map,
	 *     because that is what we use for internal data structures.
	 *
	 *   - It is optional to specify an action data type, otherwise the DefaultAction &lt;PlanningState&gt; data type will be used.
	 *     This default simply contains the cost of the action, and the resulting new state of the action, which is compact and
	 *     suitable for most common planning tasks.
	 *
	 *   - When computePlan() is called, the planner will compute either a sequence of states or a sequence of 
	 *     actions, depending on which overloaded function was called.
	 *
	 *   - Depending on how you implemented the planning domain class, it may be possible to call computePlan() any 
	 *     number of times using the same instance of the planner.  Furthermore, the planner class maintains no
	 *     state internally, so it is possible to use separate threads for separate planning tasks using the 
	 *     same instance of the planner  (assuming the search horizon stays the same).
	 *
	 *   - During initialization, a search horizon is specified.  This allows the user to limit the number of
	 *     nodes expanded during the search process (i.e. the number of times that SteerLib::PlanningDomainBase::generateTransitions()
	 *     will be called).   If a complete plan is not found within the allowed number of nodes to expand, computePlan() will
	 *     return false, but will still provide an incomplete plan that it can construct to the node that had the
	 *     best heuristic f value.
	 *
	 * <b> Recommendations: </b>
	 *
	 *   - Instead of using larger data structures for State and Action, many times it is appropriate to
	 *     use an integer ID or a pointer reference for your State data type.  This reference/index would
	 *     then refer to the actual data corresponding to that State/Action.  This allows you to maintain
	 *     a "cache" of states for yourself, which is useful for gathering statistics, debugging your 
	 *     implementation, and for performance of this planner.
	 *
	 *   - When possible, performance can be improved by inlining the functions you implement in your
	 *     PlanningDomain class.
	 *
	 *
	 * <h3> Implementing the state and action spaces </h3>
	 *
	 * The first template parameter, PlanningDomain, is a class that describes the 
	 * state space and action space of the planner.  This class should be implemented by the user,
	 * and it must have at least the same functionality as the SteerLib::PlanningDomainBase class.  
	 * Refer to the documentation in the SteerLib::PlanningDomainBase class for more information about the three
	 * functions that must be implemented.
	 *
	 * It is not necessary to inherit the SteerLib::PlanningDomainBase class; it is only required to have the
	 * same functionality.
	 *
	 *
	 * @see
	 *   - Documentation of the SteerLib::PlanningDomainBase class, which describes how states and actions are specified to the planner.
	 *
	 * <h3> Code example </h3>
	 *
	 * \code
	 * 
	 * class ExamplePlanningDomain {
	 *   public:
	 *     bool isAGoalState( const int & state,
	 *                        const int & idealGoalState)
	 *     {
	 *         return state == idealGoalState;
	 *     }
	 *
	 *     float estimateTotalCost( const int & currentState, 
	 *                              const int & idealGoalState,
	 *                              float currentg)
	 *     {
	 *         // if distance() is admissible, then this implementation is an A* search.
	 *         float h = distance( currentState, idealGoalState);
	 *         float f = currentg + h;  
	 *         return f;
	 *     }
	 *
	 *     void generateTransitions( const int & currentState,
	 *                               const int & previousState, 
	 *                               const int & idealGoalState,
	 *                               std::vector<SteerLib::DefaultAction<int> > & transitions )
	 *     {
	 *         DefaultAction<int> newAction;
	 *         for_each_transition_possible_from_currentState {
	 *             newAction.cost = cost_of_this_specific_transition;
	 *             newAction.state = new_state_after_transition;
	 *             transitions.push_back( newAction );
	 *         }
	 *     }
	 *
	 * };
	 *
	 * int main( )
	 * {
	 *     // Of course, in your code these variables would need to be initialized properly.
	 *     ExamplePlanningDomain domain;
	 *     int currentState, goalState;
	 *
	 *     // This example has effectively no limited horizon.
	 *     unsigned int numNodesInHorizon = UINT_MAX;
	 *
	 *     // Instantiate and initialize the planner
	 *     BestFirstSearchPlanner<ExamplePlanningDomain, int> examplePlanner;
	 *     examplePlanner.init( &domain, numNodesInHorizon);
	 *
	 *     // Compute the plan
	 *     std::stack<int> outputPlan;
	 *     examplePlanner.computePlan(currentState, goalState, outputPlan);
	 *
	 *     // Use the plan
	 *     while (!outputPlan.empty()) {
	 *         int nextState = outputPlan.pop();
	 *
	 *         ... // other code
	 *     }
	 *
	 *     return 0;
	 * }
	 * \endcode
	 *
	 *
	 */
	//template < class PlanningDomain, class PlanningState, class PlanningAction = DefaultAction<PlanningState> >

class BestFirstSearchPlanner : IPlannerInterface<Dictionary<DefaultState, BestFirstSearchNode>> {

    public int _maxNumNodesToExpand;
    public List<PlanningDomainBase> _planningDomain;
	bool OneStepNeedsUpdate = false, initPlanner = true;
	public bool oneStep = false; 
	List<BestFirstSearchNode> openSet, outputPlan;
	Dictionary<DefaultState, BestFirstSearchNode> Visited;
    uint numNodesExpanded = 0;
	DefaultState stateReached = default(DefaultState);
	public int[] relevantIndices;


	public BestFirstSearchPlanner(){}

		/// Initializes the planner to use the specified instance of the planning domain, and sets the search horizon limit.
	public void init(ref List<PlanningDomainBase> newPlanningDomain, int maxNumNodesToExpand)
	{
		_maxNumNodesToExpand = maxNumNodesToExpand;
		_planningDomain = new List<PlanningDomainBase>(newPlanningDomain.Capacity);
		_planningDomain = newPlanningDomain;
		//openSet = new List<BestFirstSearchNode>();
		Visited =  new Dictionary<DefaultState, BestFirstSearchNode>();
		outputPlan = new List<BestFirstSearchNode>();
	}
	
	public bool _computePlan(ref DefaultState startState, ref DefaultState idealGoalState, Dictionary<DefaultState, BestFirstSearchNode> stateMap, ref DefaultState actualStateReached, float maxTime)
    {
        //SortedList<BestFirstSearchNode<PlanningState, PlanningAction>> openSet = new SortedList<BestFirstSearchNode<PlanningState, PlanningAction>>(new CompareCosts<PlanningState, PlanningAction>());
		//List<BestFirstSearchNode> openSet = new List<BestFirstSearchNode>(); 
		HeapPriorityQueue<BestFirstSearchNode> openSet = new HeapPriorityQueue<BestFirstSearchNode> (_maxNumNodesToExpand*_maxNumNodesToExpand);

		PlanningDomainBase domain = _planningDomain.First ();
					
        stateMap.Clear();
        openSet.Clear();

        float newf = domain.estimateTotalCost(ref startState, ref idealGoalState, 0.0f);
		BestFirstSearchNode nextNode = new BestFirstSearchNode(0.0f, newf, ref startState, ref startState);
        stateMap[nextNode.action.state] = nextNode;
		openSet.Enqueue(nextNode, nextNode.action.cost); //Changed by SO for performance reasons

		float prevTime = Time.realtimeSinceStartup;
		List<DefaultAction> possibleActions = new List<DefaultAction>();
		numNodesExpanded = 0;
        while ((numNodesExpanded < _maxNumNodesToExpand) && (maxTime > 0) && (openSet.Count != 0))
        {			
            numNodesExpanded++;
			//openSet.Sort(CompareCosts.CompareCost); Changed by SO for Performance reasons

            BestFirstSearchNode x = openSet.Dequeue();

			Rules.ClearCache();

            // ask the user if this node is a goal state.  If so, then finish up.
			if (domain.isAGoalState(ref x.action.state, ref idealGoalState))
            {
                actualStateReached = x.action.state;
                return true;
            }
			if(stateMap.ContainsKey(x.action.state))
				stateMap[x.action.state].alreadyExpanded = true;

			possibleActions.Clear();

            domain.generateTransitions(ref x.action.state, ref x.previousState, ref idealGoalState, ref possibleActions);

            // iterate over each potential action, and add it to the open list.
            // if the node was already seen before, then it is updated if the new cost is better than the old cost.
            foreach (DefaultAction action in possibleActions)
            {
                float newg = x.g + action.cost;

				if (stateMap.ContainsKey(action.state))
                {
					BestFirstSearchNode existingNode = stateMap[action.state];
                    // then, that means this node was seen before.
                    if (newg < existingNode.g)
                    {
                        // then, this means we need to update the node.
                        if (existingNode.alreadyExpanded == false)
                        {
                            openSet.Remove(existingNode);
                        }
                        stateMap.Remove(existingNode.action.state);
                    }
                    else
                    {
                        // otherwise, we don't bother adding this node... it already exists with a better cost.
                        continue;
                    }
                }
                DefaultAction nextAction = action;

                newf = domain.estimateTotalCost(ref action.state, ref idealGoalState, newg);
                nextNode = new BestFirstSearchNode(newg, newf, x.action.state, nextAction.Clone());	
							
                stateMap[nextNode.action.state] = nextNode;
				openSet.Enqueue(nextNode, nextNode.action.cost);	
            }

			float actualTime = Time.realtimeSinceStartup;
			maxTime -= (actualTime-prevTime);
			prevTime = actualTime;
        }
		if(maxTime<=0)
			Debug.Log("Timeout");

        if (openSet.Count==0)
        {
            // if we get here, there was no solution.
            actualStateReached = startState;
        }
        else
        {			
            // if we get here, then we did not find a complete path.
            // instead, just return whatever path we could construct.
            // The idea is that if the user gave a reasonable heuristic,
            // state space, and transitions, then the next node that
            // would be expanded will be the most promising path anyway.
            //
            actualStateReached = (openSet.First).action.state;
        }
	    return false;  // returns false because plan is incomplete.
    }
	
	/// Computes a plan as a sequence of actions; returns true if the planner could reach the goal, or false if the plan is only partial and could not reach the goal within the specified horizon.
	public	bool computePlan(ref DefaultState startState, ref DefaultState goalState, ref Stack<DefaultAction> plan, float maxTime )
        {
		Dictionary<DefaultState, BestFirstSearchNode > stateMap = new Dictionary<DefaultState,BestFirstSearchNode>();

		DefaultState s = default(DefaultState);
		DefaultAction a;
		bool isPlanComplete = _computePlan(ref startState,ref goalState, stateMap, ref s, maxTime);
		Visited = stateMap;

		bool finished = false;
		// reconstruct path here
		Profiler.BeginSample ("Reconstruct path");
		do {
			if(s.Equals(startState)) finished=true;
			// push all actions except for the very first one which was an invalid dummy action.
            foreach(KeyValuePair<DefaultState, BestFirstSearchNode> node in stateMap)
            {
              if(node.Key.Equals(s)){
				  a = node.Value.action;	
                  plan.Push(a);
				  outputPlan.Add(node.Value);
                  if(!finished) 
				  	s = node.Value.previousState;
									
                  break;
              }
            }
		} while (!finished);

		return isPlanComplete;
	}
	
};



	


