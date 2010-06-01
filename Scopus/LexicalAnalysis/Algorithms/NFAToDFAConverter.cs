using System;
using System.Collections.Generic;

namespace Scopus.LexicalAnalysis.Algorithms
{
    /// <summary>
    /// Converts Nondeterministic Final Automata to Deterministic Final Automata using algorithm described in Dragon Book.
    /// </summary>
    internal class NFAToDFAConverter
    {

        /// <summary>
        /// Converts NFA to DFA
        /// </summary>
        /// <param name="nfa">Finite automata representing NFA</param>
        /// <returns>Target DFA</returns>
        internal static FiniteAutomata Convert(FiniteAutomata nfa)
        {
            var marked = new Dictionary<State, bool>();
            var dStates = new Dictionary<State, HashSet<State>>();

            var startState = new State("DFA start");
            dStates[startState] = EpsilonClosure(nfa.StartState);

            State tState;
            while (TryGetUnmarkedDState(dStates, marked, out tState))
            {
                marked[tState] = true;

                for (var a = char.MinValue; a < char.MaxValue; a++)
                {
                    var u = EpsilonClosure(Move(dStates[tState], InputChar.For(a)));
                    
                    if (u.Count <= 0) continue;

                    State uState = null;
                    var stateKvp = GetStatesSet(dStates, u);
                    if (stateKvp == null)
                    {
                        uState = new State("DFA state");
                        dStates[uState] = u;
                        marked[uState] = false;
                    }
                    else
                    {
                        uState = ((KeyValuePair<State, HashSet<State>>)stateKvp).Key;
                    }
                    tState.AddTransitionTo(uState, InputChar.For(a));
                }
            }

            DefineAcceptingStates(dStates);
            var dfa = new FiniteAutomata("DFA");
            dfa.StartState = startState;

            return dfa;
        }

        private static void DefineAcceptingStates(Dictionary<State, HashSet<State>> dStates)
        {
            foreach (var kvp in dStates)
            {
                foreach (State state in kvp.Value)
                {
                    if (state.IsAccepting)
                    {
                        kvp.Key.IsAccepting = true;
                        break;
                    }
                }
            }
        }

        private static KeyValuePair<State, HashSet<State>>? GetStatesSet(Dictionary<State, HashSet<State>> set, HashSet<State> state)
        {
            foreach (var kvp in set)
            {
                if (kvp.Value.IsSubsetOf(state) && state.IsSubsetOf(kvp.Value))
                    return kvp;
            }

            return null;
        }

        private static bool TryGetUnmarkedDState(Dictionary<State, HashSet<State>> dStates, Dictionary<State, bool> marked, out State unmarkedState)
        {
            unmarkedState = null;

            foreach (var dState in dStates)
            {
                bool isMarked;
                marked.TryGetValue(dState.Key, out isMarked);
                if ( ! isMarked )
                {
                    unmarkedState = dState.Key;
                    return true;
                }
            }

            return false;
        }

        private static HashSet<State> EpsilonClosure(State state)
        {
            var set = new HashSet<State>() {state};
            return EpsilonClosure(set);
        }
        
        private static HashSet<State> EpsilonClosure(HashSet<State> states)
        {
            var stack = new Stack<State>(states);
            var closure = new HashSet<State>(states);

            // Utilize BFS-like algorithm
            while (stack.Count > 0)
            {
                var topState = stack.Pop();
                List<State> epsilonTransitions;
                if (topState.Transitions.TryGetValue(InputChar.Epsilon(), out epsilonTransitions))
                {
                    foreach (State state in epsilonTransitions)
                    {
                        if (!closure.Contains(state))
                        {
                            closure.Add(state);
                            stack.Push(state);
                        }
                    }
                }
            }

            return closure;
        }

        private static HashSet<State> Move(HashSet<State> states, InputChar a)
        {
            var move = new HashSet<State>();

            foreach (State state in states)
            {
                List<State> transitions;
                if (state.Transitions.TryGetValue(a, out transitions) && transitions != null)
                {
                    move.UnionWith(transitions);
                }
            }

            return move;
        }
    }
}
