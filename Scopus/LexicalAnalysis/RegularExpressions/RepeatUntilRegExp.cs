using System;
using Scopus.LexicalAnalysis.Algorithms;

namespace Scopus.LexicalAnalysis.RegularExpressions
{
    internal class RepeatUntilRegExp : RegExp
    {
        private RegExp mRepeatedRegExp;
        private RegExp mSuffixRegExp;

        internal RepeatUntilRegExp(RegExp repeatedRegExp, RegExp suffixRegExp)
        {
            mRepeatedRegExp = repeatedRegExp;
            mSuffixRegExp = suffixRegExp;
        }

        internal override FiniteAutomata AsNFA()
        {
            var repeatedDFA = NFAToDFAConverter.Convert(mRepeatedRegExp.AsNFA(true));
            var suffixDFA = NFAToDFAConverter.Convert(mSuffixRegExp.AsNFA(true));

            return new CrossAutomata(repeatedDFA, suffixDFA);
        }
    }
}
