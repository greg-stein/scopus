using System;
using System.Text;

namespace Scopus.LexicalAnalysis.RegularExpressions
{
    internal class SequenceRegExp : RegExp
    {
        internal SequenceRegExp(params RegExp[] regExps)
        {
            if (regExps.Length < 2) throw new ArgumentException("regExps");

            ChildExpressions = regExps;
        }

        internal override FiniteAutomata AsNFA()
        {
            var nfa = new FiniteAutomata("SequenceRegExpNFA", false);
            var startNFA = ChildExpressions[0].AsNFA();                         // First RegExp Nfa
            var endNFA = ChildExpressions[ChildExpressions.Length - 1].AsNFA(); // Last RegExp Nfa
            nfa.StartState = startNFA.StartState;
            nfa.Terminator = endNFA.Terminator;

            State lastAddedTerminator = startNFA.Terminator;

            for (int i = 1; i < ChildExpressions.Length - 1; i++)
            {
                var newNfa = ChildExpressions[i].AsNFA();
                lastAddedTerminator.AddTransitionTo(newNfa.StartState, InputChar.Epsilon());
                lastAddedTerminator = newNfa.Terminator;
            }
            lastAddedTerminator.AddTransitionTo(endNFA.StartState, InputChar.Epsilon());

            return nfa;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var regExp in ChildExpressions)
            {
                builder.Append(regExp);
            }

            return builder.ToString();
        }

        public bool Equals(SequenceRegExp sequenceRegExp)
        {
            if (!base.Equals(sequenceRegExp)) return false;

            if (ChildExpressions.Length != sequenceRegExp.ChildExpressions.Length) return false;

            for (int i = 0; i < ChildExpressions.Length; i++)
            {
                if (!ChildExpressions[i].Equals(sequenceRegExp.ChildExpressions[i])) return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return base.Equals(obj);

            var sequenceRegExp = obj as SequenceRegExp;
            if (sequenceRegExp != null)
                return Equals(sequenceRegExp);

            return false;
        }
    }
}