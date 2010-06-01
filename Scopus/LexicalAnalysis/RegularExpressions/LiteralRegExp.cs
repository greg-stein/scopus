namespace Scopus.LexicalAnalysis.RegularExpressions
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

        internal override FiniteAutomata AsNFA()
        {
            var nfa = new FiniteAutomata("LiteralRegExpNFA");
            nfa.StartState.AddTransitionTo(nfa.Terminator, InputChar.For(Literal));

            return nfa;
        }

        public override string ToString()
        {
            return Literal.ToString();
        }
    }
}
