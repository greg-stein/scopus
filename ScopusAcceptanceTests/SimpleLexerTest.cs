using System.IO;
using System.Text;
using NUnit.Framework;
using Scopus;
using ScopusUnitTests.Common;

namespace ScopusAcceptanceTests
{
    [TestFixture]
    [Ignore] // TODO: Parsing of regular expressions is still TBD
    public class SimpleLexerTest
    {
        private static object[] TestCases = {
                                     new object[] {Encoding.ASCII},
                                     new object[] {Encoding.BigEndianUnicode},
                                     new object[] {Encoding.Unicode}, 
                                     new object[] {Encoding.UTF32}, 
                                     new object[] {Encoding.UTF8}
                                 };

        [TestCaseSource("TestCases")]
        [Test]
        public void SimpleTest(Encoding encoding)
        {
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
