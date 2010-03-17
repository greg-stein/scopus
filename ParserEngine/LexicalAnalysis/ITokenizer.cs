using System.Collections.Generic;


namespace ParserEngine.LexicalAnalysis
{
    /// <summary>
    /// Represents tokenizer that is used by Lexer on Lexical Analysis phase.
    /// </summary>
    public interface ITokenizer
    {
		// !!! add AddToken() ???
		
		// todo: should be removed!!!
		/// <summary>
        /// Gets or sets array containing tokens' indices.
        /// </summary>
        int[] TokensIndices { get; set; }

		// todo: should be removed!!!
		/// <summary>
        /// Gets or sets array containing tokens' classes (types).
        /// </summary>
        int[] TokensClasses { get; set; }

		/// <summary>
		/// Gets or sets hashset containing those of token classes (types), which will be omitted by token enumerator.
		/// </summary>
		HashSet<int> HiddenTokens { get; set; }

		/// <summary>
		/// Returns total tokens count added to tokenizer.
		/// </summary>
		int TotalTokensCount { get; }
		
		/// <summary>
        /// Tokenizes chunk of byte array with given offset and length. Stores tokens information in
        /// TokenIndices and TokenClasses arrays and returns index of last recognized token.
        /// </summary>
        /// <param name="buffer">Buffer containing data to tokenize.</param>
        /// <param name="offset">Offset where to start.</param>
        /// <param name="length">Length of chunk to tokenize.</param>
        int Tokenize(byte[] buffer, int offset, int length);
    }
}