using System;
using System.Collections;
using System.Collections.Generic;

namespace Scopus.LexicalAnalysis
{
    public class TokensCollection : ICollection<Token>
    {
        private readonly ILexer mLexer;

        public TokensCollection(ILexer lexer)
        {
            mLexer = lexer;

            TokensIndices = lexer.TokensIndices;
            TokensClasses = lexer.TokensClasses;
            TokensLengths = lexer.TokensLengths;

            RetrieveTokens();
        }

        internal byte[] LexemesBuffer { get; private set; }
        internal int[] TokensIndices { get; private set; }
        internal int[] TokensClasses { get; private set; }
        internal int[] TokensLengths { get; private set; }

        #region ICollection<Token> Members

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
            foreach (Token lexeme in this)
                array[arrayIndex++] = (Token) lexeme.Clone();
        }

        public int Count { get; private set; }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return new TokensEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TokensEnumerator(this);
        }

        #endregion

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