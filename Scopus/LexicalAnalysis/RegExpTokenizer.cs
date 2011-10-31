using System;
using System.Collections.Generic;
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
        private readonly Dictionary<int, Func<Token, bool>> mLexicalActions = new Dictionary<int, Func<Token, bool>>();
        private int mTotalTokensCount;
        private ITransitionFunction mTransitionFunction;

        #region ITokenizer Members

        public int[] TokensIndices { get; set; }
        public int[] TokensClasses { get; set; }
        public int[] TokensLengths { get; set; }

        public void SetTransitionFunction(ITransitionFunction function)
        {
            mTransitionFunction = function;
            mClassIdProvider.GetNext(); // skip 0, cuz it reserved for Epsilon terminal
        }

        //TODO: Func<Token, bool> => Predicate<Token>
        public Terminal UseTerminal(RegExp regExp, Func<Token, bool> lexicalAction = null)
        {
            FiniteAutomata tokenNfa = GetTokenNfa(regExp);
            if (lexicalAction != null)
            {
                mLexicalActions[tokenNfa.Terminator.TokenClass] = lexicalAction;
            }

            AddTokenToNfa(tokenNfa);

            var terminal = new Terminal(regExp.ToString(), mClassIdProvider.GetCurrent());
            return terminal;
        }

        //TODO: Func<Token, bool> => Predicate<Token>
        public void IgnoreTerminal(RegExp ignoree, Func<Token, bool> lexicalAction = null)
        {
            FiniteAutomata tokenNfa = GetTokenNfa(ignoree);
            if (lexicalAction != null)
            {
                mLexicalActions[tokenNfa.Terminator.TokenClass] = lexicalAction;
            }
            mIgnoredTokens.Add(tokenNfa.Terminator.TokenClass);

            AddTokenToNfa(tokenNfa);
        }

        private FiniteAutomata GetTokenNfa(RegExp regExp)
        {
            FiniteAutomata tokenNfa = regExp.AsNFA();
            tokenNfa.Terminator.IsAccepting = true;
            tokenNfa.Terminator.TokenClass = mClassIdProvider.GetNext();

            return tokenNfa;
        }

        public Terminal UseEpsilon()
        {
            return new Terminal(Terminal.EPSILON_TOKEN_NAME, Terminal.EPSILON_TOKEN_ID);
        }

        public int TotalTokensCount
        {
            get { return mTotalTokensCount; }
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
                TokensLengths[tokensCount] = tokenLength;

                Func<Token, bool> lexicalAction;
                bool lexicalActionResult = true; // Indicates whether the token has been passed to parser (not ignored)
                if (mLexicalActions.TryGetValue(tokenClass, out lexicalAction))
                {
                    var token = new Token(buffer, i, tokenLength) {Class = tokenClass};
                    lexicalActionResult = lexicalAction(token);
                }

                i += tokenLength;

                // In case the terminal is ignored OR has been ignored by lexicalAction, don't count it (don't pass it 
                // to parser)
                if (mIgnoredTokens.Contains(tokenClass) || !lexicalActionResult) continue;
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