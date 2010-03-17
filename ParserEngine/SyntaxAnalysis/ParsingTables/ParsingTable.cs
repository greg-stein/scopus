using System.Collections.Generic;
using ParserEngine.Exceptions;
using ParserEngine.LexicalAnalysis;


namespace ParserEngine.SyntaxAnalysis.ParsingTables
{
    public class ParsingTable
    {
        internal ActionTableEntry[,] ActionTable { get; set; }
        internal int[,] GotoTable { get; set; }
		internal Dictionary<ItemSet, ItemSet> ItemSets { get; private set; }
		internal List<ItemSet> ItemSetsList { get; private set; }		// Dictionary makes no guarantees to enumerate elements by order of addition
		internal Dictionary<int, List<Terminal>> First { get; private set; }
		internal Dictionary<int, List<Terminal>> Follow { get; private set; }

		private List<ItemSet[]> mAutomatonGraphTable;
        private readonly AugmentedGrammar mG;

        private ParsingTable()
        {
			ItemSets = new Dictionary<ItemSet, ItemSet>();
			ItemSetsList = new List<ItemSet>();
        }
		
		public ParsingTable(AugmentedGrammar grammar): this()
        {
            mG = grammar;
        	BuildParsingTable();
        }

        internal ParsingTable(ActionTableEntry[,] actionTable, int[,] gotoTable): this()
        {
            ActionTable = actionTable;
            GotoTable = gotoTable;
        }

		internal void BuildParsingTable()
		{
			mAutomatonGraphTable = new List<ItemSet[]>();
			ItemSets.Clear();
			ItemSetsList.Clear();
			
			BuildCollectionOfItemSets();
			BuildGotoTable();
			BuildFirstSets();
			BuildFollowSets();
			BuildActionTable();
			
			mAutomatonGraphTable = null;
		}

		internal void BuildCollectionOfItemSets()
		{
			// all items produced by Goto function will be automatically added to ItemSets and AutomatonGraphTable
			var initialSet = new List<Item> { new Item(mG.InitialProduction, 0) };

			var initialItemSet = new ItemSet(mG, initialSet, ItemSets.Count);
			ItemSets.Add(initialItemSet, initialItemSet);
			ItemSetsList.Add(initialItemSet);
			ItemSet[] newSetArray = new ItemSet[mG.GrammarSymbols.Count];
			mAutomatonGraphTable.Add(newSetArray);

			var collection = new List<ItemSet> { initialItemSet };
			List<ItemSet> accumulator;

			do
			{
				accumulator = new List<ItemSet>();

				foreach (var set in collection)
				{
					for (int i = 0; i < mG.GrammarSymbols.Count; i++)
					{
						var goTo = Goto(set, i);
						if (goTo != null)
						{
							if (!ItemSets.ContainsKey(goTo))
							{
								ItemSets.Add(goTo, goTo);
								ItemSetsList.Add(goTo);
								accumulator.Add(goTo);
							}
							else
								goTo = ItemSets[goTo];

							if (mAutomatonGraphTable.Count > set.ID)
							{
								mAutomatonGraphTable[set.ID][i] = goTo;
							}
							else
							{
								newSetArray = new ItemSet[mG.GrammarSymbols.Count];
								newSetArray[i] = goTo;
								mAutomatonGraphTable.Add(newSetArray);
							}
						}
					}
					if (mAutomatonGraphTable.Count == set.ID)
					{
						newSetArray = new ItemSet[mG.GrammarSymbols.Count];
						mAutomatonGraphTable.Add(newSetArray);
					}
				}
				collection = accumulator;

			} while (accumulator.Count > 0);
		}

		private void BuildGotoTable()
		{
			GotoTable = new int[mAutomatonGraphTable.Count, mG.NonTerminals.Count];
			int nTermID = 0;
			for (int symbolID = 0; symbolID < mG.GrammarSymbols.Count; symbolID++)
			{
				if (mG.GrammarSymbols[symbolID].GetType() == typeof(NonTerminal))
				{
					for (int stateID = 0; stateID < mAutomatonGraphTable.Count; stateID++)
						GotoTable[stateID, nTermID] = (mAutomatonGraphTable[stateID][symbolID] == null)
														?
															-1
														:
															mAutomatonGraphTable[stateID][symbolID].ID;
					nTermID++;
				}
			}
		}

		internal void BuildFirstSets()
		{
			First = new Dictionary<int, List<Terminal>>(mG.NonTerminals.Count);
			foreach (var nTerm in mG.NonTerminals)
			{
				var list = new List<Terminal>();
				// !! possible optimization: not to make multiple recursive calls for inner tree nodes
				AddLeftmostTerminalInNonTerminalSubtree(list, nTerm, new HashSet<NonTerminal> { nTerm });

				First.Add(nTerm.ID, list);
			}
		}
		private void AddLeftmostTerminalInNonTerminalSubtree(ICollection<Terminal> list, NonTerminal nt, HashSet<NonTerminal> processed)
		{
			foreach (var prod in mG.Productions)
				if (prod.Symbol == nt)
				{
					foreach (var symbol in prod.Expression)
					{
						if (symbol.GetType() == typeof(Terminal))
						{
							AddTerminal(list, (Terminal)symbol);
							break;
						}
						if (!processed.Contains((NonTerminal)symbol))
						{
							processed.Add((NonTerminal)symbol);
							AddLeftmostTerminalInNonTerminalSubtree(list, (NonTerminal)symbol, processed);
							break;
						}
					}
				}
		}
		
