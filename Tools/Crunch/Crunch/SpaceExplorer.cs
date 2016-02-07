// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public enum ExplorationStatus
{
    Initializing,
    Exploring,
    Expanding,
    PageRank,
    InversePageRank,
    Path,
    MinCut,
    Done,
}

public class SpaceExplorer
{
    public const int MAX_NODES = 999999;

    private static long exploredNodes = 0;
    public static long ExploredNodes { get { return exploredNodes; } }

    private static long pathedNodes = 0;
    public static long PathedNodes { get { return pathedNodes; } }

    private static long minCutNodes = 0;
    public static long MinCutNodes { get { return minCutNodes; } }

    private ExplorationStatus status;
    private ExplorationSpace space = null;

    public bool IsDone
    {
        get { return this.status == ExplorationStatus.Done; }
    }

    public ExplorationSpace StateSpace
    {
        get
        {
            if (this.IsDone == false)
                return null;
            return this.space;
        }
    }

    public ExplorationStatus Status { get { return this.status; } }

    public SpaceExplorer()
    {
        this.status = ExplorationStatus.Initializing;
        this.space = null;
    }

    public IEnumerable Explore(WorldState initial, EventDescriptor[] evtDescs)
    {
        List<ExplorationNode> stateSpace = new List<ExplorationNode>();
        this.status = ExplorationStatus.Exploring;
        foreach (var step in Step_Exploring(initial, evtDescs, stateSpace))
        {
            exploredNodes = stateSpace.Count;
            if (exploredNodes > MAX_NODES)
                break;
            yield return null;
        }

        // TODO: Temporarily disabling expanding..
        //this.status = Status.Expanding;
        //foreach (var step in Step_Expanding(stateSpace))
        //    yield return null;

        this.status = ExplorationStatus.PageRank;
        foreach (var step in Step_PageRanking(stateSpace, 0.85, 200, 5, 0.01))
            yield return null;

        this.status = ExplorationStatus.InversePageRank;
        foreach (var step in Step_InversePageRanking(stateSpace, 0.85, 200, 5, 0.01))
            yield return null;

        this.status = ExplorationStatus.Path;

#if !GPU
        foreach (var step in Step_Pathing(stateSpace))
            yield return null;
#else
        Console.WriteLine("\nProcessing paths on GPU...");
        Crunch.GPUGraphUtil.GPUPath(stateSpace);
#endif

        //this.status = ExplorationStatus.MinCut;
        //foreach (var step in Step_MinCut(stateSpace))
        //    yield return null;

        this.status = ExplorationStatus.Done;
        this.space = new ExplorationSpace(stateSpace);
        yield break;
    }

    private static IEnumerable Step_Exploring(
        WorldState initial,
        EventDescriptor[] evtDescs,
        List<ExplorationNode> stateSpaceToFill)
    {
        return ExploreStateSpace(initial, evtDescs, stateSpaceToFill);
    }

    #region Exploring Step Code
    /// <summary>
    /// Explores the state space resulting from the initial world state
    /// and produces a list of nodes in a graph
    /// </summary>
    /// <param name="initial">The initial world state</param>
    /// <param name="stateSpaceToFill">The list of nodes we add to</param>
    /// <returns>true if finished, false otherwise</returns>
    private static IEnumerable ExploreStateSpace(
        WorldState initial,
        EventDescriptor[] evtDescs,
        List<ExplorationNode> stateSpaceToFill)
    {
        List<ExplorationNode> openList = new List<ExplorationNode>();
        Dictionary<WorldState, ExplorationNode> seen =
            new Dictionary<WorldState, ExplorationNode>();

        uint i = 0;
        ExplorationNode firstNode = new ExplorationNode(initial);
        firstNode.Id = i;
        openList.Add(firstNode);
        seen.Add(initial, firstNode);

        while (openList.Count > 0)
        {
            // Pop the node off the open list
            ExplorationNode curNode = openList[0];
            openList.RemoveAt(0);

            Rules.ClearCache();
            AddExpansionsToLists(evtDescs, openList, seen, curNode);

            curNode.Id = i++;
            stateSpaceToFill.Add(curNode);
            yield return false;
        }

        yield break;
    }

