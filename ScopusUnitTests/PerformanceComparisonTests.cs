using System;
using System.Diagnostics;
using NUnit.Framework;

namespace ScopusUnitTests
{
    [TestFixture, Ignore]
    public class PerformanceComparisonTests
    {
        [Test]
        public void ConditionVsLookupTableTest()
        {
            var rnd = new Random();
            const int INPUT_LENGTH = 1000000;
            var input = new byte[INPUT_LENGTH];
            rnd.NextBytes(input);

            var watch = new Stopwatch();
            int token12Cnt = 0;
            int token13Cnt = 0;
            int counter = 0;
            watch.Start();
            while (counter < INPUT_LENGTH)
            {
                if (input[counter] == 1)
                {
                    if (input[counter + 1] == 2)
                    {
                        counter++;
                        token12Cnt++;
                    }
                    else if (input[counter + 1] == 3)
                    {
                        counter++;
                        token13Cnt++;
                    }
                }
                counter++;
            }
            Console.WriteLine("Direct code tokenizer recognized {0} instances of strings '12' and {1} instances of '13'. It took {2} milliseconds.", token12Cnt, token13Cnt, watch.ElapsedMilliseconds);
            watch.Stop();
            watch.Reset();

            var transition = new int[4,256];
            transition[0, 1] = 1;
            transition[1, 2] = 2;
            transition[1, 3] = 3;
            var acceptingStates = new bool[4] {false, false, true, true};
            var tokensRecognized = 0;
            counter = 0;
            var currentState = 0;
            watch.Start();
            while (counter < INPUT_LENGTH)
            {
                currentState = transition[currentState, input[counter]];
                if (acceptingStates[currentState])
                {
                    tokensRecognized++;
                    currentState = 0;
                }
                counter++;
            }
            Console.WriteLine("DFA tokenizer recognized {0} instances of strings '12' and '13'. It took {1} milliseconds.", tokensRecognized, watch.ElapsedMilliseconds);
            watch.Stop();
            watch.Reset();
        }
    }
}
