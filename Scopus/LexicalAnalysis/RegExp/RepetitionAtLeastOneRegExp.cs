namespace Scopus.LexicalAnalysis.RegExp
{
    internal class RepetitionAtLeastOneRegExp : RepetitionRegExp
    {
        internal RepetitionAtLeastOneRegExp(RegExp expression) : base(expression)
        {
        }

        internal override NondeterministicFiniteAutomata AsNFA()
        {
            var nfa = new NondeterministicFiniteAutomata("RepetitionAtLeastOneRegExpNFA");
            var innerExpNFA = ExpressionToRepeat.AsNFA();
            var innerExpNFA2 = ExpressionToRepeat.AsNFA();

            nfa.StartState.AddTransitionTo(innerExpNFA.StartState, InputChar.Epsilon());
            innerExpNFA.Terminator.AddTransitionTo(innerExpNFA2.StartState, InputChar.Epsilon());
            innerExpNFA2.Terminator.AddTransitionTo(innerExpNFA2.StartState, InputChar.Epsilon());
            innerExpNFA2.StartState.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            innerExpNFA2.Terminator.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());

            return nfa;
        }
    }
}
