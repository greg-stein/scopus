using System.IO;

namespace Scopus.LexicalAnalysis
{
    /// <summary>
    /// Represents lexer = LEXical analyzER. Lexical analysis is phase 
    /// </summary>
    public interface ILexer
    {
        /// <summary>
        /// Gets index of last recognized token start position in the buffer.
        /// </summary>
        int LastTokenStartIndex { get; }

        /// <summary>
        /// Gets buffer which holds input data from stream.
        /// </summary>
        byte[] Buffer { get; }

        /// <summary>
        /// Gets array where tokens indices will be stored.
        /// </summary>
        int[] TokensIndices { get; }

        /// <summary>
        /// Gets array where token classes (types) will be stored.
        /// </summary>
        int[] TokensClasses { get; }

        /// <summary>
        /// Sets stream as source of data which should be analyzed (tokenized).
        /// </summary>
        /// <param name="stream">Data source.</param>
        void SetDataSource(Stream stream);

        /// <summary>
        /// Reads data from stream into buffer and tokenizes it. Each call to this
        /// method will cause processing of chunk (&lt BufferSize in size) of data,
        /// hence most probably this method is called more than once.
        /// </summary>
        /// <returns>true if end of stream is NOT achieved yet, otherwise - false.</returns>
        bool ReadTokens();

        /// <summary>
        /// Gets or sets <see cref="ITokenizer"/> for lexer.
        /// </summary>
        ITokenizer Tokenizer { get; set; }

        /// <summary>
        /// Gets tokens as enumerable collection. This is more convinient way to scan input.
        /// </summary>
        TokensCollection TokensStream { get; }
    }
}