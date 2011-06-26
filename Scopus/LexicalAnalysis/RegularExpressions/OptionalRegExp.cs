namespace Scopus.LexicalAnalysis.RegularExpressions
{
    /// <summary>
    /// Represents optional regular expression: a?
    /// </summary>
    internal class OptionalRegExp : RegExp
    {
        internal OptionalRegExp(RegExp optionalExpr)
        {
            OptionalExpression = optionalExpr;
            ChildExpressions = new[] {optionalExpr};
        }

        internal RegExp OptionalExpression { get; set; }

        internal override FiniteAutomata AsNFA()
        {
            var nfa = new FiniteAutomata("OptionalRegExpNFA");
            FiniteAutomata optionalExpNFA = OptionalExpression.AsNFA();
            nfa.StartState.AddTransitionTo(optionalExpNFA.StartState, InputChar.Epsilon());
            nfa.StartState.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            optionalExpNFA.Terminator.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());

            return nfa;
        }

        public override string ToString()
        {
            return "(" + OptionalExpression + ")?";
        }

        public bool Equals(OptionalRegExp optionalRegExp)
        {
            if (!base.Equals(optionalRegExp)) return false;

            return OptionalExpression.Equals(optionalRegExp);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return base.Equals(obj);

            var optionalRegExp = obj as OptionalRegExp;
            if (optionalRegExp != null)
                return Equals(optionalRegExp);

            return false;
        }

    }
}