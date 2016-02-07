using System.Collections.Generic;
using System.Linq;
using TreeSharpPlus;


using UnityEngine;

/// <summary>
/// Class to be used for running a StoryArc. Also integrates the CrowdManager and the
/// EventManager to ensure SmartCrowds have a correctly functioning EventScheduler.
/// </summary>
public class StoryArcRunner : MonoBehaviour, IBehaviorUpdate
{
    /// <summary>
    /// An instantiated story beat that is capable of executing
    /// </summary>
    private class StoryBeatInstance
    {
        /// <summary>
        /// Wrapper for a StoryEvent with dependency functionality
        /// </summary>
        internal class StoryEventInstance
        {
            internal readonly StoryEvent evnt;

            internal List<EventID> dependencyIDs;

            internal StoryEventInstance(StoryEvent evnt, List<EventID> dependencyIDs)
            {
                this.evnt = evnt;
                this.dependencyIDs = dependencyIDs;
            }

        }

        //All the termination dependencies.
        private readonly Dictionary<EventID, List<EventID>> dependencies;

        //The internal actual beat.
        private readonly StoryBeat beat;

        //Maps eventIDs to EventInstances
        internal Dictionary<EventID, StoryEventInstance> IdToEvent;

        //Maps EventInstances to their SmartEvent
        internal Dictionary<StoryEventInstance, SmartEvent> Events;

        /// <summary>
        /// Creates a new StoryBeatInstance with the given beat and dependencies.
        /// </summary>
        /// <param name="beat">The actual StoryBeat.</param>
        /// <param name="dependencies">All the termination dependencies.</param>
        internal StoryBeatInstance(StoryBeat beat, Dictionary<EventID, List<EventID>> dependencies)
        {
            this.dependencies = dependencies;
            this.beat = beat;
            this.Events = null;
        }

        /// <summary>
        /// Returns whether all the SmartEvents in this StoryBeatInstance
        /// have already terminated.
        /// </summary>
        internal bool HasTerminated()
        {
            return this.Events.Values.All(
                (SmartEvent evt) =>
                    evt.Behavior.Status == EventStatus.Finished);
        }

        /// <summary>
        /// Starts this beat by starting each StoryEvent within it.
        /// </summary>
        internal void Start(float priority = 0.5f)
        {
            // Since we have a new world state, clear the rules cache
            Rules.ClearCache();

            this.IdToEvent = new Dictionary<EventID, StoryEventInstance>(this.beat.Events.Length);
            this.Events = new Dictionary<StoryEventInstance, SmartEvent>(this.beat.Events.Length);
            for (int i = 0; i < this.beat.Events.Length; i++)
            {
                StoryEventInstance instance = new StoryEventInstance(this.beat.Events[i], dependencies[this.beat.Events[i].ID]);
                this.IdToEvent[this.beat.Events[i].ID] = instance;
                this.Events[instance] =
                    this.StartEvent(this.beat.Events[i], priority);
            }

            //Adds termination dependencies to the necessary events by registering to the status changed event.
            foreach (StoryEventInstance key in this.Events.Keys)
            {
                foreach (EventID id in key.dependencyIDs)
                {
                    if (IdToEvent.ContainsKey(id))
                    {
                        StoryEventInstance localKey = key;
                        this.Events[IdToEvent[id]].Behavior.StatusChanged +=
                            (BehaviorEvent sender, EventStatus status) => 
                                TerminationDependencyHandler(Events[localKey], status);
                    }
                }
            }
        }

        /// <summary>
        /// Terminates the given event if the status of the other event is finished.
        /// </summary>
        /// <param name="evnt">The status of the other event.</param>
        /// <param name="newStatus">The event to possibly terminate.</param>
        private void TerminationDependencyHandler(SmartEvent evnt, EventStatus newStatus)
        {
            if (newStatus == EventStatus.Finished)
            {
                evnt.Behavior.StopEvent();
            }
        }
        

