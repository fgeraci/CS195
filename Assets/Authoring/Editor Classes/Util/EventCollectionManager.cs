using System.Collections.Generic;

/// <summary>
/// A class which keeps track of all currently existing EventCollections, and adds
/// functionality to both add and remove new EventCollections.
/// </summary>
public class EventCollectionManager
{
    public static EventCollectionManager Instance
    { get { return instance ?? (instance = new EventCollectionManager()); } }

    private static EventCollectionManager instance;

    private Dictionary<string, EventCollection> collections = new Dictionary<string, EventCollection>();

    /// <summary>
    /// All the EventCollections that have been saved.
    /// </summary>
    public IEnumerable<EventCollection> AllCollections { get { return collections.Values; } }

    /// <summary>
    /// Adds the given EventCollection.
    /// </summary>
    public void AddEventCollection(EventCollection collection)
    {
        collections[collection.Name] = collection;
    }

    /// <summary>
    /// Removes the EventCollection with the given name, if exists.
    /// </summary>
    public void RemoveEventCollection(string name)
    {
        collections.Remove(name);
    }

    /// <summary>
    /// Sets the set of EventCollections to include exactly the EventCollections
    /// specified in the input.
    /// </summary>
    public void SetAllCollections(IEnumerable<EventCollection> collections)
    {
        this.collections.Clear();
        foreach (EventCollection collection in collections)
        {
            this.collections[collection.Name] = collection;
        }
    }
}
