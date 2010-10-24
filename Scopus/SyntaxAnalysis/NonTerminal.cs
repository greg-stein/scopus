using System;

namespace Scopus.SyntaxAnalysis
{
    public class NonTerminal : GrammarEntity
    {
        private bool mWillbBeUsedAsProductionSymbol;

        public NonTerminal(string name) : base(name)
        {
            ID = -1;
        }

        internal int ID { get; set; }

        public static NonTerminal operator --(NonTerminal nonTerminal)
        {
            nonTerminal.mWillbBeUsedAsProductionSymbol = true;
            return nonTerminal;
        }

        public static Production operator >(NonTerminal nonTerminal, GrammarEntity entity)
        {
            if (!nonTerminal.mWillbBeUsedAsProductionSymbol)
                throw new InvalidOperationException(
                    "Syntax error. In order to generate production, operator \"--\" should stand before operator \">\".");

            return new Production(nonTerminal, entity, null);
        }

        public static Production operator <(NonTerminal nonTerminal, GrammarEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}