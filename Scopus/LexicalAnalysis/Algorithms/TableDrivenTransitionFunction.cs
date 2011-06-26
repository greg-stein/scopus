using System;
using System.Collections.Generic;
using Scopus.Auxiliary;
using Scopus.Exceptions;

namespace Scopus.LexicalAnalysis.Algorithms
{
    // TODO: Write this using two-dimensional array instead of List<int[]>
    internal class TableDrivenTransitionFunction : ITransitionFunction
    {
        private const int INVALID_STATE = -1;
        private const int INITIAL_STATE = 0;

        private const int BYTE_OPTIONS = Byte.MaxValue + 1;
        private readonly IDProvider mStateIdProvider = new IDProvider();
        private bool[] mAcceptingStates;
        private int[] mTokenIds;
        private List<int[]> mTransitionsTable;
        private Greediness[] mGreediness;

        #region ITransitionFunction Members

        public void Init(FiniteAutomata dfa)
        {
            HashSet<State> states = dfa.GetStates();

            mAcceptingStates = new bool[states.Count];
            mTokenIds = new int[states.Count];
            mGreediness = new Greediness[states.Count];
            mStateIdProvider.Reset();

            // First state should be start state of the DFA
            // It is important because it MUST get Id = 0;
            foreach (State state in states)
            {
                state.Id = mStateIdProvider.GetNext();
            }

            mTransitionsTable = new List<int[]>(states.Count);

            foreach (State state in states)
            {
                InsertStateIntoTransitionTable(state);

                if (state.IsAccepting)
                {
                    mAcceptingStates[state.Id] = true;
                    mTokenIds[state.Id] = state.TokenClass;
                    mGreediness[state.Id] = state.Greediness;
                }
            }
        }

        public int MatchToken(byte[] buffer, int offset, int length, out int tokenClass)
        {
            int state = INITIAL_STATE;
            int i;
            tokenClass = -1;
            int lastAcceptingState = INVALID_STATE;
            int lastAcceptingPos = 0;

            for (i = offset; i < length; i++)
            {
                state = mTransitionsTable[state][buffer[i]]; // zero-length words are not supported

                if (state == INVALID_STATE)
                {
                    if (lastAcceptingState == INVALID_STATE)
                        throw new UnexpectedTokenException(new Token(buffer, offset, i - offset));

                    tokenClass = mTokenIds[lastAcceptingState];
                    break;
                }
                if (mAcceptingStates[state])
                {
                    lastAcceptingState = state;
                    lastAcceptingPos = i;
                }
//                int previousState = state;
//                state = mTransitionsTable[state][buffer[i]];
//
//                if (state == INVALID_STATE)
//                {
//                    if (mAcceptingStates[previousState])
//                    {
//                        tokenClass = mTokenIds[previousState];
//                        break;
//                    }
//                    throw new UnexpectedTokenException(new Token(buffer, offset, i - offset));
//                }
//                if (mAcceptingStates[state] && mGreediness[state] == Greediness.LazyQuantification)
//                {
//                    tokenClass = mTokenIds[previousState];
//                    break;
//                }
            }

            // In case end of buffer was reached, check whether it ends with a valid token
            if (i == length)
            {
                // If last visited state is accepting state
                if (mAcceptingStates[state])
                {
                    // Update accepting state and position
                    lastAcceptingState = state;
                    lastAcceptingPos = i - 1;
                }

                // Whether there was at least one accepting state
                if (lastAcceptingState != INVALID_STATE)
                {
                    tokenClass = mTokenIds[lastAcceptingState];
                    return lastAcceptingPos - offset + 1;
                }
                else
                {
                    return length - offset;
                }
            }

            return lastAcceptingPos - offset + 1; // returns length of token
        }

        #endregion

        private void InsertStateIntoTransitionTable(State state)
        {
            int[] row = CreateRow();
            mTransitionsTable.Add(row);

            foreach (var transition in state.Transitions)
            {
                if (transition.Value.Count != 1)
                    throw new InvalidOperationException("The provided automaton is not DFA!");

                byte inputChar = transition.Key.Value;
                int destinationState = transition.Value[0].Id;
                row[inputChar] = destinationState;
            }
        }

        private int[] CreateRow()
        {
            var row = new int[BYTE_OPTIONS];

            for (int i = 0; i < BYTE_OPTIONS; i++)
                row.SetValue(INVALID_STATE, i);
            return row;
        }
    }
}