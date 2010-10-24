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
    }
}