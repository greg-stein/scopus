using Scopus.LexicalAnalysis;

namespace Scopus.Exceptions
{
    /// <summary>
    /// Thrown when unexpected lexeme was read from lexeme stream.
    /// </summary>
    public class UnexpectedTokenException : ParserException
    {
        public UnexpectedTokenException(Token token)
            : base(string.Format("Unexpected lexeme at {2} ({0}:'{1}'). Environment ASCII: {3}",
                                 token.GetType().Name, token, token.Offset, token.GetEnvironment()))
        {
            Token = (Token) token.Clone();
        }

        public Token Token { get; private set; }
    }

    internal static class TokenEnvironmentAspect
    {
        internal static string GetEnvironment(this Token token)
        {
            const int ENVIRONMENT_LENGTH = 10;


            int actualEnvironmentLength = ENVIRONMENT_LENGTH;

            if (token.Offset > token.Buffer.Length - ENVIRONMENT_LENGTH - 1)
            {
                actualEnvironmentLength = token.Buffer.Length - token.Offset;
            }

            return System.Text.Encoding.ASCII.GetString(token.Buffer, token.Offset, actualEnvironmentLength);
        }
    }
}