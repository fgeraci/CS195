using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TestGraph : MonoBehaviour
{
    public class TestEdge : IEdge
    {
        public INode Source { get; set; }
        public INode Target { get; set; }

        public bool Saturated { get; set; }

        public TestEdge(INode source, INode target)
        {
            this.Source = source;
            this.Target = target;
            this.Saturated = false;
        }

        public IEdge Clone()
        {
            TestEdge newEdge = new TestEdge(this.Source, this.Target);
            newEdge.Saturated = this.Saturated;
            return newEdge;
        }
    }

    public class TestNode : INode
    {
        public object Token { get; set; }

        private double pageRank;
        private double inversePageRank;

        private List<TestEdge> incoming;
        private List<TestEdge> outgoing;

        public TestNode(object token)
        {
            this.Token = token;
            this.pageRank = 0.0;
            this.inversePageRank = 0.0;
            this.incoming = new List<TestEdge>();
            this.outgoing = new List<TestEdge>();
        }

        public double PageRank
        {
            get { return this.pageRank; }
            set { this.pageRank = value; }
        }

        public double InversePageRank
        {
            get { return this.inversePageRank; }
            set { this.inversePageRank = value; }
        }

        public IEnumerable<IEdge> Incoming
        {
            get { return this.incoming.Cast<IEdge>(); }
        }

        public IEnumerable<IEdge> Outgoing
        {
            get { return this.outgoing.Cast<IEdge>(); }
        }

        public int NumIncoming
        {
            get { return this.incoming.Count; }
        }

        public int NumOutgoing
        {
            get { return this.outgoing.Count; }
        }

        public void AddIncoming(IEdge edge)
        {
            this.incoming.Add((TestEdge)edge);
        }

        public void AddOutgoing(IEdge edge)
        {
            this.outgoing.Add((TestEdge)edge);
        }

        public INode Clone()
        {
            TestNode newNode = new TestNode(this.Token);
            newNode.pageRank = this.pageRank;
            return newNode;
        }

        public void Add(params TestNode[] targets)
        {
            foreach (TestNode target in targets)
            {
                TestEdge edge = new TestEdge(this, target);
                this.outgoing.Add(edge);
                target.incoming.Add(edge);
            }
        }
    }

    void Start()
    {
        DoTestPR();
        DoTestCut();
    }

    public static void DoTestPR()
    {
        //     A ⇄ C
        //     ↓ ↗ ↑
        //     B    D

        TestNode A = new TestNode("A");
        TestNode B = new TestNode("B");
        TestNode C = new TestNode("C");
        TestNode D = new TestNode("D");

        A.Add(B, C);
        B.Add(C);
        C.Add(A);
        D.Add(C);

        int iterations = 0;
        IEnumerator rank = GraphUtil.ComputeRank(new[] { A, B, C, D }, 0.85, 40, 5, 0.005).GetEnumerator();
        while (rank.MoveNext() == true)
            iterations++;

        Debug.Log(iterations + ": " + A.PageRank + " " + B.PageRank + " " + C.PageRank + " " + D.PageRank);
    }

    public static void DoTestCut()
    {
        //           A
        //        ↙ ↓ ↘
        //       B → C ← D
        //       ↓   ↓   ↓
        //       E → F ← G
        //        ↘ ↓ ↙
        //           H

        TestNode A = new TestNode("A");
        TestNode B = new TestNode("B");
        TestNode C = new TestNode("C");
        TestNode D = new TestNode("D");
        TestNode E = new TestNode("E");
        TestNode F = new TestNode("F");
        TestNode G = new TestNode("G");
        TestNode H = new TestNode("H");

        A.Add(B, C, D);
        B.Add(C, E);
        C.Add(F);
        D.Add(C, G);
        E.Add(F, H);
        F.Add(H);
        G.Add(F, H);

        IEnumerable<IEdge> path = GraphUtil.BFSPath(A, H);
        int iterations = GraphUtil.MinCut(A, H, new[] { A, B, C, D, E, F, G, H }, path);

        Debug.Log("MinCut: " + iterations);
    }
}
