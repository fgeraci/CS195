/*
 * Copyright (C) 2007-2008, Jeff Thompson
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are met:
 * 
 *     * Redistributions of source code must retain the above copyright 
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright 
 *       notice, this list of conditions and the following disclaimer in the 
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the copyright holder nor the names of its contributors 
 *       may be used to endorse or promote products derived from this software 
 *       without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YieldProlog
{
    /// <summary>
    /// YP has static methods for general functions in Yield Prolog such as <see cref="GetValue"/>
    /// and <see cref="Unify"/>.
    /// </summary>
    public class YP
    {
        private static Failure fail = new Failure();
        private static Dictionary<NameArity, List<IClause>> predicatesStore =
            new Dictionary<NameArity, List<IClause>>();
        private static TextWriter outputStream = System.Console.Out;
        private static TextReader inputStream = System.Console.In;
        private static Dictionary<string, object> prologFlags = 
            new Dictionary<string, object>();
        public const int MAX_ARITY = 255;

        /// <summary>
        /// An IClause is used so that dynamic predicates can call match.
        /// </summary>
        public interface IClause
        {
            IEnumerable<bool> Match(object[] args);
            IEnumerable<bool> Clause(object Head, object Body);
        }

        /// <summary>
        /// If value is a Variable, then return its getValue.  Otherwise, just
        /// return value.  You should call YP.getValue on any object that
        /// may be a Variable to get the value to pass to other functions in
        /// your system that are not part of Yield Prolog, such as math functions
        /// or file I/O.
        /// For more details, see http://yieldprolog.sourceforge.net/tutorial1.html
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object GetValue(object value)
        {
            if (value is Variable)
                return ((Variable)value).GetValue();
            else
                return value;
        }

        /// <summary>
        /// A typed version of GetValue()
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T GetValue<T>(object value)
        {
            if (value is Variable)
                return (T)((Variable)value).GetValue();
            else
                return (T)value;
        }

        /// <summary>
        /// If arg1 or arg2 is an object with a unify method (such as Variable or
        /// Functor) then just call its unify with the other argument.  The object's
        /// unify method will bind the values or check for equals as needed.
        /// Otherwise, both arguments are "normal" (atomic) values so if they
        /// are equal then succeed (yield once), else fail (don't yield).
        /// For more details, see http://yieldprolog.sourceforge.net/tutorial1.html
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public static IEnumerable<bool> Unify(object arg1, object arg2)
        {
            arg1 = GetValue(arg1);
            arg2 = GetValue(arg2);
            if (arg1 is IUnifiable)
                return ((IUnifiable)arg1).Unify(arg2);
            else if (arg2 is IUnifiable)
                return ((IUnifiable)arg2).Unify(arg1);
            else
            {
                // Arguments are "normal" types.
                if (arg1.Equals(arg2))
                    return new Success();
                else
                    return fail;
            }
        }

        /// <summary>
        /// This is used for the lookup key in _predicatesStore.
        /// </summary>
        public struct NameArity
        {
            public readonly Atom _name;
            public readonly int _arity;

            public NameArity(Atom name, int arity)
            {
                _name = name;
                _arity = arity;
            }

            public override bool Equals(object obj)
            {
                if (obj is NameArity)
                {
                    NameArity nameArity = (NameArity)obj;
                    return nameArity._name.Equals(_name) && nameArity._arity.Equals(_arity);
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return _name.GetHashCode() ^ _arity;
            }
        }

        public static IEnumerable<bool> CopyTerm(object inTerm, object outTerm)
        {
            return YP.Unify(outTerm, YP.MakeCopy(inTerm, new Variable.CopyStore()));
        }

        public static void AddUniqueVariables(object term, List<Variable> variableSet)
        {
            term = YP.GetValue(term);
            if (term is IUnifiable)
                ((IUnifiable)term).AddUniqueVariables(variableSet);
        }

        public static object MakeCopy(object term, Variable.CopyStore copyStore)
        {
            term = YP.GetValue(term);
            if (term is IUnifiable)
                return ((IUnifiable)term).MakeCopy(copyStore);
            else
                // term is a "normal" type. Assume it is ground.
                return term;
        }

        /// <summary>
        /// Return an iterator (which you can use in a for-in loop) which does
        /// zero iterations.  This returns a pre-existing iterator which is
        /// more efficient than letting the compiler generate a new one.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<bool> Fail()
        {
            return fail;
        }

        /// <summary>
        /// Return an iterator (which you can use in a for-in loop) which does
        /// one iteration.  This returns a pre-existing iterator which is
        /// more efficient than letting the compiler generate a new one.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<bool> Succeed()
        {
            return new Success();
        }

        public static bool TermEqual(object Term1, object Term2)
        {
            Term1 = YP.GetValue(Term1);
            if (Term1 is IUnifiable)
                return ((IUnifiable)Term1).TermEqual(Term2);
            return Term1.Equals(YP.GetValue(Term2));
        }

        public static bool Ground(object Term)
        {
            Term = YP.GetValue(Term);
            if (Term is IUnifiable)
                return ((IUnifiable)Term).Ground();
            return true;
        }

        public static bool IsAtom(object Term)
        {
            return YP.GetValue(Term) is Atom;
        }

        /// <summary>
        /// Format x as a string, making sure that it won't parse as an int later.  I.e., for 1.0, don't just
        /// use "1" which will parse as an int.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static string DoubleToString(double x)
        {
            string xString = x.ToString();
            // Debug: Is there a way in C# to ask if a string parses as int without throwing an exception?
            try
            {
                Convert.ToInt32(xString);
                // The string will parse as an int, not a double, so re-format so that it does.
                // Use float if possible, else exponential if it would be too big.
                return x.ToString(x >= 100000.0 ? "E1" : "f1");
            }
            catch (FormatException)
            {
                // Assume it will parse as a double.
            }
            return xString;
        }

        /// <summary>
        /// Match all clauses of the dynamic predicate with the name and with arity
        /// arguments.Length.
        /// If the predicate is not defined, return the result of YP.unknownPredicate.
        /// </summary>
        /// <param name="name">must be an Atom</param>
        /// <param name="arguments">an array of arity number of arguments</param>
        /// <returns>an iterator which you can use in foreach</returns>
        public static IEnumerable<bool> MatchDynamic(
            Atom name, 
            object[] arguments)
        {
            List<IClause> clauses;
            if (!predicatesStore.TryGetValue(new NameArity(name, arguments.Length), out clauses))
                return UnknownPredicate(name, arguments.Length,
                     "Undefined dynamic predicate: " + name + "/" + arguments.Length);

            if (clauses.Count == 1)
                // Usually there is only one clause, so return it without needing to wrap it in an iterator.
                return clauses[0].Match(arguments);
            else
                return MatchAllClauses(clauses, arguments);
        }

        /// <summary>
        /// Call match(arguments) for each IClause in clauses.  We make this a separate
        /// function so that matchDynamic itself does not need to be an iterator object.
        /// </summary>
        /// <param name="clauses"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static IEnumerable<bool> MatchAllClauses(
            List<IClause> clauses, 
            object[] arguments)
        {
            // Debug: If the caller asserts another clause into this same predicate during yield, the iterator
            //   over clauses will be corrupted.  Should we take the time to copy clauses?
            foreach (IClause clause in clauses)
            {
                foreach (bool lastCall in clause.Match(arguments))
                {
                    yield return false;
                    if (lastCall)
                        // This happens after a cut in a clause.
                        yield break;
                }
            }
        }

        /// <summary>
        /// If _prologFlags["unknown"] is fail then return fail(), else if 
        ///   _prologFlags["unknown"] is warning then write the message to YP.write and
        ///   return fail(), else throw a PrologException for existence_error.  .
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arity"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IEnumerable<bool> UnknownPredicate(
            Atom name, 
            int arity, 
            string message)
        {
            EstablishPrologFlags();

            if (prologFlags["unknown"] == Atom.a("fail"))
                return Fail();
            else if (prologFlags["unknown"] == Atom.a("warning"))
            {
                // TODO: Debug output here, maybe?
                return Fail();
            }
            else
                throw new PrologException
                    (new Functor2
                     (Atom.a("existence_error"), Atom.a("procedure"),
                      new Functor2(Atom.SLASH, name, arity)), message);
        }

        /// <summary>
        /// This must be called by any function that uses YP._prologFlags to make sure
        /// the initial defaults are loaded.
        /// </summary>
        private static void EstablishPrologFlags()
        {
            if (prologFlags.Count > 0)
                // Already established.
                return;

            // List these in the order they appear in the ISO standard.
            prologFlags["bounded"] = Atom.TRUE;
            prologFlags["max_integer"] = Int32.MaxValue;
            prologFlags["min_integer"] = Int32.MinValue;
            prologFlags["integer_rounding_function"] = Atom.a("toward_zero");
            prologFlags["char_conversion"] = Atom.a("off");
            prologFlags["debug"] = Atom.a("off");
            prologFlags["max_arity"] = MAX_ARITY;
            prologFlags["unknown"] = Atom.a("error");
            prologFlags["double_quotes"] = Atom.a("codes");
        }

        /// <summary>
        /// An enumerator that does zero loops.
        /// </summary>
        private class Failure : IEnumerator<bool>, IEnumerable<bool>
        {
            public bool MoveNext()
            {
                return false;
            }

            public IEnumerator<bool> GetEnumerator()
            {
                return (IEnumerator<bool>)this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public bool Current
            {
                get { return true; }
            }

            object IEnumerator.Current
            {
                get { return true; }
            }

            public void Dispose()
            {
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// An enumerator that does one iteration.
        /// </summary>
        private class Success : IEnumerator<bool>, IEnumerable<bool>
        {
            private bool _didIteration = false;

            public bool MoveNext()
            {
                if (!_didIteration)
                {
                    _didIteration = true;
                    return true;
                }
                else
                    return false;
            }

            public IEnumerator<bool> GetEnumerator()
            {
                return (IEnumerator<bool>)this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public bool Current
            {
                get { return false; }
            }

            object IEnumerator.Current
            {
                get { return false; }
            }

            public void Dispose()
            {
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
