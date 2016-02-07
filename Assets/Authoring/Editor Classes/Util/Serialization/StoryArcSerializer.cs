using UnityEngine;
using System.Collections.Generic;
using System.IO;
using LitJson;
using System.Text.RegularExpressions;
using System.Linq;

/// <summary>
/// A class offering functionality for serializing and deserializing StoryArcs.
/// Also offers functionality to receive the names of all stored StoryArcs for
/// a selection.
/// </summary>
public class StoryArcSerializer
{
    /// <summary>
    /// Auto-generating singleton.
    /// </summary>
    public static StoryArcSerializer Instance
    { get { return instance ?? (instance = new StoryArcSerializer()); } }

    private static StoryArcSerializer instance;

    private string path;

    /// <summary>
    /// The currently used directory.
    /// </summary>
    public string DirectoryPath
    {
        get
        {
            return path;
        }
        set
        {
            string[] parts = value.Split(new string[] { "/", "\\" },
                System.StringSplitOptions.RemoveEmptyEntries);
            string path = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                path = path + Path.DirectorySeparatorChar + parts[i];
            }
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                this.path = path;
            }
        }
    }

    private static string DEFAULT_PATH = Path.Combine(
        Path.Combine(Application.persistentDataPath, "SerializedNarratives"),
        Application.loadedLevelName);

    private const string FILE_NAME_PATTERN = @"[\w ]+";

    public const string TEMP_FILE_NAME = "tempFile";

    private StoryArcSerializer()
    {
        EnsureDirectoryExists(DEFAULT_PATH);
        path = DEFAULT_PATH;
    }

    /// <summary>
    /// Ensure that the directory pointed to by the path exists.
    /// Creates a directory if it doesn't exist yet.
    /// </summary>
    private void EnsureDirectoryExists(string dirPath)
    {
        DirectoryInfo dir = new DirectoryInfo(dirPath);
        if (!dir.Exists)
        {
            dir.Create();
        }
    }

    /// <summary>
    /// Gets the full file name for the given file.
    /// </summary>
    private string FullFilename(string filename)
    {
        return Path.Combine(DirectoryPath, filename + ".json");
    }

    /// <summary>
    /// Serializes the given FullStoryArc under the given name.
    /// The FullStoryArc can then be retrieved using the given name
    /// and Deserialize.
    /// Only set checkFileName to false if the name is guaranteed to be valid.
    /// Do not combine with a name retrieved from user input.
    /// </summary>
    public void Serialize(FullStoryArc arc, string name, bool checkFileName = true)
    {
        if (checkFileName && !IsValidFileName(name))
        {
            return;
        }
        FullStoryArcWrapper wrapper = new FullStoryArcWrapper(arc);
        File.WriteAllText(FullFilename(name), JsonMapper.ToJson(wrapper));
        Debug.Log(FullFilename(name));
    }

    /// <summary>
    /// Returns the FullStoryArc stored under the given name.
    /// </summary>
    public FullStoryArc Deserialize(string name)
    {
        string filepath = FullFilename(name);
        if (File.Exists(filepath))
        {
            FullStoryArcWrapper wrapper = JsonMapper.ToObject<FullStoryArcWrapper>(File.ReadAllText(filepath));
            return wrapper.ToFullStoryArc();
        }
        return new FullStoryArc();
    }

    /// <summary>
    /// Deletes the FullStoryArc associated with the given name.
    /// Only set checkFileName to false if the name is guaranteed to exist.
    /// Do not combine with a name retrieved from user input.
    /// </summary>
    public void Delete(string name, bool checkFileName = true)
    {
        string filepath = FullFilename(name);
        if (File.Exists(filepath) && checkFileName && IsValidFileName(name))
        {
            File.Delete(filepath);
        }
    }

    /// <summary>
    /// Returns all the file names that correspond to existing narratives.
    /// </summary>
    public IEnumerable<string> OfferNames()
    {
        DirectoryInfo info = new DirectoryInfo(DirectoryPath);
        foreach (FileInfo file in info.GetFiles())
        {
            if (file.Extension == ".json")
            {
                //Removes the extension from the displayed file name
                string fileName = Regex.Replace(file.Name, @"\..*", "");
                if (IsValidFileName(fileName))
                {
                    yield return fileName;
                }
            }
        }
    }

    /// <summary>
    /// Returns true iff the file name is valid in a sense that using Serialize/Deserialize
    /// with the name will succeed.
    /// </summary>
    public bool IsValidFileName(string name)
    {
        return name != null && Regex.IsMatch(name, FILE_NAME_PATTERN) && name != TEMP_FILE_NAME;
    }

    #region Internal Wrapper Classes
    private class FullStoryArcWrapper
    {
        /// <summary>
        /// A wrapper for a StoryArc to be used for serialization.
        /// </summary>
        public class StoryArcWrapper
        {
            public class StoryBeatWrapper
            {
                public class StoryEventWrapper
                {
                    public string SignatureName;

                    public uint[] Participants;

                    public uint ID;

                    //Empty constructor for LitJSON
                    public StoryEventWrapper() { }

                    public StoryEventWrapper(StoryEvent evnt)
                    {
                        this.SignatureName = evnt.Signature.Name;
                        this.Participants = evnt.Participants;
                        this.ID = evnt.ID.ID;
                    }

                    /// <summary>
                    /// Returns a StoryEvent from the data contained. Also updates the mapping
                    /// oldToNewID to map the old id's value to the new one.
                    /// </summary>
                    public StoryEvent ToStoryEvent(Dictionary<uint, EventID> oldToNewID)
                    {
                        EventID id = new EventID();
                        oldToNewID[ID] = id;
                        return new StoryEvent(
                            EventLibrary.Instance.GetSignature(SignatureName),
                            id,
                            Participants);
                    }

                }

                public StoryEventWrapper[] Events;

                //Empty constructor for LitJSON
                public StoryBeatWrapper() { }

                public StoryBeatWrapper(StoryBeat beat)
                {
                    this.Events = new StoryEventWrapper[beat.Events.Length];
                    for (int i = 0; i < this.Events.Length; i++)
                    {
                        this.Events[i] = new StoryEventWrapper(beat.Events[i]);
                    }
                }

                /// <summary>
                /// Creates a StoryBeat from the contained data. Also passes the oldToNewId
                /// mapping to all contained StoryEventWrappers.
                /// </summary>
                public StoryBeat ToStoryBeat(Dictionary<uint, EventID> oldToNewID)
                {
                    StoryEvent[] events = new StoryEvent[this.Events.Length];
                    for (int i = 0; i < events.Length; i++)
                    {
                        events[i] = this.Events[i].ToStoryEvent(oldToNewID);
                    }
                    return new StoryBeat(events);
                }
            }

            public StoryBeatWrapper[] Beats;

            //Empty constructor for LitJSON
            public StoryArcWrapper() { }

            public StoryArcWrapper(StoryArc arc)
            {
                this.Beats = new StoryBeatWrapper[arc.Beats.Length];
                for (int i = 0; i < this.Beats.Length; i++)
                {
                    this.Beats[i] = new StoryBeatWrapper(arc.Beats[i]);
                }
            }

            /// <summary>
            /// Creates a StoryArc with the contained data. Also passes the oldToNewId
            /// mapping to all its StoryBeatWrappers.
            /// </summary>
            public StoryArc ToStoryArc(Dictionary<uint, EventID> oldToNewID)
            {
                StoryBeat[] beats = new StoryBeat[this.Beats.Length];
                for (int i = 0; i < beats.Length; i++)
                {
                    beats[i] = this.Beats[i].ToStoryBeat(oldToNewID);
                }
                return new StoryArc(beats);
            }
        }

        /// <summary>
        /// Wrapper for termination dependencies to be used for serlization.
        /// </summary>
        public class DependencyWrapper
        {
            public uint ID;

            public List<uint> Dependencies;

            //Empty constructor for LitJSON
            public DependencyWrapper() { }

            public DependencyWrapper(uint id, List<uint> dependencies)
            {
                this.ID = id;
                this.Dependencies = dependencies;
            }
        }

        /// <summary>
        /// A wrapper for a SmartCrowd to be used for serialization.
        /// </summary>
        public class SmartCrowdWrapper
        {
            public Vector3Wrapper FirstEdge;

            public Vector3Wrapper SecondEdge;

            public EventCollectionWrapper Collection;

            public bool IsStatic;

            public uint ID;

            //Empty constructor for LitJSON
            public SmartCrowdWrapper() { }

            public SmartCrowdWrapper(SmartCrowd crowd)
            {
                this.IsStatic = crowd.IsStatic;
                this.Collection = new EventCollectionWrapper(crowd.EventCollection);
                ISmartCrowdCriteria criteria = crowd.CrowdCriteria;
                SpatialAreaCriteria crit = (SpatialAreaCriteria)criteria;
                GroundRectangle rect = (GroundRectangle)crit.Area;
                this.FirstEdge = new Vector3Wrapper(rect.FirstEdge);
                this.SecondEdge = new Vector3Wrapper(rect.SecondEdge);
                this.ID = crowd.Id;
            }

            /// <summary>
            /// Returns a SmartCrowd corresponding to this wrapper's contained data. Adds
            /// a mapping from its old ID to its new ID to oldToNewID.
            /// </summary>
            public SmartCrowd ToSmartCrowd(Dictionary<uint, uint> oldToNewID)
            {
                Vector3 firstEdge = this.FirstEdge.ToVector3();
                Vector3 secondEdge = this.SecondEdge.ToVector3();
                SmartCrowd crowd = CrowdGenerator.Instance.GenerateCrowd(
                    new SpatialAreaCriteria(new GroundRectangle(firstEdge, secondEdge)),
                    IsStatic,
                    Collection.ToEventCollection());
                oldToNewID[ID] = crowd.Id;
                return crowd;
            }
        }

        /// <summary>
        /// A wrapper for a Vector3 to be used for serialization (convert
        /// values to strings, as LitJSON does not support floats).
        /// </summary>
        public class Vector3Wrapper
        {
            public string[] Values;

            //Empty constructor for LitJSON
            public Vector3Wrapper() { }

            public Vector3Wrapper(Vector3 value)
            {
                this.Values = new string[3];
                this.Values[0] = value.x.ToString();
                this.Values[1] = value.y.ToString();
                this.Values[2] = value.z.ToString();
            }

            /// <summary>
            /// Returns a Vector3 corresponding to the contained data.
            /// </summary>
            public Vector3 ToVector3()
            {
                return new Vector3(float.Parse(Values[0]), float.Parse(Values[1]), float.Parse(Values[2]));
            }
        }

        /// <summary>
        /// A wrapper for a Quaternion to be used for serialization (convert
        /// values to strings, as LitJSON does not support floats).
        /// </summary>
        public class QuaternionWrapper
        {
            public string[] Values;

            //Empty constructor for LitJSON
            public QuaternionWrapper() { }

            public QuaternionWrapper(Quaternion value)
            {
                this.Values = new string[4];
                this.Values[0] = value.x.ToString();
                this.Values[1] = value.y.ToString();
                this.Values[2] = value.z.ToString();
                this.Values[3] = value.w.ToString();
            }

            /// <summary>
            /// Returns a Quaternion corresponding to the contained data.
            /// </summary>
            public Quaternion ToQuaternion()
            {
                return new Quaternion(float.Parse(Values[0]), float.Parse(Values[1]),
                    float.Parse(Values[2]), float.Parse(Values[3]));
            }
        }

        /// <summary>
        /// A wrapper for a SmartWaypoint to be used for serialization.
        /// </summary>
        public class SmartWaypointWrapper
        {
            public Vector3Wrapper Position;

            public uint ID;

            //Empty constructor for LitJSON
            public SmartWaypointWrapper() { }

            public SmartWaypointWrapper(SmartWaypoint waypoint)
            {
                this.Position = new Vector3Wrapper(waypoint.transform.position);
                this.ID = waypoint.Id;
            }

            /// <summary>
            /// Creates a SmartWaypoint corresponding to the contained data.
            /// </summary>
            /// <returns></returns>
            public SmartWaypoint ToSmartWaypoint(Dictionary<uint, uint> oldToNewID)
            {
                SmartWaypoint wp = SmartWaypoint.GenerateWaypoint(Position.ToVector3());
                oldToNewID.Add(ID, wp.Id);
                return wp;
            }
        }

        /// <summary>
        /// A wrapper for a CameraArgument to be used for serialization.
        /// </summary>
        public class CameraArgumentWrapper
        {
            public CameraPositionType Type;

            public CameraRotationType RotationType;

            public int Index;

            public int[] FadeIndices;

            public Vector3Wrapper Offset;

            public QuaternionWrapper Rotation;

            public string TimeInEvent;

            public string Smoothness;

            //Empty constructor for LitJSON
            public CameraArgumentWrapper() { }

            public CameraArgumentWrapper(CameraArgument argument)
            {
                this.Type = argument.PositionType;
                this.RotationType = argument.RotationType;
                this.Index = argument.TargetIndex;
                this.FadeIndices = new int[argument.FadeIndices.Count];
                for (int i = 0; i < argument.FadeIndices.Count; i++)
                {
                    this.FadeIndices[i] = argument.FadeIndices.ElementAt(i);
                }
                this.Offset = new Vector3Wrapper(argument.Offset);
                this.Rotation = new QuaternionWrapper(argument.FixedRotation);
                this.TimeInEvent = argument.TimeInEvent.ToString();
                this.Smoothness = argument.Smoothness.ToString();
            }

            /// <summary>
            /// Returns a CameraArgument corresponding to this wrapper's contained data.
            /// </summary>
            public CameraArgument ToCameraArgument()
            {
                return new CameraArgument(
                    this.Index,
                    new HashSet<int>(this.FadeIndices),
                    this.Type,
                    this.RotationType,
                    this.Rotation.ToQuaternion(),
                    float.Parse(this.TimeInEvent),
                    this.Offset.ToVector3(),
                    float.Parse(Smoothness));
            }

        }

        /// <summary>
        /// A wrapper for full information on a CameraArgument, i.e. including
        /// the EventID.
        /// </summary>
        public class FullCameraArgumentWrapper
        {
            public uint ID;

            public CameraArgumentWrapper[] Arguments;

            //Empty constructor for LitJSON
            public FullCameraArgumentWrapper() { }

            public FullCameraArgumentWrapper(EventID id, IEnumerable<CameraArgument> arguments)
            {
                this.ID = id.ID;
                this.Arguments = new CameraArgumentWrapper[arguments.Count()];
                int index = 0;
                foreach (CameraArgument arg in arguments)
                {
                    Arguments[index] = new CameraArgumentWrapper(arg);
                    index++;
                }
            }

            /// <summary>
            /// Returns a CameraArgument list corresponding to this wrapper's contained data.
            /// </summary>
            public List<CameraArgument> ToCameraArguments()
            {
                List<CameraArgument> result = new List<CameraArgument>();
                foreach (CameraArgumentWrapper arg in Arguments)
                {
                    result.Add(arg.ToCameraArgument());
                }
                return result;
            }
        }
        public StoryArcWrapper Arc;

        public DependencyWrapper[] Dependencies;

        public SmartCrowdWrapper[] Crowds;

        public SmartWaypointWrapper[] Waypoints;

        public FullCameraArgumentWrapper[] CameraArguments;

        //Empty constructor for LitJSON
        public FullStoryArcWrapper() { }

        public FullStoryArcWrapper(FullStoryArc arc)
        {
            this.Arc = new StoryArcWrapper(arc.Arc);
            this.Dependencies = new DependencyWrapper[arc.Dependencies.Count];
            int index = 0;
            foreach (EventID id in arc.Dependencies.Keys)
            {
                List<uint> dependencies = new List<uint>();
                foreach (EventID other in arc.Dependencies[id])
                {
                    dependencies.Add(other.ID);
                }
                this.Dependencies[index] = new DependencyWrapper(id.ID, dependencies);
                index++;
            }
            this.Crowds = new SmartCrowdWrapper[arc.Crowds.Count()];
            index = 0;
            foreach (SmartCrowd crowd in arc.Crowds)
            {
                this.Crowds[index] = new SmartCrowdWrapper(crowd);
                index++;
            }
            this.Waypoints = new SmartWaypointWrapper[arc.Waypoints.Count()];
            index = 0;
            foreach (SmartWaypoint waypoint in arc.Waypoints)
            {
                this.Waypoints[index] = new SmartWaypointWrapper(waypoint);
                index++;
            }
            this.CameraArguments = new FullCameraArgumentWrapper[arc.CameraArguments.Count];
            index = 0;
            foreach (EventID key in arc.CameraArguments.Keys)
            {
                this.CameraArguments[index] = new FullCameraArgumentWrapper(key, arc.CameraArguments[key]);
                index++;
            }
        }

        /// <summary>
        /// Returns a FullStoryArc corresponding to this wrapper's contained data.
        /// </summary>
        public FullStoryArc ToFullStoryArc()
        {
            Dictionary<uint, EventID> oldToNewEventID = new Dictionary<uint, EventID>();
            //create the story arcs
            StoryArc arc = Arc.ToStoryArc(oldToNewEventID);
            Dictionary<EventID, List<EventID>> dependencies = new Dictionary<EventID, List<EventID>>();
            //create all the dependencies, use the oldToNewID mapping to update to new IDs
            foreach (DependencyWrapper wrapper in this.Dependencies)
            {
                dependencies[oldToNewEventID[wrapper.ID]] = new List<EventID>();
                foreach (uint value in wrapper.Dependencies)
                {
                    dependencies[oldToNewEventID[wrapper.ID]].Add(oldToNewEventID[value]);
                }
            }
            Dictionary<EventID, List<CameraArgument>> cameraArguments = new Dictionary<EventID, List<CameraArgument>>();
            //create all the camera arguments, use the oldToNewID mapping to update to new IDs
            foreach (FullCameraArgumentWrapper wrapper in this.CameraArguments)
            {
                //if events are removed with camera arguments, they will still be stored by the manager -> remove here.
                if (oldToNewEventID.ContainsKey(wrapper.ID))
                {
                    cameraArguments[oldToNewEventID[wrapper.ID]] = wrapper.ToCameraArguments();
                }
            }
            //create all the crowds
            List<SmartCrowd> crowds = new List<SmartCrowd>();
            Dictionary<uint, uint> oldToNewObjectID = new Dictionary<uint, uint>();
            foreach (SmartCrowdWrapper wrapper in this.Crowds)
            {
                crowds.Add(wrapper.ToSmartCrowd(oldToNewObjectID));
            }
            //create all the waypoints
            List<SmartWaypoint> waypoints = new List<SmartWaypoint>();
            foreach (SmartWaypointWrapper wrapper in this.Waypoints)
            {
                waypoints.Add(wrapper.ToSmartWaypoint(oldToNewObjectID));
            }
            //use the oldToNewObjectID mapping to update all the ids in the StoryEvents
            //and StoryArcs which use them
            foreach (StoryBeat beat in arc.Beats)
            {
                foreach (StoryEvent evnt in beat.Events)
                {
                    SetOldToNewValues(evnt.Participants, oldToNewObjectID);
                }
            }
            SetOldToNewValues(arc.Participants, oldToNewObjectID);
            return new FullStoryArc(arc, dependencies, crowds, waypoints, cameraArguments);
        }

        /// <summary>
        /// Set all old values in values to the new values depending on the oldToNew mapping.
        /// </summary>
        private void SetOldToNewValues<S>(S[] values, Dictionary<S, S> oldToNew)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (oldToNew.ContainsKey(values[i]))
                {
                    values[i] = oldToNew[values[i]];
                }
            }
        }

    }
    #endregion

}




