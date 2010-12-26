using System;
using System.Collections.Generic;
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

        [Test]
        public void ByteArrayEqualTest()
        {
            var byteArray1 = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var byteArray2 = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var byteArray3 = new byte[] { 0x00, 0x01, 0x02 };
            var byteArray4 = new byte[] {0x03, 0x02, 0x01, 0x00};

            Assert.That(ByteArrayRoutines.AreEqual(byteArray1, byteArray2));
            Assert.That(!ByteArrayRoutines.AreEqual(byteArray1, byteArray3));
            Assert.That(!ByteArrayRoutines.AreEqual(byteArray1, byteArray4));
        }

        [Test]
        public void WTFTest()
        {
            var set = new HashSet<Whatever>();

            for (int i = 0; i <= char.MaxValue; i++)
            {
                var c = (char) i;
                var whatever = new Whatever(c);
                if (!set.Contains(whatever)) set.Add(whatever);
            }

            Console.WriteLine(set.Count);
        }

        private class Whatever : IEquatable<Whatever>
        {
            internal byte[] Buffer { get; set; }

            public Whatever(char c)
            {
                Buffer = Encoding.ASCII.GetBytes(new[] {c});
            }

            public bool Equals(Whatever other)
            {
                return ByteArrayRoutines.AreEqual(Buffer, other.Buffer);
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return base.Equals(obj);

                if (obj is Whatever)
                    return Equals(obj as Whatever);

                return false;
            }

            public override int GetHashCode()
            {
                return ByteArrayRoutines.GetArrayHashCode(Buffer);
            }
        }
    }
}
