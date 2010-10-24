using System;
using System.Collections.Generic;
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
        public void LiteralRegExpNFAConstructionTest()
        {
            const char LITERAL = 'a';

            var regExp = RegExp.Literal(LITERAL);

            var nfa = regExp.AsNFA();

            Assert.That(nfa.StartState.Transitions[InputChar.For((byte) LITERAL)], 
                Is.EquivalentTo(new List<State> {nfa.Terminator}));
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
            Assert.That(Simulate(1, Simulate(nfa.StartState, null, CODE_A), null), Is.EqualTo(Simulate(nfa.StartState, null)));
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
                    throw new Exception("Simulation: no transition for the symbol.");

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
                    throw new Exception("Simulation: no transition for the symbol.");
                }
            }

            return currentState;
        }

        private static State Simulate(State s, params byte?[] inputChars)
        {
            return Simulate(0, s, inputChars);
        }
    }
}
