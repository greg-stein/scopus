using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ParserEngine.Exceptions;
using ParserEngine.LexicalAnalysis;
using ParserEngine.SyntaxAnalysis;
using ParserEngine.SyntaxAnalysis.ParsingTables;

namespace ParserEngineUnitTests
{
    [TestFixture]
    public class ProofOfConceptTests
    {
		//const string INPUT = @"2*(3+4)$";
		//private ActionTableEntry[,] actionTable;
		//private int[,] gotoTable;
		//private string fileName;
		//private ILexer lexer;
		//private Stream fileStream;
		//private RegExpTokenizer tokenizer;

        [SetUp]
        public void InitTests()
        {
        }

        [TearDown]
        public void FinalizeTests()
        {
        }

        [Test]
        public void ArithmeticStatementParseTest()
        {
			const string INPUT = @"2*(3+4)";//@"2*(3+4)$"
			ActionTableEntry[,] actionTable;
			int[,] gotoTable;
			string fileName;
			ILexer lexer;
			Stream fileStream;
			RegExpTokenizer tokenizer;


			fileName = Path.GetTempFileName();
			File.WriteAllText(fileName, INPUT);
			tokenizer = new RegExpTokenizer();
			lexer = new Lexer(tokenizer);
			fileStream = File.OpenRead(fileName);
			lexer.SetDataSource(fileStream);


			actionTable = new ActionTableEntry[12, 6];
			#region Populating Action Table

			actionTable[0, 1].Action = ParserAction.Shift;
			actionTable[0, 1].Destination = 5;
			actionTable[0, 4].Action = ParserAction.Shift;
			actionTable[0, 4].Destination = 4;
			actionTable[1, 0].Action = ParserAction.Accept; // <-- !!!!
			actionTable[1, 2].Action = ParserAction.Shift;
			actionTable[1, 2].Destination = 6;
			actionTable[2, 0].Action = ParserAction.Reduce;
			actionTable[2, 0].Destination = 2;
			actionTable[2, 2].Action = ParserAction.Reduce;
			actionTable[2, 2].Destination = 2;
			actionTable[2, 3].Action = ParserAction.Shift;
			actionTable[2, 3].Destination = 7;
			actionTable[2, 5].Action = ParserAction.Reduce;
			actionTable[2, 5].Destination = 2;
			actionTable[3, 0].Action = ParserAction.Reduce;
			actionTable[3, 0].Destination = 4;
			actionTable[3, 2].Action = ParserAction.Reduce;
			actionTable[3, 2].Destination = 4;
			actionTable[3, 3].Action = ParserAction.Reduce;
			actionTable[3, 3].Destination = 4;
			actionTable[3, 5].Action = ParserAction.Reduce;
			actionTable[3, 5].Destination = 4;
			actionTable[4, 1].Action = ParserAction.Shift;
			actionTable[4, 1].Destination = 5;
			actionTable[4, 4].Action = ParserAction.Shift;
			actionTable[4, 4].Destination = 4;
			actionTable[5, 0].Action = ParserAction.Reduce;
			actionTable[5, 0].Destination = 6;
			actionTable[5, 2].Action = ParserAction.Reduce;
			actionTable[5, 2].Destination = 6;
			actionTable[5, 3].Action = ParserAction.Reduce;
			actionTable[5, 3].Destination = 6;
			actionTable[5, 5].Action = ParserAction.Reduce;
			actionTable[5, 5].Destination = 6;
			actionTable[6, 1].Action = ParserAction.Shift;
			actionTable[6, 1].Destination = 5;
			actionTable[6, 4].Action = ParserAction.Shift;
			actionTable[6, 4].Destination = 4;
			actionTable[7, 1].Action = ParserAction.Shift;
			actionTable[7, 1].Destination = 5;
			actionTable[7, 4].Action = ParserAction.Shift;
			actionTable[7, 4].Destination = 4;
			actionTable[8, 2].Action = ParserAction.Shift;
			actionTable[8, 2].Destination = 6;
			actionTable[8, 5].Action = ParserAction.Shift;
			actionTable[8, 5].Destination = 11;
			actionTable[9, 0].Action = ParserAction.Reduce;
			actionTable[9, 0].Destination = 1;
			actionTable[9, 2].Action = ParserAction.Reduce;
			actionTable[9, 2].Destination = 1;
			actionTable[9, 3].Action = ParserAction.Shift;
			actionTable[9, 3].Destination = 7;
			actionTable[9, 5].Action = ParserAction.Reduce;
			actionTable[9, 5].Destination = 1;
			actionTable[10, 0].Action = ParserAction.Reduce;
			actionTable[10, 0].Destination = 3;
			actionTable[10, 2].Action = ParserAction.Reduce;
			actionTable[10, 2].Destination = 3;
			actionTable[10, 3].Action = ParserAction.Reduce;
			actionTable[10, 3].Destination = 3;
			actionTable[10, 5].Action = ParserAction.Reduce;
			actionTable[10, 5].Destination = 3;
			actionTable[11, 0].Action = ParserAction.Reduce;
			actionTable[11, 0].Destination = 5;
			actionTable[11, 2].Action = ParserAction.Reduce;
			actionTable[11, 2].Destination = 5;
			actionTable[11, 3].Action = ParserAction.Reduce;
			actionTable[11, 3].Destination = 5;
			actionTable[11, 5].Action = ParserAction.Reduce;
			actionTable[11, 5].Destination = 5;
			//actionTable[0, 0].Action = ParserAction.Shift;
			//actionTable[0, 0].Destination = 5;
			//actionTable[0, 3].Action = ParserAction.Shift;
			//actionTable[0, 3].Destination = 4;
			//actionTable[1, 1].Action = ParserAction.Shift;
			//actionTable[1, 1].Destination = 6;
			//actionTable[1, 5].Action = ParserAction.Accept; // <-- !!!!
			//actionTable[2, 1].Action = ParserAction.Reduce;
			//actionTable[2, 1].Destination = 2;
			//actionTable[2, 2].Action = ParserAction.Shift;
			//actionTable[2, 2].Destination = 7;
			//actionTable[2, 4].Action = ParserAction.Reduce;
			//actionTable[2, 4].Destination = 2;
			//actionTable[2, 5].Action = ParserAction.Reduce;
			//actionTable[2, 5].Destination = 2;
			//actionTable[3, 1].Action = ParserAction.Reduce;
			//actionTable[3, 1].Destination = 4;
			//actionTable[3, 2].Action = ParserAction.Reduce;
			//actionTable[3, 2].Destination = 4;
			//actionTable[3, 4].Action = ParserAction.Reduce;
			//actionTable[3, 4].Destination = 4;
			//actionTable[3, 5].Action = ParserAction.Reduce;
			//actionTable[3, 5].Destination = 4;
			//actionTable[4, 0].Action = ParserAction.Shift;
			//actionTable[4, 0].Destination = 5;
			//actionTable[4, 3].Action = ParserAction.Shift;
			//actionTable[4, 3].Destination = 4;
			//actionTable[5, 1].Action = ParserAction.Reduce;
			//actionTable[5, 1].Destination = 6;
			//actionTable[5, 2].Action = ParserAction.Reduce;
			//actionTable[5, 2].Destination = 6;
			//actionTable[5, 4].Action = ParserAction.Reduce;
			//actionTable[5, 4].Destination = 6;
			//actionTable[5, 5].Action = ParserAction.Reduce;
			//actionTable[5, 5].Destination = 6;
			//actionTable[6, 0].Action = ParserAction.Shift;
			//actionTable[6, 0].Destination = 5;
			//actionTable[6, 3].Action = ParserAction.Shift;
			//actionTable[6, 3].Destination = 4;
			//actionTable[7, 0].Action = ParserAction.Shift;
			//actionTable[7, 0].Destination = 5;
			//actionTable[7, 3].Action = ParserAction.Shift;
			//actionTable[7, 3].Destination = 4;
			//actionTable[8, 1].Action = ParserAction.Shift;
			//actionTable[8, 1].Destination = 6;
			//actionTable[8, 4].Action = ParserAction.Shift;
			//actionTable[8, 4].Destination = 11;
			//actionTable[9, 1].Action = ParserAction.Reduce;
			//actionTable[9, 1].Destination = 1;
			//actionTable[9, 2].Action = ParserAction.Shift;
			//actionTable[9, 2].Destination = 7;
			//actionTable[9, 4].Action = ParserAction.Reduce;
			//actionTable[9, 4].Destination = 1;
			//actionTable[9, 5].Action = ParserAction.Reduce;
			//actionTable[9, 5].Destination = 1;
			//actionTable[10, 1].Action = ParserAction.Reduce;
			//actionTable[10, 1].Destination = 3;
			//actionTable[10, 2].Action = ParserAction.Reduce;
			//actionTable[10, 2].Destination = 3;
			//actionTable[10, 4].Action = ParserAction.Reduce;
			//actionTable[10, 4].Destination = 3;
			//actionTable[10, 5].Action = ParserAction.Reduce;
			//actionTable[10, 5].Destination = 3;
			//actionTable[11, 1].Action = ParserAction.Reduce;
			//actionTable[11, 1].Destination = 5;
			//actionTable[11, 2].Action = ParserAction.Reduce;
			//actionTable[11, 2].Destination = 5;
			//actionTable[11, 4].Action = ParserAction.Reduce;
			//actionTable[11, 4].Destination = 5;
			//actionTable[11, 5].Action = ParserAction.Reduce;
			//actionTable[11, 5].Destination = 5;

			#endregion

			gotoTable = new int[12, 3];
			#region Populating Goto Table

			gotoTable[0, 0] = 1;
			gotoTable[0, 1] = 2;
			gotoTable[0, 2] = 3;
			gotoTable[4, 0] = 8;
			gotoTable[4, 1] = 2;
			gotoTable[4, 2] = 3;
			gotoTable[6, 1] = 9;
			gotoTable[6, 2] = 3;
			gotoTable[7, 2] = 10;

			#endregion
			

			var stack = new Stack<int>();
            var parser = new LRParser();

            var E = new NonTerminal("E");
            var T = new NonTerminal("T");
            var F = new NonTerminal("F");

            var id = tokenizer.IntegerNumber;
            var plus = tokenizer.AddToken("+");
            var mult = tokenizer.AddToken("*");
            var leftBrace = tokenizer.AddToken("(");
            var rightBrace = tokenizer.AddToken(")");
            //var endMark = tokenizer.AddToken("$");

			var grammar = new AugmentedGrammar(tokenizer.TotalTokensCount)
                              {
                                  E --> E & plus & T ^ (v => stack.Push(stack.Pop() + stack.Pop())),
                                  E --> T,
                                  T --> T & mult & F ^ (v => stack.Push(stack.Pop() * stack.Pop())),
                                  T --> F,
                                  F --> leftBrace & E & rightBrace,
                                  F --> id ^ (v => stack.Push(v[id].AsInt()))
                              };

            Console.WriteLine("Grammar is being tested: \n{0}", grammar);
            Console.WriteLine("Input is being parsed: {0}\n", INPUT);
            Console.WriteLine("Parsing process:\n");
            parser.Grammar = grammar;
            parser.Lexer = lexer;
            parser.ParsingTable = new ParsingTable(actionTable, gotoTable);
            parser.InputAccepted += (sender, eventArgs) => Console.WriteLine("Accepted!");
            parser.ParseInput();

            Assert.That(stack.Pop(), Is.EqualTo(14));
            Assert.That(stack.Count, Is.EqualTo(0));


			fileStream.Close();
			File.Delete(fileName);
        }

