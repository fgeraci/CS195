// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System.Collections;
using System.Collections.Generic;

public interface INode
{
    IEnumerable<IEdge> Incoming { get; }
    IEnumerable<IEdge> Outgoing { get; }

    int NumIncoming { get; }
    int NumOutgoing { get; }

    void AddIncoming(IEdge edge);
    void AddOutgoing(IEdge edge);

    double PageRank { get; set; }
    double InversePageRank { get; set; }

    INode Clone();
}
