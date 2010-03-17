using System.Collections;
using System.Collections.Generic;
using ParserEngine.LexicalAnalysis;


namespace ParserEngine.SyntaxAnalysis
{
    public class Grammar : ICollection<Production>
    {
        internal List<Production> Productions { get; private set; }			// contains productions, which configurate this grammar
        internal List<NonTerminal> NonTerminals { get; private set; }		// contains all non-terminal symbols, met in productions
        internal List<Terminal> UsedTerminals { get; private set; }			// contains terminals, only found in productions;
																			// may not be equal to amount of tokens added to tokenizer
    	internal int TotalTerminalsCount;									// total amount of tokens (terminals) added to tokenizer
        internal List<GrammarEntity> GrammarSymbols { get; private set; }	// contains union of UsedTerminals and NonTerminals

        private Grammar()
        {
            Productions = new List<Production>();
            NonTerminals = new List<NonTerminal>();
            UsedTerminals = new List<Terminal>();
            GrammarSymbols = new List<GrammarEntity>();

			Terminal endMark = new Terminal(Lexer.END_MARKER_TOKEN_NAME, Lexer.END_MARKER_TOKEN_ID);
			UsedTerminals.Add(endMark);
			GrammarSymbols.Add(endMark);
        }

		public Grammar(int totalTerminalsCount): this()
		{
			TotalTerminalsCount = totalTerminalsCount;
		}

        public IEnumerator<Production> GetEnumerator()
        {
            return Productions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(Production production)
        {
            Productions.Add(production);
            production.ID = Productions.IndexOf(production);

            var expression = new List<GrammarEntity>(production.Expression) { production.Symbol };

            foreach (var entity in expression)
            {
                if (!GrammarSymbols.Contains(entity)) GrammarSymbols.Add(entity);

                var terminal = (entity as Terminal);
                if (terminal != null && !UsedTerminals.Contains(terminal))
                {
                    UsedTerminals.Add(terminal);
                    continue;
                }

                var nonTerminal = (entity as NonTerminal);
                if (nonTerminal != null && !NonTerminals.Contains(nonTerminal))
                {
                    NonTerminals.Add(nonTerminal);
                    nonTerminal.ID = NonTerminals.IndexOf(nonTerminal);
                }
            }
        }

        public Production this[int index]
        {
            get
            {
                return Productions[index];
            }
        }

        public void Clear()
        {
            Productions.Clear();
        }

        public bool Contains(Production item)
        {
            return Productions.Contains(item);
        }

        public void CopyTo(Production[] array, int arrayIndex)
        {
            Productions.CopyTo(array, arrayIndex);
        }

        public bool Remove(Production item)
        {
            return Productions.Remove(item);
        }

        public int Count
        {
            get
            {
                return Productions.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override string ToString()
        {
            string value = string.Empty;
            foreach (var production in Productions)
            {
                value += string.Format("({0}) {1}\n", production.ID, production);
            }

            return value;
        }
    }
}