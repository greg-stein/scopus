using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scopus.LexicalAnalysis;

namespace ScopusUnitTests
{
    internal class SimulationException : Exception
    {
        public SimulationException(string message)
            : base(message)
        {
        }
    }

    public static class SimulateNfaAspect
    {
        internal static State Simulate(this State s, string input)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            State state = s;
            foreach (var b in bytes)
            {
                state = Simulate(state, inputChars: b);
            }

            return state;
        }

        internal static State Simulate(this State s, int transitionToUse = 0, params byte?[] inputChars)
        {
            if (inputChars == null)
                inputChars = new byte?[] {null};

            State currentState = s;
            int pos = 0;

            foreach (var inputChar in inputChars)
            {
                var ic = inputChar == null ? InputChar.Epsilon() : InputChar.For((byte) inputChar);
                List<State> transitions;
                if (!currentState.Transitions.TryGetValue(ic, out transitions))
                    ThrowSimulationException(inputChar, pos);

                if (transitions.Count > transitionToUse)
                {
                    currentState = transitions[transitionToUse];
                }
                else if (transitions.Count > 0)
                {
                    currentState = transitions[0];
                }
                else
                {
                    ThrowSimulationException(inputChar, pos);
                }
                pos++;
            }

            return currentState;
        }

        private static void ThrowSimulationException(byte? inputChar, int pos)
        {
            throw new SimulationException(string.Format("Simulation: no transition for: {0} ASCII:{1} at pos: {2}",
                                                        (inputChar == null ? "null" : inputChar.ToString()), Encoding.ASCII.GetString(new[] { inputChar ?? 0 }), pos));
        }
    }
}
