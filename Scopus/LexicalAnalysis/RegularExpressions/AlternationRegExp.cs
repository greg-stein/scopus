using System;
using System.Collections.Generic;
using System.Text;

namespace Scopus.LexicalAnalysis.RegularExpressions
{
    /// <summary>
    /// Represents alternation in regular expressions. I.e. a|b 
    /// </summary>
    internal class AlternationRegExp : RegExp
    {
//        internal AlternationRegExp(RegExp alternative1, RegExp alternative2)
//        {
//            Alternative1 = alternative1;
//            Alternative2 = alternative2;
//            ChildExpressions = new[] {alternative1, alternative2};
//        }

        internal AlternationRegExp(params RegExp[] alternatives)
        {
            if (alternatives.Length < 2) throw new ArgumentException("alternatives");

            ChildExpressions = alternatives;
//            Alternative1 = alternatives[0];
//            if (alternatives.Length == 2)
//            {
//                Alternative2 = alternatives[1];
//            }
//            else
//            {
//                Alternative2 = new AlternationRegExp(alternatives[1], alternatives[2]);
//                for (int i = 3; i < alternatives.Length; i++)
//                {
//                    Alternative2 = new AlternationRegExp(Alternative2, alternatives[i]);
//                }
//            }
//
//            ChildExpressions = new[] { Alternative1, Alternative2 };
        }

//        internal RegExp Alternative1 { get; set; }
//        internal RegExp Alternative2 { get; set; }

        internal override FiniteAutomata AsNFA()
        {
            var nfa = new FiniteAutomata("AlternationRegExpNFA");

            foreach (var regExp in ChildExpressions)
            {
                var newNfa = regExp.AsNFA();
                nfa.StartState.AddTransitionTo(newNfa.StartState, InputChar.Epsilon());
                newNfa.Terminator.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            }
//            FiniteAutomata alternative1NFA = Alternative1.AsNFA();
//            FiniteAutomata alternative2NFA = Alternative2.AsNFA();
//            nfa.StartState.AddTransitionTo(alternative1NFA.StartState, InputChar.Epsilon());
//            nfa.StartState.AddTransitionTo(alternative2NFA.StartState, InputChar.Epsilon());
//            alternative1NFA.Terminator.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
//            alternative2NFA.Terminator.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());

            return nfa;
        }

        public override string ToString()
        {
            var builder = new StringBuilder(ChildExpressions[0].ToString());

            for (int i = 1; i < ChildExpressions.Length; i++)
            {
                builder.AppendFormat("|{0}", ChildExpressions[i]);
            }

            return builder.ToString();
//            return Alternative1 + " | " + Alternative2;
        }

        public bool Equals(AlternationRegExp alternationRegExp)
        {
            if (!base.Equals(alternationRegExp)) return false;

            if (alternationRegExp.ChildExpressions.Length != ChildExpressions.Length) return false;

            // The easiest way to check whether two arrays has same regular axpressions is to construct HaashSets and check
            // if the sets are equal. Remember, the order is not important!

            var hashSetForThis = new HashSet<RegExp>(ChildExpressions);
            var hashSetForOther = new HashSet<RegExp>(alternationRegExp.ChildExpressions);

            if (hashSetForOther.IsSubsetOf(hashSetForThis) && hashSetForThis.IsSubsetOf(hashSetForOther))
            {
                return true;
            }
            
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return base.Equals(obj);

            var alternationRegExp = obj as AlternationRegExp;
            if (alternationRegExp != null)
                return Equals(alternationRegExp);

            return false;
        }
    }
}