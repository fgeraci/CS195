using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

public class Recommender
{
    private ExplorationSpace space;
    private IList<Candidate> candidates = null;
    private ExplorationNode current = null;
    private HashSet<uint> busyObjects = null;
    private ExplorationNode lastGoal = null;

    public Recommender(ExplorationSpace space)
    {
        this.space = space;
        this.current = space.Nodes[0];
        this.busyObjects = new HashSet<uint>();
    }

    public void AddBusy(uint toAdd)
    {
        this.busyObjects.Add(toAdd);
    }

    public void RemoveBusy(uint toAdd)
    {
        this.busyObjects.Remove(toAdd);
    }

    public IEnumerable<uint> GetBusy()
    {
        return busyObjects.AsEnumerable();
    }

    public IList<Candidate> GetCandidates()
    {
        this.candidates = 
            new List<Candidate>(
                GoalSelector.GetCandidates(
                    this.space,
                    this.current,
                    this.lastGoal,
                    this.busyObjects));
        return this.candidates;
    }

    public void CandidateSelected(int index)
    {
        if (this.candidates != null)
        {
            Candidate selected = this.candidates[index];
            this.current = selected.Path[0].Target;
            this.lastGoal = selected.Goal;
            this.candidates = null;
        }
        else
        {
            Console.WriteLine("WARNING: Ignoring TakeCandidate()");
        }
    }
}