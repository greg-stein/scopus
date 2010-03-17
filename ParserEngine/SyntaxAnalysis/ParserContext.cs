using System.Collections.Generic;
using ParserEngine.LexicalAnalysis;

namespace ParserEngine.SyntaxAnalysis
{
    public class ParserContext
    {
        public Stack<int> StatesStack { get; private set; }
        public ILexer Lexer { get; private set; }
        public Token CurrentToken { get; private set; }
        
        public ParserContext(Stack<int> statesStack, ILexer lexer, Token currentToken)
        {
            StatesStack = statesStack;
            Lexer = lexer;
            CurrentToken = currentToken;
        }
    }
}