/// <summary>
/// A class containing full information on a StoryArc from the Editor. Contains a StoryArc itself,
/// a mapping of all the termination dependencies, the SmartCrowds generated by the GUI as well as all
/// the SmartWaypoints that were automatically generated for this narrative.
/// </summary>
public class FullStoryArc
{
    //The actual story arc
    public StoryArc Arc { get; private set; }

    //All termination dependencies
    public IDictionary<EventID, List<EventID>> Dependencies { get; private set; }

    //All created crowds
    public IEnumerable<SmartCrowd> Crowds { get; private set; }

    //All generated waypoints
    public IEnumerable<SmartWaypoint> Waypoints { get; private set; }

    //Full camera configuration
    public IDictionary<EventID, List<CameraArgument>> CameraArguments { get; private set; }

    /// <summary>
    /// Creates a new FullStoryArc.
    /// </summary>
    public FullStoryArc(
        StoryArc arc,
        IDictionary<EventID, List<EventID>> dependencies,
        IEnumerable<SmartCrowd> crowds,
        IEnumerable<SmartWaypoint> waypoints,
        IDictionary<EventID, List<CameraArgument>> cameraArguments)
    {
        this.Arc = arc;
        this.Dependencies = dependencies;
        this.Crowds = crowds;
        this.Waypoints = waypoints;
        this.CameraArguments = cameraArguments;
    }

    public FullStoryArc()
    {
        this.Arc = new StoryArc();
        this.Dependencies = new Dictionary<EventID, List<EventID>>();
        this.Crowds = new List<SmartCrowd>();
        this.Waypoints = new List<SmartWaypoint>();
        this.CameraArguments = new Dictionary<EventID, List<CameraArgument>>();
    }
}