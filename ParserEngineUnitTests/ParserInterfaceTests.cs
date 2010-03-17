using System;
using NUnit.Framework;
using ParserEngine.LexicalAnalysis;
using ParserEngine.SyntaxAnalysis;

namespace ParserEngineUnitTests
{
    [TestFixture]
    public class ParserInterfaceTests
    {
        [Test]
        public void GrammarEntitySequenceTest()
        {
            NonTerminal E = new NonTerminal("E"), T = new NonTerminal("T"), F = new NonTerminal("F");

            var prod = E --> T & F;
            
            Assert.That(prod.ToString(), Is.EqualTo("E --> T F"));
            Console.WriteLine(prod);
        }

        [Test]
        public void GrammarEntitySequenceWithTokensTest()
        {
            var tokenizer = new RegExpTokenizer();

            NonTerminal E = new NonTerminal("E"), T = new NonTerminal("T"), F = new NonTerminal("F");

            var plus = tokenizer.AddToken("+");
            var mult = tokenizer.AddToken("*");
            var leftBrace = tokenizer.AddToken("(");
            var rightBrace = tokenizer.AddToken(")");
            var id = tokenizer.IntegerNumber;

            var prod = E --> T & mult & leftBrace & id & plus & id & rightBrace;

			Assert.That(prod.ToString(), Is.EqualTo("E --> T * ( " + Lexer.INTEGER_NUMBER_TOKEN_NAME + " + " +
																	 Lexer.INTEGER_NUMBER_TOKEN_NAME + " )"));
            Console.WriteLine(prod);
        }

        [Test]
        public void GrammarRecoursiveProductionTest()
        {
            var tokenizer = new KeywordsTokenizer();
            NonTerminal E = new NonTerminal("E"), T = new NonTerminal("T"), F = new NonTerminal("F");

            var plus = tokenizer.AddToken("+");
            var mult = tokenizer.AddToken("*");

            Assert.That((E --> E & F).ToString(), Is.EqualTo("E --> E F"));
            Assert.That((E --> F & E).ToString(), Is.EqualTo("E --> F E"));
            Assert.That((E --> E).ToString(), Is.EqualTo("E --> E"));
            Assert.That((E --> F & E & T).ToString(), Is.EqualTo("E --> F E T"));
            Assert.That((E --> plus & E).ToString(), Is.EqualTo("E --> + E"));
            Assert.That((E --> E & plus).ToString(), Is.EqualTo("E --> E +"));
            Assert.That((E --> plus & E & mult).ToString(), Is.EqualTo("E --> + E *"));
        }

        [Test]
        public void ProductionWithSemanticActionTest()
        {
            var tokenizer = new KeywordsTokenizer();
            NonTerminal E = new NonTerminal("E"), T = new NonTerminal("T"), F = new NonTerminal("F");

            var plus = tokenizer.AddToken("+");
            var mult = tokenizer.AddToken("*");

            int testValue = 0;

			var g = new Grammar(tokenizer.TotalTokensCount)
                        {
                            E --> T & F ^ (v => testValue = 3),
                            T --> plus,
                            E --> T & plus ^ (v => testValue = 5),
                            E --> plus & mult & T ^ (v => testValue = 8),
                        };

            Assert.That(testValue, Is.EqualTo(0));
            g.Productions[0].SemanticAction(null);
            Assert.That(testValue, Is.EqualTo(3));
            g.Productions[2].SemanticAction(null);
            Assert.That(testValue, Is.EqualTo(5));
            g.Productions[3].SemanticAction(null);
            Assert.That(testValue, Is.EqualTo(8));
        }

        [Test]
        public void ProductionWithSemanticActionComplicatedTest()
        {
            var tokenizer = new RegExpTokenizer();

            var E = new NonTerminal("E");
            var T = new NonTerminal("T");
            var F = new NonTerminal("F");

            var id = tokenizer.IntegerNumber;
            var plus = tokenizer.AddToken("+");
            var mult = tokenizer.AddToken("*");
            var leftBrace = tokenizer.AddToken("(");
            var rightBrace = tokenizer.AddToken(")");
            var endMark = tokenizer.AddToken("$");

            var kuj = 0;

            var prod1 = E --> E & plus & T ^ (v => kuj = 2);
            var prod2 = E --> T;
            var prod3 = T --> T & mult & F ^ (v => kuj = 3);
            var prod4 = T --> F;
            var prod5 = F --> leftBrace & E & rightBrace;
			var prod6 = F --> id ^ (v => kuj = 4);

            Assert.That(kuj, Is.EqualTo(0));
            prod1.PerformSemanticAction(null);
            Assert.That(kuj, Is.EqualTo(2));
            prod2.PerformSemanticAction(null);
            Assert.That(kuj, Is.EqualTo(2));
            prod3.PerformSemanticAction(null);
            Assert.That(kuj, Is.EqualTo(3));
            prod4.PerformSemanticAction(null);
            Assert.That(kuj, Is.EqualTo(3));
            prod5.PerformSemanticAction(null);
            Assert.That(kuj, Is.EqualTo(3));
            prod6.PerformSemanticAction(null);
            Assert.That(kuj, Is.EqualTo(4));
        }

        [Test]
        public void AugmentedGrammarInitialProductionTest()
        {
            var P = new NonTerminal("P");
			var g = new AugmentedGrammar(1)
                        {
                            P --> P
                        };

            Assert.That(g.ToString(), Is.EqualTo("(0) " + g.InitialProduction.Symbol + " --> P\n(1) P --> P\n"));
        }
    }
}
