using System;

namespace Scopus.LexicalAnalysis.RegExp
{
    internal class SequenceRegExp : RegExp
    {
        internal RegExp RegExp1 { get; set; }
        internal RegExp RegExp2 { get; set; }

        internal SequenceRegExp(RegExp regExp1, RegExp regExp2)
        {
            RegExp1 = regExp1;
            RegExp2 = regExp2;
        }

        internal SequenceRegExp(params RegExp[] regExps)
        {
            if (regExps.Length < 2) throw new ArgumentException("regExps");

            RegExp1 = regExps[0];
            if (regExps.Length == 2)
            {
                RegExp2 = regExps[1];
            }
            else
            {
                RegExp2 = new SequenceRegExp(regExps[1], regExps[2]);
                for (int i = 3; i < regExps.Length; i++)
                {
                    RegExp2 = new SequenceRegExp(RegExp2, regExps[i]);
                }
            }
        }

        protected override RegExp[] SubExpressions
        {
            get { return new[] {RegExp1, RegExp2}; }
        }

        internal override NondeterministicFiniteAutomata AsNFA()
        {
            var nfa = new NondeterministicFiniteAutomata("SequenceRegExpNFA", false);
            var regExp1AsNFA = RegExp1.AsNFA();
            var regExp2AsNFA = RegExp2.AsNFA();
            nfa.StartState = regExp1AsNFA.StartState;
            nfa.Terminator = regExp2AsNFA.Terminator;
            regExp1AsNFA.Terminator.AddTransitionTo(regExp2AsNFA.StartState, InputChar.Epsilon());

            return nfa;
        }
    }
}
