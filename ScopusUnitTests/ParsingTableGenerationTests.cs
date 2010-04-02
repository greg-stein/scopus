using System.Collections.Generic;
using NUnit.Framework;
using Scopus.LexicalAnalysis;
using Scopus.SyntaxAnalysis;
using Scopus.SyntaxAnalysis.ParsingTables;

namespace ScopusUnitTests
{
    [TestFixture]
    public class ParsingTableGenerationTests
    {
        private NonTerminal E;
        private NonTerminal T;
        private NonTerminal F;

        private Terminal id;
        private Terminal plus;
        private Terminal mult;
        private Terminal leftBrace;
        private Terminal rightBrace;
        private Terminal endMark;

        private RegExpTokenizer tokenizer;
        private AugmentedGrammar grammar;

        public ParsingTableGenerationTests()
        {
            E = new NonTerminal("E");
            T = new NonTerminal("T");
            F = new NonTerminal("F");

            tokenizer = new RegExpTokenizer();

            id = tokenizer.AddToken("[0-9]+");
            plus = tokenizer.AddToken("+");
            mult = tokenizer.AddToken("*");
            leftBrace = tokenizer.AddToken("(");
            rightBrace = tokenizer.AddToken(")");
            endMark = tokenizer.AddToken("$");

            grammar = new AugmentedGrammar(tokenizer.TotalTokensCount)
                              {
                                  E --> E & plus & T,
                                  E --> T,
                                  T --> T & mult & F,
                                  T --> F,
                                  F --> leftBrace & E & rightBrace,
                                  F --> id
                              };
        }

        [Test]
        public void ClosureCalculationTest()
        {
			var list = new List<Item> { new Item(grammar.InitialProduction, 0) };
        	var closure = ParsingTable.Closure(grammar, list);

            var expectedClosure = new HashSet<Item>
                                      {
                                          new Item(grammar.InitialProduction, 0),
                                          new Item(E --> E & plus & T, 0),
                                          new Item(E --> T, 0),
                                          new Item(T --> T & mult & F, 0),
                                          new Item(T --> F, 0),
                                          new Item(F --> leftBrace & E & rightBrace, 0),
                                          new Item(F --> id, 0)
                                      };

            Assert.That(closure, Is.EquivalentTo(expectedClosure));
        }

        [Test]
        public void GotoCalculationTest()
        {
			var list = new List<Item>
                          {
                              new Item(grammar.InitialProduction, 1),
                              new Item(E --> E & plus & T, 1)
                          };

			var goTo = new ParsingTable(grammar).Goto(new ItemSet(grammar, list, 0), grammar.GrammarSymbols.IndexOf(plus));
            var expectedGoto = new HashSet<Item>
                                      {
                                          new Item(E --> E & plus & T, 2),
                                          new Item(T --> T & mult & F, 0),
                                          new Item(T --> F, 0),
                                          new Item(F --> leftBrace & E & rightBrace, 0),
                                          new Item(F --> id, 0)
                                      };

            Assert.That(goTo, Is.EquivalentTo(expectedGoto));
        }

		[Test]
		public void GotoTableCalculationTest()
		{
			var pt = new ParsingTable(grammar);

			Assert.That(pt.GotoTable[0, 0], Is.EqualTo(1));
			Assert.That(pt.GotoTable[0, 1], Is.EqualTo(2));
			Assert.That(pt.GotoTable[0, 2], Is.EqualTo(3));
			Assert.That(pt.GotoTable[1, 0], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[1, 1], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[1, 2], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[2, 0], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[2, 1], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[2, 2], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[3, 0], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[3, 1], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[3, 2], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[4, 0], Is.EqualTo(8));
			Assert.That(pt.GotoTable[4, 1], Is.EqualTo(2));
			Assert.That(pt.GotoTable[4, 2], Is.EqualTo(3));
			Assert.That(pt.GotoTable[5, 0], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[5, 1], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[5, 2], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[6, 0], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[6, 1], Is.EqualTo(9));
			Assert.That(pt.GotoTable[6, 2], Is.EqualTo(3));
			Assert.That(pt.GotoTable[7, 0], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[7, 1], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[7, 2], Is.EqualTo(10));
			Assert.That(pt.GotoTable[8, 0], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[8, 1], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[8, 2], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[9, 0], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[9, 1], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[9, 2], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[10, 0], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[10, 1], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[10, 2], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[11, 0], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[11, 1], Is.EqualTo(-1));
			Assert.That(pt.GotoTable[11, 2], Is.EqualTo(-1));
		}

