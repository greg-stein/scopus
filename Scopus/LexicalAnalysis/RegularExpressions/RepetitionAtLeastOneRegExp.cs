namespace Scopus.LexicalAnalysis.RegularExpressions
{
    internal class RepetitionAtLeastOneRegExp : RepetitionRegExp
    {
        internal RepetitionAtLeastOneRegExp(RegExp expression, Greediness greediness = Greediness.GreedyQuantification) 
            : base(expression, greediness)
        {
        }

        internal override FiniteAutomata AsNFA()
        {
            var nfa = new FiniteAutomata("RepetitionAtLeastOneRegExpNFA");
            FiniteAutomata innerExpNFA = ExpressionToRepeat.AsNFA();
            FiniteAutomata innerExpNFA2 = ExpressionToRepeat.AsNFA();

            nfa.StartState.AddTransitionTo(innerExpNFA.StartState, InputChar.Epsilon());
            innerExpNFA.Terminator.AddTransitionTo(innerExpNFA2.StartState, InputChar.Epsilon());
            innerExpNFA2.Terminator.AddTransitionTo(innerExpNFA2.StartState, InputChar.Epsilon());
            innerExpNFA2.StartState.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            //innerExpNFA2.Terminator.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());

            return nfa;
        }

        public override string ToString()
        {
            string qSign = (Greediness == Greediness.LazyQuantification) ? "?" : string.Empty;
            return "(" + ExpressionToRepeat + ")+" + qSign;
        }
    }
}