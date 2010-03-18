using Scopus.SyntaxAnalysis;

namespace Scopus.Exceptions
{
    public class SyntaxErrorException : ParserException
    {
        public ParserContext Context { get; private set; }

        public SyntaxErrorException(ParserContext context)
            : base("Syntax error occured while parsing token stream")
        {
            Context = context;
        }
    }
}
