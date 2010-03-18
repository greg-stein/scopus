namespace Scopus.SyntaxAnalysis
{
    public class Terminal : GrammarEntity
    {
        internal int TokenClassID { get; set; }

        internal Terminal(string name, int classID) : base(name)
        {
            TokenClassID = classID;
        }
    }
}