		[Test]
		public void FollowSetsCalculationTest()
		{
			var pt = new ParsingTable(grammar);

			Assert.That(pt.Follow.Count, Is.EqualTo(4));
			var followE = new List<Terminal> { grammar.UsedTerminals[Terminal.END_MARKER_TOKEN_ID], plus, rightBrace };
			Assert.That(pt.Follow[E.ID], Is.EqualTo(followE));
			var followT = new List<Terminal> { grammar.UsedTerminals[Terminal.END_MARKER_TOKEN_ID], plus, mult, rightBrace, };
			Assert.That(pt.Follow[T.ID], Is.EqualTo(followT));
			var followF = new List<Terminal> { grammar.UsedTerminals[Terminal.END_MARKER_TOKEN_ID], plus, mult, rightBrace, };
			Assert.That(pt.Follow[F.ID], Is.EqualTo(followF));
		}

		[Test]
		public void ActionTableCalculationTest()
		{
			var pt = new ParsingTable(grammar);

			Assert.That(pt.ActionTable[0, 1].Action, Is.EqualTo(ParserAction.Shift));
			Assert.That(pt.ActionTable[0, 1].Destination, Is.EqualTo(5));
			Assert.That(pt.ActionTable[0, 4].Action, Is.EqualTo(ParserAction.Shift));
			Assert.That(pt.ActionTable[0, 4].Destination, Is.EqualTo(4));
			Assert.That(pt.ActionTable[1, 0].Action, Is.EqualTo(ParserAction.Accept)); // <-- !!!!
			Assert.That(pt.ActionTable[1, 2].Action, Is.EqualTo(ParserAction.Shift));
			Assert.That(pt.ActionTable[1, 2].Destination, Is.EqualTo(6));
			Assert.That(pt.ActionTable[2, 0].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[2, 0].Destination, Is.EqualTo(2));
			Assert.That(pt.ActionTable[2, 2].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[2, 2].Destination, Is.EqualTo(2));
			Assert.That(pt.ActionTable[2, 3].Action, Is.EqualTo(ParserAction.Shift));
			Assert.That(pt.ActionTable[2, 3].Destination, Is.EqualTo(7));
			Assert.That(pt.ActionTable[2, 5].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[2, 5].Destination, Is.EqualTo(2));
			Assert.That(pt.ActionTable[3, 0].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[3, 0].Destination, Is.EqualTo(4));
			Assert.That(pt.ActionTable[3, 2].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[3, 2].Destination, Is.EqualTo(4));
			Assert.That(pt.ActionTable[3, 3].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[3, 3].Destination, Is.EqualTo(4));
			Assert.That(pt.ActionTable[3, 5].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[3, 5].Destination, Is.EqualTo(4));
			Assert.That(pt.ActionTable[4, 1].Action, Is.EqualTo(ParserAction.Shift));
			Assert.That(pt.ActionTable[4, 1].Destination, Is.EqualTo(5));
			Assert.That(pt.ActionTable[4, 4].Action, Is.EqualTo(ParserAction.Shift));
			Assert.That(pt.ActionTable[4, 4].Destination, Is.EqualTo(4));
			Assert.That(pt.ActionTable[5, 0].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[5, 0].Destination, Is.EqualTo(6));
			Assert.That(pt.ActionTable[5, 2].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[5, 2].Destination, Is.EqualTo(6));
			Assert.That(pt.ActionTable[5, 3].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[5, 3].Destination, Is.EqualTo(6));
			Assert.That(pt.ActionTable[5, 5].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[5, 5].Destination, Is.EqualTo(6));
			Assert.That(pt.ActionTable[6, 1].Action, Is.EqualTo(ParserAction.Shift));
			Assert.That(pt.ActionTable[6, 1].Destination, Is.EqualTo(5));
			Assert.That(pt.ActionTable[6, 4].Action, Is.EqualTo(ParserAction.Shift));
			Assert.That(pt.ActionTable[6, 4].Destination, Is.EqualTo(4));
			Assert.That(pt.ActionTable[7, 1].Action, Is.EqualTo(ParserAction.Shift));
			Assert.That(pt.ActionTable[7, 1].Destination, Is.EqualTo(5));
			Assert.That(pt.ActionTable[7, 4].Action, Is.EqualTo(ParserAction.Shift));
			Assert.That(pt.ActionTable[7, 4].Destination, Is.EqualTo(4));
			Assert.That(pt.ActionTable[8, 2].Action, Is.EqualTo(ParserAction.Shift));
			Assert.That(pt.ActionTable[8, 2].Destination, Is.EqualTo(6));
			Assert.That(pt.ActionTable[8, 5].Action, Is.EqualTo(ParserAction.Shift));
			Assert.That(pt.ActionTable[8, 5].Destination, Is.EqualTo(11));
			Assert.That(pt.ActionTable[9, 0].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[9, 0].Destination, Is.EqualTo(1));
			Assert.That(pt.ActionTable[9, 2].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[9, 2].Destination, Is.EqualTo(1));
			Assert.That(pt.ActionTable[9, 3].Action, Is.EqualTo(ParserAction.Shift));
			Assert.That(pt.ActionTable[9, 3].Destination, Is.EqualTo(7));
			Assert.That(pt.ActionTable[9, 5].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[9, 5].Destination, Is.EqualTo(1));
			Assert.That(pt.ActionTable[10, 0].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[10, 0].Destination, Is.EqualTo(3));
			Assert.That(pt.ActionTable[10, 2].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[10, 2].Destination, Is.EqualTo(3));
			Assert.That(pt.ActionTable[10, 3].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[10, 3].Destination, Is.EqualTo(3));
			Assert.That(pt.ActionTable[10, 5].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[10, 5].Destination, Is.EqualTo(3));
			Assert.That(pt.ActionTable[11, 0].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[11, 0].Destination, Is.EqualTo(5));
			Assert.That(pt.ActionTable[11, 2].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[11, 2].Destination, Is.EqualTo(5));
			Assert.That(pt.ActionTable[11, 3].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[11, 3].Destination, Is.EqualTo(5));
			Assert.That(pt.ActionTable[11, 5].Action, Is.EqualTo(ParserAction.Reduce));
			Assert.That(pt.ActionTable[11, 5].Destination, Is.EqualTo(5));

			//Assert.That(pt.ActionTable[0, 0].Action, Is.EqualTo(ParserAction.Shift));
			//Assert.That(pt.ActionTable[0, 0].Destination, Is.EqualTo(5));
			//Assert.That(pt.ActionTable[0, 3].Action, Is.EqualTo(ParserAction.Shift));
			//Assert.That(pt.ActionTable[0, 3].Destination, Is.EqualTo(4));
			//Assert.That(pt.ActionTable[1, 1].Action, Is.EqualTo(ParserAction.Shift));
			//Assert.That(pt.ActionTable[1, 1].Destination, Is.EqualTo(6));
			////Assert.That(pt.ActionTable[1, 5].Action, Is.EqualTo(ParserAction.Accept)); // <-- !!!!
			//Assert.That(pt.ActionTable[2, 1].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[2, 1].Destination, Is.EqualTo(2));
			//Assert.That(pt.ActionTable[2, 2].Action, Is.EqualTo(ParserAction.Shift));
			//Assert.That(pt.ActionTable[2, 2].Destination, Is.EqualTo(7));
			//Assert.That(pt.ActionTable[2, 4].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[2, 4].Destination, Is.EqualTo(2));
			////Assert.That(pt.ActionTable[2, 5].Action, Is.EqualTo(ParserAction.Reduce));
			////Assert.That(pt.ActionTable[2, 5].Argument, Is.EqualTo(2));
			//Assert.That(pt.ActionTable[3, 1].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[3, 1].Destination, Is.EqualTo(4));
			//Assert.That(pt.ActionTable[3, 2].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[3, 2].Destination, Is.EqualTo(4));
			//Assert.That(pt.ActionTable[3, 4].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[3, 4].Destination, Is.EqualTo(4));
			////Assert.That(pt.ActionTable[3, 5].Action, Is.EqualTo(ParserAction.Reduce));
			////Assert.That(pt.ActionTable[3, 5].Argument, Is.EqualTo(4));
			//Assert.That(pt.ActionTable[4, 0].Action, Is.EqualTo(ParserAction.Shift));
			//Assert.That(pt.ActionTable[4, 0].Destination, Is.EqualTo(5));
			//Assert.That(pt.ActionTable[4, 3].Action, Is.EqualTo(ParserAction.Shift));
			//Assert.That(pt.ActionTable[4, 3].Destination, Is.EqualTo(4));
			//Assert.That(pt.ActionTable[5, 1].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[5, 1].Destination, Is.EqualTo(6));
			//Assert.That(pt.ActionTable[5, 2].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[5, 2].Destination, Is.EqualTo(6));
			//Assert.That(pt.ActionTable[5, 4].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[5, 4].Destination, Is.EqualTo(6));
			////Assert.That(pt.ActionTable[5, 5].Action, Is.EqualTo(ParserAction.Reduce));
			////Assert.That(pt.ActionTable[5, 5].Argument, Is.EqualTo(6));
			//Assert.That(pt.ActionTable[6, 0].Action, Is.EqualTo(ParserAction.Shift));
			//Assert.That(pt.ActionTable[6, 0].Destination, Is.EqualTo(5));
			//Assert.That(pt.ActionTable[6, 3].Action, Is.EqualTo(ParserAction.Shift));
			//Assert.That(pt.ActionTable[6, 3].Destination, Is.EqualTo(4));
			//Assert.That(pt.ActionTable[7, 0].Action, Is.EqualTo(ParserAction.Shift));
			//Assert.That(pt.ActionTable[7, 0].Destination, Is.EqualTo(5));
			//Assert.That(pt.ActionTable[7, 3].Action, Is.EqualTo(ParserAction.Shift));
			//Assert.That(pt.ActionTable[7, 3].Destination, Is.EqualTo(4));
			//Assert.That(pt.ActionTable[8, 1].Action, Is.EqualTo(ParserAction.Shift));
			//Assert.That(pt.ActionTable[8, 1].Destination, Is.EqualTo(6));
			//Assert.That(pt.ActionTable[8, 4].Action, Is.EqualTo(ParserAction.Shift));
			//Assert.That(pt.ActionTable[8, 4].Destination, Is.EqualTo(11));
			//Assert.That(pt.ActionTable[9, 1].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[9, 1].Destination, Is.EqualTo(1));
			//Assert.That(pt.ActionTable[9, 2].Action, Is.EqualTo(ParserAction.Shift));
			//Assert.That(pt.ActionTable[9, 2].Destination, Is.EqualTo(7));
			//Assert.That(pt.ActionTable[9, 4].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[9, 4].Destination, Is.EqualTo(1));
			////Assert.That(pt.ActionTable[9, 5].Action, Is.EqualTo(ParserAction.Reduce));
			////Assert.That(pt.ActionTable[9, 5].Argument, Is.EqualTo(1));
			//Assert.That(pt.ActionTable[10, 1].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[10, 1].Destination, Is.EqualTo(3));
			//Assert.That(pt.ActionTable[10, 2].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[10, 2].Destination, Is.EqualTo(3));
			//Assert.That(pt.ActionTable[10, 4].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[10, 4].Destination, Is.EqualTo(3));
			////Assert.That(pt.ActionTable[10, 5].Action, Is.EqualTo(ParserAction.Reduce));
			////Assert.That(pt.ActionTable[10, 5].Argument, Is.EqualTo(3));
			//Assert.That(pt.ActionTable[11, 1].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[11, 1].Destination, Is.EqualTo(5));
			//Assert.That(pt.ActionTable[11, 2].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[11, 2].Destination, Is.EqualTo(5));
			//Assert.That(pt.ActionTable[11, 4].Action, Is.EqualTo(ParserAction.Reduce));
			//Assert.That(pt.ActionTable[11, 4].Destination, Is.EqualTo(5));
			////Assert.That(pt.ActionTable[11, 5].Action, Is.EqualTo(ParserAction.Reduce));
			////Assert.That(pt.ActionTable[11, 5].Argument, Is.EqualTo(5));
		}
	}

}
