//using UnityEngine;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;

//public class Storyteller : MonoBehaviour
//{
//    private Func<ExplorationNode, ExplorationNode, double>[] scorers =
//    {
//        Score_Distance,
//        Score_SkinnyPipe,
//        Score_TrapNodes,
//        Score_HighPagerank,
//        Score_LowPagerank,
//        Score_HighInvPagerank,
//        Score_LowInvPagerank
//    };

//    private bool descending = false;

//    //Distance(source, dest)
//    //Distance(dest, source)
//    //Pagerank(source)
//    //Pagerank(dest)
//    //InvPagerank(source)
//    //InvPagerank(dest)
//    //MinCut(source, dest)
//    //MinCut(dest, source)
//    //Avg. MinCutIn(source)
//    //Avg. MinCutIn(dest)
//    //Avg. MinCutOut(source)
//    //Avg. MinCutOut(dest)
//    //NumIncoming(source)
//    //NumIncoming(dest)
//    //NumOutgoing(source)
//    //NumOutgoing(dest)

//    private class Candidate
//    {
//        public readonly ExplorationEdge[] Path;
//        public readonly ExplorationNode Goal;

//        public readonly double Score;

//        public Candidate(
//            ExplorationEdge[] edge,
//            ExplorationNode goal,
//            double score)
//        {
//            this.Goal = goal;
//            this.Path = edge;
//            this.Score = score;
//        }
//    }

//    /// <summary>
//    /// Picks far away nodes
//    /// </summary>
//    private static double Score_Distance(
//        ExplorationNode current,
//        ExplorationNode candidate)
//    {
//        return 1.0 / current.GetPathOut(candidate.Id).Length;
//    }

//    /// <summary>
//    /// Picks far away nodes with a thin bottleneck
//    /// </summary>
//    private static double Score_SkinnyPipe(
//        ExplorationNode current,
//        ExplorationNode candidate)
//    {
//        int length = current.GetPathOut(candidate.Id).Length;
//        int minCutOut = current.GetMinCutOut(candidate.Id);
//        return (double)minCutOut / (double)length;
//    }

//    /// <summary>
//    /// Picks nodes with a low average min-cut out
//    /// </summary>
//    private static double Score_TrapNodes(
//        ExplorationNode current,
//        ExplorationNode candidate)
//    {
//        return (double)candidate.AverageMinCutOut();
//    }

//    /// <summary>
//    /// Picks nodes with a high pagerank
//    /// </summary>
//    private static double Score_HighPagerank(
//        ExplorationNode current,
//        ExplorationNode candidate)
//    {
//        return 1.0 / candidate.PageRank;
//    }

//    /// <summary>
//    /// Picks nodes with a low pagerank
//    /// </summary>
//    private static double Score_LowPagerank(
//        ExplorationNode current,
//        ExplorationNode candidate)
//    {
//        return candidate.PageRank;
//    }

//    /// <summary>
//    /// Picks nodes with a high pagerank
//    /// </summary>
//    private static double Score_HighInvPagerank(
//        ExplorationNode current,
//        ExplorationNode candidate)
//    {
//        return 1.0 / candidate.InversePageRank;
//    }

//    /// <summary>
//    /// Picks nodes with a low pagerank
//    /// </summary>
//    private static double Score_LowInvPagerank(
//        ExplorationNode current,
//        ExplorationNode candidate)
//    {
//        return candidate.InversePageRank;
//    }

//    public int Recommendations = 3;
//    public ExplorationManager Explorer;
//    bool done = false;

//    void Start()
//    {
        
//    }

//    void Update()
//    {
//        if (this.done == false && this.Explorer.IsDone == true)
//        {
//            string allStories = "";
//            foreach (var func in this.scorers)
//            {
//                allStories += "#" + func.Method.Name + "\n";
//                foreach (Candidate candidate in this.GetCandidates(func))
//                {
//                    allStories += FormatStory(candidate) + "%\n";
//                }
//            }
//            Debug.Log(allStories);
//            this.done = true;
//        }
//    }

//    private string FormatStory(Candidate candidate)
//    {
//        string output = "";
//        foreach (ExplorationEdge edge in candidate.Path)
//        {
//            TransitionEvent evt = edge.Events[0];
//            EventSignature sig = (EventSignature)evt.Descriptor;
//            string name = sig.Name;

//            string[] members = new string[evt.Participants.Length];
//            for (int i = 0; i < members.Length; i++)
//                members[i] = ObjectManager.Instance.GetObjectById(evt.Participants[i]).gameObject.name;

//            output += FormatLine(name, members) + "\n";
//        }
//        return output;
//    }

//    private string FormatLine(string name, string[] members)
//    {
//        switch (name)
//        {
//            case "Mini_UnlockTellerDoor":
//                return members[0] + " unlocks the door to the teller area.";
//            case "Mini_TellerButtonPress":
//                return members[0] + " presses the button to open the safe.";
//            case "Mini_CoerceIntoGivingKey":
//                return members[0] + " coerces " + members[1] + " to hand over the keys at gunpoint.";
//            case "Mini_UnlockManagerDoor":
//                return members[0] + " unlocks the door to the room with the safe.";
//            case "Mini_PickupBriefcase":
//                return members[0] + " picks up the stolen money.";
//            default:
//                return this.RawFormat(name, members);
//        }
//    }

//    private string RawFormat(string name, string[] members)
//    {
//        string output = name + "(";
//        for (int i = 0; i < members.Length; i++)
//            output += members[i] + ((i == members.Length - 1) ? "" : ", ");
//        return output + ")";
//    }

//    private IList<Candidate> GetCandidates(
//        Func<ExplorationNode, ExplorationNode, double> scorer,
//        bool descending = false)
//    {
//        ExplorationNode firstNode = this.Explorer.StateSpace.Nodes[0];
//        List<Candidate> candidates = new List<Candidate>();

//        foreach (ExplorationNode node in this.Explorer.StateSpace.Nodes)
//        {
//            if (firstNode == node)
//                continue;

//            // Make sure it's reachable
//            ExplorationEdge[] path = firstNode.GetPathOut(node.Id);
//            if (path != null)
//            {
//                // Add it as a candidate if the score is high enough
//                double score = scorer(firstNode, node);
//                ExplorationEdge edge = path[0];
//                ExplorationNode goal = path.Last().Target;
//                candidates.Add(new Candidate(path, goal, score));
//            }
//        }

//        // Sort the candidates by score
//        candidates.Sort((c1, c2) => c1.Score.CompareTo(c2.Score));
//        if (descending == true)
//            candidates.Reverse();

//        HashSet<ExplorationNode> seenGoals = new HashSet<ExplorationNode>();

//        // A list of candidates that we've picked to display
//        List<Candidate> picked = new List<Candidate>();

//        // Pick the top N
//        foreach (Candidate candidate in candidates)
//        {
//            ExplorationNode goal = candidate.Goal;

//            // If this is a novel direction to go, add it
//            bool seenGoal = seenGoals.Contains(goal);

//            if (seenGoal == false)
//            {
//                picked.Add(candidate);
//                seenGoals.Add(goal);
//            }

//            if (picked.Count >= this.Recommendations)
//                break;
//        }

//        return picked;
//    }
//}
