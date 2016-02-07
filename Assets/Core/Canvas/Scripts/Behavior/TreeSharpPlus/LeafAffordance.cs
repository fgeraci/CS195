using System.Collections.Generic;
using System;
using UnityEngine;

namespace TreeSharpPlus
{
    public class LeafAffordance : Node
    {
        private SmartObject user;
        private SmartObject obj;
        private string affordance;

        public LeafAffordance(
            Val<string> affordance,
            Val<SmartObject> user, 
            Val<SmartObject> obj)
        {
            this.affordance = affordance.Value;
            this.user = user.Value;
            this.obj = obj.Value;
        }

        /// <summary>
        ///    Resets the wait timer
        /// </summary>
        /// <param name="context"></param>
        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override sealed IEnumerable<RunStatus> Execute()
        {
            while (true)
            {
                RunStatus result = 
                    this.obj.RunAffordance(this.affordance, this.user);
                if (result != RunStatus.Running)
                {
                    yield return result;
                    yield break;
                }
                yield return RunStatus.Running;
            }
        }
    }
}