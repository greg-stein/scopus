using System.Text;
using NUnit.Framework;
using Scopus.LexicalAnalysis;
using Scopus.LexicalAnalysis.Algorithms;
using Scopus.LexicalAnalysis.RegularExpressions;

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
            tokenizer.SetEncoding(Encoding.ASCII);

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

            int numClass = number.TokenClassID;
            int wsClass = whitespace.TokenClassID;
            int[] expectedTokenClasses = new[] { numClass, wsClass, numClass, wsClass, numClass, wsClass, 
                numClass, wsClass, numClass, wsClass, numClass,wsClass,numClass,wsClass,numClass};
            int[] expectedTokenIndices = new[] {0, 3, 4, 7, 8, 11, 12, 17, 18, 19, 20, 21, 22, 25, 26, 31};

            var rawInput = Encoding.ASCII.GetBytes(input);

            tokenizer.TokensClasses = tokenClasses;
            tokenizer.TokensIndices = tokenIndices;
            int tokensNum = tokenizer.Tokenize(rawInput, 0, rawInput.Length) + 1;

            Assert.That(tokensNum, Is.EqualTo(expectedTokenClasses.Length));

            for (int i = 0; i < tokensNum; i++)
            {
                Assert.That(tokenClasses[i], Is.EqualTo(expectedTokenClasses[i]));
                Assert.That(tokenIndices[i], Is.EqualTo(expectedTokenIndices[i]));
            }
        }

        [Test, Ignore] 
        // TODO
        // This test wont work, since Indices array holds indices of tokens. 
        // Token lengths are not saved at any array. In order to make it possible
        // we need to use additional array that will store token lengths. Further 
        // modification should be done in Lexer and TokenCollection.
        public void IgnoreTokenTest()
        {
            ITokenizer tokenizer = new RegExpTokenizer();
            tokenizer.SetTransitionFunction(new TableDrivenTransitionFunction());
            tokenizer.SetEncoding(Encoding.ASCII);

            var number = tokenizer.UseTerminal(RegExp.AtLeastOneOf(RegExp.Choice(
                RegExp.Literal('0'), RegExp.Literal('1'), RegExp.Literal('2'),
                RegExp.Literal('3'), RegExp.Literal('4'), RegExp.Literal('5'),
                RegExp.Literal('6'), RegExp.Literal('7'), RegExp.Literal('8'),
                RegExp.Literal('9'))));
            var whitespace = tokenizer.UseTerminal(RegExp.AtLeastOneOf(RegExp.Choice(
                RegExp.Literal(' '), RegExp.Literal('\t'), RegExp.Literal('\n'))));

            // "/*^(*/)*/
            tokenizer.IgnoreTerminal(RegExp.Sequence(RegExp.Literal('/'), RegExp.Literal('*'), RegExp.Not(RegExp.Literal("*/")), RegExp.Literal("*/")));
            tokenizer.BuildTransitions();

            // Number of tokens:  1  23  45                 67
            // Indices:           01234567890123456789012345678901
            const string input = "123 456 /* Some comment */ 789";
            int[] tokenClasses = new int[input.Length];
            int[] tokenIndices = new int[input.Length];

            int numClass = number.TokenClassID;
            int wsClass = whitespace.TokenClassID;
            int[] expectedTokenClasses = new[] { numClass, wsClass, numClass, wsClass, wsClass,numClass};
            int[] expectedTokenIndices = new[] { 0, 3, 4, 7, 8, 11, 12, 17, 18, 19, 20, 21, 22, 25, 26, 31 };

            var rawInput = Encoding.ASCII.GetBytes(input);

            tokenizer.TokensClasses = tokenClasses;
            tokenizer.TokensIndices = tokenIndices;
            int tokensNum = tokenizer.Tokenize(rawInput, 0, rawInput.Length) + 1;

            Assert.That(tokensNum, Is.EqualTo(expectedTokenClasses.Length));

            for (int i = 0; i < tokensNum; i++)
            {
                Assert.That(tokenClasses[i], Is.EqualTo(expectedTokenClasses[i]));
                Assert.That(tokenIndices[i], Is.EqualTo(expectedTokenIndices[i]));
            }           
        }
    }
}
