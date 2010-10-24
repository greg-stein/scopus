namespace Scopus.LexicalAnalysis.Algorithms
{
    /// <summary>
    /// Represents states transition function, mostly table driven.
    /// </summary>
    public interface ITransitionFunction
    {
        /// <summary>
        /// Matches single token from input. Returns its length and class. 
        /// The start of the token should be offset. 
        /// </summary>
        /// <param name="buffer">Buffer containing data for processing</param>
        /// <param name="offset">Offset in the buffer (where to start)</param>
        /// <param name="length">Length of processing chunk of data</param>
        /// <param name="tokenClass">out parameter. This parameter will contain token class of recignized lexeme.</param>
        /// <returns>Length of recognized token</returns>
        int MatchToken(byte[] buffer, int offset, int length, out int tokenClass);

        /// <summary>
        /// Initializes transition function with given Deterministc Finite Automaton.
        /// </summary>
        /// <param name="dfa">DFA for  transition function construction</param>
        void Init(FiniteAutomata dfa);
    }
}