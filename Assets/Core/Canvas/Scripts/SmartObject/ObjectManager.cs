using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Class that manages all the SmartObjects in the scene.
/// </summary>
public class ObjectManager 
{
    private static ObjectManager instance = null;

    /// <summary>
    /// Self-generating singleton.
    /// </summary>
    public static ObjectManager Instance
    {
        get
        {
            if (instance == null)
                instance = new ObjectManager();
            return instance;
        }
    }

    private List<SmartObject> objects;
    private Dictionary<uint, SmartObject> objectsById;

    private ObjectManager()
    {
        this.objects = new List<SmartObject>();
        this.objectsById = new Dictionary<uint, SmartObject>();
    }

    /// <summary>
    /// Get all registered objects.
    /// </summary>
    public IList<SmartObject> GetObjects()
    {
        return this.objects.AsReadOnly();
    }

    /// <summary>
    /// Get all registered objects of the given type.
    /// </summary>
    public IEnumerable<T> GetObjectsByType<T>() where T : SmartObject
    {
        foreach (SmartObject obj in this.objects)
        {
            if (obj is T)
            {
                yield return (T)obj;
            }
        }
    }
    
    /// <summary>
    /// Get all registered objects of the given type.
    /// </summary>
    public IEnumerable<SmartObject> GetObjectsByType(Type type)
    {
        foreach (SmartObject obj in this.objects)
        {
            if (type.IsAssignableFrom(obj.GetType()))
            {
                yield return obj;
            }
        }
    }

    /// <summary>
    /// Get a single object by its ID.
    /// </summary>
    public SmartObject GetObjectById(uint id)
    {
        SmartObject value;
        if (this.objectsById.TryGetValue(id, out value) == false)
            return null;
        return value;
    }

    /// <summary>
    /// Register the given object.
    /// </summary>
    public void RegisterSmartObject(SmartObject obj)
    {
        this.objects.Add(obj);
        this.objectsById.Add(obj.Id, obj);
    }

    /// <summary>
    /// Deregister the given object.
    /// </summary>
    public void DeregisterSmartObject(SmartObject obj)
    {
        this.objects.Remove(obj);
        this.objectsById.Remove(obj.Id);
    }
}
