using UnityEngine;
using TreeSharpPlus;
using System.Collections;

// A series of helper events to save on boilerplate code

public abstract class GenericEvent<T1> : SmartEvent
    where T1 : SmartObject
{
    protected T1 participant1;

    protected abstract Node Root(
        Token token,
        T1 participant1);

    public sealed override Node BakeTree(Token token)
    {
        return this.Root(
            token, 
            this.participant1);
    }

    public GenericEvent(
        T1 participant)
        : base(
            participant)
    {
        this.participant1 = participant;
    }
}

public abstract class GenericEvent<T1, T2> : SmartEvent
    where T1 : SmartObject
    where T2 : SmartObject
{
    protected T1 participant1;
    protected T2 participant2;

    protected abstract Node Root(
        Token token,
        T1 participant1, 
        T2 participant2);

    public sealed override Node BakeTree(Token token)
    {
        return this.Root(
            token,
            this.participant1, 
            this.participant2);
    }

    public GenericEvent(
        T1 participant1, 
        T2 participant2)
        : base(
            participant1, 
            participant2)
    {
        this.participant1 = participant1;
        this.participant2 = participant2;
    }
}

public abstract class GenericEvent<T1, T2, T3> : SmartEvent
    where T1 : SmartObject
    where T2 : SmartObject
    where T3 : SmartObject
{
    protected T1 participant1;
    protected T2 participant2;
    protected T3 participant3;

    protected abstract Node Root(
        Token token,
        T1 participant1,
        T2 participant2,
        T3 participant3);

    public sealed override Node BakeTree(Token token)
    {
        return this.Root(
            token,
            this.participant1,
            this.participant2,
            this.participant3);
    }

    public GenericEvent(
        T1 participant1, 
        T2 participant2,
        T3 participant3)
        : base(
            participant1, 
            participant2, 
            participant3)
    {
        this.participant1 = participant1;
        this.participant2 = participant2;
        this.participant3 = participant3;
    }
}

public abstract class GenericEvent<T1, T2, T3, T4> : SmartEvent
    where T1 : SmartObject
    where T2 : SmartObject
    where T3 : SmartObject
    where T4 : SmartObject
{
    protected T1 participant1;
    protected T2 participant2;
    protected T3 participant3;
    protected T4 participant4;

    protected abstract Node Root(
        Token token,
        T1 participant1,
        T2 participant2,
        T3 participant3,
        T4 participant4);

    public sealed override Node BakeTree(Token token)
    {
        return this.Root(
            token,
            this.participant1,
            this.participant2,
            this.participant3,
            this.participant4);
    }

    public GenericEvent(
        T1 participant1,
        T2 participant2,
        T3 participant3,
        T4 participant4)
        : base(
            participant1, 
            participant2, 
            participant3, 
            participant4)
    {
        this.participant1 = participant1;
        this.participant2 = participant2;
        this.participant3 = participant3;
        this.participant4 = participant4;
    }
}

public abstract class GenericEvent<T1, T2, T3, T4, T5> : SmartEvent
    where T1 : SmartObject
    where T2 : SmartObject
    where T3 : SmartObject
    where T4 : SmartObject
    where T5 : SmartObject
{
    protected T1 participant1;
    protected T2 participant2;
    protected T3 participant3;
    protected T4 participant4;
    protected T5 participant5;

    protected abstract Node Root(
        Token token,
        T1 participant1,
        T2 participant2,
        T3 participant3,
        T4 participant4,
        T5 participant5);

    public sealed override Node BakeTree(Token token)
    {
        return this.Root(
            token,
            this.participant1,
            this.participant2,
            this.participant3,
            this.participant4,
            this.participant5);
    }

    public GenericEvent(
        T1 participant1,
        T2 participant2,
        T3 participant3,
        T4 participant4,
        T5 participant5)
        : base(
            participant1,
            participant2,
            participant3,
            participant4,
            participant5)
    {
        this.participant1 = participant1;
        this.participant2 = participant2;
        this.participant3 = participant3;
        this.participant4 = participant4;
        this.participant5 = participant5;
    }
}

public abstract class GenericEvent<T1, T2, T3, T4, T5, T6> : SmartEvent
    where T1 : SmartObject
    where T2 : SmartObject
    where T3 : SmartObject
    where T4 : SmartObject
    where T5 : SmartObject
    where T6 : SmartObject
{
    protected T1 participant1;
    protected T2 participant2;
    protected T3 participant3;
    protected T4 participant4;
    protected T5 participant5;
    protected T6 participant6;

    protected abstract Node Root(
        Token token,
        T1 participant1,
        T2 participant2,
        T3 participant3,
        T4 participant4,
        T5 participant5,
        T6 participant6);

    public sealed override Node BakeTree(Token token)
    {
        return this.Root(
            token,
            this.participant1,
            this.participant2,
            this.participant3,
            this.participant4,
            this.participant5,
            this.participant6);
    }

    public GenericEvent(
        T1 participant1,
        T2 participant2,
        T3 participant3,
        T4 participant4,
        T5 participant5,
        T6 participant6)
        : base(
            participant1,
            participant2,
            participant3,
            participant4,
            participant5,
            participant6)
    {
        this.participant1 = participant1;
        this.participant2 = participant2;
        this.participant3 = participant3;
        this.participant4 = participant4;
        this.participant5 = participant5;
        this.participant6 = participant6;
    }
}

