using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Crunch
{
    public static class ParallelCrunch
    {
        internal static Stopwatch stopwatch = null;
        internal static long totalPairs = 0;
        internal static long pairsProcessed = 0;

        private static IEnumerable RunProcessTask(
            IList<ExplorationNode> stateSpace,
            Action<ExplorationNode, ExplorationNode> processor)
        {
            Task t = ProcessAllPairs(stateSpace, processor);
            yield return false;

            while (t.IsCompleted == false)
            {
                Thread.Sleep(1000);
                yield return false;
            }

            yield break;
        }

        private static Task ProcessAllPairs(
            IList<ExplorationNode> stateSpace,
            Action<ExplorationNode, ExplorationNode> processor)
        {
            if (stopwatch == null)
                stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();

            totalPairs =
                (stateSpace.Count * stateSpace.Count) - stateSpace.Count;
            pairsProcessed = 0;

            ParallelGraphUtil.Init();
            return Task.Factory.StartNew(() =>
                Parallel.ForEach(stateSpace, source =>
                {
                    foreach (ExplorationNode dest in stateSpace)
                    {
                        if (source != dest)
                        {
                            processor(source, dest);
                            Interlocked.Increment(ref pairsProcessed);
                        }
                    }
                }));

            //return Task.Factory.StartNew(() =>
            //    {
            //        foreach (ExplorationNode source in stateSpace)
            //        {
            //            foreach (ExplorationNode dest in stateSpace)
            //            {
            //                if (source != dest)
            //                {
            //                    processor(source, dest);
            //                    Interlocked.Increment(ref pairsProcessed);
            //                }
            //            }
            //        }
            //    });
        }

        private static void Task_ProcessPair(
            ExplorationNode source,
            ExplorationNode dest)
        {
            IList<ExplorationEdge> edges =
                ParallelGraphUtil.AStarPath(source, dest);
            if (edges != null)
                source.SetPathOut(dest.Id, edges.ToArray());
        }

        private static void Task_ProcessMinCut(
            ExplorationNode source,
            ExplorationNode dest)
        {
            int minCut = ParallelGraphUtil.MinCut(source, dest);
            source.SetMinCutOut(dest.Id, minCut);
            dest.SetMinCutIn(source.Id, minCut);
        }

        public static IEnumerable ComputePaths(IList<ExplorationNode> stateSpace)
        {
          return RunProcessTask(stateSpace, Task_ProcessPair);
        }

        public static IEnumerable ComputeMinCuts(IList<ExplorationNode> stateSpace)
        {
            return RunProcessTask(stateSpace, Task_ProcessMinCut);
        }
    }
}
