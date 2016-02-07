using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using TreeSharpPlus;

/// <summary>
/// The AuthoredEventManager keeps track of all current EventStubs and their ordering into beats.
/// It has functionality to both import and export StoryArcs to interface with the storage of finished
/// narratives. The AuthoredEventManager has more dynamic functionality and interfaces with the GUI.
/// </summary>
public class AuthoredEventManager 
{
    /// <summary>
    /// The singleton instance of the AuthoredEventManager. Auto-generates itself if none exists yet.
    /// </summary>
	public static AuthoredEventManager Instance 
    { get { return instance ?? (instance = new AuthoredEventManager()); } }

    private static AuthoredEventManager instance;

    //The default level for events.
    private const int DEFAULT_LEVEL = 0;

    /// <summary>
    /// All the event stubs this manager currently has, sorted by their priority in descending order.
    /// </summary>
    private List<EventStub> allEvents;

    /// <summary>
    /// All the currently running or scheduled events that this manager has, i.e. all events that have not
    /// been finished yet.
    /// </summary>
    public IEnumerable<EventStub> AllEvents { get { return allEvents.AsReadOnly(); } }

    /// <summary>
    /// Returns all the events ordered by their level in ascending order.
    /// </summary>
    public IEnumerable<EventStub> EventsOrdereredByLevel
    {
        get
        {
            return allEvents.OrderBy((EventStub e) => GetLevelForEvent(e));
        }
    }

    /// <summary>
    /// Whether the scene is playing or not. Set it to true using StartScene.
    /// </summary>
    public bool playing { get; private set; }

    /// <summary>
    /// Contains the authored events where a given object is a participant (i.e.
    /// EventStub evnt where evnt.Participants() contains object)
    /// </summary>
    private Dictionary<SmartObject, HashSet<EventStub>> participantToEvents;

    /// <summary>
    /// Contains the authored events where a given object is involved in (i.e.
    /// EventStub evnt where evnt.GetInvolvedObjects() contains object)
    /// </summary>
    private Dictionary<SmartObject, HashSet<EventStub>> involvedObjectToEvents;

    /// <summary>
    /// Contains the level of a given event, i.e. the height it has in the hierarchy
    /// (e.g. an event without predecessors has level 1, an event with a level 1 and level 2 predecessor
    /// has level 3).
    /// </summary>
    private Dictionary<EventStub, int> levelsForEvents;

    /// <summary>
    /// Contains the level for a given object, i.e. the highest level of any event where that object
    /// is part of.
    /// </summary>
    private Dictionary<SmartObject, int> levelsForObjects;

    /// <summary>
    /// Contains all the termination dependencies for the events.
    /// </summary>
    private Dictionary<EventID, List<EventID>> dependencies = new Dictionary<EventID,List<EventID>>();

    /// <summary>
    /// Contains all the termination dependencies for the events.
    /// </summary>
    public IDictionary<EventID, List<EventID>> Dependencies { get { return dependencies; } }

    /// <summary>
    /// Creates a new AuthoredEventManager.
    /// </summary>
	private AuthoredEventManager()
    {
        playing = false;
        allEvents = new List<EventStub>();
        participantToEvents = new Dictionary<SmartObject, HashSet<EventStub>>();
        involvedObjectToEvents = new Dictionary<SmartObject, HashSet<EventStub>>();
        levelsForEvents = new Dictionary<EventStub, int>();
        levelsForObjects = new Dictionary<SmartObject, int>();
	}

    /// <summary>
    /// Clears all events from the manager.
    /// </summary>
    public void ClearAllEvents()
    {
        allEvents.Clear();
        participantToEvents.Clear();
        levelsForEvents.Clear();
        levelsForObjects.Clear();
        dependencies.Clear();
    }

    /// <summary>
    /// Gets the list of events the given SmartObject participates in (as a participant). If it does not exist,
    /// returns an empty list. Does not return null.
    /// </summary>
    public HashSet<EventStub> GetParticipatingEvents(SmartObject obj)
    {
        if (!participantToEvents.ContainsKey(obj))
        {
            participantToEvents[obj] = new HashSet<EventStub>();
        }
        return participantToEvents[obj];
    }

    /// <summary>
    /// Gets the set of events the given SmartObject is involved in (as an involved object, but not necessarily as
    /// a participant). Returns an empty set if it isn't involved in any events.
    /// </summary>
    public HashSet<EventStub> GetInvolvedInEvents(SmartObject obj)
    {
        if (!involvedObjectToEvents.ContainsKey(obj))
        {
            involvedObjectToEvents[obj] = new HashSet<EventStub>();
        }
        return involvedObjectToEvents[obj];
    }

