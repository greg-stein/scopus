namespace Scopus.SyntaxAnalysis
{
    public class Terminal : GrammarEntity
    {
        internal int TokenClassID { get; set; }

        internal Terminal(string name, int classID) : base(name)
        {
            TokenClassID = classID;
        }

        internal const int END_MARKER_TOKEN_ID = 0;					// class ID for fully synthetic token of END_MARK
        internal const string END_MARKER_TOKEN_NAME = "<end>";		// name for fully synthetic token of END_MARK (used only in representation)
    }
}