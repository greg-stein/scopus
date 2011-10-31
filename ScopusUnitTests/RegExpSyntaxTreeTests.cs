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
    public class RegExpNFAConstructionTests
    {
        private const byte CODE_A = (byte)'a';
        private const byte CODE_B = (byte)'b';
        private const byte CODE_C = (byte)'c';
        private const byte CODE_D = (byte)'d';
        private const byte CODE_E = (byte)'e';
        private const byte CODE_F = (byte)'f';

        [Test]
        public void AcceptingStatePreserveTest()
        {
            RegExp re = RegExp.Literal('a');
            var nfa = re.AsNFA(true); // True - mark terminator as accepting state
            nfa.Terminator.TokenClass = 5; // Token ID
            var dfa = NFAToDFAConverter.Convert(nfa);

            var accState = dfa.StartState.Simulate(inputChars: CODE_A);

            Assert.That(accState.IsAccepting);
            Assert.That(accState.TokenClass, Is.EqualTo(5));
        }

        [Test]
        public void NfaToDfaConversionSimpleTest()
        {
            RegExp re = RegExp.Literal('a');
            var dfa = NFAToDFAConverter.Convert(re.AsNFA(true));

            Assert.True(dfa.StartState.Simulate(0, CODE_A).IsAccepting);
        }

        [Test]
        public void NfaToDfaConversionTest()
        {
            // (a|b)*ca
            RegExp re = RegExp.Sequence(RegExp.AnyNumberOf(RegExp.Choice(RegExp.Literal('a'), RegExp.Literal('b'))), RegExp.Literal('c'),
                RegExp.Literal('a'));
            var dfa = NFAToDFAConverter.Convert(re.AsNFA(true));

            var str = new byte?[] { CODE_A, CODE_B, CODE_C, CODE_A };

            Assert.True(dfa.StartState.Simulate(0, str).IsAccepting);
        }

        [Test]
        public void SelfTransitionsConvertionTest()
        {
            // a*b?
            var nfa = new FiniteAutomata("test", true);
            nfa.StartState.AddTransitionTo(nfa.StartState, InputChar.For((byte)'a'));
            nfa.StartState.AddTransitionTo(nfa.Terminator, InputChar.For((byte)'b'));
            nfa.StartState.AddTransitionTo(nfa.Terminator, InputChar.For((byte)'a'));
            nfa.Terminator.IsAccepting = true;

            var dfa = NFAToDFAConverter.Convert(nfa);

            var state = dfa.StartState.Simulate(0, CODE_A, CODE_A, CODE_A);
            Assert.That(state.IsAccepting);

            state = dfa.StartState.Simulate(0, CODE_A, CODE_A, CODE_B);
            Assert.That(state.IsAccepting);

            state = dfa.StartState.Simulate(0, CODE_A);
            Assert.That(state.IsAccepting);

            state = dfa.StartState.Simulate(0, CODE_B);
            Assert.That(state.IsAccepting);
        }

        [Test]
        public void LiteralRegExpNFAConstructionTest()
        {
            const char LITERAL = 'a';

            var regExp = RegExp.Literal(LITERAL);

            var nfa = regExp.AsNFA();

            Assert.That(nfa.StartState.Transitions[InputChar.For((byte) LITERAL)], 
                Is.EquivalentTo(new List<State> {nfa.Terminator}));
        }

        [Test]
        public void LiteralStringRegExpNFAConstructionTest()
        {
            const string LITERAL = "abcdef";

            var regExp = RegExp.Literal(LITERAL);
            var nfa = regExp.AsNFA(true);

            var terminator = nfa.StartState.Simulate(0, 
                (byte) 'a', (byte) 'b', (byte) 'c', (byte) 'd', (byte) 'e', (byte) 'f');

            Assert.That(terminator, Is.EqualTo(nfa.Terminator));
            Assert.That(terminator.IsAccepting);
        }

        [Test]
        public void UnicodeStringNFAConstructionTest()
        {
            const string LITERAL = "abcdef";

            var regExp = RegExp.Literal(LITERAL);
            regExp.Encoding = Encoding.Unicode;

            var nfa = regExp.AsNFA(true);

            var charCodes = Encoding.Unicode.GetBytes(LITERAL);
            var terminator = nfa.StartState.Simulate(0, charCodes[0], charCodes[1], charCodes[2], charCodes[3], charCodes[4], charCodes[5], 
                charCodes[6], charCodes[7], charCodes[8], charCodes[9], charCodes[10], charCodes[11]);

            Assert.That(terminator, Is.EqualTo(nfa.Terminator));
            Assert.That(terminator.IsAccepting);
        }

        [Test]
        public void SequenceRegExpNFAConstructionTest()
        {
            var regExp = RegExp.Sequence(RegExp.Literal('a'), RegExp.Literal('b'));
            var nfa = regExp.AsNFA();

            Assert.That(nfa.StartState.Simulate(0, CODE_A, null, CODE_B), Is.EqualTo(nfa.Terminator));
        }
        
        [Test]
        public void AlternativeRegExpNFAConstructionTest()
        {
            var regExp = RegExp.Choice(RegExp.Literal('a'), RegExp.Literal('b'));
            var nfa = regExp.AsNFA();

            Assert.That(nfa.StartState.Simulate(0, null, CODE_A, null), Is.EqualTo(nfa.Terminator));
            Assert.That(nfa.StartState.Simulate(1, null, CODE_B, null), Is.EqualTo(nfa.Terminator));
        }
        
        [Test]
        public void OptionalRegExpNFAConstructionTest()
        {
            var regExp = RegExp.Optional(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();

            Assert.That(nfa.StartState.Simulate(0, null, CODE_A, null), Is.EqualTo(nfa.Terminator));
            Assert.That(nfa.StartState.Simulate(1, null), Is.EqualTo(nfa.Terminator));
        }

        [Test]
        public void AnyNumberOfRegExpNFAConstructionTest()
        {
            var regExp = RegExp.AnyNumberOf(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();

            Assert.That(nfa.StartState.Simulate(0, null, CODE_A, null), Is.EqualTo(nfa.Terminator));
            Assert.That(nfa.StartState.Simulate(1, new byte?[] { null }), Is.EqualTo(nfa.Terminator));
            Assert.That(nfa.StartState.Simulate(0, null, CODE_A).Simulate(1, null), Is.EqualTo(nfa.StartState.Simulate(0, (byte?)null)));
        }

        [Test]
        public void AtLeastOneOfRegExpNFAConstructionTest()
        {
            var regExp = RegExp.AtLeastOneOf(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();

            Assert.That(nfa.StartState.Simulate(0, null, CODE_A, null, CODE_A, null), Is.EqualTo(nfa.StartState.Simulate(0, null, CODE_A, null)));
            Assert.That(nfa.StartState.Simulate(0, null, CODE_A, null, null), Is.EqualTo(nfa.Terminator));
        }

        [Test]
        public void NegationRegExpConstructionTest()
        {
            var regExp = RegExp.Not(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();

            var simulatedState = nfa.StartState.Simulate(0, CODE_A);

            Assert.That(simulatedState, Is.Not.EqualTo(nfa.Terminator));
            Assert.IsFalse(simulatedState.IsAccepting);


            for (int i = Byte.MinValue; i <= Byte.MaxValue; i++)
            {
                if (i == CODE_A) continue; // skip 'a' since we have already tested it above

                simulatedState = nfa.StartState.Simulate(0, (byte)i);
                Assert.That(simulatedState.IsAccepting);
                Assert.That(simulatedState, Is.EqualTo(nfa.Terminator));
            }
        }

        [Test]
        public void NegationRegExpStringConstructionTest()
        {
            var regExp = RegExp.Not(RegExp.Literal("abcdef"));
            var nfa = regExp.AsNFA();

            var simulatedState = nfa.StartState.Simulate(0, CODE_A, CODE_B, CODE_C, CODE_D, CODE_E, CODE_F);

            Assert.That(simulatedState, Is.Not.EqualTo(nfa.Terminator));
            Assert.IsFalse(simulatedState.IsAccepting);


            for (int i = Byte.MinValue; i <= Byte.MaxValue; i++)
            {
                simulatedState = nfa.StartState.Simulate(0, (byte)i);
                Assert.That(simulatedState.IsAccepting);
            }

            string[] words = new string[] {"abcdes", "a", "xde", "abc", "abcdXefgh"}; // Decide what to do with abcdefXXX

            foreach (var word in words)
            {
                simulatedState = nfa.StartState.Simulate(word);
                Assert.That(simulatedState.IsAccepting);
            }
        }

        private static
            object[] TestCases = {
                                     new object[] {Encoding.ASCII, new[] { 'a', 'b', 'c' } },
                                     new object[] {Encoding.BigEndianUnicode, new[] { 'a', 'ש', 'я' }},
                                     new object[] {Encoding.Unicode, new[] { 'a', 'ש', 'я' }}, 
                                     new object[] {Encoding.UTF32, new[] { 'a', 'ש', 'я' }}, 
                                     new object[] {Encoding.UTF8, new[] { 'a', 'ש', 'я' }}
                                 };

        // My first Data Driven Unit Test :)
        // UTF-7 is not byte-aligned and hence is not supported: [TestCase("utf-7", new[] {'a', 'b', 'c'})]
        [TestCaseSource("TestCases")]
        [Test]
        public void NegatedCharRegExpConstructionTest(Encoding encoding, params char[] exceptees)
        {
            var regExp = RegExp.LiteralExcept(encoding, exceptees);
            var nfa = regExp.AsNFA(true);

            State simulatedState;
            for (int i = 0; i <= char.MaxValue; i++)
            {
                byte[] bytes = encoding.GetBytes(new[] { (char)i });

                // If i is NOT in the exceptees
                if (Array.IndexOf(exceptees, (char) i) == -1)
                {
                    simulatedState = SimulateNFA(nfa.StartState, bytes);

                    Assert.That(simulatedState.IsAccepting, String.Format("Failed! i = {0} ({1})", i, (char) i));
                }
                else
                {
                    Assert.Throws(typeof(SimulationException), () => SimulateNFA(nfa.StartState, bytes));
                }
            }
        }

        [Test]
        public void SequenceRegExpMultipleParametersTest()
        {
            var regExp = new SequenceRegExp(new LiteralRegExp('a'), new LiteralRegExp('b'), new LiteralRegExp('c'),
                                            new LiteralRegExp('d'), new LiteralRegExp('e'), new LiteralRegExp('f'));

            //var expectedRE = new SequenceRegExp()
        }

        [Test]
        public void CrossAutomataConstructionTest()
        {
            // (aa|bb|cc)*?ab
            var repeatedRegExp = RegExp.Choice(RegExp.Literal("aa"), RegExp.Literal("bb"), RegExp.Literal("cc"));
            var suffixRegExp = RegExp.Literal("ab");
            var crossRegExp = RegExp.RepeatUntil(repeatedRegExp, suffixRegExp);

            var crossAutomata = crossRegExp.AsNFA(true);
            var simulatedState = crossAutomata.StartState.Simulate("aabbccbbaa");
            Assert.False(simulatedState.IsAccepting);

            simulatedState = crossAutomata.StartState.Simulate("ccaabbccaabbab");
            Assert.That(simulatedState.IsAccepting);
            simulatedState = crossAutomata.StartState.Simulate("ab");
            Assert.That(simulatedState.IsAccepting);

            simulatedState = crossAutomata.StartState.Simulate("ccaabbccaabba");
            Assert.False(simulatedState.IsAccepting);
            Assert.Throws<SimulationException>(() => crossAutomata.StartState.Simulate("ccaabbccaabbabab"));
        }

        [TestCase('a', 'ש', "utf-16le")]
        [TestCase('a', 'ש', "utf-16be")]
        [TestCase('a', 'ש', "utf-8")]
        [TestCase('a', 'g', "ascii")]
        [Test]
        public void RangeRegExpConstructionTest(char left, char right, string encodingName)
        {
            var encoding = CommonTestRoutines.GetEncoding(encodingName);
            var regExp = RegExp.Range(left, right, encoding);
            var nfa = regExp.AsNFA(true);
#if TESTDFA
            var dfa = NFAToDFAConverter.Convert(nfa);
#endif
            State simulatedState;
            for (int i = 0; i <= char.MaxValue; i++)
            {
                var bytes = encoding.GetBytes(new[] { (char)i });
                if (i >= left && i <= right)
                {
                    simulatedState = SimulateNFA(nfa.StartState, bytes);
                    Assert.That(simulatedState.IsAccepting, String.Format("Failed! i = {0} ({1})", i, (char)i));
#if TESTDFA
                    simulatedState = SimulateNFA(dfa.StartState, bytes);
                    Assert.That(simulatedState.IsAccepting, String.Format("Failed! i = {0} ({1})", i, (char)i));
#endif
                }
                else
                {
                    Assert.Throws(typeof (SimulationException), () => SimulateNFA(nfa.StartState, bytes));
#if TESTDFA
                    Assert.Throws(typeof(SimulationException), () => SimulateNFA(dfa.StartState, bytes));
#endif
                }
            }
        }

        private static State SimulateNFA(State state, params byte[] inputChars)
        {
            byte?[] nullableBytes = new byte?[inputChars.Length];

            for (int i = 0; i < inputChars.Length; i++)
            {
                nullableBytes[i] = inputChars[i];
            }

            return state.Simulate(0, nullableBytes);
        }
    }
}
