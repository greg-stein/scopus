namespace Scopus.SyntaxAnalysis.ParsingTables
{
    interface IParsingTableBuilder
    {
        void SetGrammar(Grammar g);
        void ConstructParsingTable();
        LRParsingTable GetTable();
    }
}