		[Test]
		public void AmbiguousGrammarParseTest()
		{
			var E = new NonTerminal("E");
			var L = new NonTerminal("L");
			var R = new NonTerminal("R");

			var tokenizer = new RegExpTokenizer();
			var id = tokenizer.IntegerNumber;
			var assign = tokenizer.AddToken("=");
			var deref = tokenizer.AddToken("*");

			var grammar = new AugmentedGrammar(tokenizer.TotalTokensCount)
                              {
								  E --> L & assign & R,
								  E --> R,
								  L --> deref & R,
								  L --> id,
								  R --> L
                              };

			Assert.Throws(typeof(ParserException), () => new ParsingTable(grammar));
		}

		[Test]
		public void ArithmeticStatementFullyAutonomousParseTest()
		{
			const string INPUT = @"2*(3+4)";

			string fileName = Path.GetTempFileName();
			File.WriteAllText(fileName, INPUT);
			RegExpTokenizer tokenizer = new RegExpTokenizer();
			ILexer lexer = new Lexer(tokenizer);
			Stream fileStream = File.OpenRead(fileName);
			lexer.SetDataSource(fileStream);


			var stack = new Stack<int>();
			var parser = new LRParser();

			var E = new NonTerminal("E");
			var T = new NonTerminal("T");
			var F = new NonTerminal("F");

			var id = tokenizer.IntegerNumber;
			var plus = tokenizer.AddToken("+");
			var mult = tokenizer.AddToken("*");
			var leftBrace = tokenizer.AddToken("(");
			var rightBrace = tokenizer.AddToken(")");

			var grammar = new AugmentedGrammar(tokenizer.TotalTokensCount)
                              {
                                  E --> E & plus & T ^ (v => stack.Push(stack.Pop() + stack.Pop())),
                                  E --> T,
                                  T --> T & mult & F ^ (v => stack.Push(stack.Pop() * stack.Pop())),
                                  T --> F,
                                  F --> leftBrace & E & rightBrace,
                                  F --> id ^ (v => stack.Push(v[id].AsInt()))
                              };

			Console.WriteLine("Grammar is being tested: \n{0}", grammar);
			Console.WriteLine("Input is being parsed: {0}\n", INPUT);
			Console.WriteLine("Parsing process:\n");
			parser.Grammar = grammar;
			parser.Lexer = lexer;
			parser.ParsingTable = new ParsingTable(grammar);
			parser.InputAccepted += (sender, eventArgs) => Console.WriteLine("Accepted!");
			parser.ParseInput();

			Assert.That(stack.Pop(), Is.EqualTo(14));
			Assert.That(stack.Count, Is.EqualTo(0));


			fileStream.Close();
			File.Delete(fileName);
		}

