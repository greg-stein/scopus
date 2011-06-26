using NUnit.Framework;
using Scopus.LexicalAnalysis;
using Scopus.LexicalAnalysis.Algorithms;
using Scopus.LexicalAnalysis.RegularExpressions;

namespace ScopusUnitTests
{
    [TestFixture]
    public class RegExpOrganizerTests
    {
        [Test]
        public void RegExpHasLazyQuantifierTest()
        {
            // a((a|b)+?|c)d
            var regExp = RegExp.Sequence(
                RegExp.Literal("a"),
                RegExp.Choice(
                    RegExp.AtLeastOneOf(
                        RegExp.Choice(
                            RegExp.Literal('a'),
                            RegExp.Literal('b')),
                        Greediness.LazyQuantification),
                    RegExp.Literal('c')),
                RegExp.Literal('d'));

            Assert.That(RegExpOrganizer.HasLazyQuantifiers(regExp));

            // a((a|b)+|c)d
            regExp = RegExp.Sequence(
                RegExp.Literal("a"),
                RegExp.Choice(
                    RegExp.AtLeastOneOf(
                        RegExp.Choice(
                            RegExp.Literal('a'),
                            RegExp.Literal('b'))
                        ),
                    RegExp.Literal('c')),
                RegExp.Literal('d'));

            Assert.IsFalse(RegExpOrganizer.HasLazyQuantifiers(regExp));
        }

        [Test]
        public void RegExpOrganizerRegExpFactorizationTest()
        {
            // a((a|b)+?|c)d
            var regExp = RegExp.Sequence(
                RegExp.Literal("a"),
                RegExp.Choice(
                    RegExp.AtLeastOneOf(
                        RegExp.Choice(
                            RegExp.Literal('a'),
                            RegExp.Literal('b')),
                        Greediness.LazyQuantification),
                    RegExp.Literal('c')),
                RegExp.Literal('d'));

            var factorizedRE = RegExpOrganizer.Factorize(regExp);

            // a((a|b)+?|c)d = a((a|b)+?d | cd)
            var expectedRE = RegExp.Sequence(
                RegExp.Choice(
                    RegExp.Sequence(
                        RegExp.AtLeastOneOf(
                            RegExp.Choice(
                                RegExp.Literal('a'),
                                RegExp.Literal('b')),
                            Greediness.LazyQuantification),
                        RegExp.Literal('d')),
                    RegExp.Sequence(
                        RegExp.Literal('c'),
                        RegExp.Literal('d')
                    )
                )
            );
            Assert.That(factorizedRE, Is.EqualTo(expectedRE));
        }
    }
}