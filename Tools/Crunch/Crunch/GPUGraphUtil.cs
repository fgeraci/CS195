using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace Crunch
{
#if GPU
  internal static class GPUGraphUtil
  {
    public static void GPUPath(IList<ExplorationNode> stateSpace)
    {
      //Stopwatch watch = new Stopwatch();
      //watch.Start();

      Dictionary<ExplorationNode, int> nodeToId = new Dictionary<ExplorationNode, int>();
      Dictionary<int, ExplorationNode> idToNode = new Dictionary<int, ExplorationNode>();

      Console.WriteLine("Initializing graph for GPU...");
      // Pass 1: Map numbers to each of the nodes
      int id = 0;
      foreach (ExplorationNode node in stateSpace)
      {
        nodeToId[node] = id;
        idToNode[id] = node;
        id++;
      }

      int maxConnectivity = 0;
      // Pass 2: Link up the IDs for connectivity
      foreach (ExplorationNode node in stateSpace)
      {
        //int[] neighbors = new int[GraphCSharp.GraphSearch.NUM_VERTEX - 1];
        int[] neighbors = new int[12992 - 1];
        for (int i = 0; i < neighbors.Length; i++)
          neighbors[i] = -1;

        int idx = 0;
        foreach (ExplorationEdge edge in node.OutgoingExploration)
        {
          ExplorationNode target = edge.Target;
          neighbors[idx] = nodeToId[target];
          idx++;
        }

        //GraphCSharp.GraphSearch.add_node(neighbors);

        if (maxConnectivity < idx)
          maxConnectivity = idx;
      }

      Console.WriteLine("Max Connectivity: " + maxConnectivity);

      Console.Write("Writing to file...");
      using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Alex\Desktop\Data\12992.txt"))
      {
        foreach (ExplorationNode node in stateSpace)
        {
          string output = nodeToId[node] + ": ";
          foreach (ExplorationEdge edge in node.OutgoingExploration)
          {
            ExplorationNode target = edge.Target;
            int tarId = nodeToId[target];
            output += tarId + ",";
          }
          file.WriteLine(output);
        }
      }
      Console.Write(" Done!");

      //Console.Write("Solving on GPU...");

      //GraphCSharp.GraphSearch.solve_graph();

      //Console.Write(" Done!");
      //watch.Stop();

      //TimeSpan t = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
      //Console.WriteLine(
      //    "GPU pathing took " + 
      //    string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
      //        t.Hours,
      //        t.Minutes,
      //        t.Seconds));
    }
  }
#endif
}
