using System;
using System.Collections.Generic;

namespace Scopus.LexicalAnalysis
{
    [Serializable]
    internal class State
    {
        internal readonly Dictionary<InputChar, List<State>> Transitions =
            new Dictionary<InputChar, List<State>>();

        internal State(string name)
        {
            Name = name;
            IsAccepting = false;
        }

        internal string Name { get; private set; } // Name of the state for debug purposes
        internal bool IsAccepting { get; set; } // Indicates whether the state is accepting
        internal int TokenClass { get; set; } // Determines class of accepted token (in case IsAccepting==true)
        internal int Id { get; set; } // Id of state for TransitionFunctionImplementation

        /// <summary>
        /// Lexical action, performed when token is accepted. The return value is indicator that 
        /// determines whether to pass the token to parser or ignore it. The return value overrides
        /// UseTerminal() and IgnoreTerminal() methods of Lexer.
        /// </summary>
        // TODO: Support it in Tokenizer/TransitionFunction
        internal Func<Token, bool> LexicalAction { get; set; } 

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