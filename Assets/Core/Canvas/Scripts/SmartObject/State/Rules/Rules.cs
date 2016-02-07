// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using YieldProlog;


public delegate IEnumerable<bool> RuleFunction(
    IEnumerable<IHasState> scope,
    object X,
    object Y);

public delegate IEnumerable<bool> StateFunction(
    IEnumerable<IHasState> scope,
    object X);

public delegate IEnumerable<bool> RelationFunction(
    IEnumerable<IHasState> scope,
    object X, 
    object Y);

#pragma warning disable 0219
public class Rules
{
    private static int? DEBUG_scopeChecksum = null;

    private class RuleCache
    {
        private Dictionary<IHasState, Dictionary<IHasState, bool>> cache;

        public RuleCache()
        {
            this.cache =
                new Dictionary<IHasState, Dictionary<IHasState, bool>>(
                    CompareByReference.Instance);
        }

        public bool? Get(IHasState x, IHasState y)
        {
            Dictionary<IHasState, bool> result;
            if (this.cache.TryGetValue(x, out result) == false)
                return null;
            bool value;
            if (result.TryGetValue(y, out value) == false)
                return null;
            return value;
        }

        public void Store(IHasState x, IHasState y, bool value)
        {
            if (this.cache.ContainsKey(x) == false)
                this.cache[x] = new Dictionary<IHasState, bool>(
                    CompareByReference.Instance);
            this.cache[x].Add(y, value);
        }
    }

    private static Dictionary<RuleName, RuleCache> cacheRules = null;
    private static Dictionary<StateName[], IList<IHasState>> cacheStates = null;
    private static Dictionary<RelationName[], IList<Tuple<IHasState, IHasState>>> cacheRelations = null;

    private static int GetScopeCheckSum(IEnumerable<IHasState> scope)
    {
        int result = 0;
        foreach (IHasState obj in scope)
            result += obj.State.GetChecksum();
        return result;
    }

    public static void ClearCache()
    {
        if (cacheRules != null)
            cacheRules.Clear();
        if (cacheStates != null)
            cacheStates.Clear();
        if (cacheRelations != null)
            cacheRelations.Clear();
        DEBUG_scopeChecksum = null;
    }

    private static bool? CheckCache(RuleName name, IHasState x, IHasState y)
    {
        RuleCache cache;
        if (cacheRules == null)
            return null;
        if (cacheRules.TryGetValue(name, out cache) == false)
            return null;
        return cache.Get(x, y);
    }

    private static IList<IHasState> CheckCache(StateName[] states)
    {
        if (cacheStates == null)
            return null;
        IList<IHasState> results;
        if (cacheStates.TryGetValue(states, out results) == false)
            return null;
        return results;
    }

    private static IList<Tuple<IHasState, IHasState>> CheckCache(RelationName[] states)
    {
        if (cacheRelations == null)
            return null;
        IList<Tuple<IHasState, IHasState>> results;
        if (cacheRelations.TryGetValue(states, out results) == false)
            return null;
        return results;
    }

    private static void StoreCache(
        RuleName rule,
        IHasState x,
        IHasState y,
        bool value)
    {
        if (cacheRules == null)
            cacheRules = new Dictionary<RuleName, RuleCache>();
        if (cacheRules.ContainsKey(rule) == false)
            cacheRules[rule] = new RuleCache();
        cacheRules[rule].Store(x, y, value);
    }

    private static void StoreCache(
        StateName[] states, 
        IList<IHasState> objs)
    {
        if (cacheStates == null)
            cacheStates = 
                new Dictionary<StateName[], IList<IHasState>>();
        cacheStates[states] = objs;
    }

    private static void StoreCache(
        RelationName[] relations,
        IList<Tuple<IHasState, IHasState>> objs)
    {
        if (cacheRelations == null)
            cacheRelations =
                new Dictionary<
                    RelationName[], 
                    IList<Tuple<IHasState, IHasState>>>();
        cacheRelations[relations] = objs;
    }

    internal static IEnumerable<bool> UnifyAllWithState(
        IEnumerable<IHasState> scope,
        object X,
        params StateName[] states)
    {
        IList<IHasState> results = CheckCache(states);

        if (results == null)
        {
            results = new List<IHasState>();
            foreach (IHasState obj in scope)
                if (obj.Require(states) == true)
                    results.Add(obj);
            StoreCache(states, results);
        }

        foreach (IHasState obj in results)
            foreach (bool l1 in YP.Unify(X, obj))
                yield return false;
    }

