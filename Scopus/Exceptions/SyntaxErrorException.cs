using Scopus.SyntaxAnalysis;

namespace Scopus.Exceptions
{
    public class SyntaxErrorException : ParserException
    {
        public SyntaxErrorException(ParserContext context)
            : base("Syntax error occured while parsing token stream")
        {
            Context = context;
        }

        public ParserContext Context { get; private set; }
    }
}