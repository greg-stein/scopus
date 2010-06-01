namespace Scopus.LexicalAnalysis.RegularExpressions
{
    internal class RepetitionRegExp : RegExp
    {
        internal RegExp ExpressionToRepeat { get; set; }

        internal RepetitionRegExp(RegExp expression)
        {
            ExpressionToRepeat = expression;
        }

        protected override RegExp[] SubExpressions
        {
            get { return new[] {ExpressionToRepeat}; }
        }

        internal override FiniteAutomata AsNFA()
        {
            var nfa = new FiniteAutomata("RepetitionRegExpNFA");
            var innerExpNFA = ExpressionToRepeat.AsNFA();

            nfa.StartState.AddTransitionTo(innerExpNFA.StartState, InputChar.Epsilon());
            nfa.StartState.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            innerExpNFA.Terminator.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            innerExpNFA.Terminator.AddTransitionTo(innerExpNFA.StartState, InputChar.Epsilon());

            return nfa;
        }

        public override string ToString()
        {
            return "(" + ExpressionToRepeat + ")*";
        }
    }
}