    internal static IEnumerable<bool> UnifyAllRelationPairs(
        IEnumerable<IHasState> scope,
        object X,
        object Y,
        params RelationName[] relations)
    {
        IList<Tuple<IHasState, IHasState>> results = CheckCache(relations);

        if (results == null)
        {
            results = new List<Tuple<IHasState, IHasState>>();
            foreach (IHasState obj1 in scope)
                foreach (IHasState obj2 in scope)
                    if (obj1.Require(obj2.Id, relations) == true)
                        results.Add(new Tuple<IHasState, IHasState>(obj1, obj2));
            StoreCache(relations, results);
        }

        foreach (var pair in results)
            foreach (bool l1 in YP.Unify(X, pair.Item1))
                foreach (bool l2 in YP.Unify(Y, pair.Item2))
                    yield return false;
    }

    internal static StateFunction State(
        params StateName[] states)
    {
        return (scope, X) => UnifyAllWithState(scope, X, states);
    }

    internal static RelationFunction Relation(
        params RelationName[] relations)
    {
        return (scope, X, Y) => UnifyAllRelationPairs(scope, X, Y, relations);
    }

    public static bool IsRuleSatisfied(
        IEnumerable<IHasState> scope,
        RuleName rule,
        IHasState X,
        IHasState Y)
    {
        int checksum = GetScopeCheckSum(scope);
        if (DEBUG_scopeChecksum.HasValue == true)
            if (checksum != DEBUG_scopeChecksum.Value)
                Debug.LogError("Scope checksum mismatch with cached checksum");
        DEBUG_scopeChecksum = checksum;

        bool? cached = CheckCache(rule, X, Y);
        if (cached.HasValue == true)
            return cached.Value;

        bool evaluated = EvaluateRule(scope, rule, X, Y);
        StoreCache(rule, X, Y, evaluated);
        return evaluated;
    }

    private static bool EvaluateRule(
        IEnumerable<IHasState> scope,
        RuleName rule,
        IHasState X,
        IHasState Y)
    {
        foreach (bool l2 in RuleDefs.RuleFunc(rule)(scope, X, Y))
            return true;
        return false;
    }

    /// <summary>
    /// Filters the scope by returning objects that match ANY of the given
    /// states. Note that given states can be combined with bitwise OR before
    /// being passed to this function. Does direct bit comparison.
    /// </summary>
    public static IEnumerable<IHasState> FilterScope(
        IEnumerable<IHasState> scope,
        params StateName[] states)
    {
        foreach (IHasState obj in scope)
        {
            foreach (StateName state in states)
            {
                long stateLong = (long)state;
                if ((obj.State.GetStateBits() & stateLong) == stateLong)
                {
                    yield return obj;
                    // Continue the outer loop so we don't return the same 
                    // object twice
                    goto nextObj;
                }
            }
            nextObj: continue;
        }
    }

    private static IEnumerable<Tuple<IHasState, IHasState>> GetRuleSatisfiers(
        IEnumerable<IHasState> scope,
        RuleName rule)
    {
        Variable X = new Variable();
        Variable Y = new Variable();
        foreach (bool l2 in RuleDefs.RuleFunc(rule)(scope, X, Y))
            yield return new Tuple<IHasState, IHasState>(
                X.GetValue<IHasState>(),
                Y.GetValue<IHasState>());
    }

    private static IEnumerable<IHasState> GetRHSSatisfiers(
        IEnumerable<IHasState> scope,
        RuleName rule,
        IHasState X)
    {
        Variable Y = new Variable();
        foreach (bool l2 in RuleDefs.RuleFunc(rule)(scope, X, Y))
            yield return Y.GetValue<IHasState>();
    }

    private static IEnumerable<IHasState> GetLHSSatisfiers(
        IEnumerable<IHasState> scope,
        RuleName rule,
        IHasState Y)
    {
        Variable X = new Variable();
        foreach (bool l2 in RuleDefs.RuleFunc(rule)(scope, X, Y))
            yield return X.GetValue<IHasState>();
    }
}
#pragma warning restore 0219