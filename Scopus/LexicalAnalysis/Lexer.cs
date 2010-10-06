using System;
using System.IO;

namespace Scopus.LexicalAnalysis
{
    public class Lexer : ILexer, IDisposable
    {
        public const string END_LINE_TOKEN_NAME = "<endln>";
		public const string INTEGER_NUMBER_TOKEN_NAME = "int_id";
		public const string FLOAT_NUMBER_TOKEN_NAME = "float_id";
		public const string STRING_CONSTANT_TOKEN_NAME = "string_const";
		public const string STRING_IDENTIFIER_TOKEN_NAME = "id";
		
		private const int PAGE_SIZE = 4096; // as of Windows XP
        private const int DEFAULT_BUFFER_SIZE = 64 * PAGE_SIZE;
        private const int DEFAULT_LEXEME_BUFFER_SIZE = PAGE_SIZE;

        private BufferedStream mDataSource;
        private int mReadLength;
        private bool mBufferIsFull;
        private int mLastLexemePos;
        private ITokenizer mTokenizer;

        public int LastTokenStartIndex { get; private set; }
        public byte[] Buffer { get; private set; }
        public int BufferSize { get; private set; }
        public int[] TokensIndices { get; private set; }
        public int[] TokensClasses { get; private set; }

        public ITokenizer Tokenizer
        {
            get { return mTokenizer; }
            set
            {
                mTokenizer = value;
                mTokenizer.TokensClasses = TokensClasses;
                mTokenizer.TokensIndices = TokensIndices;
            }
        }
        public TokensCollection TokensStream
        {
            get { return new TokensCollection(this); }
        }

        public Lexer(ITokenizer tokenizer)
        {
            BufferSize = DEFAULT_LEXEME_BUFFER_SIZE;
            AllocateBuffers();
            Tokenizer = tokenizer;
        }
        public Lexer(ITokenizer tokenizer, int bufferSize)
        {
            BufferSize = bufferSize;
            AllocateBuffers();
            Tokenizer = tokenizer;
        }
        public Lexer(Stream stream, ITokenizer tokenizer) : this(tokenizer)
        {
            SetDataSource(stream);
        }
        public Lexer(Stream stream, ITokenizer tokenizer, int bufferSize) : this(tokenizer, bufferSize)
        {
            SetDataSource(stream);
        }

        private void AllocateBuffers()
        {
            Buffer = new byte[BufferSize];
            TokensIndices = new int[BufferSize + 1]; // 1 for setting last non existing lexeme index
            TokensClasses = new int[BufferSize];
        }

		#region IDisposable members

		~Lexer()
		{
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// dispose managed resources
				mDataSource.Dispose();
			}
			// free native resources
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
		
		public void SetDataSource(Stream stream)
        {
            LastTokenStartIndex = 0;
            mDataSource = new BufferedStream(stream, DEFAULT_BUFFER_SIZE);
            mReadLength = 0;
            mBufferIsFull = false;
            mLastLexemePos = 0;
        }

        public bool ReadTokens()
        {
            if (!ReadRawData())
                return false;

            LastTokenStartIndex = Tokenizer.Tokenize(Buffer, 0, mReadLength);
            mLastLexemePos = TokensIndices[LastTokenStartIndex];

            if (mBufferIsFull)
            {
                LastTokenStartIndex--; // last lexeme will be copied in next call to ReadTokens
            }
            else
            {
                TokensIndices[LastTokenStartIndex + 1] = mReadLength;
            }
            return true;
        }

        /// <summary>
        /// Fills internal buffer.
        /// Returns true if there was available data to read, otherwise - false.
        /// </summary>
        private bool ReadRawData()
        {
            int analyzedLexemeLength = mReadLength - mLastLexemePos;
            bool lastLexemeWasCopied = false;

            var newBuffer = new byte[BufferSize];

            // copy analyzed part of token to the beginning of buffer
            if (mBufferIsFull)
            {
                Array.Copy(Buffer, mLastLexemePos, newBuffer, 0, analyzedLexemeLength);
                lastLexemeWasCopied = analyzedLexemeLength > 0;
            }

            Buffer = newBuffer;

            // fill buffer
            int expectedReadLength = BufferSize - analyzedLexemeLength;
            mReadLength = mDataSource.Read(Buffer, analyzedLexemeLength, expectedReadLength);

            mBufferIsFull = (expectedReadLength == mReadLength);

            if (mReadLength == 0 && !lastLexemeWasCopied)
                return false;

            mReadLength += analyzedLexemeLength;
            return true;
		}
	}
}