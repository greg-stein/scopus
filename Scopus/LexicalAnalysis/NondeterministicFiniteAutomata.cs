namespace Scopus.LexicalAnalysis
{
    internal class NondeterministicFiniteAutomata
    {
        private string name = "Default";
        internal State StartState { get; set; }
        internal State Terminator { get; set; }

        internal NondeterministicFiniteAutomata(string name, bool createMarginalStates)
        {
            if (createMarginalStates)
            {
                StartState = new State(name + "::StartState");
                Terminator = new State(name + "::Terminator");
            }

            this.name = name;
        }

        internal NondeterministicFiniteAutomata()
            : this("Default", true)
        {}

        internal NondeterministicFiniteAutomata(string name)
            :this(name, true)
        {}
    }
}