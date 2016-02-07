// USED IN CRUNCH
// DO NOT MOVE THIS FILE

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

using YieldProlog;

#pragma warning disable 0219
public enum RuleName
{
    CanAccessZone,
    CanAccessObject,
    CanManipulateObject,
}

public static class RuleDefs
{
    internal static RuleFunction RuleFunc(RuleName rule)
    {
        switch(rule)
        {
            case RuleName.CanAccessZone:
                return Rule_CanAccessZone;
            case RuleName.CanAccessObject:
                return Rule_CanAccessObject;
            case RuleName.CanManipulateObject:
                return Rule_CanManipulateObject;
            default: 
                return null;
        }
    }

    private static IEnumerable<bool> Rule_CanAccessObject(
        IEnumerable<IHasState> scope,
        object X,
        object Y)
    {
        return __PROLOG_can_access_object(scope, X, Y);
    }

    private static IEnumerable<bool> Rule_CanAccessZone(
        IEnumerable<IHasState> scope,
        object X,
        object Y)
    {
        return __PROLOG_can_access_zone(scope, X, Y);
    }

    private static IEnumerable<bool> Rule_CanManipulateObject(
        IEnumerable<IHasState> scope,
        object X,
        object Y)
    {
        return __PROLOG_can_manipulate_object(scope, X, Y);
    }

    private static StateFunction STA_IsUnlocked = Rules.State(StateName.IsUnlocked);
    private static StateFunction STA_IsGuardable_IsUnguarded = Rules.State(StateName.IsGuardable, ~StateName.IsGuarded);
    private static StateFunction STA_IsUnlocked_IsUnguarded = Rules.State(StateName.IsUnlocked, ~StateName.IsGuarded);

    private static RelationFunction REL_IsInZone = Rules.Relation(RelationName.IsInZone);
    private static RelationFunction REL_IsAdjacentTo = Rules.Relation(RelationName.IsAdjacentTo);
    private static RelationFunction REL_IsGuarding = Rules.Relation(RelationName.IsGuarding);
    private static RelationFunction REL_IsAlliedWith = Rules.Relation(RelationName.IsAlliedWith);

    // PROLOG:
    //
    //     can_access_object(O, T) :-
    //         rel_isinzone(T, Z),
    //         can_access_zone(O, Z).
    //
    private static IEnumerable<bool> __PROLOG_can_access_object(
        IEnumerable<IHasState> scope,
        object O,
        object T)
    {
        Variable Z = new Variable();
        foreach (bool l4 in REL_IsInZone(scope, T, Z))
            foreach (bool l6 in __PROLOG_can_access_zone(scope, O, Z))
                yield return false;
    }

    // PROLOG:
    //
    //     can_access_zone(O, T) :-
    //         rel_isinzone(O, C),
    //         path_exists_for_object(C, T, O).
    //
    private static IEnumerable<bool> __PROLOG_can_access_zone(
        IEnumerable<IHasState> scope,
        object O,
        object T)
    {
        Variable C = new Variable();
        foreach (bool l2 in REL_IsInZone(scope, O, C))
            foreach (bool l3 in __PROLOG_path_exists_for_object(scope, C, T, O))
                yield return false;
    }

    // PROLOG:
    //
    //     can_manipulate_object(U, O) :-
    //         % Use isguardable for efficiency
    //         sta_isguardable(O), sta_isunguarded(O).
    //
    //     can_manipulate_object(U, O) :-
    //         rel_isguarding(G, O),
    //         rel_isalliedwith(G, U).
    //
    private static IEnumerable<bool> __PROLOG_can_manipulate_object(
        IEnumerable<IHasState> scope,
        object U,
        object O)
    {
        {
            foreach (bool l2 in STA_IsGuardable_IsUnguarded(scope, O))
                yield return false;
        }
        {
            Variable G = new Variable();
            foreach (bool l2 in REL_IsGuarding(scope, G, O))
                foreach (bool l3 in REL_IsAlliedWith(scope, G, U))
                    yield return false;
        }
    }

    // See: http://stackoverflow.com/questions/23539327/transitive-closure-over-a-symmetric-relation-in-prolog
    // Do note the humor in linking to stackoverflow in a body of code that is
    // itself very likely to produce a real stack overflow.
    //
    // PROLOG:
    //
    //     path_exists_for_object(X, Y, O) :- 
    //         path_exists_for_object_r(X, Y, O, []).
    //
    private static IEnumerable<bool> __PROLOG_path_exists_for_object(
        IEnumerable<IHasState> scope,
        object X,
        object Y,
        object O)
    {
        foreach (bool l2 in __PROLOG_path_exists_for_object_r(scope, X, Y, O, Atom.NIL))
            yield return false;
    }

    // PROLOG:
    //
    //     path_exists_for_object_r(X, Y, _) :-
    //         zoneslinked(X, Y).
    //
    //     path_exists_for_object_r(X, Y, L) :-
    //         zoneslinked(X, Z),
    //         \+ member(Z, L),
    //         pathexists_r(Z, Y, [Z | L]).
    //
    private static IEnumerable<bool> __PROLOG_path_exists_for_object_r(
        IEnumerable<IHasState> scope,
        object X,
        object Y,
        object O,
        object L)
    {
        foreach (bool l2 in __PROLOG_zones_linked_for_object(scope, X, Y, O))
            yield return false;

        Variable Z = new Variable();
        foreach (bool l2 in __PROLOG_zones_linked_for_object(scope, X, Z, O))
        {
            foreach (bool l3 in ListPair.member(L, Z))
                goto _PROLOG_path_exists_for_object_r;
            foreach (bool l3 in __PROLOG_path_exists_for_object_r(scope, Z, Y, O, new ListPair(Z, L)))
                yield return false;
            _PROLOG_path_exists_for_object_r: continue;
        }
    }

    // PROLOG:
    //
    //     zones_linked_for_object(Z1, Z2, _) :-
    //         Z1 == Z2.
    //
    //     zones_linked_for_object(Z1, Z2, _) :-
    //         rel_isadjacentto(D, Z1),
    //         rel_isadjacentto(D, Z2),
    //         sta_isunlocked(D), sta_isunguarded(D).
    //
    //     zones_linked_for_object(Z1, Z2, O) :-
    //         rel_isadjacentto(D, Z1),
    //         rel_isadjacentto(D, Z2),
    //         sta_isunlocked(D),
    //         rel_isguarding(G, D),
    //         rel_isalledwith(G, O).
    //
    private static IEnumerable<bool> __PROLOG_zones_linked_for_object(
        IEnumerable<IHasState> scope,
        object Z1,
        object Z2,
        object O)
    {
        {
            if (YP.TermEqual(Z1, Z2))
                yield return false;
        }
        {
            Variable D = new Variable();
            foreach (bool l2 in REL_IsAdjacentTo(scope, D, Z1))
                foreach (bool l3 in REL_IsAdjacentTo(scope, D, Z2))
                    foreach (bool l4 in STA_IsUnlocked_IsUnguarded(scope, D))
                        yield return false;
        }
        {
            Variable D = new Variable();
            Variable G = new Variable();
            foreach (bool l2 in REL_IsAdjacentTo(scope, D, Z1))
                foreach (bool l3 in REL_IsAdjacentTo(scope, D, Z2))
                    foreach (bool l4 in STA_IsUnlocked(scope, D))
                        foreach (bool l5 in REL_IsGuarding(scope, G, D))
                            foreach (bool l6 in REL_IsAlliedWith(scope, G, O))
                                yield return false;
        }
    }
}
#pragma warning restore 0219