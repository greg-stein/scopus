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
        [Test]
        public void NfaToDfaConversionSimpleTest()
        {
            RegExp re = RegExp.Literal('a');
            var dfa = NFAToDFAConverter.Convert(re.AsNFA(true));

            Assert.True(Simulate(0, dfa.StartState, 'a').IsAccepting);
        }

        [Test]
        public void NfaToDfaConversionTest()
        {
            // (a|b)*ca
            RegExp re = RegExp.Sequence(RegExp.AnyNumberOf(RegExp.Choice(RegExp.Literal('a'), RegExp.Literal('b'))), RegExp.Literal('c'),
                RegExp.Literal('a'));
            var dfa = NFAToDFAConverter.Convert(re.AsNFA(true));

            var str = new char?[] {'a', 'b', 'c', 'a'};

            Assert.True(Simulate(0, dfa.StartState, str).IsAccepting);
        }


        [Test]
        public void LiteralRegExpNFAConstructionTest()
        {
            const char LITERAL = 'a';

            var regExp = RegExp.Literal(LITERAL);

            var nfa = regExp.AsNFA();

            Assert.That(nfa.StartState.Transitions[InputChar.For(LITERAL)], Is.EquivalentTo(new List<State> {nfa.Terminator}));
        }

        [Test]
        public void SequenceRegExpNFAConstructionTest()
        {
            var regExp = RegExp.Sequence(RegExp.Literal('a'), RegExp.Literal('b'));
            var nfa = regExp.AsNFA();

            Assert.That(Simulate(nfa.StartState, 'a', null, 'b'), Is.EqualTo(nfa.Terminator));
        }
        
        [Test]
        public void AlternativeRegExpNFAConstructionTest()
        {
            var regExp = RegExp.Choice(RegExp.Literal('a'), RegExp.Literal('b'));
            var nfa = regExp.AsNFA();

            Assert.That(Simulate(nfa.StartState, null, 'a', null), Is.EqualTo(nfa.Terminator));
            Assert.That(Simulate(1, nfa.StartState, null, 'b', null), Is.EqualTo(nfa.Terminator));
        }
        
        [Test]
        public void OptionalRegExpNFAConstructionTest()
        {
            var regExp = RegExp.Optional(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();

            Assert.That(Simulate(nfa.StartState, null, 'a', null), Is.EqualTo(nfa.Terminator));
            Assert.That(Simulate(1, nfa.StartState, null), Is.EqualTo(nfa.Terminator));
        }

        [Test]
        public void AnyNumberOfRegExpNFAConstructionTest()
        {
            var regExp = RegExp.AnyNumberOf(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();

            Assert.That(Simulate(nfa.StartState, null, 'a', null),  Is.EqualTo(nfa.Terminator));
            Assert.That(Simulate(1, nfa.StartState, new char?[] { null }), Is.EqualTo(nfa.Terminator));
            Assert.That(Simulate(1, Simulate(nfa.StartState, null, 'a'), null),Is.EqualTo(Simulate(nfa.StartState, null)));
        }

        [Test]
        public void AtLeastOneOfRegExpNFAConstructionTest()
        {
            var regExp = RegExp.AtLeastOneOf(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();
            
            Assert.That(Simulate(nfa.StartState, null, 'a', null, 'a', null), Is.EqualTo(Simulate(nfa.StartState, null, 'a', null)));
            Assert.That(Simulate(nfa.StartState, null, 'a', null, null), Is.EqualTo(nfa.Terminator));
            Assert.That(Simulate(1, Simulate(nfa.StartState, null, 'a', null, 'a'), null), Is.EqualTo(nfa.Terminator));
        }

        private static State Simulate(int transitionToUse, State s, params char?[] inputChars)
        {
            if (inputChars == null)
                inputChars = new char?[] {null};

            State currentState = s;
            foreach (var inputChar in inputChars)
            {
                var ic = inputChar == null ? InputChar.Epsilon() : InputChar.For((char)inputChar);
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

        private static State Simulate(State s, params char?[] inputChars)
        {
            return Simulate(0, s, inputChars);
        }
    }
}
