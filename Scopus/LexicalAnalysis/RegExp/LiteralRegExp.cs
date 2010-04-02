namespace Scopus.LexicalAnalysis.RegExp
{
    internal class LiteralRegExp : RegExp
    {
        internal char Literal { get; private set; }

        internal LiteralRegExp(char literal)
        {
            Literal = literal;
        }

        protected override RegExp[] SubExpressions
        {
            get { return new RegExp[0]; } // return empty array
        }

        internal override NondeterministicFiniteAutomata AsNFA()
        {
            var nfa = new NondeterministicFiniteAutomata("LiteralRegExpNFA");
            nfa.StartState.AddTransitionTo(nfa.Terminator, InputChar.For(Literal));

            return nfa;
        }
    }
}
