using Scopus.LexicalAnalysis;

namespace Scopus.Exceptions
{
    /// <summary>
    /// Thrown when unexpected lexeme was read from lexeme stream.
    /// </summary>
    public class UnexpectedTokenException : ParserException
    {
        public UnexpectedTokenException(Token token)
            : base(string.Format("Unexpected lexeme at {2} ({0}:'{1}')",
                                 token.GetType().Name, token, token.Offset))
        {
            Token = (Token) token.Clone();
        }

        public Token Token { get; private set; }
    }
}