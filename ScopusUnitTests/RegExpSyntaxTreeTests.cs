using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Scopus.LexicalAnalysis;
using Scopus.LexicalAnalysis.Algorithms;
using Scopus.LexicalAnalysis.RegularExpressions;

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

            var accState = Simulate(dfa.StartState, CODE_A);

            Assert.That(accState.IsAccepting);
            Assert.That(accState.TokenClass, Is.EqualTo(5));
        }

        [Test]
        public void NfaToDfaConversionSimpleTest()
        {
            RegExp re = RegExp.Literal('a');
            var dfa = NFAToDFAConverter.Convert(re.AsNFA(true));

            Assert.True(Simulate(0, dfa.StartState, CODE_A).IsAccepting);
        }

        [Test]
        public void NfaToDfaConversionTest()
        {
            // (a|b)*ca
            RegExp re = RegExp.Sequence(RegExp.AnyNumberOf(RegExp.Choice(RegExp.Literal('a'), RegExp.Literal('b'))), RegExp.Literal('c'),
                RegExp.Literal('a'));
            var dfa = NFAToDFAConverter.Convert(re.AsNFA(true));

            var str = new byte?[] { CODE_A, CODE_B, CODE_C, CODE_A };

            Assert.True(Simulate(0, dfa.StartState, str).IsAccepting);
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

            var state = Simulate(dfa.StartState, CODE_A, CODE_A, CODE_A);
            Assert.That(state.IsAccepting);

            state = Simulate(dfa.StartState, CODE_A, CODE_A, CODE_B);
            Assert.That(state.IsAccepting);

            state = Simulate(dfa.StartState, CODE_A);
            Assert.That(state.IsAccepting);

            state = Simulate(dfa.StartState, CODE_B);
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

            var terminator = Simulate(nfa.StartState,
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
            var terminator = Simulate(nfa.StartState, charCodes[0], charCodes[1], charCodes[2], charCodes[3], charCodes[4], charCodes[5], 
                charCodes[6], charCodes[7], charCodes[8], charCodes[9], charCodes[10], charCodes[11]);

            Assert.That(terminator, Is.EqualTo(nfa.Terminator));
            Assert.That(terminator.IsAccepting);
        }

        [Test]
        public void SequenceRegExpNFAConstructionTest()
        {
            var regExp = RegExp.Sequence(RegExp.Literal('a'), RegExp.Literal('b'));
            var nfa = regExp.AsNFA();

            Assert.That(Simulate(nfa.StartState, CODE_A, null, CODE_B), Is.EqualTo(nfa.Terminator));
        }
        
        [Test]
        public void AlternativeRegExpNFAConstructionTest()
        {
            var regExp = RegExp.Choice(RegExp.Literal('a'), RegExp.Literal('b'));
            var nfa = regExp.AsNFA();

            Assert.That(Simulate(nfa.StartState, null, CODE_A, null), Is.EqualTo(nfa.Terminator));
            Assert.That(Simulate(1, nfa.StartState, null, CODE_B, null), Is.EqualTo(nfa.Terminator));
        }
        
        [Test]
        public void OptionalRegExpNFAConstructionTest()
        {
            var regExp = RegExp.Optional(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();

            Assert.That(Simulate(nfa.StartState, null, CODE_A, null), Is.EqualTo(nfa.Terminator));
            Assert.That(Simulate(1, nfa.StartState, null), Is.EqualTo(nfa.Terminator));
        }

        [Test]
        public void AnyNumberOfRegExpNFAConstructionTest()
        {
            var regExp = RegExp.AnyNumberOf(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();

            Assert.That(Simulate(nfa.StartState, null, CODE_A, null), Is.EqualTo(nfa.Terminator));
            Assert.That(Simulate(1, nfa.StartState, new byte?[] { null }), Is.EqualTo(nfa.Terminator));
            Assert.That(Simulate(1, Simulate(nfa.StartState, null, CODE_A), null), Is.EqualTo(Simulate(nfa.StartState, (byte?)null)));
        }

        [Test]
        public void AtLeastOneOfRegExpNFAConstructionTest()
        {
            var regExp = RegExp.AtLeastOneOf(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();

            Assert.That(Simulate(nfa.StartState, null, CODE_A, null, CODE_A, null), Is.EqualTo(Simulate(nfa.StartState, null, CODE_A, null)));
            Assert.That(Simulate(nfa.StartState, null, CODE_A, null, null), Is.EqualTo(nfa.Terminator));
            Assert.That(Simulate(1, Simulate(nfa.StartState, null, CODE_A, null, CODE_A), null), Is.EqualTo(nfa.Terminator));
        }

        [Test]
        public void NegationRegExpConstructionTest()
        {
            var regExp = RegExp.Not(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();

            var simulatedState = Simulate(nfa.StartState, CODE_A);

            Assert.That(simulatedState, Is.Not.EqualTo(nfa.Terminator));
            Assert.IsFalse(simulatedState.IsAccepting);


            for (int i = Byte.MinValue; i <= Byte.MaxValue; i++)
            {
                if (i == CODE_A) continue; // skip 'a' since we have already tested it above

                simulatedState = Simulate(nfa.StartState, (byte) i);
                Assert.That(simulatedState.IsAccepting);
                Assert.That(simulatedState, Is.EqualTo(nfa.Terminator));
            }
        }

        [Test]
        public void NegationRegExpStringConstructionTest()
        {
            var regExp = RegExp.Not(RegExp.Literal("abcdef"));
            var nfa = regExp.AsNFA();

            var simulatedState = Simulate(nfa.StartState, CODE_A, CODE_B, CODE_C, CODE_D, CODE_E, CODE_F);

            Assert.That(simulatedState, Is.Not.EqualTo(nfa.Terminator));
            Assert.IsFalse(simulatedState.IsAccepting);


            for (int i = Byte.MinValue; i <= Byte.MaxValue; i++)
            {
                simulatedState = Simulate(nfa.StartState, (byte)i);
                Assert.That(simulatedState.IsAccepting);
            }

            string[] words = new string[] {"abcdes", "a", "xde", "abc", "abcdXefgh"}; // Decide what to do with abcdefXXX

            foreach (var word in words)
            {
                simulatedState = Simulate(nfa.StartState, word);
                Assert.That(simulatedState.IsAccepting);
            }
        }

        // My first Data Driven Unit Test :)
        //TODO: [TestCase("utf-7", new[] {'a', 'b', 'c'})]
        [TestCase("utf-16be", new[] { 'a', 'ש', 'я' })]
        [TestCase("utf-16le", new[] { 'a', 'ש', 'я' })]
        [TestCase("utf-8", new[] { 'a', 'ש', 'я' })]
        [TestCase("ascii", new[] { 'a', 'b', 'c' })]
        [Test]
        public void NegatedCharRegExpConstructionTest(string encodingName, params char[] exceptees)
        {
            // Since it is impossible to pass Encoding class in attribute parameters, we 
            // obtain it depending on given string.
            var encoding = GetEncoding(encodingName);
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

        [TestCase('a', 'ש', "utf-16le")]
        [TestCase('a', 'ש', "utf-16be")]
        [TestCase('a', 'ש', "utf-8")]
        [TestCase('a', 'g', "ascii")]
        [Test]
        public void RangeRegExpConstructionTest(char left, char right, string encodingName)
        {
            var encoding = GetEncoding(encodingName);
            var regExp = RegExp.Range(left, right, encoding);
            var nfa = regExp.AsNFA(true);

            State simulatedState;
            for (int i = 0; i <= char.MaxValue; i++)
            {
                var bytes = encoding.GetBytes(new[] { (char)i });
                if (i >= left && i <= right)
                {
                    simulatedState = SimulateNFA(nfa.StartState, bytes);
                    Assert.That(simulatedState.IsAccepting, String.Format("Failed! i = {0} ({1})", i, (char)i));
                }
                else
                {
                    Assert.Throws(typeof(SimulationException), () => SimulateNFA(nfa.StartState, bytes));
                }
            }
        }

        private static Encoding GetEncoding(string name)
        {
            switch (name.ToLower())
            {
                case "utf-16le":
                    return Encoding.Unicode;
                case "utf-8":
                    return Encoding.UTF8;
                case "utf-7":
                    return Encoding.UTF7;
                case "utf-16be":
                    return Encoding.BigEndianUnicode;
                case "utf-32le":
                    return Encoding.UTF32;
                case "ascii":
                    return Encoding.ASCII;

                default:
                    return Encoding.Unicode;
            }
        }

        private static State SimulateNFA(State transitionToUse, params byte[] inputChars)
        {
            byte?[] nullableBytes = new byte?[inputChars.Length];

            for (int i = 0; i < inputChars.Length; i++)
            {
                nullableBytes[i] = inputChars[i];
            }

            return Simulate(transitionToUse, nullableBytes);
        }

        private static State Simulate(int transitionToUse, State s, params byte?[] inputChars)
        {
            if (inputChars == null)
                inputChars = new byte?[] {null};

            State currentState = s;
            foreach (var inputChar in inputChars)
            {
                var ic = inputChar == null ? InputChar.Epsilon() : InputChar.For((byte) inputChar);
                List<State> transitions;
                if (!currentState.Transitions.TryGetValue(ic, out transitions))
                    throw new SimulationException("Simulation: no transition for the symbol.");

                if (transitions.Count > transitionToUse)
                {
                    currentState = transitions[transitionToUse];
                }
                else if (transitions.Count > 0)
                {
                    currentState = transitions[0];
                }
                else
                {
                    throw new SimulationException("Simulation: no transition for the symbol.");
                }
            }

            return currentState;
        }

        private class SimulationException : Exception
        {
            public SimulationException(string message)
                : base(message)
            {
            }
        }

        private static State Simulate(State s, params byte?[] inputChars)
        {
            return Simulate(0, s, inputChars);
        }

        private static State Simulate(State s, string input)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            State state = s;
            foreach (var b in bytes)
            {
                state = Simulate(state, b);
            }

            return state;
        }
    }
}
