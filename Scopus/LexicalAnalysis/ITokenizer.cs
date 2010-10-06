using Scopus.LexicalAnalysis.Algorithms;
using Scopus.LexicalAnalysis.RegularExpressions;
using Scopus.SyntaxAnalysis;


namespace Scopus.LexicalAnalysis
{
    /// <summary>
    /// Represents tokenizer that is used by Lexer in Lexical Analysis phase.
    /// </summary>
    public interface ITokenizer
    {		
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
        /// Sets transition function implementation to use. Transition function supports pattern matching, for
        /// example - simulates deterministic finite automata implemented as adjacent table.
        /// </summary>
        /// <param name="transitionFunction">Specific implementation of transition function</param>
        void SetTransitionFunction(ITransitionFunction transitionFunction);

        /// <summary>
        /// Tells tokenizer to recognize given pattern as terminal and pass it to Parser
        /// </summary>
        /// <param name="terminal">Regular expression representing terminal</param>
        /// <returns>Terminal variable for using in a production rules</returns>
        Terminal UseTerminal(RegExp terminal);

        /// <summary>
        /// Tells tokenizer to recognize given pattern and DO NOT pass it to Parser. Using this method
        /// it is possible to implement a filtering preprocessor.
        /// </summary>
        /// <param name="ignoree">Regular expression represening pattern to ignore.</param>
        void IgnoreTerminal(RegExp ignoree);

        /// <summary>
        /// Returns special terminal representing an epsilon (empty word) for use within production rules.
        /// </summary>
        /// <returns>Special terminal symbol representing epsiilon (empty word)</returns>
        Terminal UseEpsilon();

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