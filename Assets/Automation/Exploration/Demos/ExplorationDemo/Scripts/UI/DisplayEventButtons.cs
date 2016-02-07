using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DisplayEventButtons : MonoBehaviour 
{
    private Func<ExplorationNode, ExplorationNode, double> currentScorer = ScoreNodeSkinnyPipe;
    private bool descending = false;

    private class Candidate
    {
        public readonly ExplorationEdge Edge;
        public readonly ExplorationNode Goal;

        public readonly double Score;

        public Candidate(
            ExplorationEdge edge,
            ExplorationNode goal,
            double score)
        {
            this.Goal = goal;
            this.Edge = edge;
            this.Score = score;
        }
    }

    private static double ScoreNodeSkinnyPipe(
        ExplorationNode current, 
        ExplorationNode candidate)
    {

        int length = current.GetPathOut(candidate.Id).Length;
        int minCutOut = current.GetMinCutOut(candidate.Id);
        double modifier = ScorePath(current.GetPathOut(candidate.Id));
        return ((double)minCutOut / (double)length) / modifier;
    }

    private static double ScorePath(
        ExplorationEdge[] path)
    {
        double result = 1.0;
        foreach (ExplorationEdge edge in path)
        {
            TransitionEvent evt = edge.Events[0];
            EventSignature sig = 
                EventLibrary.Instance.GetSignature(evt.Descriptor.Name);

            foreach (string sent in sig.Sentiments)
                result += 0.1;
        }
        return result;
    }

    //private static double ScoreNodePenalizeHubs(
    //    ExplorationNode current,
    //    ExplorationNode candidate)
    //{
    //    return candidate.PageRank + candidate.AverageMinCutIn();
    //}

    //private static double ScoreNodeFindDistantOutHubs(
    //    ExplorationNode current,
    //    ExplorationNode candidate)
    //{
    //    return ScoreNodeSkinnyPipe(current, candidate) / candidate.AverageMinCutOut();
    //}

    private static System.Random rnd;

    public int Recommendations = 3;
    public Transform Window;
    public ExplorationManager Explorer;

    private ButtonID[] buttons;
    private UILabel[] labels;

    private List<Candidate> candidates = null;

    private SmartEvent currentEvent = null;
    private ExplorationNode currentNode = null;
    private ExplorationEdge currentEdge = null;
    private ExplorationNode previousGoal = null;

    void Start()
    {
        rnd = new System.Random();

        this.buttons = this.Window.GetComponentsInChildren<ButtonID>();
        List<UILabel> foundLabels = new List<UILabel>();
        int i = 0;
        foreach (ButtonID button in this.buttons)
        {
            button.ID = i++;
            Transform buttonTransform = button.transform.GetChild(0);
            foundLabels.Add(
                buttonTransform.GetComponentInChildren<UILabel>());
            button.gameObject.SetActive(false);
        }

        this.labels = foundLabels.ToArray();
        this.candidates = new List<Candidate>();
    }

    private void Populate(
        Func<ExplorationNode, ExplorationNode, double> scorer,
        bool descending = false)
    {
        if (this.currentNode == null)
            if (this.Explorer.IsDone == true)
                this.currentNode = Explorer.StateSpace.Nodes[0];

        if (this.currentNode != null)
        {
            List<Candidate> candidates = new List<Candidate>();

            foreach (ExplorationNode node in this.Explorer.StateSpace.Nodes)
            {
                if (this.currentNode == node)
                    continue;

                // Make sure it's reachable
                ExplorationEdge[] path = this.currentNode.GetPathOut(node.Id);
                if (path != null)
                {
                    // Add it as a candidate if the score is high enough
                    double score = scorer(this.currentNode, node);
                    ExplorationEdge edge = path[0];
                    ExplorationNode goal = path.Last().Target;
                    candidates.Add(new Candidate(edge, goal, score));
                }
            }

            // Sort the candidates by score
            candidates.Sort((c1, c2) => c1.Score.CompareTo(c2.Score));
            if (descending == true)
                candidates.Reverse();

            HashSet<ExplorationNode> seenNexts = new HashSet<ExplorationNode>();
            HashSet<ExplorationNode> seenGoals = new HashSet<ExplorationNode>();

            // A list of candidates that we've picked to display
            List<Candidate> picked = new List<Candidate>();

            // Add the next step of the previous goal, if we had one
            if (this.previousGoal != null
                && this.previousGoal != this.currentNode
                && this.currentNode.GetPathOut(this.previousGoal.Id) != null)
            {
                // Get the path to the previous goal
                ExplorationEdge[] path = 
                    this.currentNode.GetPathOut(this.previousGoal.Id);

                // Make sure it's still reachable
                if (path != null)
                {
                    // If so, add it as a picked candidate
                    picked.Add(
                        new Candidate(path[0], this.previousGoal, -1));
                    seenNexts.Add(path[0].Target);
                    seenGoals.Add(this.previousGoal);
                }
            }

            // Pick the top N
            foreach (Candidate candidate in candidates)
            {
                ExplorationNode next = candidate.Edge.Target;
                ExplorationNode goal = candidate.Goal;

                // If this is a novel direction to go, add it
                bool seenGoal = seenGoals.Contains(goal);
                bool seenNext = seenNexts.Contains(next);

                if (seenGoal == false && seenNext == false)
                {
                    picked.Add(candidate);
                    seenNexts.Add(next);
                    seenGoals.Add(goal);
                }

                if (picked.Count >= this.Recommendations)
                    break;
            }

            this.candidates.Clear();
            this.candidates.AddRange(picked);
            Debug.Log("On state " + this.currentNode.Id);
        }
    }

    private void PopulateAdjacent(
        Func<ExplorationNode, ExplorationNode, double> scorer,
        bool descending = false)
    {
        if (this.currentNode == null)
            if (this.Explorer.IsDone == true)
                this.currentNode = Explorer.StateSpace.Nodes[0];

        if (this.currentNode != null)
        {
            List<Candidate> picked = new List<Candidate>();

            foreach (ExplorationEdge edge in this.currentNode.OutgoingExploration)
            {
                ExplorationNode goal = edge.Target;
                picked.Add(new Candidate(edge, goal, 1.0));
            }

            this.candidates.Clear();
            this.candidates.AddRange(picked);
            Debug.Log("On state " + this.currentNode.Id);
        }
    }

    public void OnButtonClick(int which)
    {
        DebugUtil.Assert(this.candidates.Count > which);
        Candidate candidate = this.candidates[which];
        DebugUtil.Assert(candidate != null);

        // TODO: Only taking the first event
        TransitionEvent evt = candidate.Edge.Events[0];
        DebugUtil.Assert(evt != null);

        this.DeselectAll(evt);
        EventSignature sig = (EventSignature)evt.Descriptor;
        SmartEvent instance =
            sig.Create(this.GetParameters(evt));

        string output = evt.Descriptor.Name;
        foreach (object parameter in this.GetParameters(evt))
        {
            SmartObject obj = (SmartObject)parameter;
            output += " " + obj.gameObject.name;
            
        }

        Debug.LogWarning("Executing: " + output);

        instance.StartEvent(1.0f);
        this.previousGoal = candidate.Goal;
        this.Depopulate();

        this.currentEvent = instance;
        this.currentEdge = candidate.Edge;
    }

    public void OnButtonHover(int which)
    {
        DebugUtil.Assert(this.candidates.Count > which);
        Candidate candidate = this.candidates[which];
        DebugUtil.Assert(candidate != null);

        // TODO: Only taking the first event
        TransitionEvent evt = candidate.Edge.Events[0];
        DebugUtil.Assert(evt != null);

        Debug.Log("Destination: " + candidate.Edge.Target.Id + " -- Goal: " + candidate.Goal.Id + " -- Score: " + this.currentScorer(this.currentNode, candidate.Goal));

        this.SelectAll(evt);
    }

    public void OnButtonHoverOut(int which)
    {
        DebugUtil.Assert(this.candidates.Count > which);
        Candidate candidate = this.candidates[which];
        DebugUtil.Assert(candidate != null);

        // TODO: Only taking the first event
        TransitionEvent evt = candidate.Edge.Events[0];
        DebugUtil.Assert(evt != null);

        this.DeselectAll(evt);
    }

    private void SelectAll(TransitionEvent evt)
    {
        foreach (SmartObject obj in this.GetParticipants(evt))
        {
            Highlight highlight = obj.GetComponent<Highlight>();
            if (highlight != null)
                highlight.HighlightOn();
        }
    }

    private void DeselectAll(TransitionEvent evt)
    {
        foreach (SmartObject obj in this.GetParticipants(evt))
        {
            Highlight highlight = obj.GetComponent<Highlight>();
            if (highlight != null)
                highlight.HighlightOff();
        }
    }

    private void Advance()
    {
        DebugUtil.Assert(this.currentNode == this.currentEdge.Source);
        this.currentNode = this.currentEdge.Target;
    }

    //private EventPopulation FindPopulation(EventSignature sig)
    //{
    //    List<EventPopulation> applicable =
    //        new List<EventPopulation>(
    //            sig.GetValidPopulations(
    //                ObjectManager.Instance.GetObjects().Cast<IHasState>()));

    //    if (applicable != null && applicable.Count > 0)
    //        return applicable[rnd.Next(applicable.Count)];
    //    return null;
    //}

    void Update()
    {
        // Are we running an event?
        if (this.currentEvent != null)
        {
            if (this.currentEvent.Behavior.Status == EventStatus.Finished)
            {
                this.currentEvent = null;
                this.Advance();
            }
        }
        else if (this.currentEvent == null)
        {
            // Do we have a populated edge list?
            if (this.candidates.Count == 0)
            {
                DisableAllButtons();
                //this.Populate(this.currentScorer, this.descending);
                this.PopulateAdjacent(this.currentScorer, this.descending);
            }
            else
            {
                for (int i = 0; i < this.labels.Length; i++)
                {
                    if (i < this.candidates.Count && this.candidates[i] != null)
                    {
                        this.labels[i].text = this.candidates[i].Edge.Events[0].Descriptor.Name;
                        this.buttons[i].gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    private void Depopulate()
    {
        this.candidates.Clear();
        this.DisableAllButtons();
    }

    private void DisableAllButtons()
    {
        foreach (ButtonID button in this.buttons)
            button.gameObject.SetActive(false);
    }

    /// <summary>
    /// Converts the id list to actual live object instances for
    /// execution
    /// </summary>
    private IList<IHasState> GetParameters(TransitionEvent evt)
    {
        List<IHasState> result = new List<IHasState>();
        foreach (uint id in evt.Participants)
            result.Add(ObjectManager.Instance.GetObjectById(id));
        return result;
    }

    private IEnumerable<SmartObject> GetParticipants(TransitionEvent evt)
    {
        return this.GetParameters(evt).Cast<SmartObject>();
    }
}
