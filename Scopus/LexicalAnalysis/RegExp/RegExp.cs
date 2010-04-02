namespace Scopus.LexicalAnalysis.RegExp
{
    public abstract class RegExp
    {
        public static RegExp Sequence(params RegExp[] regExps)
        {
            return new SequenceRegExp(regExps);
        }

        public static RegExp Choice(params RegExp[] regExps)
        {
            return new AlternationRegExp(regExps);
        }

        public static RegExp Optional(RegExp regExp)
        {
            return new OptionalRegExp(regExp);
        }

        public static RegExp AnyNumberOf(RegExp regExp)
        {
            return new RepetitionRegExp(regExp);
        }

        public static RegExp AtLeastOneOf(RegExp regExp)
        {
            return new RepetitionAtLeastOneRegExp(regExp);
        }

        public static RegExp Literal(char literal)
        {
            return new LiteralRegExp(literal);
        }

        protected abstract RegExp[] SubExpressions { get; }
        internal abstract NondeterministicFiniteAutomata AsNFA();
    }
}