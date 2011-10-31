using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Scopus;
using Scopus.LexicalAnalysis;
using Scopus.LexicalAnalysis.RegularExpressions;
using Scopus.SyntaxAnalysis;

namespace ScopusAcceptanceTests
{
    [TestFixture]
    public class MISAParser
    {
        private Lexer lexer;
        private Encoding encoding = Encoding.ASCII;
        private Terminal mComment;
        private Terminal mInstruction;
        private Terminal mRegister;
        private Terminal mNumber;
        private Terminal mHexNumber;
        private Terminal mComma;
        private Terminal mDot;
        private Terminal mSlash;
        private Terminal mLabel;
        private Terminal mDirective;
        private Terminal mNewLine;
        private Terminal mDotC;
        private Terminal mIdentifier;
        private Terminal mSemicolon;
        private Terminal mSquareBraceOpen;
        private Terminal mSquareBraceClose;
        private Terminal mPlus;
        private Terminal mColon;

        [Test]
        public void Test()
        {
            lexer = ScopusFacade.GetLexer();
            SetUpLexer(); // This should be called before BuildGrammar, since grammar use terminals defined here
            var mISAGrammar = BuildGrammar();
            var parser = ScopusFacade.GetSLRParser(mISAGrammar, lexer);

            // TODO: PARSE DIRECTIVE, I.E. ADD TOKENS FOR inclde, text ...
            var input = @"
! ldlohi16.s
!
! This program tests the ldlo.16 and ldhi.16 opcode

.include ""mISA_test_common_s.h""
      .text

        ! NO MACRO SUPPORT YET!!!
        !TEST_START test_ldhilo_16
        and.c.32 r63, r63, 0            !

        add.32 r56, r63, 0              ! test counter
        add.32 r57, r63, data_start     ! load init addr base

case1:
        add.32/0 r56, r56, 1;           ! increment test counter

        add.32 r58, r63, 0x00002211     ! load LO expected result
        add.32 r59, r63, 0x00000000     ! load HI expected result
        add.32 r1, r63, 0xffffffff     ! clear target
        add.32 r11,  r63, 0xffffffff     ! clear target

        ldlo.16    r1, nat:[r57]        ! + <nat:> 8 aligned
        bubbles 2
        CMP_AND_FAIL_REG .32 r1, r58, fail
.if CO20005 == 0
        ldhi.16    r11, nat:[r57+8]       ! + <nat:>
.else
        ldhi.16    r11, nat:[r57+64]       ! + <nat:>
.endif
        bubbles 2
        CMP_AND_FAIL_REG .32 r11, r59, fail
        or.64      r1, r1, r11
!        cmit                           ! - nat: is not speculative

case2:
        add.32/0 r56, r56, 1;           ! increment test counter
        sub.32/0 r57,  r57,  1;         ! decrement test address

        add.32 r58, r63, 0x00000008     ! load LO expected result
        add.32 r59, r63, 0x00001100     ! load HI expected result
        add.32 r2, r63, 0xffffffff     ! clear target
        add.32 r12,  r63, 0xffffffff     ! clear target

        ldlo.16    r2, nat:[r57]        ! + <nat:>   8 misaligned (1 less)
        bubbles 2
        CMP_AND_FAIL_REG .32 r2, r58, fail
.if CO20005 == 0
        ldhi.16    r12, nat:[r57+8]       ! + <nat:>
.else
        ldhi.16    r12, nat:[r57+64]       ! + <nat:>
.endif
        bubbles 2
        CMP_AND_FAIL_REG .32 r12, r59, fail
        or.64      r2, r2, r12
!        cmit                           ! - nat: is not speculative

case3:
        add.32/0 r56, r56, 1;           ! increment test counter
        add.32/0 r57,  r57,  2;         ! increment test address

        add.32 r58, r63, 0x00003322     ! load LO expected result
        add.32 r59, r63, 0x00000000     ! load HI expected result
        add.32 r3, r63, 0xffffffff     ! clear target
        add.32 r13,  r63, 0xffffffff     ! clear target
";
            var inputStream = new MemoryStream(encoding.GetBytes(input));
            lexer.SetDataSource(inputStream);
            parser.ParseInput();
        }

