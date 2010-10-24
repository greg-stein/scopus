using System.Collections.Generic;

namespace Scopus.LexicalAnalysis
{
    internal class State
    {
        internal readonly Dictionary<InputChar, List<State>> Transitions =
            new Dictionary<InputChar, List<State>>();

        internal State(string name)
        {
            Name = name;
            IsAccepting = false;
        }

        internal string Name { get; private set; }
        internal bool IsAccepting { get; set; }
        internal int TokenClass { get; set; }
        internal int Id { get; set; }

        /// <summary>
        /// Adds transition to given state on input char
        /// </summary>
        /// <param name="state">Target state for transition</param>
        /// <param name="iChar">Input char for transition</param>
        internal void AddTransitionTo(State state, InputChar iChar)
        {
            List<State> states;

            if (Transitions.TryGetValue(iChar, out states))
            {
                states.Add(state);
            }
            else
            {
                Transitions[iChar] = new List<State> {state};
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}