using System.Collections.Generic;

namespace Scopus.LexicalAnalysis
{
    internal class State
    {
        internal readonly Dictionary<InputChar, List<State>> Transition = 
            new Dictionary<InputChar, List<State>>();

        internal string Name { get; private set;}

        internal State(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Adds transition to given state on input char
        /// </summary>
        /// <param name="state">Target state for transition</param>
        /// <param name="iChar">Input char for transition</param>
        internal void AddTransitionTo(State state, InputChar iChar)
        {
            List<State> states;

            if (Transition.TryGetValue(iChar, out states))
            {
                states.Add(state);
            }
            else
            {
                Transition[iChar] = new List<State>() {state};
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}