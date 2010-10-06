﻿using System.Collections.Generic;

namespace Scopus.LexicalAnalysis
{
    public class FiniteAutomata
    {
        internal string Name { get; private set; }

        internal State StartState { get; set; }
        internal State Terminator { get; set; }

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
        {}

        internal FiniteAutomata(string name)
            :this(name, true)
        {}

        internal HashSet<State> GetStates()
        {
            var states = new HashSet<State>();
            var queue = new Queue<State>();
            queue.Enqueue(StartState);

            while (queue.Count > 0)
            {
                var s = queue.Dequeue();
                foreach (var transition in s.Transitions)
                {
                    foreach (var state in transition.Value)
                    {
                        queue.Enqueue(state);
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
    }
}