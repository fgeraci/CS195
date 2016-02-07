using System.Collections.Generic;
using System.IO;
using LitJson;
using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// A class which can serialize a collection of EventCollections, and deserialize the 
/// collection again.
/// </summary>
public class EventCollectionSerializer
{
    /// <summary>
    /// Self-generating singleton instance.
    /// </summary>
    public static EventCollectionSerializer Instance
    { get { return instance ?? (instance = new EventCollectionSerializer()); } }

    private static EventCollectionSerializer instance;

    private static string PATH_TO_SERIALIZED_COLLECTIONS =
        Path.Combine(Application.persistentDataPath, "EventCollections.json");

    /// <summary>
    /// Saves the given EventCollcetions using JSON.
    /// </summary>
    public void Save(IEnumerable<EventCollection> collections)
    {
        File.WriteAllText(PATH_TO_SERIALIZED_COLLECTIONS, JsonMapper.ToJson(new EventCollectionList(collections)));     
    }

    /// <summary>
    /// Load previously saved EventCollections and returns them.
    /// </summary>
    public IEnumerable<EventCollection> Load()
    {
        if (File.Exists(PATH_TO_SERIALIZED_COLLECTIONS))
        {
            EventCollectionList list = JsonMapper.ToObject<EventCollectionList>(File.ReadAllText(PATH_TO_SERIALIZED_COLLECTIONS));
            return list.ToEventCollectionList();
        }
        return new EventCollection[0];
    }

    #region Internal Wrapper Classes
    /// <summary>
    /// A simple wrapper for a list of EventCollectionWrappers, which makes it easy to serialize using
    /// LitJSON.
    /// </summary>
    private class EventCollectionList
    {
        public EventCollectionWrapper[] Wrappers;

        //Empty constructor for LitJSON
        public EventCollectionList() { }

        public EventCollectionList(IEnumerable<EventCollection> allCollections)
        {
            this.Wrappers = new EventCollectionWrapper[allCollections.Count()];
            int index = 0;
            foreach (EventCollection collection in allCollections)
            {
                this.Wrappers[index] = new EventCollectionWrapper(collection);
                index++;
            }
        }

        public IEnumerable<EventCollection> ToEventCollectionList()
        {
            foreach (EventCollectionWrapper wrapper in Wrappers)
            {
                yield return wrapper.ToEventCollection();
            }
        }
    }

    #endregion
}

#region Helper Classes
/// <summary>
/// A simple wrapper for an EventCollection, making it easy to serialize using LitJSON.
/// </summary>
public class EventCollectionWrapper
{
    public string Name;

    public string TimeBetweenUpdates;

    public bool AutoBalance;

    public SchedulableEventWrapper[] SchedulableEvents;

    //Empty constructor for LitJSON
    public EventCollectionWrapper() { }

    public EventCollectionWrapper(EventCollection collection)
    {
        this.Name = collection.Name;
        this.TimeBetweenUpdates = collection.TimeBetweenUpdates.ToString();
        this.AutoBalance = false;
        this.SchedulableEvents = new SchedulableEventWrapper[collection.Events.Length];
        for (int i = 0; i < this.SchedulableEvents.Length; i++)
        {
            this.SchedulableEvents[i] = new SchedulableEventWrapper(collection.Events[i]);
        }
    }

    public EventCollection ToEventCollection()
    {
        SchedulableEvent[] events = new SchedulableEvent[SchedulableEvents.Length];
        for (int i = 0; i < events.Length; i++)
        {
            events[i] = SchedulableEvents[i].ToSchedulableEvent();
        }
        return new EventCollection(Name, float.Parse(TimeBetweenUpdates), events);
    }


    /// <summary>
    /// A simple wrapper for a SchedulableEvent, making it easy to serialize using LitJSON.
    /// </summary>
    public class SchedulableEventWrapper
    {
        public string SignatureName;

        public string Probability;

        //Empty constructor for LitJSON.
        public SchedulableEventWrapper() { }

        public SchedulableEventWrapper(SchedulableEvent evnt)
        {
            this.SignatureName = evnt.Signature.Name;
            this.Probability = evnt.Probability.ToString();
        }

        public SchedulableEvent ToSchedulableEvent()
        {
            return new SchedulableEvent(EventLibrary.Instance.GetSignature(SignatureName), float.Parse(Probability));
        }

    }
}

#endregion
