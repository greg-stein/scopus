using System.Text;
using NUnit.Framework;
using Scopus.LexicalAnalysis;
using Scopus.LexicalAnalysis.Algorithms;
using Scopus.LexicalAnalysis.RegularExpressions;

namespace ScopusUnitTests
{
    [TestFixture]
    public class NfaToDFAConverterTests
    {
        // Tests DFA to DFA conversion, the target automata should be exact the same
        [Test]
        public void DFAConversionTest()
        {
            var dfa = new FiniteAutomata("dfa");
            var state1 = new State("1");
            var state2 = new State("2");
            var state3 = new State("3");
            var state4 = new State("4");
            var state5 = new State("5");

            dfa.StartState.AddTransitionTo(state1, InputChar.For(0x01));
            dfa.StartState.AddTransitionTo(state2, InputChar.For(0x02));
            dfa.StartState.AddTransitionTo(state3, InputChar.For(0x03));
            dfa.StartState.AddTransitionTo(state3, InputChar.For(0x04));

            state1.AddTransitionTo(state4, InputChar.For(0x05));
            state2.AddTransitionTo(state4, InputChar.For(0x05));
            state3.AddTransitionTo(state4, InputChar.For(0x05));

            state1.AddTransitionTo(state5, InputChar.For(0x06));
            state2.AddTransitionTo(state5, InputChar.For(0x06));
            state3.AddTransitionTo(state5, InputChar.For(0x06));

            state4.AddTransitionTo(dfa.Terminator, InputChar.For(0x07));
            state5.AddTransitionTo(dfa.Terminator, InputChar.For(0x07));

            dfa.Terminator.IsAccepting = true;
            var resultDfa = NFAToDFAConverter.Convert(dfa);

            ValidateTerminator(resultDfa);

            Assert.That(resultDfa.GetStates().Count, Is.EqualTo(7));
            //var state = resultDfa.StartState.Simulate(0, 1, 5, 7);
            Assert.That(resultDfa.StartState.Simulate(0, 1, 5, 7), Is.SameAs(resultDfa.Terminator));
            Assert.That(resultDfa.StartState.Simulate(0, 2, 5, 7), Is.SameAs(resultDfa.Terminator));
            Assert.That(resultDfa.StartState.Simulate(0, 3, 5, 7), Is.SameAs(resultDfa.Terminator));
            Assert.That(resultDfa.StartState.Simulate(0, 4, 5, 7), Is.SameAs(resultDfa.Terminator));

            Assert.That(resultDfa.StartState.Simulate(0, 1, 6, 7), Is.SameAs(resultDfa.Terminator));
            Assert.That(resultDfa.StartState.Simulate(0, 2, 6, 7), Is.SameAs(resultDfa.Terminator));
            Assert.That(resultDfa.StartState.Simulate(0, 3, 6, 7), Is.SameAs(resultDfa.Terminator));
            Assert.That(resultDfa.StartState.Simulate(0, 4, 6, 7), Is.SameAs(resultDfa.Terminator));

            Assert.Throws<SimulationException>(() => resultDfa.StartState.Simulate(0, 1, 5, 6));
            Assert.Throws<SimulationException>(() => resultDfa.StartState.Simulate(0, 2, 5, 8));
            Assert.Throws<SimulationException>(() => resultDfa.StartState.Simulate(0, 3, 5, 6));
            Assert.Throws<SimulationException>(() => resultDfa.StartState.Simulate(0, 4, 6, 6));
        }

        [Test]
        public void EpsilonTransitionsConversionTest()
        {
            //      1        2        4
            // (S) ---> (A) ---> (B) ---> [T]
            //           |
            //           ---> (C) ---> (D) ---> [T]
            //            e        3        4
            var stateA = new State("A");
            var stateB = new State("B");
            var stateC = new State("C");
            var stateD = new State("D");

            var nfa = new FiniteAutomata();
            nfa.StartState.AddTransitionTo(stateA, InputChar.For(1));
            stateA.AddTransitionTo(stateB, InputChar.For(2));
            stateA.AddTransitionTo(stateC, InputChar.Epsilon());
            stateC.AddTransitionTo(stateD, InputChar.For(3));
            stateB.AddTransitionTo(nfa.Terminator, InputChar.For(4));
            stateD.AddTransitionTo(nfa.Terminator, InputChar.For(4));
            nfa.Terminator.IsAccepting = true;

            var dfa = NFAToDFAConverter.Convert(nfa);

            ValidateTerminator(dfa);

            Assert.That(dfa.GetStates().Count, Is.EqualTo(5));
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 4), Is.SameAs(dfa.Terminator));
            Assert.That(dfa.StartState.Simulate(0, 1, 3, 4), Is.SameAs(dfa.Terminator));