		internal void BuildFollowSets()
		{
			Follow = new Dictionary<int, List<Terminal>>(mG.NonTerminals.Count+1);
			Follow.Add(mG.InitialProduction.Symbol.ID, new List<Terminal>(1) { mG.UsedTerminals[Lexer.END_MARKER_TOKEN_ID] });
			foreach (var nTerm in mG.NonTerminals)
				Follow.Add(nTerm.ID, new List<Terminal>());

			bool itemsAdded;

			// since this parser does not support epsilon-productions, the calculation is simpler than generic algorithm
			
			do
			{
				itemsAdded = false;

				foreach(var prod in mG.Productions)
				{
					for (int symPos = 0; symPos < prod.Expression.Count; symPos++)
					{
						var symbol = prod.Expression[symPos];
						if (symbol.GetType() == typeof(NonTerminal))
						{
							var nTerm = (NonTerminal)symbol;
							if (symPos == prod.Expression.Count-1)	// A --> aB => copying Follow(A) to Follow(B)
							{
								foreach (var s in Follow[prod.Symbol.ID])
									itemsAdded |= AddTerminal(Follow[nTerm.ID], s);
							}
							else									// A --> aBb => adding First(b) to Follow(B)
							{
								var beta = prod.Expression[symPos + 1];
								if (beta.GetType() == typeof(Terminal))
									itemsAdded |= AddTerminal(Follow[nTerm.ID], (Terminal)beta);
								else
									foreach (var s in First[((NonTerminal)beta).ID])
										itemsAdded |= AddTerminal(Follow[nTerm.ID], s);
							}
						}
					}
				}

			} while (itemsAdded);
		}
		private static bool AddTerminal(ICollection<Terminal> list, Terminal t)
		{
			if (list.Contains(t))
				return false;

			list.Add(t);
			return true;
		}

		private void BuildActionTable()
		{
			// this routine humbly expects that token ID provider will always be the simpliest one (next ID = ID++)
			ActionTable = new ActionTableEntry[mAutomatonGraphTable.Count, mG.TotalTerminalsCount];//mG.UsedTerminals.Count];
			int stateID;

			// shift action is a first priority
			for (stateID = 0; stateID < mAutomatonGraphTable.Count; stateID++)
			{
				for (int symbolID = 0; symbolID < mG.GrammarSymbols.Count; symbolID++)
					if (mG.GrammarSymbols[symbolID].GetType() == typeof(Terminal)	&&
						mAutomatonGraphTable[stateID][symbolID] != null)
					{
						Terminal term = (Terminal)mG.GrammarSymbols[symbolID];
						ActionTable[stateID, term.TokenClassID].Action = ParserAction.Shift;
						ActionTable[stateID, term.TokenClassID].Destination = mAutomatonGraphTable[stateID][symbolID].ID;
					}
			}

			// reduce action is a second priority
			// accept action is a third priority
			// !!! PLINQ
			stateID = 0;
			foreach (var itemSet in ItemSetsList)
			{
				foreach (var item in itemSet.GetKernel())
				{
					if (item.NextToDotSymbol == null)
					{
						if (item.Production.Symbol != mG.InitialProduction.Symbol)
						{
							foreach (var followTerm in Follow[item.Production.Symbol.ID])
							{
								if (ActionTable[stateID, followTerm.TokenClassID].Action != ParserAction.Error)
									throw new ParserException("Provided grammar is not compliant with SLR(1) parser.");
								ActionTable[stateID, followTerm.TokenClassID].Action = ParserAction.Reduce;
								ActionTable[stateID, followTerm.TokenClassID].Destination = item.Production.ID;
							}
						}
						else
						{
							if (ActionTable[stateID, Lexer.END_MARKER_TOKEN_ID].Action != ParserAction.Error)
								throw new ParserException("Provided grammar is not compliant with SLR(1) parser.");
							ActionTable[stateID, Lexer.END_MARKER_TOKEN_ID].Action = ParserAction.Accept;
						}
					}
				}
				stateID++;
			}
		}

		internal ItemSet Goto(ItemSet itemSet, int grammarSymbolID)
		{
			var gotoKernel = new List<Item>();

			foreach (var item in itemSet)
				if (item.NextToDotSymbol == mG.GrammarSymbols[grammarSymbolID])
					gotoKernel.Add(new Item(item.Production, item.DotPosition + 1));

			return (gotoKernel.Count > 0) ? new ItemSet(mG, gotoKernel, ItemSets.Count) : null;
		}

		internal static List<Item> Closure(Grammar grammar, List<Item> itemList)
		{
			var closureSet = new HashSet<Item>(itemList);	// speeds up search
			var closureList = new List<Item>(itemList);
			List<Item> accumulatingList;
			var currentList = closureList;

			do
			{
				accumulatingList = new List<Item>();

				foreach (var item in currentList)
				{
					foreach (var production in grammar)
					{
						if (production.Symbol == item.NextToDotSymbol)
						{
							var newItem = new Item(production, 0);
							if (!closureSet.Contains(newItem) && !accumulatingList.Contains(newItem))
								accumulatingList.Add(newItem);
						}
					}
				}

				closureSet.UnionWith(accumulatingList);
				closureList.AddRange(accumulatingList);
				currentList = accumulatingList;

			} while (accumulatingList.Count > 0);

			return closureList;
		}
	}
}