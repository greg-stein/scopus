namespace Scopus.SyntaxAnalysis
{
    public class Terminal : GrammarEntity
    {
        internal const int EPSILON_TOKEN_ID = 0;
        internal const int END_MARKER_TOKEN_ID = 0; // class ID for fully synthetic token of END_MARK

        internal const string END_MARKER_TOKEN_NAME = "<end>";
                              // name for fully synthetic token of END_MARK (used only in representation)

        public static string EPSILON_TOKEN_NAME = "epsilon";

        internal Terminal(string name, int classID) : base(name)
        {
            TokenClassID = classID;
        }

        internal int TokenClassID { get; set; }
    }
}