		[Test]
		public void ODBFeatureAttributesSectionParseTest()
		{
			const string INPUT = " ;0,1=,2=	\n\r\r\r		 /*! 343 @332	@!!bvfg3 \n\r*/ -3.027,3 #fgg5 t e5 \n = , 4"; ;//@";0,1=,2=-3.027,3=,4"
			
			string fileName = Path.GetTempFileName();
			File.WriteAllText(fileName, INPUT);
			var tokenizer = new CADTokenizer();
			ILexer lexer = new Lexer(tokenizer);
			Stream fileStream = File.OpenRead(fileName);
			lexer.SetDataSource(fileStream);

			string attrValue = "";

			var parser = new LRParser();

			var FeatureAttributeSection = new NonTerminal("FAS");
			var FeatureAttributeList = new NonTerminal("FAL");
			var FeatureAttributeKeyValuePair = new NonTerminal("FAKVP");

			var int_id = tokenizer.IntegerNumber;
			var float_id = tokenizer.FloatNumber;
			var semicolon = tokenizer.AddToken(";");
			var comma = tokenizer.AddToken(",");
			var equals = tokenizer.AddToken("=");
			var sharp = tokenizer.AddToken("#");
			var amp = tokenizer.AddToken("~");
			var el = tokenizer.Endline;
			tokenizer.AddCommentWithBordersToken("/*", "*/");
			tokenizer.AddCommentToEndlineToken("#");
			tokenizer.HideToken(el);

			var grammar = new AugmentedGrammar(tokenizer.TotalTokensCount)
                              {
								  FeatureAttributeSection --> semicolon & FeatureAttributeList,
								  FeatureAttributeList --> FeatureAttributeList & comma & FeatureAttributeKeyValuePair,//FeatureAttributeKeyValuePair & comma & FeatureAttributeList,
								  FeatureAttributeList --> FeatureAttributeKeyValuePair,
								  FeatureAttributeKeyValuePair --> int_id ^ (v => Console.WriteLine("Attribute " + v[int_id].AsInt() + " = TRUE")),
								  FeatureAttributeKeyValuePair --> int_id & equals ^ (v => Console.WriteLine("Attribute " + v[int_id].AsInt() + " = DEFAULT")),
								  FeatureAttributeKeyValuePair --> int_id & equals & int_id ^ (v =>
								                                                               		{
								                                                               			int vl = v[int_id].AsInt();
																										int att = v[int_id].AsInt();
								                                                               			attrValue = vl.ToString();
																										Console.WriteLine("Attribute {1} = {0}", vl, att);
								                                                               		}),
								  FeatureAttributeKeyValuePair --> int_id & equals & float_id ^ (v =>
								                                                               		{
								                                                               			float vl = v[float_id].AsFloat();
																										int att = v[int_id].AsInt();
								                                                               			attrValue = vl.ToString();
																										Console.WriteLine("Attribute {1} = {0}", vl, att);
								                                                               		})
                              };

			Console.WriteLine("Grammar is being tested: \n{0}", grammar);
			Console.WriteLine("Input is being parsed: {0}\n", INPUT);
			Console.WriteLine("Parsing process:\n");
			parser.Grammar = grammar;
			parser.Lexer = lexer;
			parser.ParsingTable = new ParsingTable(grammar);
			parser.InputAccepted += (sender, eventArgs) => Console.WriteLine("Accepted!");
			parser.ParseInput();

			Assert.That(attrValue, Is.EqualTo("-3.027"));

			fileStream.Close();
			File.Delete(fileName);
		}
	}
}
