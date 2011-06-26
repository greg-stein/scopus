using System;
using System.Collections.Generic;

namespace Scopus.LexicalAnalysis.Algorithms
{
    internal class CrossAutomata : FiniteAutomata
    {
        // This maps pair of DFA states to state of CrossAutomata
        private Dictionary<State, Dictionary<State, State>> mCrossStates;

        public CrossAutomata(FiniteAutomata dfaRepeated, FiniteAutomata dfaSuffix)
        {
            Construct(dfaRepeated, dfaSuffix);
        }

        // TODO: Think about:
        //  - Whether it's needed or not to remove all transitions from dfa2 accepting state (i.e. mCrossState[dfa1State][dfa2State.IsAccepting].RemoveAllransitions
        //  - What to do with trap states
        internal void Construct(FiniteAutomata dfa1, FiniteAutomata dfa2)
        {
            var dfa1TrapState = AddTrapState(dfa1);
            var dfa2TrapState = AddTrapState(dfa2);

            var dfa1States = dfa1.GetStates();
            var dfa2States = dfa2.GetStates();

            BuildCrossStates(dfa1States, dfa2States);
            StartState = mCrossStates[dfa1.StartState][dfa2.StartState];
            //Terminator = new State("cross-terminator");

            foreach (var dfa1SourceState in dfa1States)
            {
                foreach (var dfa2SourceState in dfa2States)
                {
                    for (int i = 0; i <= Byte.MaxValue; i++)
                    {
                        var inputChar = InputChar.For((byte)i);
                        var dfa1DestinationState = dfa1SourceState.Transitions[inputChar][0];
                        var dfa2DestinationState = dfa2SourceState.Transitions[inputChar][0];

                        // Source state in cross-automata
                        var sourceState = mCrossStates[dfa1SourceState][dfa2SourceState];
                        sourceState.IsAccepting = dfa2SourceState.IsAccepting;

                        // This checks if we need to add this transition at all
                        if ((dfa2DestinationState == dfa2TrapState && dfa1DestinationState == dfa1TrapState) || dfa2SourceState.IsAccepting)
                            continue;

                        if (dfa1DestinationState.IsAccepting)
                        {
                            // In case we reach accepting state of the repeated automata, go to start
                            dfa1DestinationState = dfa1.StartState;
                        }

                        if (dfa2DestinationState == dfa2TrapState && dfa1DestinationState != dfa1TrapState)
                        {
                            // In case there is no transition in suffix automata, but there is one in repeated
                            // automata, go to start in suffix automata
                            dfa2DestinationState = dfa2.StartState;
                        }

                        var destinationState = mCrossStates[dfa1DestinationState][dfa2DestinationState];
                        sourceState.AddTransitionTo(destinationState, inputChar);
                    }
                }
            }
        }

        private void BuildCrossStates(HashSet<State> dfa1States, HashSet<State> dfa2States)
        {
            mCrossStates = new Dictionary<State, Dictionary<State, State>>();

            foreach (var dfa1State in dfa1States)
            {
                foreach (var dfa2State in dfa2States)
                {
                    Dictionary<State, State> dict;
                    if (!mCrossStates.TryGetValue(dfa1State, out dict))
                    {
                        dict = mCrossStates[dfa1State] = new Dictionary<State, State>();
                    }
                    dict[dfa2State] = new State("cross state") {Id = dfa1State.Id * 100 + dfa2State.Id};
                }
            }
        }

        private State AddTrapState(FiniteAutomata dfa)
        {
            var trapState = new State("trap") {Id = int.MaxValue};
            for (int i = 0; i <= Byte.MaxValue; i++)
            {
                trapState.AddTransitionTo(trapState, InputChar.For((byte) i));
            }

            var states = dfa.GetStates();
            foreach (var state in states)
            {
                bool[] usedTransitions = new bool[Byte.MaxValue + 1]; // All nulls
                foreach (var transition in state.Transitions)
                {
                    usedTransitions[transition.Key.Value] = true; // mark used symbol
                }

                for (int i = 0; i <= Byte.MaxValue; i++)
                {
                    if (!usedTransitions[i])
                    {
                        state.AddTransitionTo(trapState, InputChar.For((byte)i));
                    }
                }
            }

            return trapState;
        }
    }
}
