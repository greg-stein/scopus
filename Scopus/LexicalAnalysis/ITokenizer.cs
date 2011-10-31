using System;
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
        /// <summary>
        /// Gets or sets array containing tokens' indices.
        /// </summary>
        int[] TokensIndices { get; set; }

        /// <summary>
        /// Gets or sets array containing tokens' classes (types).
        /// </summary>
        int[] TokensClasses { get; set; }

        /// <summary>
        /// Gets or sets array containing tokens' lengths
        /// </summary>
        int[] TokensLengths { get; set; }

        /// <summary>
        /// Returns total tokens count added to tokenizer.
        /// </summary>
        int TotalTokensCount { get; }

        /// <summary>
        /// Sets transition function implementation to use. Transition function supports pattern matching, for
        /// example - simulates deterministic finite automata implemented as adjacent table.
        /// </summary>
        /// <param name="function">Specific implementation of transition function</param>
        void SetTransitionFunction(ITransitionFunction function);

        /// <summary>
        /// Tells tokenizer to recognize given pattern as terminal and pass it to Parser. This overloaded methos allows also 
        /// set a lexical action that will be performed when the given pattern has match
        /// </summary>
        /// <param name="regExp">Regular expression representing terminal</param>
        /// <param name="lexicalAction">Lexical action that will be performed if given token is recognized. Lexical action 
        /// may return boolean value that indicates whether recognized token should be used or not (ignored)</param>
        /// <returns></returns>
        Terminal UseTerminal(RegExp regExp, Func<Token, bool> lexicalAction = null);


        /// <summary>
        /// Tells tokenizer to recognize given pattern and DO NOT pass it to Parser. Using this method
        /// it is possible to implement a filtering preprocessor.
        /// </summary>
        /// <param name="ignoree">Regular expression represening pattern to ignore.</param>
        /// <param name="lexicalAction">Lexical action that will be performed if given token is recognized. Lexical action 
        /// may return boolean value that indicates whether recognized token should be used or not (ignored).
        /// When this method is used to specify token, if the lexical action will return true the token will still be ignored</param>
        void IgnoreTerminal(RegExp ignoree, Func<Token, bool> lexicalAction = null);

        /// <summary>
        /// Returns special terminal representing an epsilon (empty word) for use within production rules.
        /// </summary>
        /// <returns>Special terminal symbol representing epsiilon (empty word)</returns>
        Terminal UseEpsilon();

        /// <summary>
        /// Builds transition using previously set transition function. If no transition function is set, this method
        /// should throw an exception.
        /// </summary>
        void BuildTransitions();

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