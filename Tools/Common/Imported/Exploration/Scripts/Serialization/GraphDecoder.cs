using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public static class GraphDecoder
{
    public static ExplorationSpace Decode(
        SpaceData data,
        IEventLibrary library)
    {
        List<ExplorationNode> nodes = new List<ExplorationNode>();

        // Recreate the nodes
        Dictionary<uint, ExplorationNode> idToNode;
        List<ExplorationNode> newNodes =
            DecodeNodes(data.Nodes, out idToNode);
        nodes.AddRange(newNodes);

        // Recreate the edges
        Dictionary<uint, ExplorationEdge> idToEdge;
        List<ExplorationEdge> newEdges =
            DecodeEdges(library, idToNode, data.Edges, out idToEdge);

        // Link the instantiated edges to the instantiated nodes
        for (int i = 0; i < data.Nodes.Length; i++)
        {
            NodeData nodeData = data.Nodes[i];
            ExplorationNode node = nodes[i];
            AssignEdgesToNode(idToEdge, nodeData, node);
        }

        return new ExplorationSpace(nodes);
    }

    private static ExplorationEdge Decode(
        EdgeData data,
        Dictionary<uint, ExplorationNode> idToNode,
        IEventLibrary library)
    {
        return new ExplorationEdge(
            idToNode[data.IdFrom],
            idToNode[data.IdTo],
            RecreateStoryEvents(data, library));
    }

    private static TransitionEvent[] RecreateStoryEvents(
        EdgeData data,
        IEventLibrary library)
    {
        TransitionEvent[] events =
            new TransitionEvent[data.EventNames.Length];

        for (int i = 0; i < events.Length; i++)
        {
            EventDescriptor evtDesc =
                library.GetDescriptor(data.EventNames[i]);
            DebugUtil.Assert(evtDesc != null);
            events[i] =
                new TransitionEvent(evtDesc, data.EventParticipants[i]);
        }

        return events;
    }

    private static ExplorationNode Decode(NodeData data)
    {
        ExplorationNode newNode = new ExplorationNode(data.State.Decode());
        newNode.Id = data.NodeId;
        newNode.PageRank = data.PageRank;
        newNode.InversePageRank = data.InversePageRank;

        StoreMinCuts(newNode, data.MinCutIn, newNode.SetMinCutIn);
        StoreMinCuts(newNode, data.MinCutOut, newNode.SetMinCutOut);

        return newNode;
    }

    private static void StoreMinCuts(
        ExplorationNode node,
        uint[] minCuts,
        Action<uint, int> assign)
    {
        for (int i = 0; i < minCuts.Length >> 1; i++)
        {
            int index = i << 1;
            uint id = minCuts[index];
            int value = (int)minCuts[index + 1];
            assign(id, value);
        }
    }

    private static List<ExplorationNode> DecodeNodes(
        IEnumerable<NodeData> nodeData,
        out Dictionary<uint, ExplorationNode> idToNode)
    {
        List<ExplorationNode> newNodes = new List<ExplorationNode>();
        idToNode = new Dictionary<uint, ExplorationNode>();
        foreach (NodeData data in nodeData)
        {
            ExplorationNode newNode = Decode(data);
            newNodes.Add(newNode);
            idToNode[data.NodeId] = newNode;
        }
        return newNodes;
    }

    private static List<ExplorationEdge> DecodeEdges(
        IEventLibrary library,
        Dictionary<uint, ExplorationNode> idToNode,
        IEnumerable<EdgeData> edgeData,
        out Dictionary<uint, ExplorationEdge> idToEdge)
    {
        List<ExplorationEdge> newEdges = new List<ExplorationEdge>();
        idToEdge = new Dictionary<uint, ExplorationEdge>();
        foreach (EdgeData data in edgeData)
        {
            ExplorationEdge newEdge = Decode(data, idToNode, library);
            idToEdge[data.EdgeId] = newEdge;
            newEdges.Add(newEdge);
        }
        return newEdges;
    }

    private static void AssignEdgesToNode(
        Dictionary<uint, ExplorationEdge> idToEdge,
        NodeData data,
        ExplorationNode node)
    {
        // Add the incoming and outgoing nodes, in order
        foreach (uint id in data.Incoming)
            node.AddIncoming(idToEdge[id]);
        foreach (uint id in data.Outgoing)
            node.AddOutgoing(idToEdge[id]);

        List<ExplorationEdge> edges = new List<ExplorationEdge>();
        // Convert the path to path edges for each target
        for (int tIdx = 0; tIdx < data.Paths.Length; tIdx++)
        {
            uint[] pathIds = data.Paths[tIdx];
            ExplorationEdge[] pathEdges =
                pathIds.Convert((id) => idToEdge[id]).ToArray();
            ExplorationNode target = pathEdges.Last().Target;
            node.SetPathOut(target.Id, pathEdges);
        }
    }
}
