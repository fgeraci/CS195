using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Window to configure an EventScheduler for a newly created SmartCrowd to be displayed in the main panel
/// of the GUI.
/// </summary>
public class EventSchedulerWindow : IRenderable
{
    /// <summary>
    /// An editable version of the SchedulableEvent, also containing information whether it is
    /// used for the current Scheduler.
    /// </summary>
    private class SchedulerEventArguments
    {
        //The signature for this event.
        public readonly EventSignature Signature;

        //The probability for it to be executed as a string.
        public string Probability;

        //Whether it is selected by the user.
        public bool IsSelected;

        public SchedulerEventArguments(EventSignature signature, float probability, bool isSelected)
        {
            this.Signature = signature;
            this.Probability = probability.ToString();
            this.IsSelected = isSelected;
        }

        /// <summary>
        /// Get the current probability, or 0.0 if it can not be parsed.
        /// </summary>
        public float GetProbability()
        {
            float result;
            if (float.TryParse(Probability, out result))
            {
                return result;
            }
            return 0.0f;
        }

        /// <summary>
        /// Adapt the probability by the given factor, i.e. new probability = probability/factor.
        /// </summary>
        /// <param name="factor">Factor to adapt probability by.</param>
        public void AdaptProbability(float factor)
        {
            float value = this.GetProbability();
            this.Probability = (value / factor).ToString();
        }
    }

    //The current scroll position
    private Vector2 scrollPos;

    //All schedulable events
    private List<SchedulerEventArguments> allEvents = new List<SchedulerEventArguments>();

    //The time between updates to use for the scheduler
    private string timeBetweenUpdates = "0";

    //The name of the scheduler (used for reusing them)
    private string name = "";

    //Action bar to enable selecting previously saved event schedulers by name
    private ActionBar existingCollectionSelector;

    /// <summary>
    /// Creates a new EventSchedulerWindow.
    /// </summary>
    public EventSchedulerWindow()
    {
        IEnumerable<EventSignature> allSignatures = EventLibrary.Instance.GetSignatures();
        allSignatures = allSignatures.Where((EventSignature sig) => !sig.AffectsWorldState());
        foreach (EventSignature sig in allSignatures)
        {
            allEvents.Add(new SchedulerEventArguments(sig, 0.0f, false));
        }
        existingCollectionSelector = new ActionBar(true, true, false);
        foreach(EventCollection collection in EventCollectionManager.Instance.AllCollections)
        {
            EventCollection localCollection = collection;
            existingCollectionSelector.AddToggle(collection.Name, 
                (bool b) => 
                {
                    if (b) Set(localCollection);
                });
        }
    }

    /// <summary>
    /// Render the window with the given height and width.
    /// </summary>
    public void Render(float height, float width)
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.Label("Each SmartCrowd has an EventManager. Here you can select the Events it will choose from " +
            "and set their probabilities to be chosen. If you do not want an EventManager, simply don't check any boxes.");

        GUILayout.BeginHorizontal();
        //Enter the name of the scheduler if it should be stored.
        GUILayout.Label(new GUIContent("Name", "The name is used to distinguish different EventManagers."), GUILayout.Width(100));
        name = GUILayout.TextField(name, GUILayout.Width(200));
        GUILayout.EndHorizontal();

        //Offer selecting previously saved schedulers if any exist.
        if (EventCollectionManager.Instance.AllCollections.Count() > 0)
        {
            GUILayout.Label("You have already created some EventManagers. If you want to reuse them, select them below");
            existingCollectionSelector.Render(true);
        }

        //For each schedulable event, set their probability and whether they are selected.
        foreach (SchedulerEventArguments evnt in allEvents)
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = evnt.IsSelected;
            GUILayout.Label(evnt.Signature.Name, GUILayout.Width(400));
            GUILayout.Label("Probability: ", GUILayout.Width(100));
            evnt.Probability = GUILayout.TextField(evnt.Probability, GUILayout.Width(100));
            GUI.enabled = true;
            evnt.IsSelected = GUILayout.Toggle(evnt.IsSelected, "", GUILayout.Width(100));
            GUILayout.EndHorizontal();
        }
        //Adapts the probabilities so their sum is 1.
        if (GUILayout.Button(new GUIContent("Normalize Probabilities",
            "Will normalize the probabilities so their sum is 1, if possible"), GUILayout.Width(200)))
        {
            AdaptProbabilities();
        }
        //Removes the current collection from the saved ones (deletes by name)
        if (GUILayout.Button("Delete Current", GUILayout.Width(200)))
        {
            EventCollectionManager.Instance.RemoveEventCollection(name);
            existingCollectionSelector.RemoveToggle(name);
        }
        GUILayout.BeginHorizontal();
        //The time between event scheduler updates
        GUILayout.Label("Time Between Updates", GUILayout.Width(200));
        timeBetweenUpdates = GUILayout.TextField(timeBetweenUpdates, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
    }

    /// <summary>
    /// Adapts all the probabilities so their sum is 1. Exception: If the sum is 0, it stays so.
    /// </summary>
    private void AdaptProbabilities()
    {
        float sum = 0.0f;
        foreach (SchedulerEventArguments evnt in allEvents)
        {
            if (evnt.IsSelected)
            {
                sum += evnt.GetProbability();
            }
        }
        foreach (SchedulerEventArguments evnt in allEvents)
        {
            if (evnt.IsSelected && sum != 0.0f)
            {
                evnt.AdaptProbability(sum);
            }
        }
    }

    /// <summary>
    /// Sets the schedulable events to the ones specified by the given EventCollection.
    /// </summary>
    public void Set(EventCollection collection)
    {
        allEvents.Clear();
        IEnumerable<EventSignature> allSignatures = EventLibrary.Instance.GetSignatures().
            Where((EventSignature sig) => !sig.AffectsWorldState());
        //For each signature, check if it is used in the scheduler and set the values according to this.
        foreach(EventSignature signature in allSignatures)
        {
            if (collection.Any((SchedulableEvent evnt) => evnt.Signature == signature))
            {
                SchedulableEvent evnt = collection.First((SchedulableEvent e) => e.Signature == signature);
                allEvents.Add(new SchedulerEventArguments(signature, evnt.Probability, true));
            }
            else
            {
                allEvents.Add(new SchedulerEventArguments(signature, 0.0f, false));
            }
        }
        this.timeBetweenUpdates = collection.TimeBetweenUpdates.ToString();
        this.name = collection.Name;
    }

    /// <summary>
    /// Creates an EventCollection from the given input. If saveCollection is true, it also
    /// makes sure to save the returned EventCollection for later use.
    /// </summary>
    public EventCollection ToEventCollection(bool saveCollection)
    {
        this.AdaptProbabilities();
        SchedulableEvent[] events = new SchedulableEvent[allEvents.Where((SchedulerEventArguments arg) => arg.IsSelected).Count()];
        int index = 0;
        foreach (SchedulerEventArguments arg in allEvents)
        {
            if (arg.IsSelected)
            {
                events[index] = new SchedulableEvent(arg.Signature, arg.GetProbability());
                index++;
            }
        }
        float timeBetweenUpdates = 0.0f;
        float.TryParse(this.timeBetweenUpdates, out timeBetweenUpdates);
        EventCollection result = new EventCollection(name, timeBetweenUpdates, events);
        if (saveCollection)
        {
            EventCollectionManager.Instance.AddEventCollection(result);
        }
        return result;
    }
}
