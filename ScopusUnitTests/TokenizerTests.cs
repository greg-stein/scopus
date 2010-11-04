using System;
using System.Text;
using NUnit.Framework;
using Scopus.LexicalAnalysis;
using Scopus.LexicalAnalysis.Algorithms;
using Scopus.LexicalAnalysis.RegularExpressions;

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
            int[] tokenLengths = new int[SOURCE.Length];

            var tokenizer = new RegExpTokenizer()
                                {
                                    TokensClasses = tokensClasses,
                                    TokensIndices = tokensIndices,
                                    TokensLengths = tokenLengths
                                };

            tokenizer.SetTransitionFunction(new TableDrivenTransitionFunction());
            tokenizer.SetEncoding(Encoding.ASCII);            
            Array.ForEach(lexemes, (s) => tokenizer.UseTerminal(RegExp.Literal(s)));
            tokenizer.BuildTransitions();

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
            var tokensLengths = new int[SAMPLE.Length];

            var tokenizer = new RegExpTokenizer()
                                {
                                    TokensClasses = tokensClasses,
                                    TokensIndices = tokensIndices,
                                    TokensLengths = tokensLengths
                                };

            tokenizer.SetTransitionFunction(new TableDrivenTransitionFunction());
            tokenizer.SetEncoding(Encoding.ASCII);            

            tokenizer.UseTerminal(RegExp.Literal("aab")); // class 1
            tokenizer.UseTerminal(RegExp.Literal("acb")); // class 2
            tokenizer.UseTerminal(RegExp.Literal("abc")); // class 3
            tokenizer.UseTerminal(RegExp.Literal("aa"));  // class 4

            tokenizer.BuildTransitions();

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
            var tokensLengths = new int[SAMPLE.Length];

            var tokenizer = new RegExpTokenizer()
                                {
                                    TokensClasses = tokensClasses,
                                    TokensIndices = tokensIndices,
                                    TokensLengths = tokensLengths
                                };

            tokenizer.SetTransitionFunction(new TableDrivenTransitionFunction());
            tokenizer.SetEncoding(Encoding.ASCII);

            tokenizer.UseTerminal(RegExp.Literal("windows"));  // class: 1
            tokenizer.UseTerminal(RegExp.Literal("."));        // class: 2
            tokenizer.UseTerminal(RegExp.Literal("bugsNum"));  // class: 3
            tokenizer.UseTerminal(RegExp.Literal("="));        // class: 4
            tokenizer.UseTerminal(RegExp.Literal("long"));     // class: 5
            tokenizer.UseTerminal(RegExp.Literal("Max"));      // class: 6
            tokenizer.UseTerminal(RegExp.Literal(";"));        // class: 7
            tokenizer.UseTerminal(RegExp.Literal("Linux"));    // class: 8

            tokenizer.BuildTransitions();

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
            var tokensLengths = new int[SAMPLE.Length];

            var tokenizer = new RegExpTokenizer()
            {
                TokensClasses = tokensClasses,
                TokensIndices = tokensIndices,
                TokensLengths = tokensLengths
            };

            tokenizer.SetTransitionFunction(new TableDrivenTransitionFunction());
            tokenizer.SetEncoding(Encoding.ASCII);

            tokenizer.UseTerminal(RegExp.Choice(
                RegExp.Literal("windows"),
                RegExp.Literal("."),
                RegExp.Literal("bugsNum"),
                RegExp.Literal("="), 
                RegExp.Literal("long"),
                RegExp.Literal("Max"),    
                RegExp.Literal(";"),     
                RegExp.Literal("Linux")
            ));

            tokenizer.BuildTransitions();

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
    }
}