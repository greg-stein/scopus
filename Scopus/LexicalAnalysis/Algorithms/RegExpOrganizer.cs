using System;
using Scopus.LexicalAnalysis.RegularExpressions;

namespace Scopus.LexicalAnalysis.Algorithms
{
    class RegExpOrganizer
    {
        internal static RegExp OrganizeRegExp(RegExp regExp)
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

        public static RegExp Factorize(RegExp regExp)
        {
            throw new NotImplementedException();
        }
    }
}
