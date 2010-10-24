using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Scopus.Exceptions;
using Scopus.LexicalAnalysis;
using Scopus.LexicalAnalysis.Algorithms;

namespace ScopusUnitTests
{
    [TestFixture]
    public class TableDrivenTransitionTests
    {
        private const byte CODE_A = (byte)'a';
        private const byte CODE_B = (byte)'b';
        private const byte CODE_C = (byte)'c';

        private ITransitionFunction transitionFunction;

        public TableDrivenTransitionTests()
        {
            transitionFunction = new TableDrivenTransitionFunction();
        }

        [Test]
        public void TableConstructionTest()
        {
            var dfa = new FiniteAutomata("test", true);
            dfa.StartState.Transitions.Add(InputChar.For(CODE_A), new List<State> { dfa.Terminator });
            dfa.Terminator.IsAccepting = true;
            dfa.Terminator.TokenClass = 0xDEAD;

            transitionFunction.Init(dfa);

            var buffer = new byte[1];

            for (int i = 0; i <= byte.MaxValue;  i++)
            {
                var b = (byte) i; // It is impossible to use byte iterator, since it will cause an endless loop
                                  // Another way is to add checked {} construction, this will throw an exception
                                  // when the value will be incremented to 257, in such case we should catch the
                                  // exception which adds overhead.

                buffer[0] = b;
                int tokenClass;
                if (b == (byte) 'a')
                {
                    int tokenLength = transitionFunction.MatchToken(buffer, 0, 1, out tokenClass);
                    Assert.That(tokenClass, Is.EqualTo(0xDEAD));
                    Assert.That(tokenLength, Is.EqualTo(1));
                }
                else
                {
                    Assert.Throws(typeof (UnexpectedTokenException),
                                  () => transitionFunction.MatchToken(buffer, 0, 1, out tokenClass));
                }
            }
        }

        [Test]
        public void CommonPrefixMatchTest()
        {
            var dfa = new FiniteAutomata("test", true);
            var state1 = new State("1");
            var state2 = new State("2");
            var state3 = new State("3");
            var state4 = new State("4");

            // Words: "aab", "aac"
            dfa.StartState.AddTransitionTo(state1, InputChar.For(CODE_A));
            state1.AddTransitionTo(state2, InputChar.For(CODE_A));
            state2.AddTransitionTo(state3, InputChar.For(CODE_B));
            state2.AddTransitionTo(state4, InputChar.For(CODE_C));

            state3.IsAccepting = true;
            state4.IsAccepting = true;
            state3.TokenClass = 0xBABE;
            state4.TokenClass = 0xCAFE;

            transitionFunction.Init(dfa);
            byte[] input = Encoding.ASCII.GetBytes("aabaac");
            int tokenClass;

            int tokenLength = transitionFunction.MatchToken(input, 0, input.Length, out tokenClass);
            Assert.That(tokenClass, Is.EqualTo(0xBABE));
            Assert.That(tokenLength, Is.EqualTo(3));

            tokenLength = transitionFunction.MatchToken(input, tokenLength, input.Length, out tokenClass);
            Assert.That(tokenClass, Is.EqualTo(0xCAFE));
            Assert.That(tokenLength, Is.EqualTo(3));
        }

        //TODO: Test for two words such that one word is prefix of another (abc, ab).
        [Test]
        public void SubWordMatchTest()
        {
            var dfa = new FiniteAutomata("test", true);
            var state1 = new State("1");
            var state2 = new State("2");
            var state3 = new State("3");

            // Words: "abc", "ab"
            dfa.StartState.AddTransitionTo(state1, InputChar.For(CODE_A));
            state1.AddTransitionTo(state2, InputChar.For(CODE_B));
            state2.AddTransitionTo(state3, InputChar.For(CODE_C));

            state3.IsAccepting = true;
            state2.IsAccepting = true;
            state3.TokenClass = 0x0ABC;
            state2.TokenClass = 0x00AB;

            transitionFunction.Init(dfa);
            byte[] input = Encoding.ASCII.GetBytes("abc");
            int tokenClass;

            int tokenLength = transitionFunction.MatchToken(input, 0, input.Length, out tokenClass);
            Assert.That(tokenClass, Is.EqualTo(0x0ABC));
            Assert.That(tokenLength, Is.EqualTo(3));           
        }
    }
}
