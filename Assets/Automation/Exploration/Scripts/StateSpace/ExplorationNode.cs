// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
#if PARALLEL
using System.Collections.Concurrent;
#endif

public class ExplorationNode : INode
{
    public uint Id { get; set; }
    public readonly WorldState State;

    private double pageRank;
    private double inversePageRank;

    private List<ExplorationEdge> incoming;
    private List<ExplorationEdge> outgoing;

#if PARALLEL
    private ConcurrentDictionary<uint, int> minCutOut;
    private ConcurrentDictionary<uint, int> minCutIn;
    private ConcurrentDictionary<uint, ExplorationEdge[]> pathOut;
#else
    private Dictionary<uint, int> minCutOut;
    private Dictionary<uint, int> minCutIn;
    private Dictionary<uint, ExplorationEdge[]> pathOut;
#endif

    public ExplorationNode(WorldState state)
    {
        this.State = state;
        this.pageRank = 0.0;
        this.inversePageRank = 0.0;

        this.incoming = new List<ExplorationEdge>();
        this.outgoing = new List<ExplorationEdge>();

#if PARALLEL
        this.minCutOut = new ConcurrentDictionary<uint, int>();
        this.minCutIn = new ConcurrentDictionary<uint, int>();
        this.pathOut = new ConcurrentDictionary<uint,ExplorationEdge[]>();
#else
        this.minCutOut = new Dictionary<uint, int>();
        this.minCutIn = new Dictionary<uint, int>();
        this.pathOut = new Dictionary<uint, ExplorationEdge[]>();
#endif
    }

    public double PageRank
    {
        get { return this.pageRank; }
        set { this.pageRank = value; }
    }

    public double InversePageRank
    {
        get { return this.inversePageRank; }
        set { this.inversePageRank = value; }
    }

    public IEnumerable<IEdge> Incoming
    {
        get { return this.incoming.Cast<IEdge>(); }
    }

    public IEnumerable<IEdge> Outgoing
    {
        get { return this.outgoing.Cast<IEdge>(); }
    }

    public IList<ExplorationEdge> IncomingExploration
    {
        get { return this.incoming.AsReadOnly(); }
    }

    public IList<ExplorationEdge> OutgoingExploration
    {
        get { return this.outgoing.AsReadOnly(); }
    }

    public int NumIncoming
    {
        get { return this.incoming.Count; }
    }

    public int NumOutgoing
    {
        get { return this.outgoing.Count; }
    }

    public void AddIncoming(IEdge edge)
    {
        this.incoming.Add((ExplorationEdge)edge);
    }

    public void AddOutgoing(IEdge edge)
    {
        this.outgoing.Add((ExplorationEdge)edge);
    }

    public INode Clone()
    {
        ExplorationNode newNode = new ExplorationNode(this.State);
        newNode.pageRank = this.pageRank;
        newNode.Id = this.Id;
        return newNode;
    }

    public int GetMinCutIn(uint id)
    {
        int cut = -1;
        if (this.minCutIn.TryGetValue(id, out cut) == true)
            return cut;
        return -1;
    }

    public int GetMinCutOut(uint id)
    {
        int cut = -1;
        if (this.minCutOut.TryGetValue(id, out cut) == true)
            return cut;
        return -1;
    }

    public ExplorationEdge[] GetPathOut(uint id)
    {
        if (id == this.Id)
            return new ExplorationEdge[] { };

        ExplorationEdge[] result;
        if (this.pathOut.TryGetValue(id, out result) == true)
            return result;
        return null;
    }

    public void SetMinCutIn(uint id, int value)
    {
        this.minCutIn[id] = value;
    }

    public void SetMinCutOut(uint id, int value)
    {
        this.minCutOut[id] = value;
    }

    public void SetPathOut(uint id, ExplorationEdge[] path)
    {
        this.pathOut[id] = path;
    }

    public double AverageMinCutIn()
    {
        double val = 0.0;
        foreach (KeyValuePair<uint, int> kv in this.minCutIn)
            val += kv.Value;
        return val / (double)this.minCutIn.Count;
    }

    public double AverageMinCutOut()
    {
        double val = 0.0;
        foreach (KeyValuePair<uint, int> kv in this.minCutOut)
            val += kv.Value;
        return val / (double)this.minCutOut.Count;
    }

    #region For Encoding
    internal IList<ExplorationEdge[]> GetPathsOut()
    {
        return this.pathOut.Values.ToList();
    }

    internal IList<Tuple<uint, int>> GetMinCutsOut()
    {
        return new List<Tuple<uint, int>>(
            this.minCutOut.Convert(
                (kv) => new Tuple<uint, int>(kv.Key, kv.Value)));
    }

    internal IList<Tuple<uint, int>> GetMinCutsIn()
    {
        return new List<Tuple<uint, int>>(
            this.minCutIn.Convert(
                (kv) => new Tuple<uint, int>(kv.Key, kv.Value)));
    }
    #endregion
}