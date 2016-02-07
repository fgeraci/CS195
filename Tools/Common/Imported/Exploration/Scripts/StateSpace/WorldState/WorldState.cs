// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class WorldState : IEquatable<WorldState>
{
    private long[] matrix = null;
    private int? hashCode = null;

    // Lookup table of pairs in the format
    // (index, id)
    public readonly Dictionary<uint, int> IdToIndex;

    // These prototypes should be readonly
    public readonly ReadOnlyPrototype[] prototypes;

    public IList<ReadOnlyPrototype> Prototypes
    {
        get { return this.prototypes; }
    }

	public int GetIndexById(uint id)
	{
		return IdToIndex [id];
	}

	public ReadOnlyPrototype GetPrototypeById(uint id)
	{
		return this.prototypes[IdToIndex [id]];
	}

    /// <summary>
    /// Constructor from instantiated world state
    /// </summary>
    public WorldState(IEnumerable<IHasState> objs)
    {
        int count = objs.Count();
		this.IdToIndex = new Dictionary<uint, int> ();
        this.prototypes = new ReadOnlyPrototype[count];

        this.matrix = null;
        this.hashCode = null;

        int i = 0;
        foreach (IHasState obj in objs)
        {
            this.prototypes[i] = 
                new ReadOnlyPrototype(new ReadOnlyState(obj.State));
			this.IdToIndex.Add(obj.Id, i);
            i++;
        }
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public WorldState(WorldState other)
    {
        this.prototypes = new ReadOnlyPrototype[other.prototypes.Length];
		this.IdToIndex = other.IdToIndex;
        for (int i = 0; i < prototypes.Length; i++)
            this.prototypes[i] = other.prototypes[i];
    }

    public WorldState Transform(EventDescriptor evtDesc, IEnumerable<uint> ids)
    {
        IEnumerable<ReadOnlyPrototype> results =
            this.TransformParticipants(
                evtDesc,
                ids.Convert((uint id) => this.prototypes[IdToIndex[id]]));

        WorldState affected = new WorldState(this);
        foreach (ReadOnlyPrototype result in results)
            affected.prototypes[IdToIndex[result.Id]] = result;

        return affected;
    }

    /// <summary>
    /// Transforms the participants as if they had participated in the event
    /// </summary>
    public IEnumerable<ReadOnlyPrototype> TransformParticipants(
        EventDescriptor evtDesc,
        IList<ReadOnlyPrototype> participants)
    {
        int len = participants.Count;
        ChangeSet[] changes = new ChangeSet[len];
        for (int i = 0; i < len; i++)
            changes[i] = new ChangeSet(participants[i].Id);

        foreach (StateCondition condition in evtDesc.StateEffects)
        {
            DebugUtil.Assert(condition.Index < len);
            DebugUtil.Assert(participants[condition.Index] != null);

            changes[condition.Index].AddChange(condition.Tags);
        }

        foreach (RelationCondition condition in evtDesc.RelationEffects)
        {
            DebugUtil.Assert(condition.IndexTo < participants.Count);
            DebugUtil.Assert(condition.IndexFrom < participants.Count);
            DebugUtil.Assert(participants[condition.IndexTo] != null);
            DebugUtil.Assert(participants[condition.IndexFrom] != null);

            changes[condition.IndexFrom].AddChange(
                participants[condition.IndexTo].Id, condition.Tags);
        }

        for (int i = 0; i < len; i++)
            yield return participants[i].SetCopy(changes[i]);
        yield break;
    }

    public bool Equals(WorldState other)
    {
        int? ourHashCode = this.hashCode;
        int? otherHashCode = other.hashCode;
        if (ourHashCode.HasValue == true && otherHashCode.HasValue == true)
            if (ourHashCode.Value != otherHashCode.Value)
                return false;

        long[] ourMatrix = this.GetStateMatrix();
        long[] otherMatrix = other.GetStateMatrix();

        if (ourMatrix.Length != otherMatrix.Length)
            return false;

        for (int i = 0; i < ourMatrix.Length; i++)
            if (ourMatrix[i] != otherMatrix[i])
                return false;
        return true;
    }

    public override int GetHashCode()
    {
        if (this.hashCode.HasValue == false)
        {
            long total = 0;
            long[] matrix = this.GetStateMatrix();
            for (int i = 0; i < matrix.Length; i++)
                total = unchecked(total * 31 + matrix[i]);
            this.hashCode = (int)(total ^ (total >> 32));
        }

        return this.hashCode.Value;
    }

    public override string ToString()
    {
        string output = "";

        for (int i = 0; i < this.prototypes.Length; i++)
        {
            IHasState row = this.prototypes[i];
            if (row.Require(StateName.RoleWaypoint)) continue;

            for (int j = 0; j < this.prototypes.Length; j++)
            {
                IHasState column = this.prototypes[j];
                if (column.Require(StateName.RoleWaypoint)) continue;

                if (i == j)
                {
                    output += 
                        "[" 
                        + StateDefs.StateString(row.State, 15) 
                        + " ] ";
                }
                else
                {
                    output += 
                        "[" 
                        + StateDefs.RelationString(column.Id, row.State, 15) 
                        + " ] ";
                }
            }

            output += "\r\n";
        }

        return output;
    }

    #region Serialization
    /// <summary>
    /// Deserialization constructor
    /// </summary>
    internal WorldState(uint[] ids, long[] matrix)
    {
        this.IdToIndex = new Dictionary<uint, int>();
        this.prototypes = this.ReadStateMatrix(ids, matrix);
    }

    /// <summary>
    /// Converts the world state into a matrix of bit vectors
    /// </summary>
    internal long[] GetStateMatrix()
    {
        if (this.matrix != null)
            return this.matrix;

        int numObjects = this.prototypes.Length;
        long[] result = new long[numObjects * numObjects];
        for (int i = 0; i < numObjects; i++)
        {
            IHasState obj1 = this.prototypes[i];

            for (int j = 0; j < numObjects; j++)
            {
                int index = (i * numObjects) + j;
                IHasState obj2 = this.prototypes[j];

                // Store state in the diagonal
                if (i == j)
                    result[index] = obj1.State.GetStateBits();
                // Store relations in the triangles
                else
                    result[index] = obj1.State.GetRelationBits(obj2.Id);
            }
        }

        this.matrix = result;
        return result;
    }

    /// <summary>
    /// Returns an ordered array of each object's id
    /// </summary>
    internal uint[] GetIds()
    {
        uint[] result = new uint[this.prototypes.Length];
        for (int i = 0; i < this.prototypes.Length; i++)
            result[i] = this.prototypes[i].Id;
        return result;
    }

    /// <summary>
    /// Takes in a matrix and returns an array of read-only prototypes
    /// for use in construction
    /// </summary>
    private ReadOnlyPrototype[] ReadStateMatrix(
        uint[] ids,
        long[] matrix)
    {
        int numObjects = ids.Length;
        EditableState[] states = new EditableState[numObjects];
        for (int i = 0; i < numObjects; i++)
        {
            states[i] = new EditableState(ids[i]);
            this.IdToIndex[ids[i]] = i;

            for (int j = 0; j < numObjects; j++)
            {
                int index = (i * numObjects) + j;

                if (i == j)
                    states[i].SetStateBits(matrix[index]);
                else
                    states[i].SetRelationBits(ids[j], matrix[index]);
            }
        }

        return new List<ReadOnlyPrototype>(
            states.Convert(
                (s) => new ReadOnlyPrototype(s.AsReadOnly()))).ToArray();
    }
    #endregion
}
