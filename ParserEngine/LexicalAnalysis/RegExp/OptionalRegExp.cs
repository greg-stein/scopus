using System;
using System.Collections.Generic;

namespace ParserEngine.LexicalAnalysis.RegExp
{
    internal class OptionalRegExp : RegExp
    {
        internal RegExp OptionalExpression { get; set; }

        internal OptionalRegExp(RegExp optionalExpr)
        {
            OptionalExpression = optionalExpr;
        }

        protected override RegExp[] SubExpressions
        {
            get { return new[] {OptionalExpression};}
        }

        internal override bool CalculateNullable()
        {
            return true;
        }

        internal override HashSet<int> CalculateFirstPos()
        {
            return OptionalExpression.FirstPos;
        }

        internal override HashSet<int> CalculateLastPos()
        {
            return OptionalExpression.LastPos;
        }
    }
}
