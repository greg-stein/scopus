using System.Collections.Generic;
using NUnit.Framework;
using Scopus.LexicalAnalysis;
using Scopus.LexicalAnalysis.RegExp;

namespace ScopusUnitTests
{
    [TestFixture]
    public class RegExpNFAConstructionTests
    {
        [Test]
        public void LiteralRegExpNFAConstructionTest()
        {
            const char LITERAL = 'a';

            var regExp = RegExp.Literal(LITERAL);

            var nfa = regExp.AsNFA();

            Assert.That(nfa.StartState.Transition[InputChar.For(LITERAL)], Is.EquivalentTo(new List<State> {nfa.Terminator}));
        }

        [Test]
        public void SequenceRegExpNFAConstructionTest()
        {
            var regExp = RegExp.Sequence(RegExp.Literal('a'), RegExp.Literal('b'));
            var nfa = regExp.AsNFA();

            Assert.That(TransitPath(nfa.StartState, 'a', null, 'b'), Is.EqualTo(nfa.Terminator));
        }
        
        [Test]
        public void AlternativeRegExpNFAConstructionTest()
        {
            var regExp = RegExp.Choice(RegExp.Literal('a'), RegExp.Literal('b'));
            var nfa = regExp.AsNFA();

            Assert.That(TransitPath(nfa.StartState, null, 'a', null), Is.EqualTo(nfa.Terminator));
            Assert.That(TransitPath(1, nfa.StartState, null, 'b', null), Is.EqualTo(nfa.Terminator));
        }
        
        [Test]
        public void OptionalRegExpNFAConstructionTest()
        {
            var regExp = RegExp.Optional(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();

            Assert.That(TransitPath(nfa.StartState, null, 'a', null), Is.EqualTo(nfa.Terminator));
            Assert.That(TransitPath(1, nfa.StartState, null), Is.EqualTo(nfa.Terminator));
        }

        [Test]
        public void AnyNumberOfRegExpNFAConstructionTest()
        {
            var regExp = RegExp.AnyNumberOf(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();

            Assert.That(TransitPath(nfa.StartState, null, 'a', null),  Is.EqualTo(nfa.Terminator));
            Assert.That(TransitPath(1, nfa.StartState, new char?[] { null }), Is.EqualTo(nfa.Terminator));
            Assert.That(TransitPath(1, TransitPath(nfa.StartState, null, 'a'), null),Is.EqualTo(TransitPath(nfa.StartState, null)));
        }

        [Test]
        public void AtLeastOneOfRegExpNFAConstructionTest()
        {
            var regExp = RegExp.AtLeastOneOf(RegExp.Literal('a'));
            var nfa = regExp.AsNFA();
            
            Assert.That(TransitPath(nfa.StartState, null, 'a', null, 'a', null), Is.EqualTo(TransitPath(nfa.StartState, null, 'a', null)));
            Assert.That(TransitPath(nfa.StartState, null, 'a', null, null), Is.EqualTo(nfa.Terminator));
            Assert.That(TransitPath(1, TransitPath(nfa.StartState, null, 'a', null, 'a'), null), Is.EqualTo(nfa.Terminator));
        }

        private static State TransitPath(int transitionToUse, State s, params char?[] inputChars)
        {
            if (inputChars == null)
                inputChars = new char?[] {null};

            State currentState = s;
            foreach (var inputChar in inputChars)
            {
                var ic = inputChar == null ? InputChar.Epsilon() : InputChar.For((char)inputChar);
                if (currentState.Transition[ic].Count > transitionToUse)
                {
                    currentState = currentState.Transition[ic][transitionToUse];
                }
                else
                {
                    currentState = currentState.Transition[ic][0];
                }
            }

            return currentState;
        }

        private static State TransitPath(State s, params char?[] inputChars)
        {
            return TransitPath(0, s, inputChars);
        }
    }
}
