using System;
using System.Collections.Generic;
using Scopus.Auxiliary;

namespace Scopus.LexicalAnalysis
{
    [Serializable]
    public class FiniteAutomata : ICloneable
    {
        internal FiniteAutomata(string name, bool createMarginalStates)
        {
            if (createMarginalStates)
            {
                StartState = new State(name + "::StartState");
                Terminator = new State(name + "::Terminator");
            }

            Name = name;
        }

        internal FiniteAutomata()
            : this("Default", true)
        {
        }

        internal FiniteAutomata(string name)
            : this(name, true)
        {
        }

        internal string Name { get; private set; }
        internal State StartState { get; set; }
        internal State Terminator { get; set; }

        internal HashSet<State> GetStates()
        {
            var states = new HashSet<State>();
            var queue = new Queue<State>();
            queue.Enqueue(StartState);

            while (queue.Count > 0)
            {
                State s = queue.Dequeue();
                foreach (var transition in s.Transitions)
                {
                    foreach (State state in transition.Value)
                    {
                       if (!states.Contains(s)) queue.Enqueue(state);
                    }
                }

                if (!states.Contains(s))
                {
                    states.Add(s);
                }
            }

            return states;
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// This does deep clone!
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return CommonRoutines.DeepCopy(this);
        }
    }
}