    /// <summary>
    /// Gets the level of the given event. If the event has no level (i.e.
    /// it is not in the manager), returns 0.
    /// </summary>
    public int GetLevelForEvent(EventStub evnt)
    {
        if (!levelsForEvents.ContainsKey(evnt))
        {
            return DEFAULT_LEVEL;
        }
        return levelsForEvents[evnt];
    }

    /// <summary>
    /// Returns the level of the given object. If the object has no level (i.e.
    /// it does not participate in any event), returns 0.
    /// </summary>
    public int GetLevelForObject(SmartObject obj)
    {
        if (!levelsForObjects.ContainsKey(obj))
        {
            return DEFAULT_LEVEL;
        }
        return levelsForObjects[obj];
    }

    /// <summary>
    /// Adds the given event to the event list.
    /// The event does not need to have all parameters set yet.
    /// keepCurrentLevel indicates whether the event should stay at its level
    /// and other events should change if necessary. If it is false, the event will
    /// be added at its last possible position, i.e. after any conflicting event.
    /// </summary>
    public void AddAuthoredEvent(EventStub authoredEvent)
    {
        CalculateLevel(authoredEvent);
        //registers the object selector's listeners to update the event stub when the involved objects change
        for (int i = 0; i < authoredEvent.NrOfNeededRoles; i++)
        {
            authoredEvent.GetSelectorForIndex(i).OnObjectChanged += (SmartObject o, SmartObject n) => 
                UpdateAuthoredEvent(authoredEvent);
            authoredEvent.GetSelectorForIndex(i).OnObjectChanged += (SmartObject o, SmartObject n) =>
            {
                if (o != null && o != n)
                {
                    if (!authoredEvent.InvolvedObjects.Contains(o))
                        GetInvolvedInEvents(o).Remove(authoredEvent);
                    if (!authoredEvent.Participants.Contains(o))
                        GetParticipatingEvents(o).Remove(authoredEvent);
                    CalculateLevel(o);
                }
            };
        }
        UpdateAuthoredEvent(authoredEvent);
        allEvents.Add(authoredEvent);
        dependencies.Add(authoredEvent.ID, new List<EventID>());
    }

    /// <summary>
    /// Updates an existing event after new objects were added to its participants.
    /// </summary>
    private void UpdateAuthoredEvent(EventStub authoredEvent)
    {
        foreach (SmartObject obj in authoredEvent.InvolvedObjects)
        {
            AddAuthoredEventToObject(authoredEvent, obj);
        }
        ComputePredecessors(authoredEvent);
    }

    /// <summary>
    /// Removes the given event. Note that the event may not be predecessor
    /// of any other event, or the behavior is not defined.
    /// </summary>
    public void RemoveEvent(EventStub authoredEvent)
    {
        //Setting all to null actually makes cleanup easier, as all event handlers will be informed
        //that the old object was removed from the event
        for (int i = 0; i < authoredEvent.NrOfNeededRoles; i++)
        {
            authoredEvent.GetSelectorForIndex(i).TrySet(null);
        }
        //removes the event from any list/dictionary where it could be contained
        allEvents.Remove(authoredEvent);
        levelsForEvents.Remove(authoredEvent);
        //it is possible that the removed event is in a middle level, this makes sure that the predecessors are changed correctly,
        //i.e. any event that has a predecessor connection through the removed event, now has a direct connection
        foreach (EventStub evnt in allEvents.Where((EventStub e) => e.Predecessors.Contains(authoredEvent)))
        {
            evnt.RemovePredecessor(authoredEvent);
            foreach (EventStub e in authoredEvent.Predecessors)
            {
                evnt.AddPredecessor(e);
            }
            CalculateLevelRecursively(evnt);
        }
        dependencies.Remove(authoredEvent.ID);
    }

    /// <summary>
    /// Adds a termination dependency, so that toTerminate temrinates as soon as dependentOn does.
    /// </summary>
    public void AddTerminationDependency(EventStub dependentOn, EventStub toTerminate)
    {
        AddTerminationDependency(dependentOn.ID, toTerminate.ID);
    }

    /// <summary>
    /// Adds a termination dependency, so that toTerminate temrinates as soon as dependentOn does.
    /// </summary>
    public void AddTerminationDependency(EventID dependentOn, EventID toTerminate)
    {
        dependencies[toTerminate].Add(dependentOn);
    }

    /// <summary>
    /// Removes the termination dependency, so that toTerminate's termination no longer depends on 
    /// dependentOn's termination.
    /// </summary>
    public void RemoveTerminationDependency(EventStub dependentOn, EventStub toTerminate)
    {
        dependencies[toTerminate.ID].Add(dependentOn.ID);
    }

