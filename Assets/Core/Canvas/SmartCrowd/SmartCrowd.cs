using UnityEngine;
using System.Collections.Generic;
using TreeSharpPlus;
using System.Linq;
using System;

/// <summary>
/// A Smart Crowd is a collection of Smart Objects that itself functions as a Smart Object and can thus be used
/// for Events. Apart from being usable for Events, it also has its own EventScheduler to create its own
/// ambient behavior, so even when not participating in events, crowds seem alive.
/// </summary>
public class SmartCrowd : SmartObject 
{

    public override string Archetype
    {
        get { return "SmartCrowd"; }
    }

    /// <summary>
    /// Class to keep track of how often an object was used by the scheduler.
    /// </summary>
    private class EventSchedulerUsage
    {
        public int NrOfUses;

        public IHasState Object;

        public EventSchedulerUsage(IHasState obj)
        {
            this.Object = obj;
            this.NrOfUses = 0;
        }
    }

    //Keeps track of how often objects were used by the scheduler
    private Dictionary<IHasState, EventSchedulerUsage> usage;

    /// <summary>
    /// The criteria with which the members of this crowd are selected.
    /// </summary>
    public ISmartCrowdCriteria CrowdCriteria { get; private set; }

    //The SmartObjects contained in this crowd.
    private HashSet<SmartObject> containedSmartObjects;

    /// <summary>
    /// Whether the members of this crowd can change dynamically based on the criteria
    /// or not.
    /// </summary>
    public bool IsStatic { get; private set; }

    /// <summary>
    /// The EventCollection used by this crowd's event manager.
    /// </summary>
    public EventCollection EventCollection { get; private set; }

    /// <summary>
    /// Initializes a new SmartCrowd with the given values.
    /// </summary>
    /// <param name="criteria">The criteria used for member selection.</param>
    /// <param name="isStatic">Is member selection static?</param>
    /// <param name="collection">The EventCollection used for the scheduler.</param>
    public void Initialize(ISmartCrowdCriteria criteria, bool isStatic, EventCollection collection)
    {
        Set(StateName.RoleCrowd);
        CrowdCriteria = criteria;
        containedSmartObjects = new HashSet<SmartObject>();
        FindContained();
        this.IsStatic = isStatic;
        this.EventCollection = collection;
        this.usage = new Dictionary<IHasState, EventSchedulerUsage>();
    }
    
    /// <summary>
    /// MonoBehavior update method.
    /// </summary>
    void Update()
    {
        if (!IsStatic)
        {
            FindContained();
        }
    }

    /// <summary>
    /// Find all the objects with tag Smart Object that fulfill the crowd's criteria.
    /// </summary>
    private void FindContained()
    {
        containedSmartObjects = SmartCrowdCriteria.AllSatisfyingCriteria(CrowdCriteria, containedSmartObjects);
    }

    #region Event Scheduler
    /// <summary>
    /// Starts this crowd's EventScheduler, which automatically schedules certain events between its participants.
    /// </summary>
    public void StartScheduler()
    {
        PeriodicMethodCaller.GetInstance().StartCallPeriodically(EventScheduler, EventCollection.TimeBetweenUpdates);
        Debug.Log(EventCollection.TimeBetweenUpdates);
    }

    /// <summary>
    /// Uses the crowd's EventCollection to schedule a random event within its participants.
    /// </summary>
    private void EventScheduler()
    {
        EventSignature signature = EventCollection.GetRandomSignature();
        //Finds a signature that does not affect the world state
        if (signature != null && !signature.AffectsWorldState())
        {
            IEnumerable<EventPopulation> populations =
                EventPopulator.GetValidPopulations(
                    signature,
                    GetObjectsByAvailability(0.3f).
                        OrderBy((SmartObject obj) => GetUsage(obj).NrOfUses).
                        Cast<IHasState>().ToList(),
                    ObjectManager.Instance.GetObjects().Cast<IHasState>());
            //Check if we have a population for the event and start it if so.
            foreach (EventPopulation population in populations)
            {
                if (signature.CheckTypes(population.AsParams()))
                {
                    SmartEvent evnt = signature.Create(population);
                    evnt.StartEvent(0.3f);
                    foreach (IHasState obj in population.Members)
                    {
                        GetUsage(obj).NrOfUses++;
                    }
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Get the usage for a single SmartObject.
    /// </summary>
    private EventSchedulerUsage GetUsage(IHasState obj)
    {
        if (!usage.ContainsKey(obj))
        {
            usage[obj] = new EventSchedulerUsage(obj);
        }
        return usage[obj];
    }
    #endregion

    #region Contained object getters
    /// <summary>
    /// Get all the objects currently contained in the crowd.
    /// </summary>
    public IEnumerable<SmartObject> GetObjects()
    {
        foreach (SmartObject obj in containedSmartObjects)
        {
            yield return obj;
        }
        yield break;
    }

    /// <summary>
    /// Get all the objects currently contained in the crowd
    /// of the given type.
    /// </summary>
    public IEnumerable<T> GetObjectsByType<T>()
        where T : SmartObject
    {
        return containedSmartObjects.Where((SmartObject obj) => obj is T).Cast<T>();
    }

    /// <summary>
    /// Gets all the objects currently contained in the crowd
    /// fulfilling the given state
    /// </summary>
    public IEnumerable<SmartObject> GetObjectsByState(params StateName[] states)
    {
        return containedSmartObjects.Where((SmartObject obj) => obj.Require(states));
    }

    /// <summary>
    /// Gets all the objects currently contained in the crowd
    /// that can participate in an event of the given priority.
    /// </summary>
    public IEnumerable<SmartObject> GetObjectsByAvailability(float priority)
    {
        return containedSmartObjects.Where((SmartObject obj) => obj.Behavior.CurrentPriority < priority);
    }
    #endregion
}

