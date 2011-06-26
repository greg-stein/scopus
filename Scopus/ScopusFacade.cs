using Scopus.LexicalAnalysis;
using Scopus.LexicalAnalysis.Algorithms;
using Scopus.SyntaxAnalysis;
using Scopus.SyntaxAnalysis.ParsingTables;

namespace Scopus
{
    public static class ScopusFacade
    {
        public static Lexer GetLexer()
        {
            var tokenizer = new RegExpTokenizer();
            tokenizer.SetTransitionFunction(new TableDrivenTransitionFunction());

            return new Lexer(tokenizer);
        }

        public static LRParser GetSLRParser(AugmentedGrammar grammar, Lexer lexer)
        {
            var parsingTableBuilder = new SLRParsingTableBuilder(lexer.TokensNumber);
            parsingTableBuilder.SetGrammar(grammar);
            parsingTableBuilder.ConstructParsingTable();
            var parser = new LRParser
                             {
                                 ParsingTable = parsingTableBuilder.GetTable(),
                                 Lexer = lexer,
                                 Grammar = grammar
                             };

            return parser;
        }
    }
}
