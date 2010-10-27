using System.Text;
using NUnit.Framework;
using Scopus.LexicalAnalysis;
using Scopus.LexicalAnalysis.Algorithms;
using Scopus.LexicalAnalysis.RegularExpressions;

namespace ScopusUnitTests
{
    [TestFixture]
    public class LexerEncodingTests
    {
        [Test]
        public void UnicodeTest()
        {
            ITokenizer tokenizer = new RegExpTokenizer();
            tokenizer.SetTransitionFunction(new TableDrivenTransitionFunction());
            tokenizer.SetEncoding(Encoding.Unicode);

            var hebrewWord = tokenizer.UseTerminal(RegExp.Literal("עברית", Encoding.Unicode)); // 1
            var russianWord = tokenizer.UseTerminal(RegExp.Literal("русский", Encoding.Unicode)); // 2
            var englishWord = tokenizer.UseTerminal(RegExp.Literal("english", Encoding.Unicode)); // 3

            var whitespace = tokenizer.UseTerminal(RegExp.AtLeastOneOf(RegExp.Choice(
                RegExp.Literal(' ', Encoding.Unicode), 
                RegExp.Literal('\t', Encoding.Unicode), 
                RegExp.Literal('\n', Encoding.Unicode))));

            tokenizer.BuildTransitions();

            const string tokens  = "1    23      45      67     89      01      23    ";
            const string indices = "01234567890123456789012345678901234567890123456789";
            const string input   = "עברית русский english עברית english русский עברית";
            int[] tokenClasses = new int[input.Length];
            int[] tokenIndices = new int[input.Length];

            int hebClass = hebrewWord.TokenClassID;
            int engClass = englishWord.TokenClassID;
            int rusClass = russianWord.TokenClassID;
            int wsClass = whitespace.TokenClassID;

            int[] expectedTokenClasses = new[] { hebClass, wsClass, rusClass, wsClass, engClass, 
                wsClass, hebClass, wsClass, engClass, wsClass, rusClass, wsClass, hebClass};
            int[] expectedTokenIndices = new[] { 0, 5, 6, 13, 14, 21, 22, 27, 28, 35, 36, 43, 44 };

            var rawInput = Encoding.Unicode.GetBytes(input);

            tokenizer.TokensClasses = tokenClasses;
            tokenizer.TokensIndices = tokenIndices;
            int tokensNum = tokenizer.Tokenize(rawInput, 0, rawInput.Length) + 1;

            Assert.That(tokensNum, Is.EqualTo(expectedTokenClasses.Length));

            for (int i = 0; i < tokensNum; i++)
            {
                Assert.That(tokenClasses[i], Is.EqualTo(expectedTokenClasses[i]));
                Assert.That(tokenIndices[i], Is.EqualTo(expectedTokenIndices[i]*2)); // Each symbol takes 2 bytes
            }
        }
    }
}
