using System;

namespace Scopus.LexicalAnalysis.RegularExpressions
{
    /// <summary>
    /// Represents alternation in regular expressions. I.e. a|b 
    /// </summary>
    internal class AlternationRegExp : RegExp
    {
        internal RegExp Alternative1 { get; set; }
        internal RegExp Alternative2 { get; set; }

        internal AlternationRegExp(RegExp alternative1, RegExp alternative2)
        {
            Alternative1 = alternative1;
            Alternative2 = alternative2;
        }

        internal AlternationRegExp(params RegExp[] alternatives)
        {
            if (alternatives.Length < 2) throw new ArgumentException("alternatives");

            Alternative1 = alternatives[0];
            if (alternatives.Length == 2)
            {
                Alternative2 = alternatives[1];
            }
            else
            {
                Alternative2 = new AlternationRegExp(alternatives[1], alternatives[2]);
                for (int i = 3; i < alternatives.Length; i++)
                {
                    Alternative2 = new AlternationRegExp(Alternative2, alternatives[i]);
                }
            }
        }

        internal override FiniteAutomata AsNFA()
        {
            var nfa = new FiniteAutomata("AlternationRegExpNFA");
            var alternative1NFA = Alternative1.AsNFA();
            var alternative2NFA = Alternative2.AsNFA();
            nfa.StartState.AddTransitionTo(alternative1NFA.StartState, InputChar.Epsilon());
            nfa.StartState.AddTransitionTo(alternative2NFA.StartState, InputChar.Epsilon());
            alternative1NFA.Terminator.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            alternative2NFA.Terminator.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());

            return nfa;
        }

        public override string ToString()
        {
            return "(" + Alternative1 + " | " + Alternative2 + ")";
        }
    }
}
