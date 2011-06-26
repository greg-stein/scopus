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
        private static
            object[] Encodings = {Encoding.ASCII, Encoding.BigEndianUnicode, Encoding.Unicode, Encoding.UTF32, Encoding.UTF8};
        
        [TestCaseSource("Encodings")]
        [Test]
        public void DoesTokenizationWorksAtAll(Encoding encoding)
        {
            // Indices:             01234567890123456
            const string SOURCE = @"lexem123ley,6.7&#";
            var buffer = encoding.GetBytes(SOURCE);
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
            Array.ForEach(lexemes, (s) => tokenizer.UseTerminal(RegExp.Literal(s, encoding)));
            tokenizer.BuildTransitions();

            var lastTokenIndex = tokenizer.Tokenize(buffer, 0, buffer.Length);

            Assert.That(lastTokenIndex + 1 == lexemes.Length); // correct #tokens?
            // Check whether each token has been recognized correctly
            for (int i = 0; i < lastTokenIndex; i++)
            {
                var tokenValue = encoding.GetString(buffer, tokensIndices[i], tokenLengths[i]);

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

        [Test]
        public void LexicalActionTest()
        {
            // Classes              2 13 14 15
            // Indices              01234567890
            const string SOURCE = @"aa bb cc dd";
            var buffer = Encoding.ASCII.GetBytes(SOURCE);

            int[] tokensIndices = new int[SOURCE.Length];
            int[] tokensClasses = new int[SOURCE.Length];
            int[] tokenLengths = new int[SOURCE.Length];

            var tokenizer = new RegExpTokenizer()
            {
                TokensClasses = tokensClasses,
                TokensIndices = tokensIndices,
                TokensLengths = tokenLengths
            };

            tokenizer.SetTransitionFunction(new TableDrivenTransitionFunction());
            var lexicalActionExecuted = false;
            Token tokenBB=null, tokenCC=null;

            tokenizer.UseTerminal(RegExp.Literal(" "));  // class 1
            tokenizer.UseTerminal(RegExp.Literal("aa")); // class 2
            tokenizer.UseTerminal(RegExp.Literal("bb"), (t) =>
                                                            {
                                                                tokenBB = t;
                                                                lexicalActionExecuted = true;
                                                                return true; // Pass this token to parser
                                                            });

            tokenizer.UseTerminal(RegExp.Literal("cc"), (t) =>
                                                            {
                                                                tokenCC = t;
                                                                lexicalActionExecuted &= true;
                                                                return false; // Ignore token
                                                            });

            tokenizer.UseTerminal(RegExp.Literal("dd")); 

            tokenizer.BuildTransitions();

            var tokensCount = tokenizer.Tokenize(buffer, 0, SOURCE.Length) + 1;

            Assert.That(tokenBB.Buffer == buffer);
            Assert.That(tokenBB.Offset, Is.EqualTo(3));
            Assert.That(tokenBB.Class, Is.EqualTo(3));
            Assert.That(tokenBB.Length, Is.EqualTo(2));

            Assert.That(tokenCC.Buffer == buffer);
            Assert.That(tokenCC.Offset, Is.EqualTo(6));
            Assert.That(tokenCC.Class, Is.EqualTo(4));
            Assert.That(tokenCC.Length, Is.EqualTo(2));

            Assert.True(lexicalActionExecuted, "The lexical actions were not executed partially or at all");

            Assert.That(tokensCount, Is.EqualTo(6));
            Assert.That(tokensClasses[0], Is.EqualTo(2));
            Assert.That(tokensClasses[1], Is.EqualTo(1));
            Assert.That(tokensClasses[2], Is.EqualTo(3));
            Assert.That(tokensClasses[3], Is.EqualTo(1));
            Assert.That(tokensClasses[4], Is.EqualTo(1));
            Assert.That(tokensClasses[5], Is.EqualTo(5));
        }
    }
}