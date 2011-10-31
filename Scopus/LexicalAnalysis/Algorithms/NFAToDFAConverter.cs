using System;
using System.Collections.Generic;
using Scopus.Auxiliary;

namespace Scopus.LexicalAnalysis.Algorithms
{
    /// <summary>
    /// Converts Nondeterministic Final Automata to Deterministic Final Automata 
    /// using algorithm described in Dragon Book.
    /// </summary>
    internal static class NFAToDFAConverter
    {
        /// <summary>
        /// Converts NFA to DFA. The result DFA is not minimalized, hence it 
        /// probably will contain redundant states.
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

                for (int i = byte.MinValue; i <= byte.MaxValue; i++)
                {
                    byte a = (byte) i;

                    HashSet<State> u = EpsilonClosure(Move(dStates[tState], InputChar.For(a)));

                    if (u.Count <= 0) continue;

                    State uState;
                    KeyValuePair<State, HashSet<State>>? stateKvp = GetStatesSet(dStates, u);
                    if (stateKvp == null)
                    {
                        uState = new State("DFA state");
                        dStates[uState] = u;
                        marked[uState] = false;
                    }
                    else
                    {
                        uState = ((KeyValuePair<State, HashSet<State>>) stateKvp).Key;
                    }
                    tState.AddTransitionTo(uState, InputChar.For(a));
                }
            }

            DefineAcceptingStates(dStates);
            var dfa = new FiniteAutomata("DFA") {StartState = startState};

            AssignIdsToStates(dfa);
            return dfa;
        }

        private static void AssignIdsToStates(FiniteAutomata automata)
        {
            var idProvider = new IDProvider();

            var states = automata.GetStates();
            foreach (var state in states)
            {
                state.Id = idProvider.GetNext();
            }
        }

        /// <summary>
        /// Sets IsAccepting property to accepting states.
        /// </summary>
        /// <param name="dStates">States of DFA</param>
        private static void DefineAcceptingStates(Dictionary<State, HashSet<State>> dStates)
        {
            foreach (var kvp in dStates)
            {
                foreach (State state in kvp.Value)
                {
                    if (state.IsAccepting)
                    {
                        if (kvp.Key.IsAccepting)
                        {
                            kvp.Key.TokenClass = Math.Max(kvp.Key.TokenClass, state.TokenClass);
                        }
                        else
                        {
                            kvp.Key.IsAccepting = true;
                            kvp.Key.TokenClass = state.TokenClass;
                        }
                        // Go through all accepting states don't break on first
                        //break;
                    }
                }
            }
        }

        /// <summary>
        /// Finds set in given distionary that contains exactly the same values as given set. 
        /// </summary>
        /// <param name="set">Dictionary of sets where the search is performed</param>
        /// <param name="state">set-state to look for</param>
        /// <returns>Returns KeyValuePair containing</returns>
        private static KeyValuePair<State, HashSet<State>>? GetStatesSet(
            Dictionary<State, HashSet<State>> set, HashSet<State> state)
        {
            foreach (var kvp in set)
            {
                if (kvp.Value.IsSubsetOf(state) && state.IsSubsetOf(kvp.Value))
                    return kvp;
            }

            return null;
        }

        private static bool TryGetUnmarkedDState(Dictionary<State, HashSet<State>> dStates,
                                                 Dictionary<State, bool> marked, out State unmarkedState)
        {
            unmarkedState = null;

            foreach (var dState in dStates)
            {
                bool isMarked;
                marked.TryGetValue(dState.Key, out isMarked);
                if (! isMarked)
                {
                    unmarkedState = dState.Key;
                    return true;
                }
            }

            return false;
        }

        private static HashSet<State> EpsilonClosure(State state)
        {
            var set = new HashSet<State> {state};
            return EpsilonClosure(set);
        }

        private static HashSet<State> EpsilonClosure(HashSet<State> states)
        {
            var stack = new Stack<State>(states);
            var closure = new HashSet<State>(states);

            // Utilize BFS-like algorithm
            while (stack.Count > 0)
            {
                State topState = stack.Pop();
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