    /// <summary>
    /// Expands all of the transitions and adds the resulting world states
    /// to the list of nodes during exploration
    /// </summary>
    /// <param name="openList">The list of nodes we haven't explored
    /// yet</param>
    /// <param name="seen">The list of nodes we've seen</param>
    /// <param name="curNode">The current node from the open list</param>
    private static void AddExpansionsToLists(
        EventDescriptor[] evtDescs,
        List<ExplorationNode> openList,
        Dictionary<WorldState, ExplorationNode> seen,
        ExplorationNode curNode)
    {
        // Find all of the event signatures
        foreach (EventDescriptor evtDesc in evtDescs)
        {
            IEnumerable<Tuple<WorldState, uint[]>> expansions =
                GetExpansions(curNode.State, evtDesc);

            // Get all of the expansions
            foreach (Tuple<WorldState, uint[]> expansion in expansions)
            {
                WorldState expansionState = expansion.Item1;
                ExplorationNode expansionNode;

                // If we haven't seen the world state
                if (seen.TryGetValue(expansionState, out expansionNode) == false)
                {
                    expansionNode = new ExplorationNode(expansionState);
                    openList.Add(expansionNode);
                    seen.Add(expansionState, expansionNode);
                }

                DebugUtil.Assert(expansionNode != null);

                TransitionEvent transition = new TransitionEvent(evtDesc, expansion.Item2);
                ExplorationEdge edge = new ExplorationEdge(
                    curNode,
                    expansionNode,
                    transition);
                curNode.AddOutgoing(edge);
                expansionNode.AddIncoming(edge);
            }
        }
    }

    /// <summary>
    /// Gets all the expansions for an event signature from a given state
    /// </summary>
    private static IEnumerable<Tuple<WorldState, uint[]>> GetExpansions(
        WorldState state,
        EventDescriptor evtDesc)
    {
        IEnumerable<IHasState> prototypes =
            state.Prototypes.Cast<IHasState>();

        IEnumerable<EventPopulation> pops =
            EventPopulator.GetValidPopulations(
                evtDesc,
                prototypes,
                prototypes);

        foreach (EventPopulation pop in pops)
        {
            uint[] ids = new List<uint>(
                pop.Members.Convert((obj) => obj.Id)).ToArray();
            yield return
                new Tuple<WorldState, uint[]>(
                    state.Transform(evtDesc, ids), ids);
        }

        yield break;
    }
    #endregion

    private static IEnumerable Step_Expanding(List<ExplorationNode> stateSpace)
    {
        return ExpandGraph(stateSpace);
    }

    #region Expanding Step Code
    /// <summary>
    /// Expands the graph by adding edges that involve multiple
    /// simultaneous events when those events don't overlap in terms of
    /// participants.
    /// </summary>
    /// <returns>true if finished, false otherwise</returns>
    private static IEnumerable ExpandGraph(List<ExplorationNode> stateSpace)
    {
        foreach (ExplorationEdge edge in GetEdgesToAdd(stateSpace))
        {
            edge.Source.AddOutgoing(edge);
            edge.Target.AddIncoming(edge);
            yield return null;
        }
        yield break;
    }

    /// <summary>
    /// Traverses the graph and finds edges to add where there's no overlap
    /// between events in the two sequential jumps
    /// </summary>
    /// <param name="nodeList">The list of nodes in the graph</param>
    /// <returns></returns>
    private static IEnumerable<ExplorationEdge> GetEdgesToAdd(
        IList<ExplorationNode> nodeList)
    {
        List<ExplorationEdge> edgesToAdd = new List<ExplorationEdge>();

        foreach (ExplorationNode beginning in nodeList)
        {
            foreach (ExplorationEdge jump1 in beginning.Outgoing)
            {
                List<TransitionEvent> jump1Events =
                    new List<TransitionEvent>(jump1.Events);

                // All the objects involved in the beginning->middle transition
                HashSet<uint> involvedFirst = new HashSet<uint>();
                GetInvolved(jump1Events).ForEach(
                    (uint id) =>
                    {
                        DebugUtil.Assert(involvedFirst.Contains(id) == false);
                        involvedFirst.Add(id);
                    });

                // Analyze the second jumps (middle-end)
                AnalyzeEndpoints(
                    beginning,
                    jump1,
                    jump1Events,
                    involvedFirst,
                    edgesToAdd);
            }
        }

        return edgesToAdd;
    }

