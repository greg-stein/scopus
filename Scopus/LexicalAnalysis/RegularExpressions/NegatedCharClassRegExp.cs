using System;
using System.Collections.Generic;
using System.Text;
using Scopus.Auxiliary;

namespace Scopus.LexicalAnalysis.RegularExpressions
{
    internal sealed class NegatedCharClassRegExp : RegExp
    {
        private static Dictionary<Encoding, FiniteAutomata> sEncodingDfas = new Dictionary<Encoding, FiniteAutomata>();

        private readonly char[] mLiterals;
        // Maps states of the ignorees tree to the distance from root (level)
        private Dictionary<State, int> mLayers = new Dictionary<State, int>();
        // Maps states of the ignorees tree to the length of char in bytes to which they relate
        private Dictionary<State, int> mLiteralBytesCount = new Dictionary<State, int>();
        // Holds matrix of trunk states
        private State[,] mStatesMatrix;

        // Thread unsafe
        static FiniteAutomata ConstructEncodingDfa(Encoding encoding)
        {
            FiniteAutomata encodingDfa;

            if (!sEncodingDfas.TryGetValue(encoding, out encodingDfa))
            {
                int bytesNum = encoding.GetMaxByteCount(1);
                encodingDfa = new FiniteAutomata("encoding-dfa", true);

                // Construct matrix with states for building trunk DFA
                var statesMatrix = new State[bytesNum,bytesNum];

                for (int i = 0; i < bytesNum; i++)
                {
                    statesMatrix[i, i] = encodingDfa.Terminator;
                    for (int j = 0; j < i; j++)
                    {
                        // i - path length, j - state number
                        statesMatrix[i, j] = new State(String.Format("state-for-byte-[{0},{1}]", i, j));
                    }
                }

                // Set transitions between states of trunk DFA
                for (int c = 0; c <= char.MaxValue; c++)
                {
                    var bytes = encoding.GetBytes(new[] {(char) c});
                    State state = statesMatrix[bytes.Length - 1, 0];
                    var inputChar = InputChar.For(bytes[0]);
                    List<State> transitions;

                    if (!encodingDfa.StartState.Transitions.TryGetValue(inputChar, out transitions) ||
                        !transitions.Contains(state))
                    {
                        encodingDfa.StartState.AddTransitionTo(state, inputChar);
                    }

                    for (int j = 1; j < bytes.Length; j++)
                    {
                        var nextState = statesMatrix[bytes.Length - 1, j];
                        state.AddTransitionTo(nextState, InputChar.For(bytes[j]));
                        state = nextState;
                    }
                }
                sEncodingDfas[encoding] = encodingDfa;
            }

            return encodingDfa;
        }

        internal NegatedCharClassRegExp(Encoding encoding, params char[] literals)
        {
            Encoding = encoding;
            mLiterals = literals;
        }

        //TODO: I've Found the solution!
        // In order to recognize only desired characters, we need first construct a DFA
        // for all possible chars (if we deal with multibyte encoding we will have a chain
        // of states connected with 'anychar' transitions). After this we should deal with 
        // each ignored char: build a separate path for this char, on each state in the path
        // add transitions to the trunk on bytes that ARE NOT in the char path. Repeat this
        // for all ignored chars.
        internal override FiniteAutomata AsNFA()
        {
            var trunkNFA = BuildNFAForTrunk();
            var ignoreesTreeNFA = BuildIgnoreesTree();
            mStatesMatrix = GenerateStatesMatrix(trunkNFA);

            foreach (var transition in ignoreesTreeNFA.StartState.Transitions)
            {
                // TODO: This won't work if we want to support multiple paths in the ignorees
                // TODO: tree! Since we set only one possible path in the ignree tree (DFA), 
                // TODO: the byte array must begin with different bytes for different paths
                var transitions = trunkNFA.StartState.Transitions[transition.Key];
                transitions.Clear();

                if (transition.Value.Contains(ignoreesTreeNFA.Terminator))
                {
                    transition.Value.Remove(ignoreesTreeNFA.Terminator);
                }

                if (transition.Value.Count != 0)
                {
                    transitions.AddRange(transition.Value);
                }
            }

            EstablishTransitionsToTrunk(ignoreesTreeNFA, trunkNFA);
            ignoreesTreeNFA.Terminator.IsAccepting = false;

            return trunkNFA;
        }

