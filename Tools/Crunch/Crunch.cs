using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common;

namespace Crunch
{
    public class Crunch
    {
        private static string TimeRemaining()
        {
            long milis = ParallelCrunch.stopwatch.ElapsedMilliseconds;

            if (milis < 10000)
                return "???";
            long pairsRemaining = 
                ParallelCrunch.totalPairs - ParallelCrunch.pairsProcessed;
            double milisPerPair =
                (double)milis / (double)ParallelCrunch.pairsProcessed;
            long milisRemaining = (long)(milisPerPair * (double)pairsRemaining);

            TimeSpan t = TimeSpan.FromMilliseconds(milisRemaining);
            return string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds);
        }

        private static string TimeElapsed()
        {
            long milis = ParallelCrunch.stopwatch.ElapsedMilliseconds;
            TimeSpan t = TimeSpan.FromMilliseconds(milis);
            return string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds);
        }

        static void Main(string[] args)
        {
            WorldState worldState;
            EventDescriptor[] evtDescs;
            bool loaded = 
                DataIO.LoadWorldXML(
                    ExternalIO.WorldDataPath(), 
                    out worldState, 
                    out evtDescs);

            if (loaded == false)
            {
                Console.WriteLine(
                    "Failed to load file " + ExternalIO.WorldDataPath());
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Loaded " + evtDescs.Length + " events");
                SpaceExplorer explorer = new SpaceExplorer();
                IEnumerable exploration = explorer.Explore(worldState, evtDescs);

                // Pick an arbitrary status that is neither the start nor end
                ExplorationStatus status = ExplorationStatus.Exploring;
                foreach (var step in exploration)
                {
                    if (status != explorer.Status)
                        Console.WriteLine();
                    status = explorer.Status;

                    if (explorer.Status == ExplorationStatus.Exploring)
                    {
                        Console.Write("\rExploring... " + SpaceExplorer.ExploredNodes);
                    }
                    else if (explorer.Status == ExplorationStatus.Path)
                    {
#if PARALLEL
                        string timeRemaining = TimeRemaining() + " r, " + TimeElapsed() + " e";
                        Console.Write(
                            String.Format("\rPath... {0}/{1} ({2:F}%), ",
                                ParallelCrunch.pairsProcessed,
                                ParallelCrunch.totalPairs,
                                ((double)ParallelCrunch.pairsProcessed / (double)ParallelCrunch.totalPairs) * 100.0f)
                            + timeRemaining);
#else
                        Console.Write("\rPath... " + SpaceExplorer.PathedNodes + "/" + SpaceExplorer.ExploredNodes);
#endif
                    }
                    else if (explorer.Status == ExplorationStatus.MinCut)
                    {
#if PARALLEL
                        string timeRemaining = TimeRemaining() + " r, " + TimeElapsed() + " e";
                        Console.Write(
                            String.Format("\rMinCut... {0}/{1} ({2:F}%), ",
                                ParallelCrunch.pairsProcessed,
                                ParallelCrunch.totalPairs,
                                ((float)ParallelCrunch.pairsProcessed / (float)ParallelCrunch.totalPairs) * 100.0f)
                            + timeRemaining);
#else
                        Console.Write("\rMinCut... " + SpaceExplorer.MinCutNodes + "/" + SpaceExplorer.ExploredNodes);
#endif
                    }
                    else
                    {
                        Console.Write("\r" + explorer.Status + "...");
                    }
                }

                ExplorationSpace space = explorer.StateSpace;
                Console.WriteLine("\nProduced " + space.Nodes.Count + " nodes");

                DataIO.StoreGraphBinary(ExternalIO.GraphDataPath(), space);
                Console.WriteLine("Wrote graph to " + ExternalIO.GraphDataPath());
                Console.WriteLine("Press any key to continue . . .");
                Console.ReadKey();
            }
        }
    }
}
