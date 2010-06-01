namespace Scopus.LexicalAnalysis
{
    internal class AcceptingState : State
    {
        internal int TokenClassID { get; set; }
        
        internal AcceptingState(string name) : base(name)
        {
        }
    }
}