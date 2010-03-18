using System;

namespace Scopus.Exceptions
{
    /// <summary>
    /// Repersents base for all exceptions occurred during parsing process.
    /// </summary>
    public class ParserException : Exception
    {
		internal ParserException() {}

		internal ParserException(string message) : base(message) {}

		internal ParserException(string message, Exception innerException) : base(message, innerException) {}
    }
}