    /// <summary>
    /// Returns whether toTerminate's termination depends on the one of dependentOn
    /// </summary>
    public bool HasDependency(EventStub dependentOn, EventStub toTerminate)
    {
        return dependencies[toTerminate.ID].Contains(dependentOn.ID);
    }

    /// <summary>
    /// Sets the level of the given event to the given level, also applying changes necessary to other events
    /// that might have an overlap in participants.
    /// </summary>
    public void SetLevel(EventStub evnt, int level, bool cascade = false)
    {
        int oldLevel = GetLevelForEvent(evnt);
        //if level does not change, don't need to do anyrhing
        if (oldLevel == level)
        {
            return;
        }
        //if we add it to a higher level, must adapt predecessors and make sure any event in the
        //same level with overlapping participants is recursively set to a higher level
        if (oldLevel > level)
        {
            for (int i = level; i <= oldLevel; i++)
            {
                foreach (EventStub other in GetEventsInLevel(i))
                {
                    evnt.RemovePredecessor(other);
                    if (i == level + 1 && other != evnt)
                    {
                        other.AddPredecessor(evnt);
                    }
                }
            }
            foreach(EventStub other in GetEventsInLevel(level - 1))
            {
                evnt.AddPredecessor(other);
            }
            levelsForEvents[evnt] = level;
            foreach (EventStub other in new List<EventStub>(
                GetEventsInLevel(level).Except(new EventStub[] { evnt })))
            {
                if (other.ParticipantsOverlap(evnt))
                {
                    SetLevel(other, level + 1);
                }
            }
        }
        //if we add it to a lower level, must essentially do the same as wehn adding to a higher level but
        //adapting the predecessors is different
        if (oldLevel < level)
        {
            for (int i = oldLevel; i < level; i++)
            {
                foreach (EventStub other in GetEventsInLevel(i))
                {
                    other.RemovePredecessor(evnt);
                    if (i == level - 1 && other != evnt)
                    {
                        evnt.AddPredecessor(other);
                    }
                }
            }
            levelsForEvents[evnt] = level;
            foreach (EventStub other in new List<EventStub>(
                GetEventsInLevel(level).Except(new EventStub[] { evnt })))
            {
                if (other.ParticipantsOverlap(evnt) || (other.Predecessors.Contains(evnt) && cascade))
                {
                    SetLevel(other, level + 1, true);
                }
                else
                {
                    other.RemovePredecessor(evnt);
                }
            }
        }
        //Clean up any level that may be empty.
        CleanupEmptyLevels(1);
        //Calculate the level for any object in the event, as it might change
        foreach (SmartObject obj in evnt.InvolvedObjects)
        {
            CalculateLevel(obj);
        }
    }

    /// <summary>
    /// Gets all the events in the given level.
    /// </summary>
    private IEnumerable<EventStub> GetEventsInLevel(int level)
    {
        return levelsForEvents.Keys.Where((EventStub evnt) => GetLevelForEvent(evnt) == level);
    }

    /// <summary>
    /// Cleans up the levels with no events, starting at the start level.
    /// </summary>
    private void CleanupEmptyLevels(int start)
    {
        int max = levelsForEvents.Values.Max();
        for (int i = start; i <= max; i++)
        {
            if (GetEventsInLevel(i).Count() == 0)
            {
                //all the events above the empty level should move down by one
                new List<EventStub>(levelsForEvents.Keys.Where((EventStub evnt) => GetLevelForEvent(evnt) > i)).
                    ForEach((EventStub evnt) => levelsForEvents[evnt] = GetLevelForEvent(evnt) - 1);
                max -= 1;
            }
        }
    }

    /// <summary>
    /// Adds the predecessor event to the given event stub.
    /// </summary>
    public void AddPredecessor(EventStub eventStub, EventStub predecessor)
    {
        eventStub.AddPredecessor(predecessor);
        CalculateLevelRecursively(eventStub);
    }

    /// <summary>
    /// Calculates the level of a given event by looking at the levels of its
    /// predecessors. Also recursively calculates the level of any event, which
    /// has this event as predecessor.
    /// </summary>
    private void CalculateLevelRecursively(EventStub eventStub)
    {
        CalculateLevel(eventStub);
        foreach (EventStub evnt in allEvents.Where((EventStub e) => e.Predecessors.Contains(eventStub)))
        {
            CalculateLevelRecursively(evnt);
        }
        eventStub.InvolvedObjects.ForEach((SmartObject o) => CalculateLevel(o));
    }

