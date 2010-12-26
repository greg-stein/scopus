using System;
using Scopus.LexicalAnalysis.Algorithms;

namespace Scopus.LexicalAnalysis.RegularExpressions
{
    // TODO: Obsolete: doesn't work and will never work! Remove it!
    public class NegationRegExp : RegExp
    {
        private RegExp mPositiveRegExp;

        public NegationRegExp(RegExp regExp)
        {
            mPositiveRegExp = regExp;
        }

        internal override FiniteAutomata AsNFA()
        {
            // This method first converts nfa of positiveRegExp to dfa and then 
            // inverts non accepting states to accepting and vice versa

            var nfa = mPositiveRegExp.AsNFA(true); // Mark Terminator as accepting state!!!
                                                   // Otherwise the convertion will not work 
            var dfa = NFAToDFAConverter.Convert(nfa);

            var states = dfa.GetStates();

            foreach (var state in states)
            {
                if (state.IsAccepting)
                {
                    state.IsAccepting = false;
                }
                else
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
                            state.AddTransitionTo(dfa.Terminator, InputChar.For((byte) i));
                        }
                    }

                    state.IsAccepting = true;
                }

                for (int i = 0; i <= Byte.MaxValue + 1; i++)
                {
                    dfa.Terminator.AddTransitionTo(dfa.Terminator, InputChar.For((byte)i));
                }
                dfa.Terminator.IsAccepting = true;
            }

            return dfa;
        }

        public override string ToString()
        {
            return '^' + mPositiveRegExp.ToString();
        }
    }
}
