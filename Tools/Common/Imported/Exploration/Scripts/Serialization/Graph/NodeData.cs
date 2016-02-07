// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class NodeData
{
    public uint NodeId;
    public uint[][] Paths;

    public StateData State;

    // Stored in blocks of two elements, first entry is id, second is minCut
    public uint[] MinCutOut;
    public uint[] MinCutIn;

    // We don't -really- need these, but we'll use
    // them to preserve the order of the edges
    public uint[] Incoming;
    public uint[] Outgoing;

    public double PageRank;
    public double InversePageRank;

    public NodeData()
    {
        this.NodeId = uint.MaxValue;
        this.Paths = null;
        this.State = null;

        this.MinCutOut = null;
        this.MinCutIn = null;

        this.Incoming = null;
        this.Outgoing = null;

        this.PageRank = -1;
        this.InversePageRank = -1;
    }

    public NodeData(
        ExplorationNode node,
        Dictionary<ExplorationEdge, uint> edgeToId)
    {
        this.NodeId = node.Id;
        this.State = new StateData(node.State);

        this.PageRank = node.PageRank;
        this.InversePageRank = node.InversePageRank;

        this.MinCutIn = this.EncodeMinCuts(node.GetMinCutsIn());
        this.MinCutOut = this.EncodeMinCuts(node.GetMinCutsOut());

        IList<ExplorationEdge> incoming = node.IncomingExploration;
        this.Incoming = new uint[incoming.Count];
        for (int i = 0; i < incoming.Count; i++)
            this.Incoming[i] = edgeToId[incoming[i]];

        IList<ExplorationEdge> outgoing = node.OutgoingExploration;
        this.Outgoing = new uint[outgoing.Count];
        for (int i = 0; i < outgoing.Count; i++)
            this.Outgoing[i] = edgeToId[outgoing[i]];

        IList<ExplorationEdge[]> paths = node.GetPathsOut();
        this.Paths = new uint[paths.Count][];

        for (int i = 0; i < paths.Count; i++)
        {
            ExplorationEdge[] path = paths[i];

            this.Paths[i] = new uint[path.Length];
            for (int j = 0; j < path.Length; j++)
                this.Paths[i][j] = edgeToId[path[j]];
        }
    }

    private uint[] EncodeMinCuts(IList<Tuple<uint, int>> minCuts)
    {
        int length = minCuts.Count;
        uint[] encoded = new uint[length * 2];
        for (int i = 0; i < length; i++)
        {
            Tuple<uint, int> entry = minCuts[i];
            int index = i << 1;
            encoded[index] = entry.Item1;
            encoded[index + 1] = (uint)entry.Item2;
        }
        return encoded;
    }
}
