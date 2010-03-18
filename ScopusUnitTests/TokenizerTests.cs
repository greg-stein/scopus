using System.Text;
using NUnit.Framework;
using Scopus.LexicalAnalysis;

namespace ScopusUnitTests
{
    [TestFixture]
    public class TokenizerTests
    {
        [Test]
        public void DoesTokenizationWorksAtAll()
        {
            // Indices:             01234567890123456
            const string SOURCE = @"lexem123ley,6.7&#";
            var buffer = Encoding.ASCII.GetBytes(SOURCE);
            var lexemes = new[] { "lexem", "123", "ley", ",", "6", ".", "7", "&", "#" };

            int[] tokensIndices = new int[SOURCE.Length + 1];
            int[] tokensClasses = new int[SOURCE.Length];

            var tokenizer = new KeywordsTokenizer
                                {
                                    TokensClasses = tokensClasses,
                                    TokensIndices = tokensIndices
                                };

            tokenizer.AddTokens(lexemes);
            var lastTokenIndex = tokenizer.Tokenize(buffer, 0, SOURCE.Length);

            Assert.That(lastTokenIndex + 1 == lexemes.Length); // correct #tokens?
            // Check whether each token has been recognized correctly
            for (int i = 0; i < lastTokenIndex; i++)
            {
                int tokenIndex = tokensIndices[i];
                var tokenValue = Encoding.ASCII.GetString(buffer, tokenIndex,
                    tokensIndices[i + 1] - tokenIndex);

                Assert.AreEqual(lexemes[i], tokenValue);
            }
        }

        [Test]
        public void TokenizerClassTest()
        {
            // Should be: aa, aab, abc
            const string SAMPLE = "aaaababc";

            var tokensClasses = new int[SAMPLE.Length];
            var tokensIndices = new int[SAMPLE.Length];

            var tokenizer = new KeywordsTokenizer
                                {
                                    TokensClasses = tokensClasses,
                                    TokensIndices = tokensIndices
                                };

            tokenizer.AddToken("aab"); // class 0
            tokenizer.AddToken("acb"); // class 1
            tokenizer.AddToken("abc"); // class 2
            tokenizer.AddToken("aa");  // class 3

            var tokensNum = tokenizer.Tokenize(Encoding.ASCII.GetBytes(SAMPLE), 0, SAMPLE.Length);
            tokensNum++;

            Assert.That(tokensNum == 3);
            Assert.That(tokensClasses[0] == 4);
            Assert.That(tokensClasses[1] == 1);
            Assert.That(tokensClasses[2] == 3);
            Assert.That(tokensIndices[0] == 0);
            Assert.That(tokensIndices[1] == 2);
            Assert.That(tokensIndices[2] == 5);
        }

        [Test]
        public void UnendedTokenAtTheEndTest()
        {
            // Indices:            00000000001111111111222222222
            //                     01234567890123456789012345678
            const string SAMPLE = "windows.bugsNum=long.Max;Linu";
            var tokensClasses = new int[SAMPLE.Length];
            var tokensIndices = new int[SAMPLE.Length];

            var tokenizer = new KeywordsTokenizer
            {
                TokensClasses = tokensClasses,
                TokensIndices = tokensIndices
            };

            tokenizer.AddToken("windows");  // class: 0
            tokenizer.AddToken(".");        // class: 1
            tokenizer.AddToken("bugsNum");  // class: 2
            tokenizer.AddToken("=");        // class: 3
            tokenizer.AddToken("long");     // class: 4
            tokenizer.AddToken("Max");      // class: 5
            tokenizer.AddToken(";");        // class: 6
            tokenizer.AddToken("Linux");    // class: 7

            var tokensNum = tokenizer.Tokenize(Encoding.ASCII.GetBytes(SAMPLE), 0, SAMPLE.Length) + 1;

            Assert.That(tokensNum == 9);

            Assert.That(tokensIndices[0] == 0);
            Assert.That(tokensIndices[1] == 7);
            Assert.That(tokensIndices[2] == 8);
            Assert.That(tokensIndices[3] == 15);
            Assert.That(tokensIndices[4] == 16);
            Assert.That(tokensIndices[5] == 20);
            Assert.That(tokensIndices[6] == 21);
            Assert.That(tokensIndices[7] == 24);
            Assert.That(tokensIndices[8] == 25);

            Assert.That(tokensClasses[0] == 1);
            Assert.That(tokensClasses[1] == 2);
            Assert.That(tokensClasses[2] == 3);
            Assert.That(tokensClasses[3] == 4);
            Assert.That(tokensClasses[4] == 5);
            Assert.That(tokensClasses[5] == 2);
            Assert.That(tokensClasses[6] == 6);
            Assert.That(tokensClasses[7] == 7);
        }

        [Test]
        public void ManyTokensWithSameClassTest()
        {
            // Indices:            00000000001111111111222222222
            //                     01234567890123456789012345678
            const string SAMPLE = "windows.bugsNum=long.Max;Linu";
            var tokensClasses = new int[SAMPLE.Length];
            var tokensIndices = new int[SAMPLE.Length];

            var tokenizer = new KeywordsTokenizer
            {
                TokensClasses = tokensClasses,
                TokensIndices = tokensIndices
            };

            // Classes:          0          1    2          3    4       5      6    7
            tokenizer.AddTokens("windows", ".", "bugsNum", "=", "long", "Max", ";", "Linux");

            var tokensNum = tokenizer.Tokenize(Encoding.ASCII.GetBytes(SAMPLE), 0, SAMPLE.Length) + 1;

            Assert.That(tokensNum == 9);

            Assert.That(tokensIndices[0] == 0);
            Assert.That(tokensIndices[1] == 7);
            Assert.That(tokensIndices[2] == 8);
            Assert.That(tokensIndices[3] == 15);
            Assert.That(tokensIndices[4] == 16);
            Assert.That(tokensIndices[5] == 20);
            Assert.That(tokensIndices[6] == 21);
            Assert.That(tokensIndices[7] == 24);
            Assert.That(tokensIndices[8] == 25);

            Assert.That(tokensClasses[0] == 1);
            Assert.That(tokensClasses[1] == 1);
            Assert.That(tokensClasses[2] == 1);
            Assert.That(tokensClasses[3] == 1);
            Assert.That(tokensClasses[4] == 1);
            Assert.That(tokensClasses[5] == 1);
            Assert.That(tokensClasses[6] == 1);
            Assert.That(tokensClasses[7] == 1);
        }

        [Test]
        public void StatesArraysReallocationTest()
        {
            // Number of states in use > initial states number => reallocation
            string token = new string('a', KeywordsTokenizer.INITIAL_STATES_COUNT + 1);
            // Two consequent tokens
            string sample = new string('a', 2 * token.Length);

            var tokensClasses = new int[sample.Length];
            var tokensIndices = new int[sample.Length];

            var tokenizer = new KeywordsTokenizer
            {
                TokensClasses = tokensClasses,
                TokensIndices = tokensIndices
            };

            var tokenClass = tokenizer.AddToken(token); // should be 0

            var tokensNum = tokenizer.Tokenize(Encoding.ASCII.GetBytes(sample), 0, sample.Length) + 1;

            Assert.That(tokensNum == 2);
            Assert.That(tokensClasses[0] == 1);
            Assert.That(tokensClasses[1] == 1);
            Assert.That(tokensIndices[0] == 0);
            Assert.That(tokensIndices[1] == token.Length);
        }
    }
}