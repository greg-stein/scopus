namespace ParserEngine.LexicalAnalysis.RegExp
{
    internal interface IRegExpVisitor
    {
        void Visit(RepetitionRegExp regExp);
        void Visit(SequenceRegExp regExp);
    }
}
