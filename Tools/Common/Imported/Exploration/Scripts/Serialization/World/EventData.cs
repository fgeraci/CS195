// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EventData
{
    [System.Serializable]
    public class StateConditionData : IEquatable<StateConditionData>
    {
        public static StateCondition[] Decode(StateConditionData[] data)
        {
            return data.Convert(
                scd => new StateCondition(
                    scd.Index, 
                    scd.Tags.Convert(t => (StateName)t).ToArray())).ToArray();
        }

        public int Index;
        public long[] Tags;

        public StateConditionData()
        {
            this.Index = -1;
            this.Tags = null;
        }

        public StateConditionData(int index, StateName[] tags)
        {
            this.Index = index;
            this.Tags = tags.Convert(t => (long)t).ToArray();
        }

        public StateConditionData(StateCondition condition)
        {
            this.Index = condition.Index;
            this.Tags = condition.Tags.Convert(t => (long)t).ToArray();
        }

        public bool Equals(StateConditionData other)
        {
            if (this.Index != other.Index)
                return false;
            if (this.Tags.Length != other.Tags.Length)
                return false;
            for (int i = 0; i < this.Tags.Length; i++)
                if (this.Tags[i] != other.Tags[i])
                    return false;
            return true;
        }
    }

    [System.Serializable]
    public class RelationConditionData : IEquatable<RelationConditionData>
    {
        public static RelationCondition[] Decode(RelationConditionData[] data)
        {
            return data.Convert(
                rcd => new RelationCondition(
                    rcd.IndexFrom,
                    rcd.IndexTo,
                    rcd.Tags.Convert(t => (RelationName)t).ToArray())).ToArray();
        }

        public int IndexFrom;
        public int IndexTo;
        public long[] Tags;

        public RelationConditionData()
        {
            this.IndexFrom = -1;
            this.IndexTo = -1;
            this.Tags = null;
        }

        public RelationConditionData(int indexFrom, int indexTo, RelationName[] tags)
        {
            this.IndexFrom = indexFrom;
            this.IndexTo = indexTo;
            this.Tags = tags.Convert(t => (long)t).ToArray();
        }

        public RelationConditionData(RelationCondition condition)
        {
            this.IndexFrom = condition.IndexFrom;
            this.IndexTo = condition.IndexTo;
            this.Tags = condition.Tags.Convert(t => (long)t).ToArray();
        }

        public bool Equals(RelationConditionData other)
        {
            if (this.IndexFrom != other.IndexFrom)
                return false;
            if (this.IndexTo != other.IndexTo)
                return false;
            if (this.Tags.Length != other.Tags.Length)
                return false;
            for (int i = 0; i < this.Tags.Length; i++)
                if (this.Tags[i] != other.Tags[i])
                    return false;
            return true;
        }
    }

    [System.Serializable]
    public class RuleConditionData : IEquatable<RuleConditionData>
    {
        public static RuleCondition[] Decode(RuleConditionData[] data)
        {
            return data.Convert(
                rcd => new RuleCondition(
                    rcd.IndexFrom,
                    rcd.IndexTo,
                    rcd.Rules.Convert(t => (RuleName)t).ToArray())).ToArray();
        }

        public int IndexFrom;
        public int IndexTo;
        public long[] Rules;

        public RuleConditionData()
        {
            this.IndexFrom = -1;
            this.IndexTo = -1;
            this.Rules = null;
        }

        public RuleConditionData(int indexFrom, int indexTo, RuleName[] rules)
        {
            this.IndexFrom = indexFrom;
            this.IndexTo = indexTo;
            this.Rules = rules.Convert(t => (long)t).ToArray();
        }

        public RuleConditionData(RuleCondition condition)
        {
            this.IndexFrom = condition.IndexFrom;
            this.IndexTo = condition.IndexTo;
            this.Rules = condition.Rules.Convert(t => (long)t).ToArray();
        }

        public bool Equals(RuleConditionData other)
        {
            if (this.IndexFrom != other.IndexFrom)
                return false;
            if (this.IndexTo != other.IndexTo)
                return false;
            if (this.Rules.Length != other.Rules.Length)
                return false;
            for (int i = 0; i < this.Rules.Length; i++)
                if (this.Rules[i] != other.Rules[i])
                    return false;
            return true;
        }
    }

    public string Name;
    public string Type;
    public string[] RequiredTypes;
    public string[] Sentiments;

    public StateConditionData[] StateReqs;
    public RelationConditionData[] RelationReqs;
    public RuleConditionData[] RuleReqs;

    public StateConditionData[] StateEffects;
    public RelationConditionData[] RelationEffects;

    public int[] CanEqualPairs;

    public EventData()
    {
        this.Name = null;
        this.Type = null;
        this.RequiredTypes = null;
        this.Sentiments = null;

        this.StateReqs = null;
        this.RelationReqs = null;
        this.RuleReqs = null;

        this.StateEffects = null;
        this.RelationEffects = null;
        this.CanEqualPairs = null;
    }

    public EventData(EventDescriptor descriptor)
    {
        this.Name = descriptor.Name;
        this.Type = descriptor.Type.FullName;
        this.RequiredTypes = 
            descriptor.RequiredTypes.Convert(t => t.FullName).ToArray();

        this.Sentiments = new string[descriptor.Sentiments.Length];
        Array.Copy(
            descriptor.Sentiments, 
            this.Sentiments, 
            this.Sentiments.Length);

        this.StateReqs = this.Extract(descriptor.StateReqs);
        this.RelationReqs = this.Extract(descriptor.RelationReqs);
        this.RuleReqs = this.Extract(descriptor.RuleReqs);

        this.StateEffects = this.Extract(descriptor.StateEffects);
        this.RelationEffects = this.Extract(descriptor.RelationEffects);

        this.CanEqualPairs = descriptor.CanEqualPairs;
    }

    private StateConditionData[] Extract(
        IEnumerable<StateCondition> conditions)
    {
        return conditions.Convert(c => new StateConditionData(c)).ToArray();
    }

    private RelationConditionData[] Extract(
        IEnumerable<RelationCondition> conditions)
    {
        return conditions.Convert(c => new RelationConditionData(c)).ToArray();
    }

    private RuleConditionData[] Extract(
        IEnumerable<RuleCondition> conditions)
    {
        return conditions.Convert(c => new RuleConditionData(c)).ToArray();
    }

    private bool Compare<T>(T[] source, T[] dest)
        where T : IEquatable<T>
    {
        if (source.Length != dest.Length)
            return false;
        for (int i = 0; i < source.Length; i++)
            if (source[i].Equals(dest[i]) == false)
                return false;
        return true;
    }

    public EventDescriptor Decode()
    {
        // NOTE: The type dereferencing from string here will only work if the
        //       type is in the SAME ASSEMBLY as this EventData class. This
        //       shouldn't be a problem for Unity, but it's worth remembering.
        return new EventDescriptor(
            this.Name,
            System.Type.GetType(this.Type),
            this.RequiredTypes.Convert(t => System.Type.GetType(t)).ToArray(),
            this.Sentiments,
            StateConditionData.Decode(this.StateReqs),
            RelationConditionData.Decode(this.RelationReqs),
            RuleConditionData.Decode(this.RuleReqs),
            StateConditionData.Decode(this.StateEffects),
            RelationConditionData.Decode(this.RelationEffects),
            this.CanEqualPairs);
    }
}
