using System.IO;
using System.Text;
using NUnit.Framework;
using Scopus;
using ScopusUnitTests.Common;

namespace ScopusAcceptanceTests
{
    [TestFixture]
    public class SimpleLexerTest
    {
        [Test]
        public void SimpleTest(string encodingStr)
        {
            Encoding encoding = CommonTestRoutines.GetEncoding(encodingStr);
            string data = ""; //TODO
            byte[] dataBytes = encoding.GetBytes(data);
            var stream = new MemoryStream(dataBytes);
            var lexer = ScopusFacade.GetLexer();
            lexer.SetEncoding(encoding);
            var number = lexer.UseTerminal("[0-9]+");
            var comment = lexer.UseTerminal("/**.***/|//.*\n");
            var leftCurlBrace = lexer.UseTerminal("{");
            var rightCurlBrace = lexer.UseTerminal("}");
            var leftBrace = lexer.UseTerminal("(");
            var rightBrace = lexer.UseTerminal(")");
            var keyword = lexer.UseTerminal("if|for|while");
            var whitespace = lexer.UseTerminal(" |\t|\n");

            lexer.SetDataSource(stream);
            lexer.Initialize();
        }
    }
}
