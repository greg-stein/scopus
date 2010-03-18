using System.Collections.Generic;

namespace Scopus.LexicalAnalysis.RegExp
{
    internal class RepetitionRegExp : RegExp
    {
        internal RegExp ExpressionToRepeat { get; set; }
        internal bool AtLeastOneOccurrence { get; set; }

        internal RepetitionRegExp(RegExp expression)
        {
            ExpressionToRepeat = expression;
            AtLeastOneOccurrence = false;
        }

        internal RepetitionRegExp(RegExp expression, bool atLeastOneOccurrence)
        {
            ExpressionToRepeat = expression;
            AtLeastOneOccurrence = atLeastOneOccurrence;
        }

        protected override RegExp[] SubExpressions
        {
            get { return new[] {ExpressionToRepeat}; }
        }

        internal override bool CalculateNullable()
        {
            // This is less readable, but do the same: 
            // return !AtLeastOneOccurrence || ExpressionToRepeat.Nullable;

            if (AtLeastOneOccurrence)
            {
                return ExpressionToRepeat.Nullable;
            }

            return true;
        }

        internal override HashSet<int> CalculateFirstPos()
        {
            return ExpressionToRepeat.FirstPos;
        }

        internal override HashSet<int> CalculateLastPos()
        {
            return ExpressionToRepeat.LastPos;
        }
    }
}
