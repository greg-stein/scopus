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
        private readonly FiniteAutomata mAllTokensNfa = new FiniteAutomata();
        private readonly IDProvider mClassIdProvider = new IDProvider();
        private readonly HashSet<int> mIgnoredTokens = new HashSet<int>();
        private Encoding mEncoding;
        private int mTotalTokensCount;
        private ITransitionFunction mTransitionFunction;

        #region ITokenizer Members

        public int[] TokensIndices { get; set; }
        public int[] TokensClasses { get; set; }

        public void SetTransitionFunction(ITransitionFunction function)
        {
            mTransitionFunction = function;
            mClassIdProvider.GetNext(); // skip 0, cuz it reserved for Epsilon terminal
        }

        public Terminal UseTerminal(RegExp regExp)
        {
            regExp.Encoding = mEncoding;
            FiniteAutomata tokenNfa = regExp.AsNFA();
            tokenNfa.Terminator.IsAccepting = true;
            tokenNfa.Terminator.TokenClass = mClassIdProvider.GetNext();

            AddTokenToNfa(tokenNfa);

            var terminal = new Terminal(tokenNfa.ToString(), mClassIdProvider.GetCurrent());
            return terminal;
        }

        public void IgnoreTerminal(RegExp ignoree)
        {
            ignoree.Encoding = mEncoding;
            FiniteAutomata tokenNfa = ignoree.AsNFA();
            tokenNfa.Terminator.IsAccepting = true;
            tokenNfa.Terminator.TokenClass = mClassIdProvider.GetNext();
            mIgnoredTokens.Add(tokenNfa.Terminator.TokenClass);

            AddTokenToNfa(tokenNfa);
        }

        public Terminal UseEpsilon()
        {
            return new Terminal(Terminal.EPSILON_TOKEN_NAME, Terminal.EPSILON_TOKEN_ID);
        }

        public int TotalTokensCount
        {
            get { return mTotalTokensCount; }
        }

        public void SetEncoding(Encoding encoding)
        {
            mEncoding = encoding;
        }

        public void BuildTransitions()
        {
            if (mTransitionFunction == null)
                throw new InvalidOperationException("No transition function is set. Could not build transitions.");

            var dfa = NFAToDFAConverter.Convert(mAllTokensNfa);
            var minimizedDfa = DFAMinimizer.Minimize(dfa);
            mTransitionFunction.Init(minimizedDfa);
        }

        public int Tokenize(byte[] buffer, int offset, int length)
        {
            int tokensCount = 0;

            for (int i = offset; i < offset + length;)
            {
                TokensIndices[tokensCount] = i;
                int tokenClass;
                int tokenLength = mTransitionFunction.MatchToken(buffer, i, length, out tokenClass);
                TokensClasses[tokensCount] = tokenClass;

                i += tokenLength;

                if (!mIgnoredTokens.Contains(tokenClass))
                    tokensCount++;
            }

            mTotalTokensCount += tokensCount;
            return tokensCount - 1; // returns index of last token
        }

        #endregion

        private void AddTokenToNfa(FiniteAutomata regExpNfa)
        {
            mAllTokensNfa.StartState.AddTransitionTo(regExpNfa.StartState, InputChar.Epsilon());
            regExpNfa.Terminator.AddTransitionTo(mAllTokensNfa.Terminator, InputChar.Epsilon());
        }
    }
}