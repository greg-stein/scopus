using System;
using Scopus.Auxiliary;
using Scopus.LexicalAnalysis.Algorithms;
using Scopus.LexicalAnalysis.RegularExpressions;
using Scopus.SyntaxAnalysis;

namespace Scopus.LexicalAnalysis
{
    public class RegExpTokenizer : ITokenizer
    {
        private readonly FiniteAutomata allTokensNfa = new FiniteAutomata();
        private readonly IDProvider classIdProvider = new IDProvider();

        public int[] TokensIndices
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int[] TokensClasses
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void SetTransitionFunction(ITransitionFunction transitionFunction)
        {
            throw new NotImplementedException();
        }

        public Terminal UseTerminal(RegExp regExp)
        {
            var tokenNfa = regExp.AsNFA();
            tokenNfa.Terminator.IsAccepting = true;
            tokenNfa.Terminator.TokenClass = classIdProvider.GetNext();

            AddTokenNfa(tokenNfa);

            var terminal = new Terminal(tokenNfa.ToString(), classIdProvider.GetCurrent());
            return terminal;
        }

        public void IgnoreTerminal(RegExp ignoree)
        {
            throw new NotImplementedException();
        }

        public Terminal UseEpsilon()
        {
            return new Terminal(Terminal.EPSILON_TOKEN_NAME, Terminal.EPSILON_TOKEN_ID);
        }

        public int TotalTokensCount
        {
            get { throw new NotImplementedException(); }
        }

        public int Tokenize(byte[] buffer, int offset, int length)
        {
            throw new NotImplementedException();
        }

        private void AddTokenNfa(FiniteAutomata regExpNfa)
        {
            allTokensNfa.StartState.AddTransitionTo(regExpNfa.StartState, InputChar.Epsilon());
            regExpNfa.Terminator.AddTransitionTo(allTokensNfa.Terminator, InputChar.Epsilon());
        }
    }
}
