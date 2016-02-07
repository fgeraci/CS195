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
using System.Collections.Generic;

namespace YieldProlog
{
    public class Functor1 : IUnifiable
    {
        public readonly Atom _name;
        public readonly object _arg1;

        public Functor1(Atom name, object arg1)
        {
            _name = name;
            _arg1 = arg1;
        }

        public Functor1(string name, object arg1)
            : this(Atom.a(name), arg1)
        {
        }

        /// <summary>
        /// If arg is another Functor1, then succeed (yield once) if this and arg have the
        /// same name and the functor args unify, otherwise fail (don't yield).
        /// If arg is a Variable, then call its unify to unify with this.
        /// Otherwise fail (don't yield).
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public IEnumerable<bool> Unify(object arg)
        {
            arg = YP.GetValue(arg);
            if (arg is Functor1)
            {
                Functor1 argFunctor = (Functor1)arg;
                if (_name.Equals(argFunctor._name))
                {
                    foreach (bool l1 in YP.Unify(_arg1, argFunctor._arg1))
                        yield return false;
                }
            }
            else if (arg is Variable)
            {
                foreach (bool l1 in ((Variable)arg).Unify(this))
                    yield return false;
            }
        }

        public override string ToString()
        {
            return _name + "(" + YP.GetValue(_arg1) + ")";
        }

        public bool TermEqual(object term)
        {
            term = YP.GetValue(term);
            if (term is Functor1)
            {
                Functor1 termFunctor = (Functor1)term;
                return _name.Equals(termFunctor._name) && YP.TermEqual(_arg1, termFunctor._arg1);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode() ^ YP.GetValue(_arg1).GetHashCode();
        }

        public bool Ground()
        {
            return YP.Ground(_arg1);
        }

        public void AddUniqueVariables(List<Variable> variableSet)
        {
            YP.AddUniqueVariables(_arg1, variableSet);
        }

        public object MakeCopy(Variable.CopyStore copyStore)
        {
            return new Functor1(_name, YP.MakeCopy(_arg1, copyStore));
        }
    }
}
