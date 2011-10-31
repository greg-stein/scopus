using System;
using Scopus.LexicalAnalysis.RegularExpressions;

namespace Scopus.LexicalAnalysis.Algorithms
{
    //TODO: Clarify whether we should factorize the whole re or part is OK
    // Example:
    // (a|b)(c|d)(e|f) --> ace|acf|ade|adf|...|bdf
    class RegExpFactorizer
    {
        internal static RegExp FactorizeRegExp(RegExp regExp)
        {
            if (!HasLazyQuantifiers(regExp))
            {
                return regExp; // Do nothing, because no lazy quantifiers are used.
            }
            return null;
        }

        internal static bool HasLazyQuantifiers(RegExp regExp)
        {
            var repetitionRegExp = regExp as RepetitionRegExp;
            if (repetitionRegExp != null && repetitionRegExp.Greediness == Greediness.LazyQuantification)
            {
                return true;
            }
            else
            {
                bool hasLazyChilds = false;

                if (regExp.ChildExpressions != null)
                {
                    foreach (var childExpression in regExp.ChildExpressions)
                    {
                        hasLazyChilds |= HasLazyQuantifiers(childExpression);
                        if (hasLazyChilds)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // The idea:
        // a((a|b)+?|c)d = a((a|b)+?d | cd)
        // if there is no regexp after lazy quantifier, change it to greedy quantifier.
        // a(d(a|b)+?|c) --> a(d(a|b)+|c)
        // otherwise, move the following regexp into the regexp and add it to sequence:
        // a(d(a|b)+?|c)d(f|g) --> a(d(a|b)+?d(f|g)|cd(f|g))
        // ((a(d(a|b)+?|c)d(f|g))|(whatever))something -->  
        // Algorithm:
        //    1) Find closest parent of type SequenceRegExp
        //    2) if position of current regexp is last, change lazy to greedy and exit
        //    3) otherwise 
        public static RegExp Factorize(RegExp regExp)
        {
            throw new NotImplementedException();
        }
    }
}
