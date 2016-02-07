using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Candidate
    {
        public readonly ExplorationEdge[] Path;
        public readonly ExplorationNode Goal;

        public readonly double Score;

        public Candidate(
            ExplorationEdge[] path,
            ExplorationNode goal,
            double score)
        {
            this.Goal = goal;
            this.Path = path;
            this.Score = score;
        }
    }
}
