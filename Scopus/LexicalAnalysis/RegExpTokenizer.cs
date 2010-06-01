using Scopus.Auxiliary;
using Scopus.LexicalAnalysis.RegularExpressions;
using Scopus.SyntaxAnalysis;

namespace Scopus.LexicalAnalysis
{
    public class RegExpTokenizer 
    {
        private readonly FiniteAutomata allTokensNfa = new FiniteAutomata();
        private readonly IDProvider classIdProvider = new IDProvider();

        public Terminal UseTerminal(RegExp regExp)
        {
            var tokenNfa = regExp.AsNFA();
            tokenNfa.Terminator.IsAccepting = true;
            tokenNfa.Terminator.TokenClass = classIdProvider.GetNext();

            AddTokenNfa(tokenNfa);

            var terminal = new Terminal(tokenNfa.ToString(), classIdProvider.GetCurrent());
            return terminal;
        }

        private void AddTokenNfa(FiniteAutomata regExpNfa)
        {
            allTokensNfa.StartState.AddTransitionTo(regExpNfa.StartState, InputChar.Epsilon());
            regExpNfa.Terminator.AddTransitionTo(allTokensNfa.Terminator, InputChar.Epsilon());
        }
    }
}
