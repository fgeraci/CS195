// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ExplorationEdge : IEdge
{
    public ExplorationNode Source { get; set; }
    public ExplorationNode Target { get; set; }

#if PARALLEL
    public bool Saturated
    {
        get { throw new NotSupportedException("Don't use this."); }
        set { throw new NotSupportedException("Really. Don't."); }
    }
#else
    public bool Saturated { get; set; }
#endif

    public TransitionEvent[] Events { get; private set; }

    public ExplorationEdge(
        ExplorationNode source,
        ExplorationNode target,
        params TransitionEvent[] transitions)
    {
        this.Source = source;
        this.Target = target;
        this.Events = transitions;

#if !PARALLEL
        this.Saturated = false;
#endif  
    }

    public IEdge Clone()
    {
        ExplorationEdge newEdge =
            new ExplorationEdge(
                this.Source,
                this.Target,
                this.Events);

#if !PARALLEL
        newEdge.Saturated = this.Saturated;
#endif
        return newEdge;
    }

    INode IEdge.Source
    {
        get
        {
            return this.Source;
        }
        set
        {
            this.Source = (ExplorationNode)value;
        }
    }

    INode IEdge.Target
    {
        get
        {
            return this.Target;
        }
        set
        {
            this.Target = (ExplorationNode)value;
        }
    }
}