    /// <summary>
    /// Calculates the level of the given event stub.
    /// </summary>
    private void CalculateLevel(EventStub eventStub)
    {
        int max = DEFAULT_LEVEL + 1;
        foreach (EventStub evnt in eventStub.Predecessors)
        {
            max = Mathf.Max(max, GetLevelForEvent(evnt) + 1);
        }
        levelsForEvents[eventStub] = max;
    }

    /// <summary>
    /// Calculates the level of a given Smart Object, by looking at the levels of
    /// the events where it is involved with.
    /// </summary>
    private void CalculateLevel(SmartObject obj)
    {
        int max = DEFAULT_LEVEL;
        foreach (EventStub evnt in involvedObjectToEvents[obj])
        {
            max = Mathf.Max(max, GetLevelForEvent(evnt));
        }
        levelsForObjects[obj] = max;
    }

    /// <summary>
    /// Computes the predecessors for the given event, checking only for objects of type
    /// in nonOverLappingTypes. Can be called multiple times for any given event without
    /// causing duplicate predecessors.
    /// Also calculates the event's level as well as the object's level.
    /// </summary>
    public void ComputePredecessors(EventStub authoredEvent)
    {
        foreach (SmartObject obj in authoredEvent.Participants)
        {
            foreach (EventStub evnt in GetParticipatingEvents(obj).
                Where((EventStub evnt) => evnt != authoredEvent))
            {
                if (GetLevelForEvent(evnt) >= GetLevelForEvent(authoredEvent))
                {
                    AddPredecessor(evnt, authoredEvent);
                }
            }
        }
        //CalculateLevelRecursively(authoredEvent);
    }

    /// <summary>
    /// Get all the direct predecessors of the given set of events.
    /// </summary>
    private HashSet<EventStub> AllPredecessors(IEnumerable<EventStub> current)
    {
        HashSet<EventStub> result = new HashSet<EventStub>();
        foreach (EventStub evnt in current)
        {
            result.UnionWith(evnt.Predecessors);
        }
        return result;
    }

    /// <summary>
    /// Adds the given authored event to the list of events for the given object.
    /// </summary>
    private void AddAuthoredEventToObject(EventStub authoredEvent, SmartObject obj)
    {
        if (authoredEvent.Participants.Contains(obj))
        {
            GetParticipatingEvents(obj).Add(authoredEvent);
        }
        GetInvolvedInEvents(obj).Add(authoredEvent);
    }

    /// <summary>
    /// Starts running the manager's events by instantiating a new StoryArcRunner.
    /// </summary>
    public void Start()
    {
        StoryArc arc = this.ToStoryArc();
        StoryArcRunner.GetRunner(arc, dependencies, CameraArgumentManager.Instance).StartRunning();
    }

    /// <summary>
    /// Creates a story arc with the given events that can then be played back.
    /// </summary>
    public StoryArc ToStoryArc()
    {
        int max = DEFAULT_LEVEL;
        levelsForEvents.Values.ForEach((int m) => max = (max >= m) ? max : m);
        StoryBeat[] beats = new StoryBeat[max];
        for (int i = DEFAULT_LEVEL + 1; i <= max; i++)
        {
            beats[i - 1] = ToStoryBeat(i);
        }
        return new StoryArc(beats);
    }

    /// <summary>
    /// Creates a single story beat for the given level.
    /// </summary>
    private StoryBeat ToStoryBeat(int level)
    {
        IEnumerable<EventStub> participating = levelsForEvents.Keys.Where((EventStub e) => levelsForEvents[e] == level);
        StoryEvent[] events = new StoryEvent[participating.Count()];
        int index = 0;
        foreach (EventStub stub in participating)
        {
            events[index++] = stub.ToStoryEvent();
        }
        return new StoryBeat(events);
    }

    /// <summary>
    /// Imports the given story arc, converting the story events within to event stubs.
    /// </summary>
    public void ImportStoryArc(StoryArc arc)
    {
        for (int i = 0; i < arc.Beats.Length; i++)
        {
            ImportStoryBeat(arc.Beats[i], i);
        }
    }

    /// <summary>
    /// Imports a single story beat into the given level.
    /// </summary>
    private void ImportStoryBeat(StoryBeat beat, int level)
    {
        for (int i = 0; i < beat.Events.Length; i++)
        {
            EventStub stub = EventStub.FromStoryEvent((beat.Events[i]));
            if (level > 0)
            {
                foreach (EventStub pred in allEvents.Where((EventStub e) => GetLevelForEvent(e) == level))
                {
                    stub.AddPredecessor(pred);
                }
            }
            AddAuthoredEvent(stub);
        }
    }
}