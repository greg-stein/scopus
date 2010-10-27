using System.Text;

namespace Scopus.LexicalAnalysis.RegularExpressions
{
    /// <summary>
    /// Used for building regular expressions
    /// </summary>
    public abstract class RegExp
    {
        private Encoding mEncoding = Encoding.ASCII;

        /// <summary>
        /// Holds all child regular expressions
        /// </summary>
        protected RegExp[] ChildExpressions { get; set; }

        /// <summary>
        /// Encoding that used for translating LiteralRegExp into NFA.
        /// </summary>
        protected internal virtual Encoding Encoding
        {
            get { return mEncoding; }
            set
            {
                mEncoding = value;
                SetChildEncodingToParental();
            }
        }

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

        public static RegExp AllExcept(params char[] exceptees)
        {
            throw new System.NotImplementedException(); // This will fail unit test. Intended
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
        /// Builds regular expression that matches given string
        /// </summary>
        /// <param name="literal">word that should be recognized by regular 
        /// expression</param>
        /// <returns></returns>
        public static RegExp Literal(string literal)
        {
            return new LiteralRegExp(literal);
        }

        /// <summary>
        /// Builds regular expression that matches single symbol exactly: a
        /// </summary>
        /// <param name="literal">character that should be recognized by regular 
        /// expression</param>
        /// <param name="encoding">Encoding for given literal. Encoding is used to store given literal as byte array</param>
        /// <returns></returns>
        public static RegExp Literal(char literal, Encoding encoding)
        {
            return new LiteralRegExp(literal, encoding);
        }

        /// <summary>
        /// Builds regular expression that matches given string
        /// </summary>
        /// <param name="literal">word that should be recognized by regular 
        /// expression</param>
        /// <param name="encoding">Encoding for given literal. Encoding is used to store given literal as byte array</param>
        /// <returns></returns>
        public static RegExp Literal(string literal, Encoding encoding)
        {
            return new LiteralRegExp(literal, encoding);
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
            FiniteAutomata nfa = AsNFA();
            nfa.Terminator.IsAccepting = markTerminatorAsAcceptingState;
            return nfa;
        }

        /// <summary>
        /// Sets child encoding to be as parental encoding.
        /// </summary>
        /// <param name="regExps"></param>
        protected void SetChildEncodingToParental()
        {
            if (ChildExpressions == null) return;

            foreach (RegExp regExp in ChildExpressions)
            {
                regExp.Encoding = Encoding;
                regExp.SetChildEncodingToParental();
            }
        }
    }
}