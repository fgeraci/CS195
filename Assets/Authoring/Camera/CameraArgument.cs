using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The Camera Argument Manager keeps track of all current camera arguments and maps
/// them to an EventID. With the EventID, the arguments can be retrieved in multiple ways.
/// </summary>
public class CameraArgumentManager
{
    /// <summary>
    /// The singleton instance for the CameraArgumentManager. Creates itself if it doesn't exist.
    /// </summary>
    public static CameraArgumentManager Instance
    { get { return instance ?? (instance = new CameraArgumentManager()); } }

    private static CameraArgumentManager instance;

    //Maps EventIds to their corresponding CameraArguments.
    private Dictionary<EventID, List<CameraArgument>> idToArguments = 
        new Dictionary<EventID,List<CameraArgument>>();

    /// <summary>
    /// The mapping between EventIDs and CameraArguments.
    /// </summary>
    public IDictionary<EventID, List<CameraArgument>> IDToArguments { get { return idToArguments; } }

    /// <summary>
    /// Creates a new empty CameraArgumentManager.
    /// </summary>
    private CameraArgumentManager() { }

    /// <summary>
    /// Adds the data stored in the given dictionary to the stored camera arguments.
    /// </summary>
    public void AddArguments(IDictionary<EventID, List<CameraArgument>> arguments)
    {
        foreach (EventID key in arguments.Keys)
        {
            GetArgumentsForIDInternal(key).AddRange(arguments[key]);
        }
    }

    /// <summary>
    /// Adds the given argument for the given ID.
    /// </summary>
    public void AddArgument(EventID id, CameraArgument argument)
    {
        if (!idToArguments.ContainsKey(id))
        {
            idToArguments[id] = new List<CameraArgument>();
        }
        idToArguments[id].Add(argument);
    }

    /// <summary>
    /// Removes all the arguments for the given ID.
    /// </summary>
    public void RemoveArguments(EventID id)
    {
        if (idToArguments.ContainsKey(id))
        {
            idToArguments.Remove(id);
        }
    }

    /// <summary>
    /// Internal version of GetArgumentsForId returning the result
    /// as a list.
    /// </summary>
    private List<CameraArgument> GetArgumentsForIDInternal(EventID id)
    {
        if (!idToArguments.ContainsKey(id))
        {
            idToArguments[id] = new List<CameraArgument>();
        }
        return idToArguments[id];
    }

    /// <summary>
    /// Gets the list of camera arguments stored for the given ID.
    /// </summary>
    public IEnumerable<CameraArgument> GetArgumentsForID(EventID id)
    {
        return GetArgumentsForIDInternal(id);
    }

    /// <summary>
    /// Get the camera argument for the given event, at the given time
    /// local to the event. Returns null if no match is found.
    /// </summary>
    public CameraArgument GetArgumentForTime(EventID id, float time)
    {
        IEnumerable<CameraArgument> allForId = GetArgumentsForID(id);
        if (allForId.Count() == 0)
        {
            return null;
        }
        CameraArgument bestMatch = null;
        foreach (CameraArgument arg in allForId)
        {
            if (arg.TimeInEvent <= time &&
                (bestMatch == null || bestMatch.TimeInEvent < arg.TimeInEvent))
            {
                bestMatch = arg;
            }
        }
        return bestMatch;
    }
}

/// <summary>
/// A camera argument for the configuration.
/// </summary>
public class CameraArgument
{
    //The index of the target object in the event
    public int TargetIndex;

    //The index of the objects that should be used for wall fading
    public HashSet<int> FadeIndices = new HashSet<int>();

    //The position type for position calculation
    public CameraPositionType PositionType;

    //The rotation type for rotation calculation
    public CameraRotationType RotationType;

    //The fixed rotation if needed by RotationType
    public Quaternion FixedRotation;

    //The event-local time when this argument starts
    public float TimeInEvent;

    //The offset from the target/position if fixed position
    public Vector3 Offset;

    //The smoothness of the transitions
    public float Smoothness = 1.0f;

    //Did we already set position for fixed offset?
    private bool fixedOffsetPositionSet;
    
    //The position for fixed offset position type
    private Vector3 fixedOffsetPosition;

    /// <summary>
    /// Creates a new Camera Argument
    /// </summary>
    public CameraArgument(
        int targetIndex,
        HashSet<int> fadeIndices,
        CameraPositionType positionType,
        CameraRotationType rotationType,
        Quaternion fixedRotation,
        float timeInEvent,
        Vector3 offset,
        float smoothness)
    {
        this.TargetIndex = targetIndex;
        this.FadeIndices = fadeIndices;
        this.PositionType = positionType;
        this.RotationType = rotationType;
        this.FixedRotation = fixedRotation;
        this.TimeInEvent = timeInEvent;
        this.Offset = offset;
        this.Smoothness = smoothness;
    }

    public CameraArgument() { }

    /// <summary>
    /// Gets the desired camera position depending on the given
    /// SmartObject as a lookat target. Depends on the lookAt target's
    /// position and the type of CameraPosition.
    /// </summary>
    public Vector3 GetDesiredPosition(SmartObject lookAt)
    {
        if (PositionType == CameraPositionType.Fixed)
        {
            return Offset;
        }
        else if (PositionType == CameraPositionType.Offset)
        {
            return lookAt.transform.position -
                    Offset.x * lookAt.transform.forward +
                    Offset.y * lookAt.transform.up;
        }
        else if (PositionType == CameraPositionType.FixedOffset)
        {
            if (!fixedOffsetPositionSet)
            {
                fixedOffsetPosition = lookAt.transform.position + Offset;
                fixedOffsetPositionSet = true;
            }
            return fixedOffsetPosition;
        }
        return new Vector3();
    }

    /// <summary>
    /// Gets the desired camera rotation depending on the given SmartObject
    /// as a lookat target and the type of rotation.
    /// </summary>
    public Quaternion GetDesiredRotation(SmartObject lookAt)
    {
        if (RotationType == CameraRotationType.Fixed)
        {
            return FixedRotation;
        }
        else
        {
            Animator animator = lookAt.GetComponent<Animator>();
            Vector3 target = animator == null ?
                lookAt.transform.position :
                animator.GetBoneTransform(HumanBodyBones.Head).position;
            return Quaternion.LookRotation(target - Camera.main.transform.position);
        }
    }
}

public enum CameraPositionType
{
    //Camera stays at same point all the time
    Fixed,
    //Camera moves with lookAt target, stays at offset
    Offset,
    //Camera stays at same point all the time, however
    //point is relative to lookAt target, not absolute
    FixedOffset
}

public enum CameraRotationType
{
    //Camera rotation is fixed
    Fixed,
    //Camera looks at some target
    LookAt
}