            Assert.Throws<SimulationException>(() => dfa.StartState.Simulate(0, 2, 2, 4));
            Assert.Throws<SimulationException>(() => dfa.StartState.Simulate(0, 1, 2, 5));
            Assert.Throws<SimulationException>(() => dfa.StartState.Simulate(0, 1, 3, 2));
        }

        [Test]
        public void BackEpsilonTransitionTest()
        {
            //      1        2        3 
            // (S) ---> (A) ---> (B) ---> [T]
            //            \______/
            //               e
            var stateA = new State("A");
            var stateB = new State("B");

            var nfa = new FiniteAutomata("nfa");
            nfa.StartState.AddTransitionTo(stateA, InputChar.For(0x01));
            stateA.AddTransitionTo(stateB, InputChar.For(0x02));
            stateB.AddTransitionTo(nfa.Terminator, InputChar.For(0x03));
            stateB.AddTransitionTo(stateA, InputChar.Epsilon());
            nfa.Terminator.IsAccepting = true;

            var dfa = NFAToDFAConverter.Convert(nfa);

            ValidateTerminator(dfa);

            Assert.That(dfa.StartState.Simulate(0, 1, 2, 3), Is.SameAs(dfa.Terminator));
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2, 3), Is.SameAs(dfa.Terminator));
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2, 2, 3), Is.SameAs(dfa.Terminator));
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2, 2, 2, 3), Is.SameAs(dfa.Terminator));
            Assert.Throws<SimulationException>(() => dfa.StartState.Simulate(0, 1, 2, 4));
            Assert.Throws<SimulationException>(() => dfa.StartState.Simulate(0, 1, 2, 3, 2));
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2), Is.Not.SameAs(dfa.Terminator));
        }

        [Test]
        public void DoubleEpsilonTransitionTest()
        {
            //      1        2        3 
            // (S) ---> (A) ---> (B) ---> [T]
            //   \______/ \______/
            //      e        e
            var stateA = new State("A");
            var stateB = new State("B");

            var nfa = new FiniteAutomata("nfa");
            nfa.StartState.AddTransitionTo(stateA, InputChar.For(0x01));
            stateA.AddTransitionTo(stateB, InputChar.For(0x02));
            stateB.AddTransitionTo(nfa.Terminator, InputChar.For(0x03));
            stateB.AddTransitionTo(stateA, InputChar.Epsilon());
            stateA.AddTransitionTo(nfa.StartState, InputChar.Epsilon());
            nfa.Terminator.IsAccepting = true;

            var dfa = NFAToDFAConverter.Convert(nfa);

            ValidateTerminator(dfa);

            Assert.That(dfa.StartState.Simulate(0, 1, 2, 3), Is.SameAs(dfa.Terminator));
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2, 3), Is.SameAs(dfa.Terminator));
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2, 2, 3), Is.SameAs(dfa.Terminator));
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2, 2, 2, 3), Is.SameAs(dfa.Terminator));
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 1, 2, 1, 2, 3), Is.SameAs(dfa.Terminator));
            Assert.That(dfa.StartState.Simulate(0, 1, 1, 1, 2, 2, 3), Is.SameAs(dfa.Terminator));
            Assert.Throws<SimulationException>(() => dfa.StartState.Simulate(0, 1, 2, 4));
            Assert.Throws<SimulationException>(() => dfa.StartState.Simulate(0, 1, 2, 3, 2));
            Assert.Throws<SimulationException>(() => dfa.StartState.Simulate(0, 1, 2, 1, 2, 1, 3));
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2), Is.Not.SameAs(dfa.Terminator));
        }

        [Test] 
        public void KleeneClosureTest()
        {
            //      e        2        e  
            // (S) ---> (1) ---> (2) ---> [T]
            //   \        \___e__/        /
            //    \___________e__________/
            var stateA = new State("A");
            var stateB = new State("B");

            var nfa = new FiniteAutomata("nfa");
            nfa.StartState.AddTransitionTo(stateA, InputChar.Epsilon());
            stateA.AddTransitionTo(stateB, InputChar.For(2));
            stateB.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            stateB.AddTransitionTo(stateA, InputChar.Epsilon());
            nfa.StartState.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            nfa.Terminator.IsAccepting = true;

            var dfa = NFAToDFAConverter.Convert(nfa);

            Assert.That(dfa.StartState.Simulate().IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 2).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 2, 2).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 2, 2, 2).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 2, 2, 2, 2).IsAccepting);

            // The result DFA has the following form:
            //              ___
            //      1      /   \
            // [S] ---> [T]     | 1
            //             \___/
            // However, generally speaking it should be single accepting state with self transition on '1' 
            // Note that both S and T are accepting states
        }

        [Test]
        public void PrefixedKleeneClosureTest()
        {
            //      1        e        2        e  
            // (S) ---> (0) ---> (1) ---> (2) ---> [T]
            //            \        \___e__/        /
            //             \___________e__________/
            var state0 = new State("0");
            var stateA = new State("A");
            var stateB = new State("B");

            var nfa = new FiniteAutomata("nfa");
            nfa.StartState.AddTransitionTo(state0, InputChar.For(1));
            state0.AddTransitionTo(stateA, InputChar.Epsilon());
            stateA.AddTransitionTo(stateB, InputChar.For(2));
            stateB.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            stateB.AddTransitionTo(stateA, InputChar.Epsilon());
            state0.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            nfa.Terminator.IsAccepting = true;

            var dfa = NFAToDFAConverter.Convert(nfa);

            Assert.That(dfa.StartState.Simulate(0, 1).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 1, 2).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2, 2).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2, 2, 2).IsAccepting);
        }

        [Test]
        public void PrefixedSuffixedKleeneClosureTest()
        {
            //      1        e        e        2        e        e        1
            // (S) ---> (0) ---> (1) ---> (2) ---> (3) ---> (4) ---> (5) ---> [T]
            //                     \        \___e__/        /
            //                      \___________e__________/
            var state0 = new State("0");
            var state1 = new State("1");
            var state2 = new State("2");
            var state3 = new State("3");
            var state4 = new State("4");
            var state5 = new State("5");

            var nfa = new FiniteAutomata("nfa");
            nfa.StartState.AddTransitionTo(state0, InputChar.For(1));
            state0.AddTransitionTo(state1, InputChar.Epsilon());
            state1.AddTransitionTo(state2, InputChar.Epsilon());
            state2.AddTransitionTo(state3, InputChar.For(2));
            state3.AddTransitionTo(state4, InputChar.Epsilon());
            state4.AddTransitionTo(state5, InputChar.Epsilon());
            state5.AddTransitionTo(nfa.Terminator, InputChar.For(1));
            state1.AddTransitionTo(state4, InputChar.Epsilon());
            state3.AddTransitionTo(state2, InputChar.Epsilon());
            nfa.Terminator.IsAccepting = true;

            var dfa = NFAToDFAConverter.Convert(nfa);

            Assert.That(dfa.StartState.Simulate(0, 1, 1).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 1).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2, 1).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2, 2, 1).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 1, 2, 2, 2, 2, 1).IsAccepting);
        }

        [Test]
        public void PrefixedSuffixedKleeneClosureSameByteTest()
        {
            //      1        e        e        1        e        e        1
            // (S) ---> (0) ---> (1) ---> (2) ---> (3) ---> (4) ---> (5) ---> [T]
            //                     \        \___e__/        /
            //                      \___________e__________/
            var state0 = new State("0");
            var state1 = new State("1");
            var state2 = new State("2");
            var state3 = new State("3");
            var state4 = new State("4");
            var state5 = new State("5");

            var nfa = new FiniteAutomata("nfa");
            nfa.StartState.AddTransitionTo(state0, InputChar.For(1));
            state0.AddTransitionTo(state1, InputChar.Epsilon());
            state1.AddTransitionTo(state2, InputChar.Epsilon());
            state2.AddTransitionTo(state3, InputChar.For(1));
            state3.AddTransitionTo(state4, InputChar.Epsilon());
            state4.AddTransitionTo(state5, InputChar.Epsilon());
            state5.AddTransitionTo(nfa.Terminator, InputChar.For(1));
            state1.AddTransitionTo(state4, InputChar.Epsilon());
            state3.AddTransitionTo(state2, InputChar.Epsilon());
            nfa.Terminator.IsAccepting = true;

            var dfa = NFAToDFAConverter.Convert(nfa);

            Assert.That(dfa.StartState.Simulate(0, 1, 1).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 1, 1, 1).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 1, 1, 1, 1).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 1, 1, 1, 1, 1).IsAccepting);
            Assert.That(dfa.StartState.Simulate(0, 1, 1, 1, 1, 1, 1).IsAccepting);
        }

        [Test]
        public void FuckingTest()
        {
            var ascii = Encoding.ASCII;

            var regExp = RegExp.Sequence(
                    RegExp.Literal("/*", ascii), 
                    RegExp.AnyNumberOf(
                        RegExp.Range((char)0, (char)255, ascii)
                    ),
                    RegExp.Literal("*/", ascii)
                );

            var nfa = regExp.AsNFA(true);
            var dfa = NFAToDFAConverter.Convert(nfa);
            //Assert.That(dfa.StartState.Simulate("/abc/").IsAccepting);
        }

        private void ValidateTerminator(FiniteAutomata dfa)
        {
            State acceptingState = null;
            var dfaStates = dfa.GetStates();
            foreach (State state in dfaStates)
            {
                if (state.IsAccepting)
                {
                    if (acceptingState != null)
                        Assert.Fail("More than one accepting states!");
                    acceptingState = state;
                }
            }
            dfa.Terminator = acceptingState;
        }
    }
}
