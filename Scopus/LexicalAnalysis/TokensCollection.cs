using System;
using System.Collections;
using System.Collections.Generic;

namespace Scopus.LexicalAnalysis
{
    public class TokensCollection : ICollection<Token>
    {
    	private static readonly HashSet<int> DefaultHiddenTokens = new HashSet<int>();

		internal byte[] LexemesBuffer { get; private set; }
        internal int[] TokensIndices { get; private set; }
        internal int[] TokensClasses { get; private set; }

		internal HashSet<int> HiddenTokens;

		private readonly ILexer mLexer;

        #region ICollection<Lexeme> Members

        public void Add(Token item)
        {
            throw new InvalidOperationException("Attempt to add item to a read only collection.");
        }

        public void Clear()
        {
            throw new InvalidOperationException("Attempt to clear a read only collection.");
        }

        public bool Remove(Token item)
        {
            throw new InvalidOperationException("Attempt to remove item from a read only collection.");
        }

        public bool Contains(Token item)
        {
            return item.Buffer == LexemesBuffer
                   && item.Offset >= 0
                   && item.Offset + item.Length < LexemesBuffer.Length;
        }

        public void CopyTo(Token[] array, int arrayIndex)
        {
            foreach (var lexeme in this)
                array[arrayIndex++] = (Token)lexeme.Clone();
        }

        public int Count
        {
            get; private set;
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        #endregion

        #region IEnumerable<Lexeme> Members

        public IEnumerator<Token> GetEnumerator()
        {
            return new TokensEnumerator(this);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TokensEnumerator(this);
        }

        #endregion

        public TokensCollection(ILexer lexer, HashSet<int> hiddenTokens)
        {
            mLexer = lexer;

            TokensIndices = lexer.TokensIndices;
            TokensClasses = lexer.TokensClasses;
			HiddenTokens = (hiddenTokens) ?? DefaultHiddenTokens;

            RetrieveTokens();
        }

        public bool RetrieveTokens()
        {
            if (mLexer.ReadTokens())
            {
                LexemesBuffer = mLexer.Buffer;
                Count = mLexer.LastTokenStartIndex + 1;
                return true;
            }

            return false;
        }
	}
}