using System;
using System.Collections;
using System.Collections.Generic;
using Scopus.SyntaxAnalysis;

namespace Scopus.LexicalAnalysis
{
    public sealed class TokensEnumerator : IEnumerator<Token>
    {
        private readonly TokensCollection mCollection;
        private Token mCurrent;
        private int mIndex;
        private bool mReachedEndMarker;

        public TokensEnumerator(TokensCollection collection)
        {
            mCurrent = new Token();
            mCollection = collection;
            ((IEnumerator) this).Reset();
        }

        #region IEnumerator<Token> Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Token Current
        {
            get { return mCurrent; }
        }

        object IEnumerator.Current
        {
            get { return mCurrent; }
        }

        bool IEnumerator.MoveNext()
        {
            while (true)
            {
                if (mIndex < mCollection.Count - 1)
                {
                    ConstructNextToken();
                    return true;
                }

                // since end marker token is synthesized on its own buffer, this check should stand
                // before attempt to retrieve tokens from the synthesized buffer
                if (mReachedEndMarker)
                    return false;

                if (mCollection.RetrieveTokens())
                {
                    ((IEnumerator) this).Reset();

                    ConstructNextToken();
                    return true;
                }

                mReachedEndMarker = true;
                mCurrent = new Token(Terminal.END_MARKER_TOKEN_NAME) {Class = Terminal.END_MARKER_TOKEN_ID};

                return true;
            }
        }

        void IEnumerator.Reset()
        {
            mIndex = -1;
        }

        #endregion

        ~TokensEnumerator()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                mCurrent.Dispose();
            }
            // free native resources
        }

        private void ConstructNextToken()
        {
            ++mIndex;
            mCurrent.Offset = mCollection.TokensIndices[mIndex];
            mCurrent.Length = mCollection.TokensLengths[mIndex]; // mCollection.TokensIndices[mIndex + 1] - mCollection.TokensIndices[mIndex];
            mCurrent.Class = mCollection.TokensClasses[mIndex];
            mCurrent.Buffer = mCollection.LexemesBuffer;
        }
    }
}