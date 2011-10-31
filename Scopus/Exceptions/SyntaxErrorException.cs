using Scopus.SyntaxAnalysis;

namespace Scopus.Exceptions
{
    public class SyntaxErrorException : ParserException
    {
        public SyntaxErrorException(ParserContext context)
            : base(string.Format("Syntax error occured while parsing token stream. Token environment: {0}", 
            context.CurrentToken.GetEnvironment()))
        {
            Context = context; 
        }

        public ParserContext Context { get; private set; }
    }
}