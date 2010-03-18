namespace Scopus.SyntaxAnalysis
{
    internal enum ParserAction
    {
        Error = 0,
        Shift,
        Reduce,
        Accept,
    }
}