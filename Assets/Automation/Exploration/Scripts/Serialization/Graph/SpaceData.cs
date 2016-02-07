// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System.Linq;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SpaceData
{
    public NodeData[] Nodes;
    public EdgeData[] Edges;

    public SpaceData()
    {
        this.Nodes = null;
        this.Edges = null;
    }

    public SpaceData(ExplorationSpace space)
    {
        IList<ExplorationNode> nodes = space.Nodes;

        Dictionary<ExplorationEdge, uint> edgeToId;
        this.Edges = EncodeEdges(nodes, out edgeToId).ToArray();
        this.Nodes = EncodeNodes(nodes, edgeToId).ToArray();
    }

    private static IEnumerable<EdgeData> EncodeEdges(
        IList<ExplorationNode> nodes,
        out Dictionary<ExplorationEdge, uint> edgeToId)
    {
        HashSet<ExplorationEdge> edgeSet = new HashSet<ExplorationEdge>();
        foreach (ExplorationNode node in nodes)
        {
            node.OutgoingExploration.ForEach(e => edgeSet.Add(e));
            node.IncomingExploration.ForEach(e => edgeSet.Add(e));
        }

        edgeToId = new Dictionary<ExplorationEdge, uint>();
        uint edgeId = 0;
        foreach (ExplorationEdge edge in edgeSet)
            edgeToId[edge] = edgeId++;

        List<EdgeData> edgeData = new List<EdgeData>();
        edgeToId.ForEach(
            (kv) => edgeData.Add(new EdgeData(kv.Value, kv.Key)));
        return edgeData.OrderBy((data) => data.EdgeId);
    }

    private static IEnumerable<NodeData> EncodeNodes(
         IList<ExplorationNode> nodes,
         Dictionary<ExplorationEdge, uint> edgeToId)
    {
        List<NodeData> nodeData = new List<NodeData>();
        nodes.ForEach(
            (node) => nodeData.Add(new NodeData(node, edgeToId)));
        return nodeData.OrderBy((node) => node.NodeId);
    }
}
