namespace Scopus.LexicalAnalysis.RegularExpressions
{
    /// <summary>
    /// Represents literal regular expression for single character.
    /// </summary>
    internal class LiteralRegExp : RegExp
    {
        internal char LiteralSymbol { get; private set; }

        internal LiteralRegExp(char literal)
        {
            LiteralSymbol = literal;
        }

        internal override FiniteAutomata AsNFA()
        {
            var nfa = new FiniteAutomata("LiteralRegExpNFA");
            nfa.StartState.AddTransitionTo(nfa.Terminator, InputChar.For(LiteralSymbol));

            return nfa;
        }

        public override string ToString()
        {
            return LiteralSymbol.ToString();
        }
    }
}
