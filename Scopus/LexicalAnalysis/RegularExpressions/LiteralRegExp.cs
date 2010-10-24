namespace Scopus.LexicalAnalysis.RegularExpressions
{
    /// <summary>
    /// Represents literal regular expression for single character.
    /// </summary>
    internal class LiteralRegExp : RegExp
    {
        internal LiteralRegExp(char literal)
        {
            Literals = Encoding.GetBytes(new[] {literal});
        }

        internal LiteralRegExp(string literals)
        {
            Literals = Encoding.GetBytes(literals);
        }

        private byte[] Literals { get; set; }

        internal override FiniteAutomata AsNFA()
        {
            var nfa = new FiniteAutomata("LiteralRegExpNFA");
            State state = nfa.StartState;
            int i;

            for (i = 0; i < Literals.Length - 1; i++)
            {
                var newState = new State(Literals[i].ToString());
                state.AddTransitionTo(newState, InputChar.For(Literals[i]));
                state = newState;
            }

            state.AddTransitionTo(nfa.Terminator, InputChar.For(Literals[i])); // the last transition

            return nfa;
        }

        public override string ToString()
        {
            return Encoding.GetString(Literals);
        }
    }
}