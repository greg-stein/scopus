using System;
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
        public RegExp[] ChildExpressions { get; protected set; }

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
        /// Builds range regular expression that matches any char within given range.
        /// </summary>
        /// <param name="left">Char represents first symbol in range</param>
        /// <param name="right">Char represents last symbol in range</param>
        /// <returns></returns>
        public static RegExp Range(char left, char right, Encoding encoding)
        {
            return new RangeRegExp(left, right, encoding);
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
        /// <param name="greediness">Indicates greediness of the quantifier (greedy/lazy)</param>
        /// <returns></returns>
        public static RegExp AnyNumberOf(RegExp regExp, Greediness greediness = Greediness.GreedyQuantification)
        {
            return new RepetitionRegExp(regExp, greediness);
        }

        /// <summary>
        /// Builds plus (repeatition - at least one occurence) regular expression: a+
        /// </summary>
        /// <param name="regExp">Regular expression to repeat</param>
        /// <param name="greediness">Indicates greediness of the quantifier (greedy/lazy)</param>
        /// <returns></returns>
        public static RegExp AtLeastOneOf(RegExp regExp, Greediness greediness = Greediness.GreedyQuantification)
        {
            return new RepetitionAtLeastOneRegExp(regExp, greediness);
        }

        /// <summary>
        /// Should :) return regular expression that will accept every pattern except that given in parameter.
        /// </summary>
        /// <param name="regExp">Regular expression represents an exception. Every other pattern will be accepted, but not this one.</param>
        /// <returns></returns>
        public static RegExp Not(RegExp regExp, bool createSelfLoopsForTerminator = true)
        {
            return new NegationRegExp(regExp, createSelfLoopsForTerminator);
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
        /// Returns regular expression that matches given byte
        /// </summary>
        /// <param name="literal">A single byte literal</param>
        /// <returns></returns>
        public static RegExp Literal(byte literal)
        {
            return new LiteralRegExp(literal);
        }

        /// <summary>
        /// Returns regular expression that matches given sequence of bytes
        /// </summary>
        /// <param name="literals">byte array representing desired sequence of bytes</param>
        /// <returns></returns>
        public static RegExp Literal(params byte[] literals)
        {
            return new LiteralRegExp(literals);
        }

        /// <summary>
        /// Builds regular expression for matching any character except given chars.
        /// </summary>
        /// <param name="encoding">Encoding for given literals. Encoding is used to store given literal as byte array</param>
        /// <param name="exceptees">Chars that should NOT be matched by this regular expression</param>
        /// <returns></returns>
        public static RegExp LiteralExcept(Encoding encoding, params char[] exceptees)
        {
            return new NegatedCharClassRegExp(encoding, exceptees);
        }

        /// <summary>
        /// Returns ready regular expression for parsing integer numbers.
        /// </summary>
        /// <returns></returns>
        public static RegExp GetNumberRegExp()
        {
            var number = RegExp.AtLeastOneOf(RegExp.Choice(
                RegExp.Literal('0'), RegExp.Literal('1'), RegExp.Literal('2'),
                RegExp.Literal('3'), RegExp.Literal('4'), RegExp.Literal('5'),
                RegExp.Literal('6'), RegExp.Literal('7'), RegExp.Literal('8'),
                RegExp.Literal('9')));

            return number;
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

        public override bool Equals(object obj)
        {
            var regExp = obj as RegExp;
            if (regExp == null)
                return false;

            return regExp.Encoding.Equals(Encoding);
        }
    }
}