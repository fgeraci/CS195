using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class GoalSelector
    {
        public static string[] SentimentWant = { };
        public static string[] SentimentDont = { };

        public delegate double Scorer(
            ExplorationNode start,
            ExplorationNode end);

        public static readonly Scorer DEFAULT_SCORER = Score_SkinnyPipe;
        public const int DEFAULT_RECOMMENDATIONS = 3;
        public const bool DEFAULT_DESCENDING = false;

        public static readonly Scorer[] Scorers =
        {
            Score_Distance,
            Score_SkinnyPipe,
            Score_TrapNodes,
            Score_HighPagerank,
            Score_LowPagerank,
            Score_HighInvPagerank,
            Score_LowInvPagerank
        };

        //Distance(source, dest)
        //Distance(dest, source)
        //Pagerank(source)
        //Pagerank(dest)
        //InvPagerank(source)
        //InvPagerank(dest)
        //MinCut(source, dest)
        //MinCut(dest, source)
        //Avg. MinCutIn(source)
        //Avg. MinCutIn(dest)
        //Avg. MinCutOut(source)
        //Avg. MinCutOut(dest)
        //NumIncoming(source)
        //NumIncoming(dest)
        //NumOutgoing(source)
        //NumOutgoing(dest)

        public static double SentimentScore(double score, ExplorationEdge[] path)
        {
          foreach (ExplorationEdge edge in path)
          {
            EventDescriptor transDesc = edge.Events[0].Descriptor;
            foreach (string sentiment in transDesc.Sentiments)
            {
              if (SentimentWant.Contains(sentiment) == true)
                score *= 100.0;
              if (SentimentDont.Contains(sentiment) == true)
                score *= 0.01;
            }
          }

          return score;
        }

        /// <summary>
        /// Picks far away nodes
        /// </summary>
        public static double Score_Distance(
            ExplorationNode current,
            ExplorationNode candidate)
        {
            return 1.0 / current.GetPathOut(candidate.Id).Length;
        }

        /// <summary>
        /// Picks far away nodes with a thin bottleneck
        /// </summary>
        public static double Score_SkinnyPipe(
            ExplorationNode current,
            ExplorationNode candidate)
        {
            int length = current.GetPathOut(candidate.Id).Length;
            int minCutOut = current.GetMinCutOut(candidate.Id);
            return (double)minCutOut / (double)length;
        }

        /// <summary>
        /// Picks nodes with a low average min-cut out
        /// </summary>
        public static double Score_TrapNodes(
            ExplorationNode current,
            ExplorationNode candidate)
        {
            return (double)candidate.AverageMinCutOut();
        }

        /// <summary>
        /// Picks nodes with a high pagerank
        /// </summary>
        public static double Score_HighPagerank(
            ExplorationNode current,
            ExplorationNode candidate)
        {
            return 1.0 / candidate.PageRank;
        }

        /// <summary>
        /// Picks nodes with a low pagerank
        /// </summary>
        public static double Score_LowPagerank(
            ExplorationNode current,
            ExplorationNode candidate)
        {
            return candidate.PageRank;
        }

        /// <summary>
        /// Picks nodes with a high pagerank
        /// </summary>
        public static double Score_HighInvPagerank(
            ExplorationNode current,
            ExplorationNode candidate)
        {
            return 1.0 / candidate.InversePageRank;
        }

        /// <summary>
        /// Picks nodes with a low pagerank
        /// </summary>
        public static double Score_LowInvPagerank(
            ExplorationNode current,
            ExplorationNode candidate)
        {
            return candidate.InversePageRank;
        }

        public static IEnumerable<Candidate> GetCandidates(
            ExplorationSpace space,
            ExplorationNode start)
        {
            return GetCandidates(
                space,
                start,
                null,
                null,
                DEFAULT_SCORER,
                DEFAULT_RECOMMENDATIONS,
                DEFAULT_DESCENDING);
        }

        public static IEnumerable<Candidate> GetCandidates(
            ExplorationSpace space,
            ExplorationNode start,
            ExplorationNode lastGoal,
            HashSet<uint> busyObjects)
        {
            return GetCandidates(
                space,
                start,
                lastGoal,
                busyObjects,
                DEFAULT_SCORER,
                DEFAULT_RECOMMENDATIONS,
                DEFAULT_DESCENDING);
        }

        public static IEnumerable<Candidate> GetCandidates(
            ExplorationSpace space,
            ExplorationNode start,
            Scorer scorer)
        {
            return GetCandidates(
                space,
                start,
                null,
                null,
                scorer, 
                DEFAULT_RECOMMENDATIONS, 
                DEFAULT_DESCENDING);
        }

        public static IEnumerable<Candidate> GetCandidates(
            ExplorationSpace space,
            ExplorationNode start,
            int numRecommendations)
        {
            return GetCandidates(
                space,
                start,
                null,
                null,
                DEFAULT_SCORER,
                numRecommendations,
                DEFAULT_DESCENDING);
        }

        public static IEnumerable<Candidate> GetCandidates(
            ExplorationSpace space,
            ExplorationNode start,
            ExplorationNode lastGoal,
            HashSet<uint> busyObjects,
            Scorer scorer,
            int numRecommendations,
            bool descending)
        {
            if (busyObjects == null)
                busyObjects = new HashSet<uint>();

            List<Candidate> candidates = new List<Candidate>();
            foreach (ExplorationNode node in space.Nodes)
            {
                if (start == node)
                    continue;

                // Make sure it's reachable
                ExplorationEdge[] path = start.GetPathOut(node.Id);
                if (path != null)
                {
                    // Add it as a candidate if the score is high enough
                    double score = scorer(start, node);
                    ExplorationEdge edge = path[0];
                    ExplorationNode goal = path.Last().Target;
                    double sentimentScore = SentimentScore(score, path);
                    candidates.Add(new Candidate(path, goal, sentimentScore));
                }
            }

            // Sort the candidates by score
            candidates.Sort((c1, c2) => c1.Score.CompareTo(c2.Score));
            if (descending == true)
                candidates.Reverse();

            // Add the previous goal to the front if we still have a path to it
            if (lastGoal != null)
            {
                ExplorationEdge[] oldPath = start.GetPathOut(lastGoal.Id);
                if (oldPath != null)
                    candidates.Insert(0, new Candidate(oldPath, lastGoal, 0.0));
            }

            HashSet<ExplorationNode> seenGoals = new HashSet<ExplorationNode>();
            HashSet<ExplorationEdge> seenEdges = new HashSet<ExplorationEdge>();

            // Pick the top N
            int count = 0;
            foreach (Candidate candidate in candidates)
            {
                ExplorationNode goal = candidate.Goal;
                if (candidate.Path.Length == 0)
                    continue;
                ExplorationEdge edge = candidate.Path[0];

                // Nix this event if it uses an object that's currently in use
                // TODO: Assumes only one event per edge
                TransitionEvent evt = edge.Events[0];
                foreach (uint id in evt.Participants)
                    if (busyObjects.Contains(id) == true)
                        goto skipCandidate;

                // If this is a novel direction to go, add it
                bool seenGoal = seenGoals.Contains(goal);
                bool seenEdge = seenEdges.Contains(edge);

                if (seenGoal == false && seenEdge == false)
                {
                    seenGoals.Add(goal);
                    seenEdges.Add(edge);
                    count++;
                    yield return candidate;
                }

                if (count >= numRecommendations)
                    break;

                skipCandidate : continue;
            }
        }
    }
}

