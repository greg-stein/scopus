using System;
using System.Collections.Generic;
using System.Text;
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
        private readonly HashSet<int> ignoredTokens = new HashSet<int>();
        private Encoding mEncoding;
        private int totalTokensCount;
        private ITransitionFunction transitionFunction;

        #region ITokenizer Members

        public int[] TokensIndices { get; set; }
        public int[] TokensClasses { get; set; }

        public void SetTransitionFunction(ITransitionFunction function)
        {
            transitionFunction = function;
            classIdProvider.GetNext(); // skip 0, cuz it reserved for Epsilon terminal
        }

        public Terminal UseTerminal(RegExp regExp)
        {
            regExp.Encoding = mEncoding;
            FiniteAutomata tokenNfa = regExp.AsNFA();
            tokenNfa.Terminator.IsAccepting = true;
            tokenNfa.Terminator.TokenClass = classIdProvider.GetNext();

            AddTokenToNfa(tokenNfa);

            var terminal = new Terminal(tokenNfa.ToString(), classIdProvider.GetCurrent());
            return terminal;
        }

        public void IgnoreTerminal(RegExp ignoree)
        {
            ignoree.Encoding = mEncoding;
            FiniteAutomata tokenNfa = ignoree.AsNFA();
            tokenNfa.Terminator.IsAccepting = true;
            tokenNfa.Terminator.TokenClass = classIdProvider.GetNext();
            ignoredTokens.Add(tokenNfa.Terminator.TokenClass);

            AddTokenToNfa(tokenNfa);
        }

        public Terminal UseEpsilon()
        {
            return new Terminal(Terminal.EPSILON_TOKEN_NAME, Terminal.EPSILON_TOKEN_ID);
        }

        public int TotalTokensCount
        {
            get { return totalTokensCount; }
        }

        public void SetEncoding(Encoding encoding)
        {
            mEncoding = encoding;
        }

        public void BuildTransitions()
        {
            if (transitionFunction == null)
                throw new InvalidOperationException("No transition function is set. Could not build transitions.");

            var dfa = NFAToDFAConverter.Convert(allTokensNfa);
            var minimizedDfa = DFAMinimizer.Minimize(dfa);
            transitionFunction.Init(minimizedDfa);
        }

        public int Tokenize(byte[] buffer, int offset, int length)
        {
            int tokensCount = 0;

            for (int i = offset; i < offset + length;)
            {
                TokensIndices[tokensCount] = i;
                int tokenClass;
                int tokenLength = transitionFunction.MatchToken(buffer, i, length, out tokenClass);
                TokensClasses[tokensCount] = tokenClass;

                i += tokenLength;

                if (!ignoredTokens.Contains(tokenClass))
                    tokensCount++;
            }

            totalTokensCount += tokensCount;
            return tokensCount - 1; // returns index of last token
        }

        #endregion

        private void AddTokenToNfa(FiniteAutomata regExpNfa)
        {
            allTokensNfa.StartState.AddTransitionTo(regExpNfa.StartState, InputChar.Epsilon());
            regExpNfa.Terminator.AddTransitionTo(allTokensNfa.Terminator, InputChar.Epsilon());
        }
    }
}