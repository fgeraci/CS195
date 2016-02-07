using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// An EventStub is essentially a wrapper for a set of EventSignatures, offering an interface to the GUI
/// to be able to set parameters for the Event without directly needing to interface with the EventSignature.
/// The EventStub cares only about the roles of the parameters, but not their state or relationships.
/// </summary>
public class EventStub
{
    //All signatures that this EventStub could possibly correspond to.
    private List<EventSignature> allSignatures;

    /// <summary>
    /// The EventSignature this EventStub was initialized to. Will always stay the same.
    /// </summary>
    public EventSignature MainSignature { get; private set; }

    /// <summary>
    /// The EventSignature currently used by this EventStub. May change if merged.
    /// </summary>
    public EventSignature Signature { get; private set; }

    //The predecessors, i.e. the events that must terminate before this event.
    private List<EventStub> predecessors = new List<EventStub>();

    /// <summary>
    /// Returns a readonly collection of the predecessors of this EventStub.
    /// </summary>
    public IEnumerable<EventStub> Predecessors { get { return predecessors.AsReadOnly(); } }
    
    //The parameter selectors for this event
    private List<SmartObjectSelector> selectors = new List<SmartObjectSelector>();

    //The list of involved objects in this event
    private List<SmartObject> involvedObjects = new List<SmartObject>();

    /// <summary>
    /// All the contained objects in this event. Note that the order of the objects in this enumeration
    /// does not necessarily correspond to the order of the corresponding selectors. If the order is important,
    /// use OrderedInvolvedObjects instead. Note that OrderedInvolvedObjects is significantly less efficient though.
    /// </summary>
    public IEnumerable<SmartObject> InvolvedObjects { get { return involvedObjects.AsReadOnly(); } }

    //The list of participants in this event (i.e. those that do not have a NonParticipant attribute)
    private List<SmartObject> participants = new List<SmartObject>();

    /// <summary>
    /// All the participants in this event.
    /// </summary>
    public IEnumerable<SmartObject> Participants { get { return participants.AsReadOnly(); } }

    /// <summary>
    /// Returns the number of SmartObject roles that must be filled in for this EventStub.
    /// </summary>
    public int NrOfNeededRoles { get { return selectors.Count; } }

    /// <summary>
    /// The ID corresponding to this event.
    /// </summary>
    public EventID ID { get; private set; }

    //The name to be displayed in the GUI.
    private string name;

    /// <summary>
    /// The visible name of this EventStub.
    /// </summary>
    public string Name { get { return name; } }

    /// <summary>
    /// Private constructor, as TryGetEventStub should be used instead.
    /// </summary>
    private EventStub(EventSignature mainSignature, List<EventSignature> signatures)
        :this(mainSignature, signatures, new EventID())
    {
    }

    /// <summary>
    /// Private constructor, as TryGetEventStub should be used instead. 
    /// </summary>
    private EventStub(EventSignature mainSignature, List<EventSignature> signatures, EventID id)
    {
        this.MainSignature = this.Signature = mainSignature;
        this.allSignatures = signatures;
        AnalyzeSignatures(this.allSignatures);
        this.name = mainSignature.Name;
        this.name = Regex.Replace(this.name, "([a-z])([A-Z])", "$1 $2");
        this.name = Regex.Replace(this.name, "(\\(.*\\))", "");
        this.ID = id;
    }

    /// <summary>
    /// Creates a copy of the other EventStub. However, any parameters that were set are not carried over.
    /// Uses a new unique EventID.
    /// </summary>
    public EventStub(EventStub other)
        : this(other.MainSignature, other.allSignatures)
    {
    }


