using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Scopus.LexicalAnalysis;
using Scopus.LexicalAnalysis.Algorithms;
using Scopus.LexicalAnalysis.RegularExpressions;
using Scopus.SyntaxAnalysis;

namespace ScopusUnitTests
{
    [TestFixture]
    public class LexerTests
    {
        // Indices:                     01234567890123456
        private const string SOURCE = @"lexem123ley,6.7&#";

		private readonly string[] lexemes = new[] { "lexem", "123", "ley", ",", "6", ".", "7", "&", "#", Terminal.END_MARKER_TOKEN_NAME };
        private string fileName;
        private Lexer lexer;
        private Stream fileStream;
        private ITokenizer tokenizer;

        [SetUp]
        public void InitTests()
        {
            fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, SOURCE);
            
            tokenizer = new RegExpTokenizer();
            tokenizer.SetTransitionFunction(new TableDrivenTransitionFunction());
            Array.ForEach(lexemes, (s) => tokenizer.UseTerminal(RegExp.Literal(s)));
            tokenizer.BuildTransitions();

            lexer = new Lexer(tokenizer);
            fileStream = File.OpenRead(fileName);
            lexer.SetDataSource(fileStream);
        }

        [TearDown]
        public void FinalizeTests()
        {
            fileStream.Close();
            File.Delete(fileName);
        }

        [Test]
        public void ScannerCommonTest()
        {
            int[] tokensIndices = lexer.TokensIndices;
            lexer.ReadTokens();
            byte[] buffer = lexer.Buffer;

            Assert.That(lexer.LastTokenStartIndex == lexemes.Length - 2);	// -1 for synthesized END_MARK token
            for (int i = 0; i < lexer.LastTokenStartIndex; i++)
            {
                int tokenIndex = tokensIndices[i];
                var tokenValue = Encoding.ASCII.GetString(buffer, tokenIndex,
                    tokensIndices[i + 1] - tokenIndex);

                Assert.AreEqual(lexemes[i], tokenValue);
            }
        }

        [Test]
        public void TokensIteratorTest()
        {
            int tokensCount = 0;
            var tokensCollection = new TokensCollection(lexer);

            foreach (var lexeme in tokensCollection)
            {
                Assert.AreEqual(lexemes[tokensCount++], lexeme.ToString());
            }
            Assert.AreEqual(lexemes.Length, tokensCount);
        }

        [Test]
        public void FillingBufferSeveralTimesTest()
        {
            const int BUFFER_SIZE = 6; // note that sample file contains 17 bytes

            lexer = new Lexer(tokenizer, BUFFER_SIZE);
            lexer.SetDataSource(fileStream);

            TokensIteratorTest();
        }

        [Test]
        public void TokenEqualsTest()
        {
            var token1 = new Token("01234");
            var token2 = new Token("0123");
            var token3 = new Token("012345");
            var token4 = new Token(new ArraySegment<byte>(new byte[] {0x30, 0x31, 0x32, 0x33}, 0, 4));

            Assert.That(token1, Is.Not.EqualTo(token2));
            Assert.That(token2, Is.Not.EqualTo(token3));
            Assert.That(token1, Is.Not.EqualTo(token3));
            Assert.That(token4, Is.EqualTo(token2));
        }
    }
}
