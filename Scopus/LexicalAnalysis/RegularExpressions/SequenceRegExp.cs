using System;

namespace Scopus.LexicalAnalysis.RegularExpressions
{
    internal class SequenceRegExp : RegExp
    {
        internal SequenceRegExp(RegExp regExp1, RegExp regExp2)
        {
            RegExp1 = regExp1;
            RegExp2 = regExp2;
            ChildExpressions = new[] {regExp1, regExp2};
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

            ChildExpressions = new[] {RegExp1, RegExp2};
        }

        internal RegExp RegExp1 { get; set; }
        internal RegExp RegExp2 { get; set; }

        internal override FiniteAutomata AsNFA()
        {
            var nfa = new FiniteAutomata("SequenceRegExpNFA", false);
            FiniteAutomata regExp1AsNFA = RegExp1.AsNFA();
            FiniteAutomata regExp2AsNFA = RegExp2.AsNFA();
            nfa.StartState = regExp1AsNFA.StartState;
            nfa.Terminator = regExp2AsNFA.Terminator;
            regExp1AsNFA.Terminator.AddTransitionTo(regExp2AsNFA.StartState, InputChar.Epsilon());

            return nfa;
        }

        public override string ToString()
        {
            return RegExp1 + RegExp2.ToString();
        }

        public bool Equals(SequenceRegExp sequenceRegExp)
        {
            if (!base.Equals(sequenceRegExp)) return false;

            if (sequenceRegExp.RegExp1.Equals(RegExp1)) return false;
            if (sequenceRegExp.RegExp2.Equals(RegExp2)) return false;

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