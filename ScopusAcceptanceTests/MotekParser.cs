using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Scopus;
using Scopus.LexicalAnalysis;
using Scopus.LexicalAnalysis.RegularExpressions;
using Scopus.SyntaxAnalysis;

namespace ScopusAcceptanceTests
{
    [TestFixture]
    public class MotekParser
    {
        private Lexer lexer;
        private Encoding encoding = Encoding.ASCII;
        private Terminal mIdentifier;
        private Terminal mEqualSign;
        private Terminal mPlusSign;
        private Terminal mAsterisk;
        private Terminal mDevideSign;
        private Terminal mNumber;
        private Terminal mSemicolon;
        private Terminal mLeftBrace;
        private Terminal mRightBrace;
        private Terminal mComma;
        private Terminal mMinusSign;

        [Test]
        public void Test()
        {
            lexer = ScopusFacade.GetLexer();
            SetUpLexer(); // This should be called before BuildGrammar, since grammar use terminals defined here
            var motekGrammar = BuildGrammar();
            var parser = ScopusFacade.GetSLRParser(motekGrammar, lexer);

            var input = @"
domain::other  = 7;
variable = domain::other + 5;
another = sin(variable)*other;
";
            var inputStream = new MemoryStream(encoding.GetBytes(input));
            lexer.SetDataSource(inputStream);
            parser.ParseInput();
        }

        private void SetUpLexer()
        {
            mIdentifier = lexer.UseTerminal(RegExp.Sequence( // ([a-z]+::)?[a-z]+
                                                  RegExp.Optional( // ([a-z]+::)?
                                                      RegExp.Sequence( //[a-z]+::
                                                          RegExp.AtLeastOneOf(RegExp.Range('a', 'z', encoding)),
                                                          RegExp.Literal("::"))
                                                      ),
                                                  RegExp.AtLeastOneOf(RegExp.Range('a', 'z', encoding))
                                                  ));
            mEqualSign = lexer.UseTerminal(RegExp.Literal('='));
            // Ignore white space
            lexer.IgnoreTerminal(RegExp.AtLeastOneOf(RegExp.Choice(RegExp.Literal(' '), RegExp.Literal('\t'), RegExp.Literal('\n'), RegExp.Literal(0x0D), RegExp.Literal(0x0A))));
            mPlusSign = lexer.UseTerminal(RegExp.Literal('+'));
            mMinusSign = lexer.UseTerminal(RegExp.Literal('-'));
            mAsterisk = lexer.UseTerminal(RegExp.Literal('*'));
            mDevideSign = lexer.UseTerminal(RegExp.Literal('/'));
            mNumber = lexer.UseTerminal(RegExp.Sequence(RegExp.Optional(RegExp.Literal('-')), RegExp.AtLeastOneOf(RegExp.Range('0', '9', encoding))));
            mSemicolon = lexer.UseTerminal(RegExp.Literal(';'));
            mLeftBrace = lexer.UseTerminal(RegExp.Literal('('));
            mRightBrace = lexer.UseTerminal(RegExp.Literal(')'));
            mComma = lexer.UseTerminal(RegExp.Literal(','));
        }

        private AugmentedGrammar BuildGrammar()
        {
            var motekCode = new NonTerminal("motekCode"); // M
            var expression = new NonTerminal("expression"); // S
            var statement = new NonTerminal("statement"); // H
            var addedStatement = new NonTerminal("addedStatement"); // A
            var multipliedStatement = new NonTerminal("multipliedStatement"); // F
            var parameter = new NonTerminal("parameter");// p
            var parameters = new NonTerminal("parameters"); // P
            var functionCall = new NonTerminal("functionCall"); // U
            var motekGrammar = new AugmentedGrammar()
            {
                motekCode --> statement & mSemicolon & motekCode ^ (ProcessTerminals), // M --> H ; M
                motekCode --> statement & mSemicolon ^ (ProcessTerminals), // M --> H ;
                statement --> mIdentifier & mEqualSign & expression ^ (ProcessTerminals), // H --> id = S
                expression --> expression & mPlusSign & addedStatement ^ (ProcessTerminals), // S --> S + A
                expression --> expression & mMinusSign & addedStatement ^ (ProcessTerminals), // S --> S - A
                expression --> addedStatement ^ (ProcessTerminals), // S --> A,
                addedStatement --> addedStatement & mAsterisk & multipliedStatement ^ (ProcessTerminals), // A --> A * F
                addedStatement --> addedStatement & mDevideSign & multipliedStatement ^ (ProcessTerminals), // A --> A / F
                addedStatement --> multipliedStatement ^ (ProcessTerminals), // A -> F
                multipliedStatement --> functionCall, // F --> U
                multipliedStatement --> mLeftBrace & expression & mRightBrace ^ (ProcessTerminals), // F --> ( S )
                multipliedStatement --> mNumber ^ (ProcessTerminals), // F --> number
                multipliedStatement --> mIdentifier ^ (ProcessTerminals), // F --> I
                functionCall --> mIdentifier & mLeftBrace & parameters & mRightBrace ^ (ProcessTerminals), // U --> I ( P )
                parameters --> parameters & mComma & parameter ^ (ProcessTerminals), // P --> P , p
                parameters --> parameter  ^ (ProcessTerminals),// P --> p
                parameter --> expression // p --> S
            };
            return motekGrammar;
        }

        private void ProcessTerminals(TerminalValues terminalValues)
        {
            Console.WriteLine(terminalValues);
        }
    }
}
