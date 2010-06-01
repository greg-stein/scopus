namespace Scopus.LexicalAnalysis
{
    internal class FiniteAutomata
    {
        private string name = "Default";
        internal State StartState { get; set; }
        internal State Terminator { get; set; }

        internal FiniteAutomata(string name, bool createMarginalStates)
        {
            if (createMarginalStates)
            {
                StartState = new State(name + "::StartState");
                Terminator = new State(name + "::Terminator");
            }

            this.name = name;
        }

        internal FiniteAutomata()
            : this("Default", true)
        {}

        internal FiniteAutomata(string name)
            :this(name, true)
        {}
    }
}