using System;
using Scopus.SyntaxAnalysis;

namespace Scopus.LexicalAnalysis.RegularExpressions
{
    // To be implemented using following grammars:
    // POSIX - http://www.opengroup.org/onlinepubs/009695399/basedefs/xbd_chap09.html
    // FLEX  - http://dinosaur.compilertools.net/flex/flex_7.html#SEC7

    internal class POSIXRegExpParser : RegExpParser
    {
        private void ConstructParser()
        {
            // a*
            var lexer = ScopusFacade.GetLexer();
            // var literal = ???
            var star = lexer.Tokenizer.UseTerminal(RegExp.Literal("*"));
            var plus = lexer.Tokenizer.UseTerminal(RegExp.Literal("+"));
            var leftBr = lexer.Tokenizer.UseTerminal(RegExp.Literal("("));
            var rightBr = lexer.Tokenizer.UseTerminal(RegExp.Literal(")"));

            var regexp = new NonTerminal("regexp");
            
            var grammar = new AugmentedGrammar()
                              {
                                  regexp --> regexp & star
                              };

            var parser = ScopusFacade.GetSLRParser(grammar, lexer);
        }

        internal override RegExp Parse(string regexp)
        {
            throw new NotImplementedException();
        }
    }
}
