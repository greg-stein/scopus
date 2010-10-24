using System;
using Scopus.LexicalAnalysis;

namespace Scopus.SyntaxAnalysis
{
    public delegate void SyntaxErrorEventHandler(IParser sender, ParserContext context);


    public interface IParser
    {
        ILexer Lexer { get; set; }
        Grammar Grammar { get; set; }

        event SyntaxErrorEventHandler SyntaxError;
        event EventHandler InputAccepted;

        void ParseInput();
    }
}