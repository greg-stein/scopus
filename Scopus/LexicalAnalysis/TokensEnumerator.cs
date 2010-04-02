using System;
using System.Collections;
using System.Collections.Generic;
using Scopus.SyntaxAnalysis;

namespace Scopus.LexicalAnalysis
{
    public sealed class TokensEnumerator: IEnumerator<Token>
    {
        private readonly TokensCollection mCollection;
        private int mIndex;
        private Token mCurrent;
    	private bool mReachedEndMarker;

        public TokensEnumerator(TokensCollection collection)
        {
            mCurrent = new Token();
            mCollection = collection;
            ((IEnumerator) this).Reset();
        }

		#region IDisposable Members

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

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
		
		#region IEnumerator<Token> Members

        public Token Current
        {
            get { return mCurrent; }
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current
        {
            get { return mCurrent; }
        }

        bool IEnumerator.MoveNext()
        {
            while(true)
            {
				if (mIndex < mCollection.Count - 1)
				{
					ConstructNextToken();
					if (mCollection.HiddenTokens.Contains(mCurrent.Class))
						continue;

					return true;
				}

				// since end marker token is synthesized on its own buffer, this check should stand
				// before attempt to retrieve tokens from the synthesized buffer
				if (mReachedEndMarker)
					return false;

				if (mCollection.RetrieveTokens())
				{
					((IEnumerator)this).Reset();
					
					ConstructNextToken();
					if (mCollection.HiddenTokens.Contains(mCurrent.Class))
						continue;

					return true;
				}

				mReachedEndMarker = true;
				mCurrent = new Token(Terminal.END_MARKER_TOKEN_NAME) { Class = Terminal.END_MARKER_TOKEN_ID };

				return true;
			}
		}

        private void ConstructNextToken()
        {
            ++mIndex;
            mCurrent.Offset = mCollection.TokensIndices[mIndex];
            mCurrent.Length = mCollection.TokensIndices[mIndex + 1] - mCollection.TokensIndices[mIndex];
			mCurrent.Class = mCollection.TokensClasses[mIndex];
			mCurrent.Buffer = mCollection.LexemesBuffer;
		}

        void IEnumerator.Reset()
        {
            mIndex = -1;
        }

        #endregion
    }
}