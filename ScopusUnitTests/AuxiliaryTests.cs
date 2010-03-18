using System;
using System.Text;
using NUnit.Framework;
using Scopus;
using Scopus.Auxiliary;
using Scopus.LexicalAnalysis;

namespace ScopusUnitTests
{
	[TestFixture]
    public class AuxiliaryTests
    {
        [Test]
        public void IdProviderTest()
        {
            var idProvider = new IDProvider();
            Assert.That(idProvider.GetNext(), Is.EqualTo(0));
            Assert.That(idProvider.GetNext(), Is.EqualTo(1));
            Assert.That(idProvider.GetCurrent(), Is.EqualTo(1));
            Assert.That(idProvider.GetNext(), Is.EqualTo(2));
            Assert.That(idProvider.GetCurrent(), Is.EqualTo(2));
            Assert.That(idProvider.GetCurrent(), Is.EqualTo(2));

            idProvider.Reset();
            Assert.That(idProvider.GetNext() == 0);
            Assert.That(idProvider.GetNext() == 1);
        }

        [Test]
        //[ExpectedException(typeof(InvalidOperationException))]
        public void IdProviderAttemptToUseGetCurrentBeforeGetNextTest()
        {
			Assert.Throws(typeof(InvalidOperationException), () => { var idProvider = new IDProvider(); idProvider.GetCurrent();});
		}

		[Test]
        public void IdProviderStartTest()
        {
            var idProvider = new IDProvider(3);
            Assert.That(idProvider.GetNext(), Is.EqualTo(3));
            Assert.That(idProvider.GetNext(), Is.EqualTo(4));
        }

        [Test]
        public void IdProviderRollingTest()
        {
            var idProvider = new IDProvider(3, 5) {RollingCounter = true};

            Assert.That(idProvider.GetNext(), Is.EqualTo(3));
            Assert.That(idProvider.GetNext(), Is.Not.EqualTo(3));
            Assert.That(idProvider.GetCurrent(), Is.EqualTo(4));
            Assert.That(idProvider.GetNext(), Is.EqualTo(5));
            Assert.That(idProvider.GetNext(), Is.EqualTo(3));
            Assert.That(idProvider.GetNext(), Is.EqualTo(4));
            Assert.That(idProvider.GetNext(), Is.EqualTo(5));
        }

        [Test]
        public void NoRollingTest()
        {
            var idProvider = new IDProvider(3, 5);

            Assert.That(idProvider.GetNext(), Is.EqualTo(3));
            Assert.That(idProvider.GetNext(), Is.Not.EqualTo(3));
            Assert.That(idProvider.GetCurrent(), Is.EqualTo(4));
            Assert.That(idProvider.GetNext(), Is.EqualTo(5));
            Assert.Throws(typeof(InvalidOperationException), () => idProvider.GetNext());
        }

        [Test]
        public void StringParsingTest()
        {
            var samples = new[] {"abcde", "abcd", "abc", "ab", "a"};

            foreach (var sample in samples)
            {
                byte[] asBytes = Encoding.ASCII.GetBytes(sample);
                string asString = PrimitivesParser.ParseString(new Token(asBytes, 0, sample.Length));
                Assert.That(sample, Is.EqualTo(asString));
            }
        }
    }
}
