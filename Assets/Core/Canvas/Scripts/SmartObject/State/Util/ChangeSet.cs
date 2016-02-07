// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.Collections;
using System.Collections.Generic;

public class ChangeSet
{
    public readonly uint Id;

    private List<StateName> states;
    private Dictionary<uint, IList<RelationName>> relations;

    public ChangeSet(uint id)
    {
        this.states = new List<StateName>();
        this.relations = new Dictionary<uint, IList<RelationName>>();
    }

    public void AddChange(IEnumerable<StateName> states)
    {
        this.states.AddRange(states);
    }

    public void AddChange(uint id, IEnumerable<RelationName> relations)
    {
        if (this.relations.ContainsKey(id) == false)
            this.relations.Add(id, new List<RelationName>());
        relations.ForEach(
            (RelationName relation) => this.relations[id].Add(relation));
    }

    public IList<StateName> GetStates()
    {
        return this.states.AsReadOnly();
    }

    public IEnumerable<Tuple<uint, IList<RelationName>>> GetRelations()
    {
        foreach (KeyValuePair<uint, IList<RelationName>> kv in this.relations)
            yield return new Tuple<uint, IList<RelationName>>(kv.Key, kv.Value);
        yield break;
    }
}