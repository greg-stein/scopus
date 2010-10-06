namespace Scopus.LexicalAnalysis.RegularExpressions
{
    /// <summary>
    /// Used for building regular expressions
    /// </summary>
    public abstract class RegExp
    {
        /// <summary>
        /// Builds sequence regular expression, i.e. 'a','b' --> 'ab'
        /// </summary>
        /// <param name="regExps">List of regular expression in chain</param>
        /// <returns></returns>
        public static RegExp Sequence(params RegExp[] regExps)
        {
            return new SequenceRegExp(regExps);
        }

        /// <summary>
        /// Builds alternatives regular expression: a|b, a|b|c| ...
        /// </summary>
        /// <param name="regExps">List of alternative regular expressions</param>
        /// <returns></returns>
        public static RegExp Choice(params RegExp[] regExps)
        {
            return new AlternationRegExp(regExps);
        }

        /// <summary>
        /// Builds optional regular expression from given one: a?
        /// </summary>
        /// <param name="regExp">Optional regular expression</param>
        /// <returns></returns>
        public static RegExp Optional(RegExp regExp)
        {
            return new OptionalRegExp(regExp);
        }

        /// <summary>
        /// Builds star (repeatition - any number of occurences) regular expression: a* 
        /// </summary>
        /// <param name="regExp">Regular expression to repeat</param>
        /// <returns></returns>
        public static RegExp AnyNumberOf(RegExp regExp)
        {
            return new RepetitionRegExp(regExp);
        }

        /// <summary>
        /// Builds plus (repeatition - at least one occurence) regular expression: a+
        /// </summary>
        /// <param name="regExp">Regular expression to repeat</param>
        /// <returns></returns>
        public static RegExp AtLeastOneOf(RegExp regExp)
        {
            return new RepetitionAtLeastOneRegExp(regExp);
        }

        /// <summary>
        /// Builds regular expression that matches single symbol exactly: a
        /// </summary>
        /// <param name="literal">character that should be recognized by regular 
        /// expression</param>
        /// <returns></returns>
        public static RegExp Literal(char literal)
        {
            return new LiteralRegExp(literal);
        }

        /// <summary>
        /// Builds an Non-determenistic Finite Automaton that accepts language 
        /// defined by the regular expression.
        /// </summary>
        /// <returns>Non-determenistic Finite Automaton for the regular expression</returns>
        internal abstract FiniteAutomata AsNFA();

        /// <summary>
        /// Builds an Non-determenistic Finite Automaton that accepts language 
        /// defined by the regular expression.
        /// </summary>
        /// <param name="markTerminatorAsAcceptingState">Indicates if last state 
        /// of the regular expression
        ///  should be marked as accepting state</param>
        /// <returns>Non-determenistic Finite Automaton for the regular expression</returns>
        internal FiniteAutomata AsNFA(bool markTerminatorAsAcceptingState)
        {
            var nfa = AsNFA();
            nfa.Terminator.IsAccepting = markTerminatorAsAcceptingState;
            return nfa;
        }
    }
}