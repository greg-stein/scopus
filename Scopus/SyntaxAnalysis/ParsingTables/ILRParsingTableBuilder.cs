namespace Scopus.SyntaxAnalysis.ParsingTables
{
    interface ILRParsingTableBuilder
    {
        void SetGrammar(Grammar g);
        void ConstructParsingTable();
        LRParsingTable GetTable();
    }
}
