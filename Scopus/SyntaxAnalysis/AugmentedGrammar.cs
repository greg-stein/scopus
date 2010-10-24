namespace Scopus.SyntaxAnalysis
{
    public class AugmentedGrammar : Grammar
    {
        public const int INITAL_SYMBOL_ID = 0;
        public const string INITAL_SYMBOL_NAME = "S'";

        public Production InitialProduction
        {
            get { return this[INITAL_SYMBOL_ID]; }
        }

        public override void Add(Production item)
        {
            if (Productions.Count == 0)
            {
                var initialProduction = new Production(new NonTerminal(INITAL_SYMBOL_NAME), item.Symbol, null)
                                            {ID = INITAL_SYMBOL_ID};
                Productions.Add(initialProduction);
            }

            base.Add(item);
        }
    }
}