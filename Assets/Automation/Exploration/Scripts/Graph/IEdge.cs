// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System.Collections;

public interface IEdge
{
    INode Source { get; set; }
    INode Target { get; set; }

    bool Saturated { get; set; }

    IEdge Clone();
}