    /// <summary>
    /// Checks the second transition in a 2-transition jump and sees if we can
    /// perform the collapse (there's no overlap, etc.)
    /// </summary>
    /// <param name="beginning">The first expanded node (beginning, middle, 
    /// end)</param>
    /// <param name="jump1">The first edge (beginning-middle)</param>
    /// <param name="jump1Events">The events in the first edge</param>
    /// <param name="involvedInFirst">The objects involved in jump1</param>
    /// <param name="edgesToAdd">(out) A list in which we write the edges to
    /// add to the graph</param>
    private static void AnalyzeEndpoints(
        ExplorationNode beginning,
        ExplorationEdge jump1,
        IEnumerable<TransitionEvent> jump1Events,
        HashSet<uint> involvedInFirst,
        IList<ExplorationEdge> edgesToAdd)
    {
        // Get all of the final endpoint jumps (middle-end)
        foreach (ExplorationEdge jump2 in jump1.Target.Outgoing)
        {
            ExplorationNode end = jump2.Target;
            List<TransitionEvent> jump2Events =
                new List<TransitionEvent>(jump2.Events);

            // All the objects in jump2 
            HashSet<uint> involvedSecond = new HashSet<uint>();
            GetInvolved(jump2Events).ForEach(
                (uint id) =>
                {
                    DebugUtil.Assert(involvedSecond.Contains(id) == false);
                    involvedSecond.Add(id);
                });

            // There's no overlap
            if (involvedInFirst.Overlaps(involvedSecond) == false)
            {
                List<TransitionEvent> combined = new List<TransitionEvent>();
                combined.AddRange(
                    jump1Events.Convert(s => new TransitionEvent(s)));
                combined.AddRange(
                    jump2Events.Convert(s => new TransitionEvent(s)));
                edgesToAdd.Add(
                    new ExplorationEdge(beginning, end, combined.ToArray()));
            }
        }
    }

    /// <summary>
    /// Gets all of the ids of the objects involved in a list of events
    /// </summary>
    private static IEnumerable<uint> GetInvolved(
        IEnumerable<TransitionEvent> events)
    {
        foreach (TransitionEvent evt in events)
            foreach (uint id in evt.Participants)
                yield return id;
        yield break;
    }
    #endregion

    private static IEnumerable Step_PageRanking(
        List<ExplorationNode> stateSpace,
        double damp,
        int maxIterations,
        int minIterations = 5,
        double epsilon = 0.05)
    {
        return GraphUtil.ComputeRank(
            stateSpace.Cast<INode>().ToList(),
            damp,
            maxIterations,
            minIterations,
            epsilon);
    }

    private static IEnumerable Step_InversePageRanking(
        List<ExplorationNode> stateSpace,
        double damp,
        int maxIterations,
        int minIterations = 5,
        double epsilon = 0.05)
    {
        return GraphUtil.ComputeRankInverse(
            new List<INode>(stateSpace.Cast<INode>()),
            damp,
            maxIterations,
            minIterations,
            epsilon);
    }

#if !GPU
    private static IEnumerable Step_Pathing(List<ExplorationNode> stateSpace)
    {
#if PARALLEL
        return Crunch.ParallelCrunch.ComputePaths(stateSpace);
#else
        return ComputePaths(stateSpace);
#endif
    }
#endif

    #region Path Step Code
    /// <summary>
    /// Computes the shortest path between every pair of nodes
    /// </summary>
    /// <returns>true if finished, false otherwise</returns>
    private static IEnumerable ComputePaths(IList<ExplorationNode> stateSpace)
    {
        IEnumerable<INode> castNodes = stateSpace.Cast<INode>();

        foreach (ExplorationNode node1 in stateSpace)
        {
            foreach (ExplorationNode node2 in stateSpace)
            {
                if (node1 != node2)
                {
                    IEnumerable<IEdge> edges = GraphUtil.BFSPath(node1, node2);

                    ExplorationEdge[] castEdges = null;
                    if (edges != null)
                        castEdges =
                            new List<ExplorationEdge>(
                                edges.Cast<ExplorationEdge>()).ToArray();

                    if (castEdges != null)
                        node1.SetPathOut(node2.Id, castEdges);
                }

                yield return null;
            }

            // For progress tracking
            pathedNodes++;
        }

        yield break;
    }
    #endregion

    private static IEnumerable Step_MinCut(List<ExplorationNode> stateSpace)
    {
#if PARALLEL
        return Crunch.ParallelCrunch.ComputeMinCuts(stateSpace);
#else
        return ComputeMinCuts(stateSpace);
#endif
    }

    #region MinCut Step Node
    /// <summary>
    /// Computes MinCut between every pair of nodes
    /// </summary>
    /// <returns>true if finished, false otherwise</returns>
    private static IEnumerable ComputeMinCuts(List<ExplorationNode> stateSpace)
    {
        IEnumerable<INode> castNodes = stateSpace.Cast<INode>();

        foreach (ExplorationNode node1 in stateSpace)
        {
            foreach (ExplorationNode node2 in stateSpace)
            {
                if (node1 != node2)
                {
                    int minCut =
                        GraphUtil.MinCut(
                            node1,
                            node2,
                            castNodes,
                            node1.GetPathOut(node2.Id));
                    node1.SetMinCutOut(node2.Id, minCut);
                    node2.SetMinCutIn(node1.Id, minCut);
                }

                yield return null;
            }

            // For progress tracking
            minCutNodes++;
        }

        yield break;
    }
    #endregion
}
