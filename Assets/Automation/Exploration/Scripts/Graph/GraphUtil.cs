// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections;
using System.Collections.Generic;

public static class GraphUtil
{
    #region Clone
    public static IList<INode> CloneGraph(IList<INode> nodes)
    {
        List<INode> newNodes = new List<INode>();
        Dictionary<INode, INode> remap = new Dictionary<INode, INode>();

        // Clone all of the nodes first without transitions
        foreach (INode oldNode in nodes)
        {
            INode newNode = oldNode.Clone();
            newNodes.Add(newNode);
            remap.Add(oldNode, newNode);
        }

        // Now clone the transitions
        foreach (INode oldNode in nodes)
        {
            INode newNode = remap[oldNode];

            foreach (IEdge edge in oldNode.Outgoing)
                newNode.AddOutgoing(CreateRemappedEdge(edge, remap));

            foreach (IEdge edge in oldNode.Incoming)
                newNode.AddIncoming(CreateRemappedEdge(edge, remap));
        }

        return newNodes;
    }

    private static IEdge CreateRemappedEdge(
        IEdge edge,
        Dictionary<INode, INode> remap)
    {
        IEdge newEdge = edge.Clone();
        newEdge.Source = remap[edge.Source];
        newEdge.Target = remap[edge.Target];
        return newEdge;
    }
    #endregion

    #region MinCut
    public static int MinCut(
        INode start, 
        INode end, 
        IEnumerable<INode> nodes,
        IEnumerable<IEdge> path)
    {
        if (path == null)
            return 0;

        ResetSaturated(nodes);

        // Reuse the stored path as the first run
        foreach (IEdge edge in path)
            edge.Saturated = true;

        IEnumerable<IEdge> result = null;
        int iterations = 1;
        while ((result = BFSSaturated(start, end)) != null)
        {
            foreach (IEdge edge in result)
                edge.Saturated = true;
            iterations++;
        }
        return iterations;
    }

    private static void ResetSaturated(IEnumerable<INode> nodes)
    {
        foreach (INode node in nodes)
            foreach (IEdge edge in node.Outgoing)
                edge.Saturated = false;
    }

    private static IList<IEdge> BFSSaturated(INode start, INode end)
    {
        return BFSPath(start, end, (edge) => edge.Saturated == false);
    }

    public static IList<IEdge> BFSPath(
        INode start, 
        INode end, 
        Func<IEdge, bool> filter = null)
    {
        Queue<Tuple<INode, List<IEdge>>> nodes =
            new Queue<Tuple<INode, List<IEdge>>>();
        HashSet<INode> seen = new HashSet<INode>();

        nodes.Enqueue(new Tuple<INode, List<IEdge>>(start, new List<IEdge>()));
        seen.Add(start);

        while (nodes.Count > 0)
        {
            Tuple<INode, List<IEdge>> current = nodes.Dequeue();
            INode node = current.Item1;
            List<IEdge> path = current.Item2;

            if (current.Item1 == end)
                return path;

            foreach (IEdge edge in node.Outgoing)
            {
                if (seen.Contains(edge.Target) == false)
                {
                    if (filter == null || filter(edge) == true)
                    {
                        List<IEdge> newPath = new List<IEdge>(path);
                        newPath.Add(edge);
                        nodes.Enqueue(
                            new Tuple<INode, List<IEdge>>(edge.Target, newPath));
                        seen.Add(edge.Target);
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Pagerank
    /// <summary>
    /// Iterator function for computing PageRank. Yields after every step.
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="damp"></param>
    /// <param name="maxIterations"></param>
    /// <param name="minIterations"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public static IEnumerable ComputeRank(
        IList<INode> nodes,
        double damp,
        int maxIterations,
        int minIterations = 5,
        double epsilon = 0.05)
    {
        foreach (INode node in nodes)
            node.PageRank = 1.0 / nodes.Count;

        for (int i = 0; i < maxIterations; i++)
        {
            double delta = NodeRank(nodes, damp);
            if (i > minIterations && delta < epsilon)
                break;
            else
                yield return null;
        }

        yield break;
    }

    /// <summary>
    /// Iterator function for computing Inverse PageRank. Yields after every 
    /// step.
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="damp"></param>
    /// <param name="maxIterations"></param>
    /// <param name="minIterations"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public static IEnumerable ComputeRankInverse(
        IList<INode> nodes,
        double damp,
        int maxIterations,
        int minIterations = 5,
        double epsilon = 0.05)
    {
        foreach (INode node in nodes)
            node.InversePageRank = 1.0 / nodes.Count;

        for (int i = 0; i < maxIterations; i++)
        {
            double delta = NodeRankInverse(nodes, damp);
            if (i > minIterations && delta < epsilon)
                break;
            else
                yield return null;
        }

        yield break;
    }

    private static double NodeRank(
        IEnumerable<INode> nodes, 
        double damp)
    {
        double delta = 0.0;
        foreach (INode node in nodes)
        {
            double sum = ScoreSum(node);
            double oldScore = node.PageRank;
            node.PageRank = (1.0 - damp) + (damp * sum);
            delta += Math.Abs(node.PageRank - oldScore);
        }
        return delta;
    }

    private static double NodeRankInverse(
        IEnumerable<INode> nodes,
        double damp)
    {
        double delta = 0.0;
        foreach (INode node in nodes)
        {
            double sum = ScoreSumInverse(node);
            double oldScore = node.InversePageRank;
            node.InversePageRank = (1.0 - damp) + (damp * sum);
            delta += Math.Abs(node.InversePageRank - oldScore);
        }
        return delta;
    }

    private static double ScoreSum(INode node)
    {
        double sum = 0.0;
        node.Incoming.ForEach(
            (IEdge e) => sum += (e.Source.PageRank / (double)e.Source.NumOutgoing));
        return sum;
    }

    private static double ScoreSumInverse(INode node)
    {
        double sum = 0.0;
        node.Outgoing.ForEach(
            (IEdge e) => sum += (e.Target.InversePageRank / (double)e.Target.NumIncoming));
        return sum;
    }
    #endregion
}
