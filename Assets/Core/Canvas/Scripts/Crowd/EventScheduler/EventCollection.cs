using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A collection of SchedulerEvents, each with its own probability to be chosen. Can get random
/// EventSignatures according to the probabilities.
/// </summary>
public class EventCollection : IEnumerable<SchedulableEvent>
{
    /// <summary>
    /// The name given to this collection.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The time between updates in seconds, meaning a new EventSignature should
    /// be fetched all TimeBetweenUpdates seconds.
    /// </summary>
    public readonly float TimeBetweenUpdates;

    /// <summary>
    /// All the events used within this EventCollection.
    /// </summary>
    public readonly SchedulableEvent[] Events;

    private bool isEnabled;

    public EventCollection(string name, float timeBetweenUpdates, params SchedulableEvent[] events)
    {
        this.Name = name;
        this.TimeBetweenUpdates = timeBetweenUpdates;
        this.Events = events;
        this.isEnabled = true;
        AdaptProbabilities();
    }

    /// <summary>
    /// Enables or disables the current EventCollection. A disabled EventCollection
    /// will always return null on GetRandomSignature.
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        this.isEnabled = enabled;
    }

    /// <summary>
    /// Makes sure the sum of all probabilities is 1.
    /// </summary>
    private void AdaptProbabilities()
    {
        float sum = 0.0f;
        foreach (SchedulableEvent evnt in Events)
        {
            sum += evnt.Probability;
        }
        foreach (SchedulableEvent evnt in Events)
        {
            evnt.AdaptProbability(sum);
        }
    }

    /// <summary>
    /// Gets a random EventSignature according to the probabilities set to each.
    /// If none can be found, or the EventCollection is disabled, returns null.
    /// </summary>
    public EventSignature GetRandomSignature()
    {
        if (!isEnabled)
        {
            return null;
        }
        float randomValue = Random.Range(0.0f, 1.0f);
        float sumOfProbabilities = 0.0f;
        foreach (SchedulableEvent evnt in Events)
        {
            if (sumOfProbabilities < randomValue && 
                randomValue <= sumOfProbabilities + evnt.Probability)
            {
                return evnt.Signature;
            }
            sumOfProbabilities += evnt.Probability;
        }
        //if there are no events at all
        return null;
    }

    public IEnumerator<SchedulableEvent> GetEnumerator()
    {
        return ((IEnumerable<SchedulableEvent>)Events).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return Events.GetEnumerator();
    }
}
