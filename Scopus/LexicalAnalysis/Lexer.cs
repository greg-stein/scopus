using System;
using System.IO;
using System.Text;
using Scopus.LexicalAnalysis.RegularExpressions;
using Scopus.SyntaxAnalysis;

namespace Scopus.LexicalAnalysis
{
    public class Lexer : ILexer, IDisposable
    {
        public const string END_LINE_TOKEN_NAME = "<endln>";

        private const int PAGE_SIZE = 4096; // as of Windows XP
        private const int DEFAULT_BUFFER_SIZE = 64*PAGE_SIZE;
        private const int DEFAULT_LEXEME_BUFFER_SIZE = PAGE_SIZE;
        private Encoding encoding = Encoding.ASCII;

        private bool mBufferIsFull;
        private BufferedStream mDataSource;
        private int mLastLexemePos;
        private int mReadLength;
        private ITokenizer mTokenizer;
        private RegExpParser regexpParser;
        private RegExpNotation mNotation;
        private RegExpParser mRegexpParser;


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

        public int BufferSize { get; private set; }

        #region ILexer Members

        public int LastTokenStartIndex { get; private set; }
        public byte[] Buffer { get; private set; }
        public int[] TokensIndices { get; private set; }
        public int[] TokensClasses { get; private set; }
        public int[] TokensLengths { get; private set; }

        public RegExpNotation RegExpNotation
        {
            get { return mNotation; }
            set 
            { 
                mNotation = value;
                mRegexpParser = RegExpParser.GetParser(mNotation);
            }
        }
        public ITokenizer Tokenizer
        {
            get { return mTokenizer; }
            set
            {
                mTokenizer = value;
                mTokenizer.TokensClasses = TokensClasses;
                mTokenizer.TokensIndices = TokensIndices;
                mTokenizer.TokensLengths = TokensLengths;
                mTokenizer.SetEncoding(encoding);
            }
        }

        public TokensCollection TokensStream
        {
            get { return new TokensCollection(this); }
        }

        public void SetEncoding(Encoding encoding)
        {
            this.encoding = encoding;
            if (Tokenizer != null) Tokenizer.SetEncoding(encoding);
        }

        public void Initialize()
        {
            mTokenizer.BuildTransitions();
        }

        public Terminal UseEpsilon()
        {
            return mTokenizer.UseEpsilon();
        }

        public Terminal UseTerminal(string regexp)
        {
            var regExpObj = mRegexpParser.Parse(regexp);
            return mTokenizer.UseTerminal(regExpObj);
        }

        public Terminal UseTerminal(string regexp, RegExpNotation notation)
        {
            RegExpNotation = notation;
            var regExpObj = mRegexpParser.Parse(regexp);
            return mTokenizer.UseTerminal(regExpObj);
        }

        public void IgnoreTerminal(string ignoree)
        {
            var regExpObj = mRegexpParser.Parse(ignoree);
            mTokenizer.IgnoreTerminal(regExpObj);
        }

        public void IgnoreTerminal(string ignoree, RegExpNotation notation)
        {
            RegExpNotation = notation;
            var regExpObj = mRegexpParser.Parse(ignoree);
            mTokenizer.UseTerminal(regExpObj);
        }

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

        #endregion

        private void AllocateBuffers()
        {
            Buffer = new byte[BufferSize];
            TokensIndices = new int[BufferSize + 1]; // 1 for setting last non existing lexeme index
            TokensClasses = new int[BufferSize];
            TokensLengths = new int[BufferSize];
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

        #region IDisposable members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        #endregion
    }
}