        // Merges ignorees tree with trunk DFA utilizing BFS-like algorithm
        // Example:
        // Ignorees       Trunk
        // (S) -------a----(S)
        // a| /a            |
        // (a)---          (1)
        // b|    \          |
        // (b)    ---∑\b-->(2)
        //  |               |
        // [T]             (T)
        private void EstablishTransitionsToTrunk(FiniteAutomata ignoreesTreeNFA, FiniteAutomata trunkNFA)
        {
            var queue = new Queue<State>();
            var ignoreesTreeStartState = ignoreesTreeNFA.StartState;

            foreach (var transition in ignoreesTreeStartState.Transitions)
            {
                if (transition.Value.Count > 0) queue.Enqueue(transition.Value[0]);
            }

            // Get next state

            while (queue.Count > 0)
            {
                var ignoreeState = queue.Dequeue();
                if (ignoreeState.IsAccepting) continue;
                
                var layer = mLayers[ignoreeState];
                var bytesCount = mLiteralBytesCount[ignoreeState];
                var trunkState = mStatesMatrix[bytesCount-1, layer+1];

                foreach (var transition in ignoreeState.Transitions)
                {
                    queue.Enqueue(transition.Value[0]);                        
                }

                ComplementTransitions(ignoreeState, trunkState);
            }
        }

        private State[,] GenerateStatesMatrix(FiniteAutomata trunkNFA)
        {
            var byteCount = Encoding.GetMaxByteCount(1);
            var statesMatrix = new State[byteCount, byteCount];
            var visitedStates = new HashSet<State>();
            
            foreach (var transition in trunkNFA.StartState.Transitions)
            {
                foreach (var state in transition.Value)
                {
                    if (visitedStates.Contains(state)) break;
                    visitedStates.Add(state);

                    // Calculate length of path
                    var pathLength = 0;
                    for (var nextState = state; nextState != trunkNFA.Terminator; nextState = CommonRoutines.GetKVP(nextState.Transitions, 0).Value[0])
                    {
                        pathLength++;
                    }

                    // Fill matrix
                    var i = 0;
                    for (var nextState = state; nextState != trunkNFA.Terminator; nextState = CommonRoutines.GetKVP(nextState.Transitions, 0).Value[0])
                    {
                        statesMatrix[pathLength, i++] = nextState;
                    }
                    statesMatrix[pathLength, i] = trunkNFA.Terminator;
                }
            }

            return statesMatrix;
        }

        // Adds transitions for bytes for which no transition is set. The target state for
        // newly created transitions is targetState
        private static void ComplementTransitions(State state, State targetState)
        {
            for (int i = 0; i < Byte.MaxValue + 1; i++)
            {
                var inputChar = InputChar.For((byte) i);

                List<State> statesList;
                // If no transition on i at all
                if (!state.Transitions.TryGetValue(inputChar, out statesList) || statesList.Count == 0)
                {
                    state.AddTransitionTo(targetState, inputChar);                    
                }
                else if (statesList[0].IsAccepting) // If this transition is to the Terminator of ignorees tree
                {
                    // Delete it
                    state.Transitions.Remove(inputChar);
                }
            }
        }

        // Builds trunk (main automata accepting every literal in given encoding)
        // Example for length 3:
        //    (S)
        //   / | \
        // (1)(1)(T)
        //  |  |
        // (2)(T)
        //  |
        // (T)
        private FiniteAutomata BuildNFAForTrunk()
        {
            var staticNfa = ConstructEncodingDfa(Encoding);

            return (FiniteAutomata)staticNfa.Clone();
        }

        // Builds only tree of ignored characters
        // Example. Literals: aaa, abc, bac:
        //      (S)
        //      / \
        //    (a) (b)
        //    / \   \
        //  (a) (b) (a)
        //   |   |   |
        //  (a) (c) (c)  <-- These are terminators
        //    \  |  /    <-- These transitions do not exist 
        //     \ | /         I placed them here only for better understanding
        //      [T]
        // Terminator is marked as accepting state
        private FiniteAutomata BuildIgnoreesTree()
        {
            var nfa = new FiniteAutomata();

            foreach (var literal in mLiterals)
            {
                var bytes = Encoding.GetBytes(new[] { literal });

                State state = nfa.StartState;
                InputChar inputChar;

                for (int i = 0; i < bytes.Length - 1; i++)
                {
                    List<State> transitions;
                    inputChar = InputChar.For(bytes[i]);
                    if (state.Transitions.TryGetValue(inputChar, out transitions) && transitions.Count > 0)
                    {
                        state = transitions[0];
                    }
                    else
                    {
                        var newState = new State("ignorees-tree:" + bytes[i]);
                        mLayers[newState] = i;
                        mLiteralBytesCount[newState] = bytes.Length;
                        state.Transitions.Add(inputChar, new List<State> {newState});
                        state = newState;
                    }
                }
                // Assign last transition to Terminator for everyone
                inputChar = InputChar.For(bytes[bytes.Length - 1]);
                state.Transitions.Add(inputChar, new List<State> { nfa.Terminator });
                mLayers[nfa.Terminator] = int.MaxValue;
            }

            nfa.Terminator.IsAccepting = true;
            return nfa;
        }
    }
}