    #region Creating EventStubs
    /// <summary>
    /// Tries to create an EventStub for the given EventSignature. Returns whether it was successful.
    /// The EventStub returned as an out argument will be null if no EventStub can be generated, and the
    /// generated EventStub otherwise.
    /// Retarget indicates if the signature has a MergeAtAttribute, whether that link is followed or not.
    /// </summary>
    public static bool TryGetEventStub(EventSignature signature, out EventStub stub, 
        bool retarget = false, EventID id = null)
    {
        EventSignature chosen = signature;
        MergeAtAttribute[] mergeAt = chosen.GetAttributes<MergeAtAttribute>();
        if (mergeAt.Length != 0)
        {
            if (!retarget)
            {
                stub = null;
                return false;
            }
            var target = EventLibrary.Instance.GetSignaturesOfType(mergeAt[0].Target);
            if (target.Count() != 0)
            {
                chosen = target.First();
            }
        }
        MergeAttribute[] merge = chosen.GetAttributes<MergeAttribute>();
        IEnumerable<EventSignature> allSigs = new EventSignature[] { chosen };
        if (merge.Length != 0)
        {
            for (int i = 0; i < merge[0].Types.Length; i++)
            {
                allSigs = allSigs.Union(EventLibrary.Instance.GetSignaturesOfType(merge[0].Types[i]));
            }
        }
        try
        {
            if (id == null)
            {
                stub = new EventStub(chosen, new List<EventSignature>(allSigs));
            }
            else
            {
                stub = new EventStub(chosen, new List<EventSignature>(allSigs), id);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
            stub = null;
            return false;
        }
        return true;
    }

    /// <summary>
    /// Creates an EventStub from the given StoryEvent, fills in the set parameters.
    /// </summary>
    public static EventStub FromStoryEvent(StoryEvent evnt)
    {
        EventStub created = null;
        TryGetEventStub(evnt.Signature, out created, true, evnt.ID);
        for (int i = 0; i < created.NrOfNeededRoles; i++)
        {
            created.GetSelectorForIndex(i).TrySet(ObjectManager.Instance.GetObjectById(evnt.Participants[i]));
        }
        return created;
    }
    #endregion

    #region API
    /// <summary>
    /// Returns a StoryEvent with this event stub's signature and involved objects.
    /// </summary>
    public StoryEvent ToStoryEvent()
    {
        List<SmartObject> parameters = GetFullParams();
        uint[] ids = new uint[parameters.Count];
        for (int i = 0; i < parameters.Count; i++)
        {
            ids[i] = (parameters[i] == null) ? uint.MaxValue : parameters[i].Id;
        }
        return new StoryEvent(Signature, ID, ids);
    }

    /// <summary>
    /// Returns an ordered collection of the involved objects, meaning in the same ordering
    /// as the selectors themselves.
    /// </summary>
    public IEnumerable<SmartObject> OrderedInvolvedObjects()
    {
        foreach (SmartObjectSelector selector in selectors)
        {
            if (selector.selectedObject != null)
            {
                yield return selector.selectedObject;
            }
        }
        yield break;
    }

    /// <summary>
    /// Returns the SmartObjectSelector for the given index, where index is between
    /// 0 and NrOfneededRoles - 1
    /// </summary>
    public SmartObjectSelector GetSelectorForIndex(int index)
    {
        return selectors[index];
    }

    /// <summary>
    /// Adds another EventStub as predecessor to this event, so it will need to be executed first.
    /// </summary>
    public void AddPredecessor(EventStub other)
    {
        if (!predecessors.Contains(other))
        {
            predecessors.Add(other);
        }
    }

    /// <summary>
    /// Removes another EventStub as predecessor for this event.
    /// </summary>
    public void RemovePredecessor(EventStub other)
    {
        predecessors.Remove(other);
    }

    /// <summary>
    /// Set this EventStub's parameters to the given parameters where
    /// possible.
    /// </summary>
    public void TrySetParams(IEnumerable<SmartObject> parameters)
    {
        for (int i = 0; i < NrOfNeededRoles; i++)
        {
            this.GetSelectorForIndex(i).TrySet(parameters.ElementAt(i));
        }
    }

    /// <summary>
    /// Returns whether the participants of the two EventStubs overlap.
    /// </summary>
    public bool ParticipantsOverlap(EventStub other)
    {
        bool result = false;
        foreach (SmartObject obj in other.participants)
        {
            result |= this.participants.Contains(obj);
        }
        return result;
    }
    #endregion

    #region Internal
    /// <summary>
    /// Analyze the signatures to create the necessary selectors.
    /// </summary>
    /// <param name="signatures">The list of signatures for this event.</param>
    private void AnalyzeSignatures(IList<EventSignature> signatures)
    {
        Type[] types = signatures.First().RequiredTypes;
        NonParticipantAttribute[] attrs = signatures.First().GetAttributes<NonParticipantAttribute>();
        NonParticipantAttribute attribute = (attrs.Length > 0) ? attrs[0] : new NonParticipantAttribute();
        //The maximal index still shown in the GUI.
        int maxShownIndex = signatures.Min((EventSignature sig) => sig.ParameterCount);
        IsImplicitAttribute[] maxIndex = signatures.First().GetAttributes<IsImplicitAttribute>();
        maxShownIndex = (maxIndex.Length > 0) ? 
            Mathf.Min(maxIndex[0].FirstImplicitIndex, maxShownIndex) : maxShownIndex;
        for (int index = 0; index < maxShownIndex; index++)
        {
            Type type = types[index];
            //We only allow SmartObject parameters.
            if (typeof(SmartObject).IsAssignableFrom(type))
            {
                List<IList<StateName>> roles = new List<IList<StateName>>();
                for (int i = 0; i < signatures.Count; i++)
                {
                    roles.Add(ExtractRoles(signatures[i].StateReqs.
                        Where((StateCondition s) => s.Index == index)));
                }
                SmartObjectSelector selector = new SmartObjectSelector(type, roles.ToArray(), !attribute.Indices.Contains(index));
                RegisterSelector(selector, index);
            }
            else
            {
                Debug.LogError("Unrecognized type " + type.Name + " in Event signature " + Signature.Name);
                throw new Exception();
            }
        }
    }

    /// <summary>
    /// Extracts all the StateNames corresponding to roles from the given
    /// collection of conditions.
    /// </summary>
    private IList<StateName> ExtractRoles(IEnumerable<StateCondition> conditions)
    {
        List<StateName> result = new List<StateName>();
        foreach (StateCondition condition in conditions)
        {
            result.AddRange(Filter.StateList(condition.Tags, StateDefs.GetRoleStates()));
        }
        return result;
    }

    /// <summary>
    /// Registers with the given selector's listeners.
    /// </summary>
    private void RegisterSelector(SmartObjectSelector selector, int index)
    {
        selectors.Add(selector);
        selector.OnObjectChanged += (SmartObject o, SmartObject n) => Replace(involvedObjects, o, n);
        if (selector.IsParticipant)
        {
            selector.OnObjectChanged += (SmartObject o, SmartObject n) => Replace(participants, o, n);
        }
        selector.OnObjectChanged += (SmartObject o, SmartObject n) => OnSelect(o, n, index);
    }

    /// <summary>
    /// Executes logic when setting the value of a selector to a new object, making sure there is an
    /// index for the signatures available for all the roles. If there is none, resets the value of
    /// the selector to its old object.
    /// </summary>
    private void OnSelect(SmartObject oldObj, SmartObject newObj, int index)
    {
        List<int> indexes = CalculatePossibleIndexes();
        if (indexes.Count == 0)
        {
            //reset the selector's object if the selectors can not agree on an applicable index
            GetSelectorForIndex(index).TrySet(oldObj);
        }
        else
        {
            this.Signature = allSignatures[indexes[0]];
        }
    }

    /// <summary>
    /// Calculates all indexes in allSignatures that could be used with the currently set objects.
    /// </summary>
    private List<int> CalculatePossibleIndexes()
    {
        List<int> result = new List<int>();
        for (int i = 0; i < allSignatures.Count; i++)
        {
            result.Add(i);
        }
        for (int i = 0; i < NrOfNeededRoles; i++)
        {
            result.RemoveAll((int index) => !GetSelectorForIndex(i).MatchingIndexes.Contains(index));
        }
        return result;

    }

    /// <summary>
    /// Returns the full list of parameters for the given event stub, which may be more than just
    /// the ones in the selectors.
    /// </summary>
    private List<SmartObject> GetFullParams()
    {
        List<SmartObject> result = new List<SmartObject>();
        for (int i = 0; i < NrOfNeededRoles; i++)
        {
            result.Add(GetSelectorForIndex(i).selectedObject);
        }
        for (int i = NrOfNeededRoles; i < Signature.ParameterCount; i++)
        {
            result.Add(null);
        }
        return result;
    }

    /// <summary>
    /// Replaces oldObj with newObj in the given list. Does not preserve the index.
    /// </summary>
    private void Replace(List<SmartObject> list, SmartObject oldObj, SmartObject newObj)
    {
        list.Remove(oldObj);
        if (newObj != null)
        {
            list.Add(newObj);
        }
    }
    #endregion

    public override string ToString()
    {
        string result = "";
        result += "Name of Event signature: " + Signature.Name + "\n\n";
        result += "Selectors: \n";
        foreach (SmartObjectSelector selector in selectors)
        {
            result += selector.ToString() + " \n\n";
        }
        return result;
    }
}

