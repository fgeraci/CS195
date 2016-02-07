using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Class offering logic when trying to add SmartObject parameters to an event.
/// </summary>
public class SmartObjectSelector
{
    /// <summary>
    /// The event handler for when the selected object changes. Takes both the old
    /// and the new object as input
    /// </summary>
    /// <param name="oldObject">The old selceted object.</param>
    /// <param name="newObject">The new selected object.</param>
    public delegate void ObjectChangedEventHandler(
        SmartObject oldObject,
        SmartObject newObject);

    /// <summary>
    /// The type of which the given object needs to be.
    /// </summary>
    public Type NeededType { get; private set; }

    /// <summary>
    /// These roles are needed for each index in Roles. It is however not guaranteed
    /// that an object with only these roles satisfies the roles of a single index.
    /// These are just used for displaying the roles in the GUI.
    /// </summary>
    public IList<StateName> MinimalRoles { get; private set; }

    /// <summary>
    /// The needed roles for the object to fulfill.
    /// As one selector can refer to multiple signatures, this may be different
    /// for different roles.
    /// </summary>
    private IList<StateName>[] Roles;

    /// <summary>
    /// The indexes in Roles to which the currently selected object matches.
    /// </summary>
    public IEnumerable<int> MatchingIndexes { get; private set; }

    /// <summary>
    /// Whether the given object should be included in the Participants() method of
    /// an EventStub.
    /// </summary>
    public bool IsParticipant { get; private set; }

    /// <summary>
    /// The selected object. Set it using TrySet. Guaranteed to be either of correct type and role, or null.
    /// </summary>
    public SmartObject selectedObject { get; private set; }

    /// <summary>
    /// Register to this event to receive an event with both the old and the newly selected object whenever
    /// there is a change to selectedObject.
    /// </summary>
    public event ObjectChangedEventHandler OnObjectChanged;

    /// <summary>
    /// Create a new SmartObjectSelector with the given type, roles and isParticipant.
    /// </summary>
    /// <param name="type">The needed type of the object.</param>
    /// <param name="roles">The roles needed to be satisfied for the different indices.</param>
    /// <param name="isParticipant">Whether the object is a participant.</param>
    public SmartObjectSelector(Type type, IList<StateName>[] roles, bool isParticipant)
    {
        this.NeededType = type;
        this.Roles = roles;
        IEnumerable<StateName> minimalRoles = new List<StateName>(roles[0]);
        this.MatchingIndexes = new List<int>();
        for (int i = 0; i < roles.Length; i++)
        {
            minimalRoles = minimalRoles.Intersect(roles[i]);
            ((List<int>)this.MatchingIndexes).Add(i);
        }
        this.MinimalRoles = new List<StateName>(minimalRoles);
        this.IsParticipant = isParticipant;
    }

    /// <summary>
    /// Try adding the given object. Adding succeeds iff CanSet holds for the object.
    /// If it succeeds, also calls all listeners.
    /// Returns whether the object could be added.
    /// A null object can always be set.
    /// </summary>
    public bool TrySet(SmartObject obj)
    {
        List<int> matching = null;
        if (CanSet(obj, out matching))
        {
            MatchingIndexes = matching;
            SmartObject old = this.selectedObject;
            this.selectedObject = obj;
            OnObjectChanged.Invoke(old, this.selectedObject);
        }
        return selectedObject == obj;
    }

    /// <summary>
    /// Checks whether the given object can be added. If it returns true, TrySet will also succeed
    /// for the given object.
    /// The object must either be null or else be of correct type and role.
    /// </summary>
    public bool CanSet(SmartObject obj)
    {
        List<int> ignore = null;
        return CanSet(obj, out ignore);
    }

    /// <summary>
    /// Checks whether the given object can be added. Also returns the list of indices where the roles
    /// match the given object's roles.
    /// </summary>
    private bool CanSet(SmartObject obj, out List<int> matching)
    {
        bool matchesRole = false;
        matching = new List<int>();
        for (int i = 0; i < Roles.Length; i++)
        {
            if (CanSet(obj, i))
            {
                matching.Add(i);
                matchesRole = true;
            }
        }
        return obj == null || (NeededType.IsAssignableFrom(obj.GetType()) && matchesRole);
    }

    /// <summary>
    /// Checks whether the given object can be set at the given index.
    /// </summary>
    private bool CanSet(SmartObject obj, int index)
    {
        return obj == null || obj.State.Require(Roles[index]);
    }

    public override string ToString()
    {
        return NeededType.Name + "\n" + RolesToString(MinimalRoles);
    }

    /// <summary>
    /// Returns a string for the given collection of roles.
    /// </summary>>
    /// <returns></returns>
    private string RolesToString(IList<StateName> roles)
    {
        if (roles.Count == 0)
        {
            return "";
        }
        string result = "Roles: ";
        for (int i = 0; i < roles.Count - 1; i++)
        {
            result += RoleToString(roles[i]) + ", ";
        }
        result += RoleToString(roles[roles.Count - 1]);
        return result;
    }

    ///// <summary>
    ///// Returns a string representation of a single role.
    ///// </summary>
    private string RoleToString(StateName role)
    {
        if (role < 0)
        {
            return "~" + (~role).ToString();
        }
        else
        {
            return role.ToString();
        }
    }
}