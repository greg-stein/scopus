using System;
using System.Collections.Generic;
using Scopus.LexicalAnalysis;

namespace Scopus.SyntaxAnalysis
{
    public class TerminalValues
    {
        private readonly List<Token> mTokens = new List<Token>();
        private int mPopableTokensCount;
        private int mPoppedTokensCount;

        /// <summary>
        /// Gets value of last terminal symbol provided.
        /// </summary>
        /// <param name="term">Terminal symbol used in the production rule.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown in case there is no such token value.</exception>
        public Token this[Terminal term]
        {
            // TODO: Reverse order of popping tokens
            get
            {
                if (term == null) throw new ArgumentNullException("term");

                if (mPoppedTokensCount < mPopableTokensCount)
                {
                    int stackPeek = mTokens.Count - 1;
                    for (int i = stackPeek; i >= 0; i--)
                    {
                        if (stackPeek - mPopableTokensCount == i)
                            break;

                        if (mTokens[i].Class == term.TokenClassID)
                        {
                            Token token = mTokens[i];
                            mTokens.RemoveAt(i);
                            mPoppedTokensCount++;
                            return token;
                        }
                    }
                }
                throw new ArgumentOutOfRangeException("term",
                                                      "The terminal symbol " + term +
                                                      " is not a part of the production.");
            }
        }

        internal void PushToken(Token token)
        {
            mTokens.Add(token);
        }

        internal void SetPopableTokensCount(int count)
        {
            mPopableTokensCount = count;
            mPoppedTokensCount = 0;
        }

        internal void RemoveUnusedTokens()
        {
            // cleans up all remaining remaining unpopped tokens in last action
            int remainedTokens = mPopableTokensCount - mPoppedTokensCount;
            if (remainedTokens > 0)
                mTokens.RemoveRange(mTokens.Count - remainedTokens, remainedTokens);
        }
    }
}