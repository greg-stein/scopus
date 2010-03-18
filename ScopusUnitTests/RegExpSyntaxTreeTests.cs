using System.Collections.Generic;
using NUnit.Framework;
using Scopus.LexicalAnalysis.RegExp;

namespace ScopusUnitTests
{
    [TestFixture]
    public class RegExpSyntaxTreeTests
    {
        private RegExp regExp;
        private RegExp starExp;
        private RegExp choiceExp;
        private RegExp termA1;
        private RegExp termB1;
        private RegExp termA2;
        private RegExp termB2;

        public RegExpSyntaxTreeTests()
        {
            // (a|b)*ab
            //var regExp = RegExp.Sequence(RegExp.AnyNumberOf(RegExp.Choice(RegExp.Term('a'), RegExp.Term('b'))),
            //                             RegExp.Term('a'), RegExp.Term('b'));           
            termA1 = RegExp.Term('a');
            termB1 = RegExp.Term('b');
            termA2 = RegExp.Term('a');
            termB2 = RegExp.Term('b');
            choiceExp = RegExp.Choice(termA1, termB1);
            starExp = RegExp.AnyNumberOf(choiceExp);
            regExp = RegExp.Sequence(starExp, termA2, termB2);
        }

        [Test]
        public void SyntaxTree_NullableFunctionTest()
        {
            Assert.IsFalse(termA1.Nullable);
            Assert.IsFalse(termB1.Nullable);
            Assert.IsFalse(termA2.Nullable);
            Assert.IsFalse(termB2.Nullable);
            Assert.IsFalse(choiceExp.Nullable);
            Assert.IsTrue(starExp.Nullable);
            Assert.IsFalse(regExp.Nullable);
        }

        [Test]
        public void SyntaxTree_FirstPosTest()
        {
            Assert.That(AreSame(termA1.FirstPos, new HashSet<int> { 1 }));
            Assert.That(AreSame(termB1.FirstPos, new HashSet<int> { 2 }));
            Assert.That(AreSame(termA2.FirstPos, new HashSet<int> { 3 }));
            Assert.That(AreSame(termB2.FirstPos, new HashSet<int> { 4 }));
            Assert.That(AreSame(choiceExp.FirstPos, new HashSet<int> { 1, 2 }));
            Assert.That(AreSame(starExp.FirstPos, new HashSet<int> { 1, 2 }));
            Assert.That(AreSame(regExp.FirstPos, new HashSet<int> { 1, 2, 3 }));
        }

        [Test]
        public void SyntaxTree_LastPosTest()
        {
            Assert.That(AreSame(termA1.LastPos, new HashSet<int> { 1 }));
            Assert.That(AreSame(termB1.LastPos, new HashSet<int> { 2 }));
            Assert.That(AreSame(termA2.LastPos, new HashSet<int> { 3 }));
            Assert.That(AreSame(termB2.LastPos, new HashSet<int> { 4 }));
            Assert.That(AreSame(choiceExp.LastPos, new HashSet<int> { 1, 2 }));
            Assert.That(AreSame(starExp.LastPos, new HashSet<int> { 1, 2 }));
            Assert.That(AreSame(regExp.LastPos, new HashSet<int> { 4 }));
        }

        private static bool AreSame(HashSet<int> set1, HashSet<int> set2)
        {
            return set1.IsSubsetOf(set2) && set2.IsSubsetOf(set1);
        }
    }
}
