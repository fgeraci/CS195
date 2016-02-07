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

namespace YieldProlog
{
    public interface IUnifiable
    {
        IEnumerable<bool> Unify(object arg);
        void AddUniqueVariables(List<Variable> variableSet);
        object MakeCopy(Variable.CopyStore copyStore);
        bool TermEqual(object term);
        bool Ground();
    }

    /// <summary>
    /// A Variable is passed to a function so that it can be unified with
    /// value or another Variable. See getValue and unify for details.
    /// </summary>
    public class Variable : IUnifiable
    {
        // Use _isBound separate from _value so that it can be bound to any value,
        //   including null.
        private bool _isBound = false;
        private object _value;

        public object Value { get { return this.GetValue(); } }

        /// <summary>
        /// Shortcut to cast a GetValue() call.
        /// </summary>
        public T GetValue<T>()
        {
            return (T)this.GetValue();
        }

        /// <summary>
        /// If this Variable is unbound, then just return this Variable.
        /// Otherwise, if this has been bound to a value with unify, return the value.
        /// If the bound value is another Variable, this follows the "variable chain"
        /// to the end and returns the final value, or the final Variable if it is unbound.
        /// For more details, see http://yieldprolog.sourceforge.net/tutorial1.html
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            if (!_isBound)
                return this;

            object result = _value;
            while (result is Variable)
            {
                if (!((Variable)result)._isBound)
                    return result;

                // Keep following the Variable chain.
                result = ((Variable)result)._value;
            }

            return result;
        }

        /// <summary>
        /// If this Variable is bound, then just call YP.unify to unify this with arg.
        /// (Note that if arg is an unbound Variable, then YP.unify will bind it to
        /// this Variable's value.)
        /// Otherwise, bind this Variable to YP.getValue(arg) and yield once.  After the
        /// yield, return this Variable to the unbound state.
        /// For more details, see http://yieldprolog.sourceforge.net/tutorial1.html
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public IEnumerable<bool> Unify(object arg)
        {
            if (!_isBound)
            {
                _value = YP.GetValue(arg);
                if (_value == this)
                    // We are unifying this unbound variable with itself, so leave it unbound.
                    yield return false;
                else
                {
                    _isBound = true;
                    try
                    {
                        yield return false;
                    }
                    finally
                    {
                        // Remove the binding.
                        _isBound = false;
                    }
                }
            }
            else
            {
                foreach (bool l1 in YP.Unify(this, arg))
                    yield return false;
            }
        }

        public override string ToString()
        {
            object value = this.GetValue();
            if (value == this)
                return "_Variable";
            else
                return this.GetValue().ToString();
        }

        /// <summary>
        /// If bound, call YP.addUniqueVariables on the value.  Otherwise, if this unbound
        /// variable is not already in variableSet, add it.
        /// </summary>
        /// <param name="variableSet"></param>
        public void AddUniqueVariables(List<Variable> variableSet)
        {
            if (_isBound)
            {
                YP.AddUniqueVariables(this.GetValue(), variableSet);
            }
            else
            {
                if (variableSet.IndexOf(this) < 0)
                    variableSet.Add(this);
            }
        }

        /// <summary>
        /// If bound, return YP.makeCopy for the value, else return copyStore.getCopy(this).
        /// However, if copyStore is null, just return this.
        /// </summary>
        /// <param name="copyStore"></param>
        /// <returns></returns>
        public object MakeCopy(Variable.CopyStore copyStore)
        {
            if (_isBound)
                return YP.MakeCopy(this.GetValue(), copyStore);
            else
                return copyStore == null ? this : copyStore.getCopy(this);
        }

        public bool TermEqual(object term)
        {
            if (_isBound)
                return YP.TermEqual(this.GetValue(), term);
            else
                return this == YP.GetValue(term);
        }

        public override int GetHashCode()
        {
            if (_isBound)
                return this.GetValue().GetHashCode();
            else
                // Treat each unbound Variable as a separate object.
                return base.GetHashCode();
        }

        public bool Ground()
        {
            if (_isBound)
                // This is usually called by YP.ground which already did getValue, so this
                //   should never be reached, but check anyway.
                return YP.Ground(this.GetValue());
            else
                return false;
        }

        /// <summary>
        /// A CopyStore is used by makeCopy to track which Variable objects have
        /// been copied.
        /// </summary>
        public class CopyStore
        {
            private List<Variable> _inVariableSet = new List<Variable>();
            private List<Variable> _outVariableSet = new List<Variable>();

            /// <summary>
            /// If inVariable has already been copied, return its copy. Otherwise,
            /// return a fresh Variable associated with inVariable.
            /// </summary>
            /// <param name="inVariable"></param>
            /// <returns></returns>
            public Variable getCopy(Variable inVariable)
            {
                int index = _inVariableSet.IndexOf(inVariable);
                if (index >= 0)
                    return _outVariableSet[index];
                else
                {
                    Variable outVariable = new Variable();
                    _inVariableSet.Add(inVariable);
                    _outVariableSet.Add(outVariable);
                    return outVariable;
                }
            }

            /// <summary>
            /// Return the number of unique variables that have been copied.
            /// </summary>
            /// <returns></returns>
            public int getNUniqueVariables()
            {
                return _inVariableSet.Count;
            }
        }
    }
}
