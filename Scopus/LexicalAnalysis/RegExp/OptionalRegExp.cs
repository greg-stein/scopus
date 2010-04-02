namespace Scopus.LexicalAnalysis.RegExp
{
    internal class OptionalRegExp : RegExp
    {
        internal RegExp OptionalExpression { get; set; }

        internal OptionalRegExp(RegExp optionalExpr)
        {
            OptionalExpression = optionalExpr;
        }

        protected override RegExp[] SubExpressions
        {
            get { return new[] {OptionalExpression};}
        }

        internal override NondeterministicFiniteAutomata AsNFA()
        {
            var nfa = new NondeterministicFiniteAutomata("OptionalRegExpNFA");
            var optionalExpNFA = OptionalExpression.AsNFA();
            nfa.StartState.AddTransitionTo(optionalExpNFA.StartState, InputChar.Epsilon());
            nfa.StartState.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            optionalExpNFA.Terminator.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());

            return nfa;
        }
    }
}
