using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Scopus.LexicalAnalysis;
using Scopus.LexicalAnalysis.Algorithms;
using Scopus.LexicalAnalysis.RegularExpressions;
using ScopusUnitTests.Common;

namespace ScopusUnitTests
{
    [TestFixture]
    public class RegExpTokenizerTests
    {
        [Test]
        public void GeneralTest()
        {
            ITokenizer tokenizer = new RegExpTokenizer();
            tokenizer.SetTransitionFunction(new TableDrivenTransitionFunction());

            var number = tokenizer.UseTerminal(RegExp.AtLeastOneOf(RegExp.Choice(
                RegExp.Literal('0'), RegExp.Literal('1'), RegExp.Literal('2'), 
                RegExp.Literal('3'), RegExp.Literal('4'), RegExp.Literal('5'), 
                RegExp.Literal('6'), RegExp.Literal('7'), RegExp.Literal('8'), 
                RegExp.Literal('9'))));
            var whitespace = tokenizer.UseTerminal(RegExp.AtLeastOneOf(RegExp.Choice(
                RegExp.Literal(' '), RegExp.Literal('\t'), RegExp.Literal('\n'))));

            tokenizer.BuildTransitions();

            // Number of tokens:  1  23  45  67    890123  45
            // Indices:           01234567890123456789012345678901
            const string input = "123 456 789 02348 0 3 452 55555";
            int[] tokenClasses = new int[input.Length];
            int[] tokenIndices = new int[input.Length];
            int[] tokenLengths = new int[input.Length];

            int numClass = number.TokenClassID;
            int wsClass = whitespace.TokenClassID;
            int[] expectedTokenClasses = new[] { numClass, wsClass, numClass, wsClass, numClass, wsClass, 
                numClass, wsClass, numClass, wsClass, numClass,wsClass,numClass,wsClass,numClass};
            int[] expectedTokenIndices = new[] {0, 3, 4, 7, 8, 11, 12, 17, 18, 19, 20, 21, 22, 25, 26, 31};

            var rawInput = Encoding.ASCII.GetBytes(input);

            tokenizer.TokensClasses = tokenClasses;
            tokenizer.TokensIndices = tokenIndices;
            tokenizer.TokensLengths = tokenLengths;
            int tokensNum = tokenizer.Tokenize(rawInput, 0, rawInput.Length) + 1;

            Assert.That(tokensNum, Is.EqualTo(expectedTokenClasses.Length));

            for (int i = 0; i < tokensNum; i++)
            {
                Assert.That(tokenClasses[i], Is.EqualTo(expectedTokenClasses[i]));
                Assert.That(tokenIndices[i], Is.EqualTo(expectedTokenIndices[i]));
            }
        }

        [TestCase("ascii")]
        [Test]
        public void Temptest(string encodingStr)
        {
            var encoding = CommonTestRoutines.GetEncoding(encodingStr);
            var tokenizer = new RegExpTokenizer();
            tokenizer.SetTransitionFunction(new TableDrivenTransitionFunction());

            //tokenizer.IgnoreTerminal(RegExp.Sequence(RegExp.Literal("/*", encoding), RegExp.AnyNumberOf(RegExp.Range((char)0, (char)255, encoding)),
            //    RegExp.Literal("*/", encoding)));
            tokenizer.IgnoreTerminal(RegExp.Sequence(
                RegExp.Literal("/*", encoding), 
                //RegExp.AnyNumberOf(
                //    RegExp.Range((char)0, (char)255, encoding)
                //),
                //RegExp.Choice(
                //    RegExp.AnyNumberOf(
                //        RegExp.Range((char)0, (char)255, encoding)
                //    ),
                    RegExp.Not(RegExp.Literal("*/", encoding), false)
                //)
            ));
            tokenizer.UseTerminal(RegExp.AtLeastOneOf(RegExp.Range('0', '9', encoding)));
            tokenizer.UseTerminal(RegExp.AtLeastOneOf(RegExp.Literal(' ', encoding)));
            tokenizer.BuildTransitions();  

            const string input = "/*111*/ 222 /*333*/ 444";
            int bufferLength = encoding.GetByteCount(input);
            tokenizer.TokensClasses = new int[bufferLength];
            tokenizer.TokensIndices = new int[bufferLength];
            tokenizer.TokensLengths = new int[bufferLength];

            var rawInput = encoding.GetBytes(input);
            //rawInput = new byte[] {00, 49, 00, 50, 00, 51, 00, 32};
            int tokensNum = tokenizer.Tokenize(rawInput, 0, rawInput.Length) + 1;
        }

        private static
            object[] Encodings = { Encoding.ASCII, Encoding.BigEndianUnicode, Encoding.Unicode, Encoding.UTF32, Encoding.UTF8 };

        [TestCaseSource("Encodings")]
        [Test]
        public void IgnoreTokenLazyQuantificationTest(Encoding encoding)
        {
            var tokenizer = new RegExpTokenizer();
            tokenizer.SetTransitionFunction(new TableDrivenTransitionFunction());

            var number = tokenizer.UseTerminal(RegExp.AtLeastOneOf(RegExp.Range('0', '9', encoding)));
            var whitespace = tokenizer.UseTerminal(RegExp.AtLeastOneOf(RegExp.Choice(
                RegExp.Literal(' ', encoding), RegExp.Literal('\t', encoding), RegExp.Literal('\n', encoding))));
            tokenizer.IgnoreTerminal(RegExp.Sequence(RegExp.Literal("/*", encoding), RegExp.AnyNumberOf(RegExp.Range((char)0, (char)255, encoding)),
                RegExp.Literal("*/", encoding)));
            tokenizer.BuildTransitions();

            // Number of tokens:  1  23  45       67 89     01
            // Indices:           012345678901234567890123456789
            const string input = "123 456 /*cdnp*/ 87 /*ae*/ 789";
            int bufferLength = encoding.GetByteCount(input);
            int[] tokenClasses = new int[bufferLength];
            int[] tokenIndices = new int[bufferLength];
            int[] tokenLengths = new int[bufferLength];

            int numClass = number.TokenClassID;
            int wsClass = whitespace.TokenClassID;
            int[] expectedTokenClasses = new[] { numClass, wsClass, numClass, wsClass, wsClass, numClass, wsClass, wsClass, numClass };
            var expectedTokenIndices = new List<int>(15); //new[] { 0, 3, 4, 7, 16, 17, 19, 26, 27 };
            var tokens = new[] {"123", " ", "456", " ", "/*cdnp*/", " ", "87", " ", "/*ae*/", " ", "789"};

            expectedTokenIndices.Add(0);
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                expectedTokenIndices.Add(expectedTokenIndices[i] + encoding.GetByteCount(token));
            }
            // Delete ingored tokens
            expectedTokenIndices.RemoveAt(8);
            expectedTokenIndices.RemoveAt(4);

            var rawInput = encoding.GetBytes(input);

            tokenizer.TokensClasses = tokenClasses;
            tokenizer.TokensIndices = tokenIndices;
            tokenizer.TokensLengths = tokenLengths;
            int tokensNum = tokenizer.Tokenize(rawInput, 0, rawInput.Length) + 1;

            Assert.That(tokensNum, Is.EqualTo(expectedTokenClasses.Length));

            for (int i = 0; i < tokensNum; i++)
            {
                Assert.That(tokenClasses[i], Is.EqualTo(expectedTokenClasses[i]), "Error On token class comparison: " + i);
                Assert.That(tokenIndices[i], Is.EqualTo(expectedTokenIndices[i]), "Error On token index comparison: " + i);
            }
        }
    }
}