        /// <summary>
        /// Gets all the participants in this wrapper's beat (as a participant, if there
        /// is a NonParticipant attribute they are not included
        /// </summary>
        private HashSet<IHasState> GetParticipants()
        {
            HashSet<IHasState> result = new HashSet<IHasState>();
            for (int i = 0; i < beat.Events.Length; i++)
            {
                for (int j = 0; j < beat.Events[i].Participants.Length; j++)
                {
                    IHasState obj =
                        ObjectManager.Instance.GetObjectById(beat.Events[i].Participants[j]);
                    //Exclude the ones with a NonParticipant attribute
                    if (obj != null && (beat.Events[i].Signature.GetAttributes<NonParticipantAttribute>().Length == 0
                        || !beat.Events[i].Signature.GetAttributes<NonParticipantAttribute>()[0].Indices.Contains(j))) 
                    {
                        result.Add(obj);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Starts an individual event in the beat
        /// </summary>
        private SmartEvent StartEvent(StoryEvent evt, float priority)
        {
            //it is possible that some parameters are only filled implicitly, so we have to fill those in
            IsImplicitAttribute[] attrs = evt.Signature.GetAttributes<IsImplicitAttribute>();
            if (attrs.Length > 0)
            {
                HashSet<IHasState> beatParticipants = GetParticipants();

                Dictionary<int, IHasState> fixedSlots = new Dictionary<int, IHasState>();
                //we make sure all user filled slots are kept, and simply fill in the rest
                for (int i = 0; i < attrs[0].FirstImplicitIndex; i++)
                {
                    fixedSlots[i] = ObjectManager.Instance.GetObjectById(evt.Participants[i]);
                }

                //must fill in implicit parameters, if we can not it is ok if we crash..
                IList<IHasState> allWorldObjs =
                    ObjectManager.Instance.GetObjects().Cast<IHasState>().ToList();
                //make sure the same object is not used more than once as a participants
                List<IHasState> availableObjs =
                    ObjectManager.Instance.GetObjects().Cast<IHasState>().ToList();
                availableObjs.RemoveAll(beatParticipants.Contains);
                IEnumerable<EventPopulation> pops =
                    EventPopulator.GetValidPopulations(
                        evt.Signature,
                        availableObjs,
                        allWorldObjs,
                        fixedSlots);
                EventPopulation pop = null;
                foreach (EventPopulation p in pops)
                {
                    //make sure the types actually match, else check the next match.
                    if (evt.Signature.CheckTypes(p.AsParams()))
                        pop = p;
                }
                //Set the implicit parameters with the given population
                for (int i = attrs[0].FirstImplicitIndex; i < pop.Count; i++)
                {
                    evt.Participants[i] = pop[i].Id;
                }
            }
            SmartEvent instance = evt.Signature.Create(this.GetObjectInstances(evt));
            instance.Behavior.StartEvent(priority);
            return instance;
        }

        /// <summary>
        /// Converts the id list to actual live object instances for
        /// execution
        /// </summary>
        private IList<IHasState> GetObjectInstances(StoryEvent evt)
        {
            List<IHasState> result = new List<IHasState>();
            foreach (uint id in evt.Participants)
                result.Add(ObjectManager.Instance.GetObjectById(id));
            return result;
        }
    }

    //The events in the current beat that have a CameraArgument attached to them
    private List<StoryBeatInstance.StoryEventInstance> currentCameraEvents 
        = new List<StoryBeatInstance.StoryEventInstance>();

    //The elapsed time for the current beat
    private float elapsedTimeForBeat;
    
    //The CameraArgumentManager keeping track of all camera arguments
    private CameraArgumentManager cameraManager;

    //The current LookAt position of the camera.
    private Vector3 currentLookAt;

    //The actual arc stored in this StoryArcRunner
    private StoryArc arc;

    //All the beat instances
    private StoryBeatInstance[] beats;
    
    //The index of the currently executing beat
    private int currentBeat = -1;

    /// <summary>
    /// Create a new StoryArcRunner with the given StoryArc, the given dependencies 
    /// and the given CameraArgumentManager.
    /// </summary>
    /// <param name="arc">The given StoryArc.</param>
    /// <param name="dependencies">The termination dependencies.</param>
    /// <param name="cameraManager">The camera manager.</param>
    /// <returns>Returns a new StoryArcRunner initialized with the given values.</returns>
    public static StoryArcRunner GetRunner(StoryArc arc, Dictionary<EventID, List<EventID>> dependencies, 
        CameraArgumentManager cameraManager)
    {
        //Create a new GameObject to hold the StoryArcRunner. We have it as a MonoBehavior to make the
        //camera work smoothly
        GameObject newGO = new GameObject("Runner");
        StoryArcRunner runner = newGO.AddComponent<StoryArcRunner>();
        runner.beats = new StoryBeatInstance[arc.Beats.Length];
        for (int i = 0; i < arc.Beats.Length; i++)
            runner.beats[i] = new StoryBeatInstance(arc.Beats[i], dependencies);
        runner.arc = arc;
        runner.cameraManager = cameraManager;
        return runner;
    }

    /// <summary>
    /// Starts running the given story arc by registering it to
    /// the BehaviorManager.
    /// </summary>
    internal void StartRunning()
    {
        BehaviorManager.Instance.Register(this);
        List<SmartObject> participants = new List<SmartObject>();
        //the smart crowd should not change the state of any character participating in a narrative.
        foreach (uint id in arc.Participants)
        {
            participants.Add(ObjectManager.Instance.GetObjectById(id));
        }
        foreach (SmartCrowd crowd in ObjectManager.Instance.GetObjectsByType<SmartCrowd>())
        {
            crowd.StartScheduler();
        }
    }

    /// <summary>
    /// The BehaviorUpdate function to implement IBehaviorUpdate.
    /// Updates execution of the current beat, and updates the current camera argument.
    /// </summary>
    /// <param name="deltaTime">Time since last BehaviorUpdate call.</param>
    /// <returns>A RunStatus indicating whether we are done or not.</returns>
    public RunStatus BehaviorUpdate(float deltaTime)
    {
        if (this.currentBeat < 0
            || this.beats[currentBeat].HasTerminated() == true)
        {
            this.currentBeat++;
            //When starting a new Beat, start the beat itself and also update the camera arguments.
            if (this.currentBeat < this.beats.Length)
            {
                this.beats[currentBeat].Start();
                UpdateCameraArgument();
                return RunStatus.Running;
            }
            return RunStatus.Success;
        }
        return RunStatus.Running;
    }

    /// <summary>
    /// The Update method from the MonoBehavior, where we update the camera's position and rotation
    /// to make it smooth.
    /// </summary>
    void Update()
    {
        this.elapsedTimeForBeat += Time.deltaTime;
        UpdateCamera(this.elapsedTimeForBeat, Time.deltaTime);
    }

    /// <summary>
    /// Updates the currentCameraEvent when a new beat is started, so its camera arguments will be retrieved.
    /// Note that if there are multiple events with camera arguments in a beat, all but one are ignored.
    /// </summary>
    private void UpdateCameraArgument()
    {
        this.currentCameraEvents.Clear();
        foreach (StoryBeatInstance.StoryEventInstance evnt in beats[currentBeat].Events.Keys)
        {
            if (cameraManager.GetArgumentsForID(evnt.evnt.ID).Count() > 0)
            {
                this.currentCameraEvents.Add(evnt);
                this.elapsedTimeForBeat = 0.0f;
                //if this is the first beat, don't change camera smoothly but instead just make changes
                //instantaneous so camera position when starting does not matter.
                if (currentBeat == 0 && cameraManager.GetArgumentForTime(evnt.evnt.ID, 0.0f) != null)
                {
                    CameraArgument arg = cameraManager.GetArgumentForTime(evnt.evnt.ID, 0.0f);
                    if (arg.TimeInEvent == 0.0f)
                    {
                        SmartObject target = ObjectManager.Instance.GetObjectById(evnt.evnt.Participants[arg.TargetIndex]);
                        Camera.main.transform.position = arg.GetDesiredPosition(target);
                        Camera.main.transform.LookAt(target.transform);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Updates the current camera based on the currentCameraEvent to retrieve the current CameraArgument
    /// and then update the camera's position based on it.
    /// </summary>
    private void UpdateCamera(float elapsedTime, float deltaTime)
    {
        if (currentCameraEvents.Count > 0)
        {
            //Get the best CameraArgument, s.t. startTime > elapsedTime and the startTime is the minimal time
            CameraArgument best = null;
            int chosenIndex = 0;
            for (int i = 0; i < currentCameraEvents.Count; i++)
            {
                CameraArgument current = cameraManager.GetArgumentForTime(currentCameraEvents[i].evnt.ID, elapsedTime);
                if (best == null || (current != null && current.TimeInEvent > best.TimeInEvent))
                {
                    best = current;
                    chosenIndex = i;
                }
            }
            CameraArgument currentArgument = best;
            if (currentArgument != null)
            {
                //Sets the wall fade targets to the ones specified in the current argument, if we are in a scene
                //where we can actually fade walls.
                if (SelectWallFade.Instance != null)
                {
                    SelectWallFade.Instance.FadeTargets = new Transform[currentArgument.FadeIndices.Count];
                    for (int i = 0; i < currentArgument.FadeIndices.Count; i++)
                    {
                        SelectWallFade.Instance.FadeTargets[i] =
                            ObjectManager.Instance.GetObjectById(
                                currentCameraEvents[chosenIndex].evnt.Participants[currentArgument.FadeIndices.ElementAt(i)]).transform;
                    }
                }
                SmartObject lookAtTarget = ObjectManager.Instance.GetObjectById(
                    currentCameraEvents[chosenIndex].evnt.Participants[currentArgument.TargetIndex]);
                //get the desired position and change it smoothly
                Vector3 desiredPosition = currentArgument.GetDesiredPosition(lookAtTarget);
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, desiredPosition, currentArgument.Smoothness * deltaTime);
                //get the desired rotation and change it smoothly
                Quaternion desiredRotation = currentArgument.GetDesiredRotation(lookAtTarget);
                Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, desiredRotation, currentArgument.Smoothness * deltaTime);
            }
        }
    }
}
