using System.Collections.Generic;
using NUnit.Framework;
using Scopus.LexicalAnalysis;

namespace ScopusUnitTests
{
    [TestFixture]
    public class FiniteAutomatonTests
    {
        private const byte CODE_A = (byte)'a';
        private const byte CODE_B = (byte)'b';
        private const byte CODE_C = (byte)'c';
        private const byte CODE_D = (byte)'d';
        private const byte CODE_E = (byte)'e';
        private const byte CODE_F = (byte)'f';

        private FiniteAutomata mFiniteAutomata;

        private State stateA;
        private State stateB;
        private State stateC;
        private State stateD;
        private State stateE;
        private State stateF;

        private HashSet<State> expectedSet;

        [SetUp]
        public void InitTests()
        {
            stateA = new State("A");
            stateB = new State("B");
            stateC = new State("C");
            stateD = new State("D");
            stateE = new State("E");
            stateF = new State("F");

            mFiniteAutomata = new FiniteAutomata("Test")
            {
                StartState = new State("start"),
                Terminator = new State("terminator")
            };

            expectedSet = new HashSet<State>()
            {
             mFiniteAutomata.StartState,
             stateA,
             stateB,
             stateC,
             stateD,
             stateE,
             stateF,
             mFiniteAutomata.Terminator
            };
        }

        [Test]
        public void GetAllStatesChainedStatesTest()
        {
            mFiniteAutomata.StartState.AddTransitionTo(stateA, InputChar.For(CODE_A));
            stateA.AddTransitionTo(stateB, InputChar.For(CODE_B));
            stateB.AddTransitionTo(stateC, InputChar.For(CODE_C));
            stateC.AddTransitionTo(stateD, InputChar.For(CODE_D));
            stateD.AddTransitionTo(stateE, InputChar.For(CODE_E));
            stateE.AddTransitionTo(stateF, InputChar.For(CODE_F));
            stateF.AddTransitionTo(mFiniteAutomata.Terminator, InputChar.For(CODE_F));

            Assert.That(mFiniteAutomata.GetStates(), Is.EquivalentTo(expectedSet));
        }

        [Test]
        public void GetAllStatesChainedStatesWithEpsilonTest()
        {
            mFiniteAutomata.StartState.AddTransitionTo(stateA, InputChar.For(CODE_A));
            stateA.AddTransitionTo(stateB, InputChar.For(CODE_B));
            stateB.AddTransitionTo(stateC, InputChar.For(CODE_C));
            stateC.AddTransitionTo(stateD, InputChar.For(CODE_D));
            stateD.AddTransitionTo(stateE, InputChar.For(CODE_E));
            stateE.AddTransitionTo(stateF, InputChar.Epsilon());
            stateF.AddTransitionTo(mFiniteAutomata.Terminator, InputChar.For(CODE_F));

            var actualStates = mFiniteAutomata.GetStates();
            Assert.That(actualStates, Is.EquivalentTo(expectedSet));            
        }

        [Test]
        public void GetAllCircularChainedStatesTest()
        {
            mFiniteAutomata.StartState.AddTransitionTo(stateA, InputChar.For(CODE_A));
            stateA.AddTransitionTo(stateB, InputChar.For(CODE_B));
            stateB.AddTransitionTo(stateC, InputChar.For(CODE_C));
            stateC.AddTransitionTo(stateD, InputChar.For(CODE_D));
            stateD.AddTransitionTo(stateE, InputChar.For(CODE_E));
            stateE.AddTransitionTo(stateF, InputChar.For(CODE_F));
            stateF.AddTransitionTo(mFiniteAutomata.Terminator, InputChar.For(CODE_F));
            mFiniteAutomata.Terminator.AddTransitionTo(mFiniteAutomata.StartState, InputChar.Epsilon());

            var actualStates = mFiniteAutomata.GetStates();
            Assert.That(actualStates, Is.EquivalentTo(expectedSet));
        }

        [Test]
        public void GetAllTreeStatesTest()
        {
            //     (Start)
            //     /    \
            //   (A)    (B)
            //   / \    / \
            // (C) (D)(E) (F)
            //  \   |  |   /
            //   \  |  |  /
            //    \ |  | /
            //  (Terminator)
            mFiniteAutomata.StartState.AddTransitionTo(stateA, InputChar.For(CODE_A));
            mFiniteAutomata.StartState.AddTransitionTo(stateB, InputChar.For(CODE_B));
            stateA.AddTransitionTo(stateC, InputChar.For(CODE_C));
            stateA.AddTransitionTo(stateD, InputChar.For(CODE_D));
            stateB.AddTransitionTo(stateE, InputChar.For(CODE_E));
            stateB.AddTransitionTo(stateF, InputChar.For(CODE_F));

            stateC.AddTransitionTo(mFiniteAutomata.Terminator, InputChar.Epsilon());
            stateD.AddTransitionTo(mFiniteAutomata.Terminator, InputChar.Epsilon());
            stateE.AddTransitionTo(mFiniteAutomata.Terminator, InputChar.Epsilon());
            stateF.AddTransitionTo(mFiniteAutomata.Terminator, InputChar.Epsilon());

            var actualStates = mFiniteAutomata.GetStates();
            Assert.That(actualStates, Is.EquivalentTo(expectedSet));
        }

    }
}
