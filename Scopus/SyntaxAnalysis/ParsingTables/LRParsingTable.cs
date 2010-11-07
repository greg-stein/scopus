namespace Scopus.SyntaxAnalysis.ParsingTables
{
    public class LRParsingTable
    {
        internal ActionTableEntry[,] ActionTable { get; set; }
        internal int[,] GotoTable { get; set; }

        internal LRParsingTable()
        {
        }

        internal LRParsingTable(ActionTableEntry[,] actionTable, int[,] gotoTable)
        {
            ActionTable = actionTable;
            GotoTable = gotoTable;
        }
    }
}