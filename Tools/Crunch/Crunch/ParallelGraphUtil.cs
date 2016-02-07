using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace Crunch
{
    internal static class ParallelGraphUtil
    {
        public const int HEURISTIC_WEIGHT = 10;
        public static long nodesExpanded = 0;

        private static OpenList<ExplorationNode, ExplorationEdge>.EntryComparer comparer = null;

        public static void Init()
        {
            if (comparer == null)
            {
                CompareExplorationNode nodeComparer = new CompareExplorationNode();
                comparer =
                    OpenList<ExplorationNode, ExplorationEdge>.CreateComparer(
                    nodeComparer,
                    nodeComparer,
                    HEURISTIC_WEIGHT);
            }
        }

        private class CompareExplorationNode : 
            IComparer<ExplorationNode>, 
            IEqualityComparer<ExplorationNode>
        {
            private CompareByMatrix compare = null;

            public CompareExplorationNode()
            {
                this.compare = new CompareByMatrix();
            }

            public int Compare(ExplorationNode x, ExplorationNode y)
            {
                return this.compare.Compare(x.State, y.State);
            }

            public bool Equals(ExplorationNode x, ExplorationNode y)
            {
                return this.compare.Equals(x.State, y.State);
            }

            public int GetHashCode(ExplorationNode obj)
            {
                return this.compare.GetHashCode(obj.State);
            }
        }

        internal static IList<ExplorationEdge> AStarPath(
            ExplorationNode start,
            ExplorationNode end,
            Func<ExplorationEdge, bool> filter = null)
        {
            OpenList<ExplorationNode, ExplorationEdge> open = 
                new OpenList<ExplorationNode, ExplorationEdge>(
                    comparer, 
                    HEURISTIC_WEIGHT);
            HashSet<ExplorationNode> seen = new HashSet<ExplorationNode>();

            open.Add(start, new List<ExplorationEdge>());
            seen.Add(start);

            while (open.Count > 0)
            {
                ExplorationNode node;
                IList<ExplorationEdge> path;
                open.Pop(out node, out path);

                if (node == end)
                    return path;

                foreach (ExplorationEdge edge in node.Outgoing)
                {
                    if (seen.Contains(edge.Target) == false)
                    {
                        if (filter == null || filter(edge) == true)
                        {
                            List<ExplorationEdge> newPath =
                                new List<ExplorationEdge>(path);
                            newPath.Add(edge);
                            open.Add(edge.Target, newPath);
                            seen.Add(edge.Target);
                        }
                    }
                }

                //Interlocked.Increment(ref nodesExpanded);
            }

            return null;
        }

        internal static List<ExplorationEdge> BFSPath(
            ExplorationNode start,
            ExplorationNode end,
            Func<ExplorationEdge, bool> filter = null)
        {
            Queue<Tuple<ExplorationNode, List<ExplorationEdge>>> open =
                new Queue<Tuple<ExplorationNode, List<ExplorationEdge>>>();
            HashSet<ExplorationNode> seen = new HashSet<ExplorationNode>();

            open.Enqueue(
                new Tuple<ExplorationNode, List<ExplorationEdge>>(
                    start, new List<ExplorationEdge>()));
            seen.Add(start);

            while (open.Count > 0)
            {
                Tuple<ExplorationNode, List<ExplorationEdge>> current = 
                    open.Dequeue();
                ExplorationNode node = current.Item1;
                List<ExplorationEdge> path = current.Item2;

                if (current.Item1 == end)
                    return path;

                foreach (ExplorationEdge edge in node.Outgoing)
                {
                    if (seen.Contains(edge.Target) == false)
                    {
                        if (filter == null || filter(edge) == true)
                        {
                            List<ExplorationEdge> newPath = 
                                new List<ExplorationEdge>(path);
                            newPath.Add(edge);
                            open.Enqueue(
                                new Tuple<ExplorationNode, List<ExplorationEdge>>(
                                    edge.Target, 
                                    newPath));
                            seen.Add(edge.Target);
                        }
                    }
                }

                //Interlocked.Increment(ref nodesExpanded);
            }

            return null;
        }

        internal static int MinCut(
            ExplorationNode start,
            ExplorationNode end)
        {
            ExplorationEdge[] path = start.GetPathOut(end.Id);
            if (path == null)
                return 0;

            HashSet<ExplorationEdge> saturated = 
                new HashSet<ExplorationEdge>();

            // Reuse the stored path as the first run
            foreach (ExplorationEdge edge in path)
                saturated.Add(edge);

            IEnumerable<ExplorationEdge> result = null;
            int iterations = 1;
            while ((result = BFSSaturated(saturated, start, end)) != null)
            {
                foreach (ExplorationEdge edge in result)
                    saturated.Add(edge);
                iterations++;
            }
            return iterations;
        }

        private static IList<ExplorationEdge> BFSSaturated(
            HashSet<ExplorationEdge> saturated,
            ExplorationNode start, 
            ExplorationNode end)
        {
            return BFSPath(
                start, 
                end, 
                (edge) => saturated.Contains(edge) == false);
        }
    }
}