        private void SetUpLexer()
        {
            // Ignore white space
            lexer.IgnoreTerminal(RegExp.AtLeastOneOf(RegExp.Choice(RegExp.Literal(' '), RegExp.Literal('\t'))));
            // Ignore comments (![a-zA-Z0-9...]\n)
            lexer.IgnoreTerminal(RegExp.Sequence(RegExp.Literal('!'),
                                                RegExp.AnyNumberOf((
                                                    RegExp.Range(' ', (char)255, encoding)))));
            // New line is delimiter between code lines
            mNewLine =
                lexer.UseTerminal(RegExp.Choice(RegExp.Literal('\n'), RegExp.Literal(0x0D), RegExp.Literal(0x0A)));

            mIdentifier =
                lexer.UseTerminal(
                    RegExp.Sequence(
                        RegExp.Choice(RegExp.Range('a', 'z', encoding), RegExp.Range('A', 'Z', encoding),
                                      RegExp.Literal('_')),
                        RegExp.AnyNumberOf(RegExp.Choice(RegExp.Range('a', 'z', encoding), RegExp.Range('A', 'Z', encoding),
                                      RegExp.Literal('_'), RegExp.Range('0', '9', encoding)))));

            mRegister = lexer.UseTerminal(RegExp.Sequence(RegExp.Literal('r'), RegExp.Range('0', '9', encoding), RegExp.Optional(RegExp.Range('0', '9', encoding))));
            mNumber = lexer.UseTerminal(RegExp.Sequence(RegExp.Optional(RegExp.Literal('-')), RegExp.AtLeastOneOf(RegExp.Range('0', '9', encoding))));
            mHexNumber =
                lexer.UseTerminal(RegExp.Sequence(RegExp.Literal("0x"),
                                                  RegExp.AtLeastOneOf(RegExp.Choice(
                                                      RegExp.Range('0', '9', encoding),
                                                      RegExp.Range('a', 'f', encoding)))));
            mComma = lexer.UseTerminal(RegExp.Literal(','));
            mDot = lexer.UseTerminal(RegExp.Literal('.'));
            mSemicolon = lexer.UseTerminal(RegExp.Literal(';'));
            mColon = lexer.UseTerminal(RegExp.Literal(':'));
            mSlash = lexer.UseTerminal(RegExp.Literal('/'));
            mSquareBraceOpen = lexer.UseTerminal(RegExp.Literal('['));
            mSquareBraceClose = lexer.UseTerminal(RegExp.Literal(']'));
            mPlus = lexer.UseTerminal(RegExp.Literal('+'));

            mDirective = lexer.UseTerminal(RegExp.Sequence(RegExp.Literal('.'), RegExp.Choice(RegExp.Literal("include"), RegExp.Literal("text"), RegExp.Literal("if"), RegExp.Literal("else"), RegExp.Literal("endif")),
                                                RegExp.AnyNumberOf(
                                                    RegExp.Range(' ', (char)255, encoding))));
            mDotC = lexer.UseTerminal(RegExp.Literal(".c"));
        }

        private AugmentedGrammar BuildGrammar()
        {
            var misaCode = new NonTerminal("mISA_code");
            var codeLine = new NonTerminal("codeline");
            var code = new NonTerminal("code");
            var instruction = new NonTerminal("instruction");
            var operands = new NonTerminal("operands");
            var operand = new NonTerminal("operand");
            var slashedInstruction = new NonTerminal("slashedInstruction");

            var motekGrammar = new AugmentedGrammar()
                                       {
                                           misaCode --> misaCode & codeLine,
                                           misaCode --> codeLine,
                                           codeLine --> code & mNewLine,
                                           codeLine --> mNewLine,
                                           code --> instruction & operands,
                                           code --> slashedInstruction & operands & mSemicolon,
                                           code --> mIdentifier & mColon,
                                           code --> mDirective,
                                           instruction --> mIdentifier,
                                           instruction --> mIdentifier & mDot & mNumber,
                                           instruction --> mIdentifier & mDotC & mDot & mNumber,
                                           instruction --> mIdentifier & mDotC & mDot & mNumber & mSlash & mNumber,
                                           slashedInstruction --> mIdentifier & mDot & mNumber & mSlash & mNumber,
                                           operands --> operand & mComma & operands,
                                           operands --> operand,
                                           operand --> mNumber,
                                           operand --> mHexNumber,
                                           operand --> mRegister,
                                           operand --> mIdentifier,
                                           operand --> mIdentifier & mColon & mSquareBraceOpen & mRegister & mSquareBraceClose,
                                           operand --> mIdentifier & mColon & mSquareBraceOpen & mRegister & mPlus & mNumber & mSquareBraceClose,
                                       };
            return motekGrammar;
        }

        private void ProcessTerminals(TerminalValues terminalValues)
        {
            Console.WriteLine(terminalValues);
        }
    }
}
