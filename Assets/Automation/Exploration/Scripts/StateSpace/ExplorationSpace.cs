// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ExplorationSpace
{
    private List<ExplorationNode> nodes;

    public IList<ExplorationNode> Nodes
    {
        get
        {
            return this.nodes.AsReadOnly();
        }
    }

    public ExplorationSpace(IEnumerable<ExplorationNode> nodes)
    {
        this.nodes = new List<ExplorationNode>(nodes);
    }

    public override string ToString()
    {
        string output = "";
        foreach (ExplorationNode node in this.nodes)
        {
            output += "Node: " + node.Id + "\n";
            foreach (ExplorationEdge transition in node.Outgoing)
            {
                output += "(" + transition.Target.Id + ": ";
                foreach (TransitionEvent evt in transition.Events)
                {
                    output += "[" + evt.Descriptor.Name + " { ";
                    foreach (uint id in evt.Participants)
                    {
                        output += id + " ";
                    }
                    output += "}]";
                }
                output += ")\n";
            }

            output += "Paths to:";
            foreach (ExplorationEdge[] path in node.GetPathsOut())
            {
                ExplorationNode destination = path.Last().Target;
                output += " " + destination.Id;
            }
            output += "\n";

            int totalPathDistance = 0;
            int numPaths = 0;
            foreach (ExplorationEdge[] edges in node.GetPathsOut())
            {
                totalPathDistance += edges.Length;
                numPaths++;
            }

            output += "\n";
            output += node.State.ToString();
            output += "\n";
            output += "Num Paths: " + numPaths + " --- Avg. Length: " + ((double)totalPathDistance / (double)numPaths);
            output += "\n";
            output += "Pagerank: " + node.PageRank + " --- Inverse: " + node.InversePageRank;
            output += "\n";
            output += "AvgMinCutIn: " + node.AverageMinCutIn() + " -- AvgMinCutOut: " + node.AverageMinCutOut();
            output += "\n";
            output += "MinCutIn from Start: " + node.GetMinCutIn(this.nodes[0].Id) + " -- MinCutOut from Start: " + node.GetMinCutOut(this.nodes[0].Id);
            output += "\n\n\n";
        }
        return output;
    }
}
